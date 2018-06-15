using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : PhysicsCharacter {

    #region Properties

    int Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            if (OnHealthChanged != null)
            {
                OnHealthChanged(health);
            }
        }
    }

    int HealthJuice
    {
        get
        {
            return healthJuice;
        }
        set
        {
            healthJuice = value;
            if (OnHealthJuiceChanged != null)
            {
                OnHealthJuiceChanged(healthJuice);
            }
        }
    }
    public int Exp
    {
        get
        {
            return exp;
        }
        set
        {
            if(value > exp)
            {
                exp = value;
            }
            else
            {
                Debug.LogWarning("The players exp cannot decrease!");
            }
            if (OnExpChanged != null)
            {
                OnExpChanged(exp, expToNextLevel);
            }
        }
    }
    public int ExpToNextLevel
    {
        get
        {
            return expToNextLevel;
        }
        private set
        {
            expToNextLevel = value;
            if (OnExpChanged != null)
            {
                OnExpChanged(exp, expToNextLevel);
            }
        }
    }
    public int Level
    {
        get
        {
            return level;
        }
        private set
        {
            level = value;
            if (OnLevelChanged != null)
            {
                OnLevelChanged(level);
            }
        }
    }

    #endregion

    #region Fields

    // The events the player can call
    #region Events

    // Delegate for Health changes
    public event System.Action<int> OnHealthChanged;

    // Delegate for Health  Juice changes
    public event System.Action<int> OnHealthJuiceChanged;

    // Delegate for Exp changes
    public event System.Action<int, int> OnExpChanged;

    // Delegate for Level changes
    public event System.Action<int> OnLevelChanged;

    #endregion

    // The attributes of the player
    #region Stats and Attributes

    [Header("Stats"), SerializeField] int maxHealth = 100;
    private int health = 100;

    [SerializeField] int maxHealthJuice = 100;
    private int healthJuice = 100;

    [SerializeField] int baseAttack = 5;
    [SerializeField] int attackPerLevelUp = 3;
    private int attack = 5;

    [SerializeField] int baseDefense = 5;
    [SerializeField] int defensePerLevelUp = 3;
    private int defense = 5;

    private int level = 1;

    private int expToNextLevel = 1;
    private int exp = 0;

    #endregion

    public enum State
    {
        freeToMove,
        dodging,
        attacking,
        knockedBack,
        healing
    }; // State machine for the player
    State playerState = State.freeToMove; // Stores the current state of the player

    PlayerInput input;
    Animator anim;

    bool bDodgeStill = false;

    // Fields to manipulate the jump
    [Header("Jump & Physics"), SerializeField] float jumpPower = 10;
    [SerializeField] float fallMultiplier = 2f; // The higher this value, the slower the player will fall after jumping up, when still holding jump and the faster he will fall when not holding it

    // Fields to manipulate the knockback Applied to the player
    [Header("Knockback"), SerializeField] float knockBackCapY = 2f; // the highest velocity the player can be vertically knocked back
    Vector2 knockBackForce;
    [SerializeField] float knockBackDuration = 0.05f; // The amount of seconds, the player will be knocked back
    [SerializeField] int framesFreezedAfterHit = 8; // The amount of frames the player will be forced to stand still when he is being hit

    // Fields to manipulate the Dodge
    [Header("Dodge"), SerializeField] float dodgePower = 100f; // Force forward when dodging
    [SerializeField] float dodgeUpPower = 20f; // This defines the applied Dodge Up Power
    private float appliedDodgeUpPower; // The actual force getting applied upwards when dodging
    [SerializeField] float dodgeCooldown = 1f;
    bool bDodgable = true; // Stores wether the player is able to dodge or not

    // Fields to manipulate the attack
    [Header("Attack"), SerializeField] float attackReach = 0.2f; // How far the attack hitbox reaches
    [SerializeField] float attackCooldown = 1f;
    bool bAttackable = true; // Stores wether the player is able to attack or not
    [SerializeField] float knockBackStrength = 3f; // The amount of knockback the player is applying to hit enemies
    Vector2 attackDirection; // The direction for the raycast, checking for enemies to hit
    [SerializeField] float upwardsVeloAfterHitDown = 0.06f; // The velocity with which the player gets pushed upwards after hitting an enemy under him with a successful attack
    [SerializeField] float upwardsVeloAfterHitDownTime = 0.008f; // The duration the player gets pushed upwards after hitting an enemy under him with a successful attack

    // Fields to manipulate the healing
    [Header("Healing"), SerializeField] int healDuration = 5; // The frames one has to wait in between one transfer of Health juice to health
    private int healCounter = 0; // The actual counter for the heal duration
    [SerializeField] int juiceRegenValue = 10; // The amount of Juice restored when collecting a juice particle
    [SerializeField] float zoomAmountWhenHealing = 0.002f;

    [Header("General"), SerializeField] float invincibilityTime = 1f; // The amount of seconds, the player is invincible after getting hit
    private float invincibilityCounter = 0f; // This counts down until player can be hit again. Only if this value is 0, the player can be hit.

    // Walking speed of the Player
    [SerializeField] float speed = 1;

    [SerializeField] float wallSlideSpeed = 3f; // How fast the player slides down a wall while holding towards it

    #endregion

    // Use this for initialization
    void Awake ()
    {
        input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        InitializeAttributes();
    }

    // Update is called once per frame
    void Update () {
		
	}

    #region Helper Methods

    /// <summary>
    ///  Respawns the player at the currently activated checkpoint
    /// </summary>
    private void Respawn()
    {
        transform.position = GameManager.Instance.currentCheckpoint;
        playerState = State.freeToMove;
        Health = maxHealth;
        HealthJuice = maxHealthJuice;
    }

    #region Attribute Handling

    /// <summary>
    /// Changes the players attributes for the level up
    /// </summary>
    private void LevelUp()
    {
        Level++;
        Exp -= expToNextLevel;
        ExpToNextLevel = (int)Mathf.Pow(level, 2);
        defense += defensePerLevelUp;
        attack += attackPerLevelUp;
    }

    private void InitializeAttributes()
    {
        // Make the player have full health
        Health = maxHealth;

        // Make the player have full health Juice
        HealthJuice = maxHealthJuice;

        // Make the player have the base attack value at start
        attack = baseAttack;

        ExpToNextLevel = 1;
        Exp = 0;

        Level = 1;
    }

    #endregion

    #region Input

    /// <summary>
    /// Checks if the player is holding the direction, hes facing in
    /// </summary>
    /// <returns> True if the player is holding in the direction, he is facing. False if he is not.</returns>
    private bool HoldingInDirection()
    {
        if (input.Horizontal < 0 && transform.localScale.x < 0)
        {
            return true;
        }
        else if (input.Horizontal > 0 && transform.localScale.x > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the Player is giving directional input to walk or not and turn him accordingly
    /// </summary>
    private void CheckForInput()
    {
        if (input.Horizontal < 0)
        {
            transform.localScale = -Vector3.right;
            anim.SetBool("Idling", false);
        }
        else if (input.Horizontal > 0)
        {
            transform.localScale = Vector3.right;
            anim.SetBool("Idling", false);
        }
        else
        {
            anim.SetBool("Idling", true);
        }
    }

    #endregion

    #region DamageCalculation

    /// <summary>
    /// Damages the player and sets the knockback
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="knockBack"></param>
    public void TakeDamage(int damage, Vector2 knockBack)
    {
        if (playerState != State.attacking && playerState != State.dodging)
        {
            playerState = State.knockedBack;
            if (invincibilityCounter == 0)
            {
                if (damage - defense > 0)
                {
                    Health -= damage - defense;
                }
                else
                {
                    Health--;
                }
                invincibilityCounter = invincibilityTime;
            }
            // Set the knockback force to be applied
            knockBackForce = knockBack;
        }
    }

    #endregion

    #endregion

}
