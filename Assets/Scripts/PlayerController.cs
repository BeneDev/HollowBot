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
            if(value >= exp)
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

    public int Damage
    {
        get
        {
            return damage;
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

    public event System.Action<float> OnAttack;

    #endregion

    // The attributes of the player
    #region Stats and Attributes

    [Header("Stats"), SerializeField] int maxHealth = 100;
    private int health = 100;

    [SerializeField] int maxHealthJuice = 100;
    private int healthJuice = 100;

    [SerializeField] int baseDamage = 5;
    [SerializeField] int damagePerLevelUp = 3;
    private int damage = 5;

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
    Camera cam;

    LayerMask enemiesMask;

    // Fields to manipulate the jump
    [Header("Jump & Physics"), SerializeField] float jumpPower = 10;
    float appliedJumpPower;
    [SerializeField] float jumpDuration = 0.5f;
    float lastTimeJumped;
    [SerializeField] float fallMultiplier = 2f; // The higher this value, the slower the player will fall after jumping up, when still holding jump and the faster he will fall when not holding it

    // Fields to manipulate the knockback Applied to the player
    [Header("Knockback"), SerializeField] float knockBackCapY = 2f; // the highest velocity the player can be vertically knocked back
    Vector3 knockBackForce;
    [SerializeField] float knockBackDuration = 0.05f; // The amount of seconds, the player will be knocked back
    [SerializeField] int framesFreezedAfterHit = 8; // The amount of frames the player will be forced to stand still when he is being hit

    // Fields to manipulate the Dodge
    [Header("Dodge"), SerializeField] float dodgePower = 100f; // Force forward when dodging
    [SerializeField] float dodgeDuration = 1f;
    float timeWhenDodgeStarted;
    [SerializeField] float dodgeCooldown = 1f;

    // Fields to manipulate the attack
    [Header("Attack"), SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackDuration = 0.5f;
    float timeWhenAttackStarted;
    [SerializeField] float upwardsVeloAfterHitDown = 0.06f; // The velocity with which the player gets pushed upwards after hitting an enemy under him with a successful attack
    [SerializeField] float upwardsVeloAfterHitDownTime = 0.008f; // The duration the player gets pushed upwards after hitting an enemy under him with a successful attack
    [SerializeField] GameObject arm;
    [SerializeField] Sprite grabbingArm;

    // Fields to manipulate the healing
    [Header("Healing"), SerializeField] int healDuration = 5; // The frames one has to wait in between one transfer of Health juice to health
    private int healCounter = 0; // The actual counter for the heal duration
    [SerializeField] int juiceRegenValue = 10; // The amount of Juice restored when collecting a juice particle
    [SerializeField] float zoomAmountWhenHealing = 0.002f;

    [Header("General"), SerializeField] float invincibilityTime = 1f; // The amount of seconds, the player is invincible after getting hit
    private float invincibilityCounter = 0f; // This counts down until player can be hit again. Only if this value is 0, the player can be hit.

    [Header("Weapons"), SerializeField] Vector2 itemCheckReach;
    Collider2D[] objectsInReach;
    [SerializeField] Vector3 posForEquippedWeapons;
    [SerializeField] Vector3 rotForEquippedWeapons;
    GameObject weapon;

    // Walking speed of the Player
    [SerializeField] float speed = 1;

    [SerializeField] float wallSlideSpeed = 3f; // How fast the player slides down a wall while holding towards it

    #endregion

    // Use this for initialization
    protected override void Awake ()
    {
        base.Awake();
        input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        cam = Camera.main;
        InitializeAttributes();

        // Create LayerMask
        int enemiesLayer = LayerMask.NameToLayer("Enemies");
        enemiesMask = 1 << enemiesLayer;
    }

    private void Update()
    {
        objectsInReach = Physics2D.OverlapBoxAll(transform.position, itemCheckReach, 0f);
        if(playerState == State.freeToMove && input.Vertical > 0.5f && objectsInReach.Length > 0)
        {
            foreach(Collider2D coll in objectsInReach)
            {
                if(coll.gameObject.GetComponent<WeaponController>())
                {
                    coll.gameObject.GetComponent<WeaponController>().Equip();
                }
            }
        }
    }

    // Update is called once per frame
    protected override void FixedUpdate ()
    {
        UpdateRaycasts();
        CheckGrounded();
        CheckForInput();
		if(playerState == State.freeToMove)
        {
            // Setting the x velocity when player is not knocked back
            velocity = new Vector3(input.Horizontal * speed * Time.fixedDeltaTime, velocity.y);
            if (input.Jump == 1 && bGrounded || input.Jump == 1 && bOnWall)
            {
                lastTimeJumped = Time.realtimeSinceStartup;
                appliedJumpPower = jumpPower;
            }
            if(Time.realtimeSinceStartup < lastTimeJumped + jumpDuration && input.Jump == 2)
            {
                velocity.y += appliedJumpPower * Time.fixedDeltaTime;
                appliedJumpPower *= lastTimeJumped + jumpDuration - Time.realtimeSinceStartup;
            }
            // Make the player fall faster when not holding the jump button
            if (input.Jump == 0 && !bGrounded)
            {
                velocity.y -= fallMultiplier * Time.fixedDeltaTime;
            }
            if (input.Attack && Time.realtimeSinceStartup >= timeWhenAttackStarted + attackDuration + attackCooldown)
            {
                Attack();
            }
            if (input.Dodge && Time.realtimeSinceStartup >= timeWhenDodgeStarted + dodgeDuration + dodgeCooldown)
            {
                playerState = State.dodging;
                timeWhenDodgeStarted = Time.realtimeSinceStartup;
                anim.SetBool("Dodging", true);
            }
            // Checks for input for healing
            if (input.Heal && HealthJuice > 0 && Health < maxHealth)
            {
                playerState = State.healing;
            }
        }
        else if(playerState == State.attacking)
        {
            // Setting the x velocity when player is not knocked back
            velocity = new Vector3(input.Horizontal * speed * Time.fixedDeltaTime, velocity.y);
            if(Time.realtimeSinceStartup > timeWhenAttackStarted + attackDuration)
            {
                playerState = State.freeToMove;
            }
        }
        else if(playerState == State.knockedBack)
        {
            // Apply knockback when the player is currently getting knocked back
            if (knockBackForce.y > knockBackCapY)
            {
                knockBackForce.y = knockBackCapY;
            }
            if (knockBackForce.y < 0)
            {
                knockBackForce.y = 0;
            }
            // Check if knockback would let player end up in wall, if not apply it
            RaycastHit2D knockBackRay = Physics2D.Raycast(transform.position, knockBackForce, knockBackForce.magnitude, groundMask);
            if (!knockBackRay)
            {
                velocity += knockBackForce;
            }
            else
            {
                velocity += knockBackForce * knockBackRay.distance;
                // Get Knocked back onto the wall
                //while (!Physics2D.Raycast(transform.position, knockBackForce, knockBackForce.magnitude / 10, groundMask))
                //{
                //    velocity += knockBackForce / 10;
                //}
            }
        }
        else if(playerState == State.dodging)
        {
            if (Time.realtimeSinceStartup < timeWhenDodgeStarted + dodgeDuration)
            {
                velocity += new Vector3(dodgePower * transform.localScale.x * speed * Time.fixedDeltaTime, 0f);
                velocity.y = 0f;
            }
            else if(Time.realtimeSinceStartup > timeWhenDodgeStarted + dodgeDuration)
            {
                playerState = State.freeToMove;
                anim.SetBool("Dodging", false);
            }
        }
        else if(playerState == State.healing)
        {
            if (input.Heal)
            {
                velocity = new Vector3(0f, velocity.y);
                if (HealthJuice > 0 && Health < maxHealth)
                {
                    Heal();
                }
            }
            else
            {
                playerState = State.freeToMove;
            }
        }
        // Apply gravity
        if (!bGrounded)
        {
            velocity += new Vector3(0, -gravity * Time.fixedDeltaTime);
        }
        CheckForValidVelocity();
        if(velocity.x >= 0)
        {
            anim.SetFloat("XVelocity", velocity.x);
        }
        else
        {
            anim.SetFloat("XVelocity", -velocity.x);
        }
        anim.SetFloat("YVelocity", velocity.y);
        if (bGrounded && input.Horizontal == 0f)
        {
            anim.SetBool("Idle", true);
        }
        else
        {
            anim.SetBool("Idle", false);
        }
        transform.position += velocity;
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

    public void Equip(GameObject weapon)
    {
        if(arm)
        {
            this.weapon = weapon;
            weapon.transform.parent = arm.transform;
            weapon.transform.localPosition = posForEquippedWeapons;
            weapon.transform.localRotation = Quaternion.Euler(rotForEquippedWeapons);
            weapon.transform.localScale = Vector3.one;
            weapon.GetComponent<WeaponController>().OnSomethingHit += OnSomethingHit;
            arm.GetComponent<SpriteRenderer>().sprite = grabbingArm;
        }
    }

    /// <summary>
    /// This gets called, when the equipped weapon hits something
    /// </summary>
    void OnSomethingHit()
    {

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
        damage += damagePerLevelUp;
    }

    private void InitializeAttributes()
    {
        // Make the player have full health
        Health = maxHealth;

        // Make the player have full health Juice
        HealthJuice = maxHealthJuice;

        // Make the player have the base attack value at start
        damage = baseDamage;

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
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (input.Horizontal > 0)
        {
            transform.localScale = Vector3.one;
        }
    }

    #endregion

    #region Attack

    void Attack()
    {
        if (OnAttack != null)
        {
            OnAttack(attackDuration);
        }
        if (input.Vertical > 0.5f)
        {
            anim.SetTrigger("AttackUp");
        }
        else if (input.Vertical < -0.5f)
        {
            anim.SetTrigger("AttackDown");
        }
        else
        {
            anim.SetTrigger("Attack");
        }
        playerState = State.attacking;
        timeWhenAttackStarted = Time.realtimeSinceStartup;
    }

    IEnumerator ExtraUpVeloAfterHitDown(float duration)
    {
        for (float t = 0; t < duration; t += Time.fixedDeltaTime)
        {
            velocity.y += upwardsVeloAfterHitDown * Time.fixedDeltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    #endregion

    #region Healing

    /// <summary>
    /// If the Health is not up to its maximum, the player takes one health juice and heals himself, gaining one health point. The healCounter prevents the healing from taking place too fast
    /// </summary>
    private void Heal()
    {
        if (Health < maxHealth)
        {
            if (healCounter < healDuration)
            {
                healCounter++;
            }
            else
            {
                healCounter = 0;
                HealthJuice--;
                Health++;
                cam.orthographicSize -= zoomAmountWhenHealing;
            }
        }
    }

    #endregion

    #region Physics

    protected override void CheckGrounded()
    {
        base.CheckGrounded();
        anim.SetBool("Grounded", bGrounded);
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
