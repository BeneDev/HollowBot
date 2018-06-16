using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Script of which all enemies inherit, providing them with general methods and attributes, necessary for an enemy to work properly
/// </summary>
public class BaseEnemy : MonoBehaviour{

    #region Properties

    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            if(damageNumber)
            {
                DamageNumberController number = Instantiate(damageNumber, transform.position, transform.rotation);
                number.Init((health - value), gameObject);
            }
            health = value;
        }
    }

    #endregion

    #region Fields

    // Enemy Attributes
    [SerializeField] protected int health = 20;
    [SerializeField] protected int attack = 2;
    [SerializeField] protected int defense = 2;
    [SerializeField] protected int expToGive = 3;

    // The amount of particles getting instantiated when the enemy dies
    [SerializeField] int particleCountAtDeath = 1;
    // The particle getting instantiated when the enemy dies
    [SerializeField] GameObject juiceParticle;
    // The offset a Juice Particle can have 
    [SerializeField] float spawnOffset = 0.3f;

    // The canvas used to show damage numbers
    [SerializeField] DamageNumberController damageNumber;

    // The heaviness of the camera shake
    [SerializeField] float cameraShakeAmount = 0.02f;
    // The amount of freeze frames, taking place when the enemy is hit
    [SerializeField] int amountFreezeFrames = 8;

    // The amount of seconds, the sprite is shown planely in white
    [SerializeField] float flashDuration = 0.2f;
    // The amount of seconds the knockback force will be applied
    [SerializeField] float knockedBackDuration = 0.2f;
    // The amount of seconds the enemy will be stunned after the knockback
    protected float stunDuration;
    // The force the enemy gets knocked back
    protected Vector3 knockBackForce;

    protected bool bAlreadyDead = false;

    // Variables to find the player
    protected GameObject player;
    protected Vector3 toPlayer;

    protected Rigidbody2D rb;
    Camera cam;

    protected float knockBackCounter = 0f;
    protected float stunnedCounter = 0f;

    [SerializeField] float hitRange = 2f;
    [SerializeField] float knockBackStrength = 3f;

    // The layer mask used to collide with only walls
    protected LayerMask layersToCollideWith;
    protected Collider2D coll;

    private SpriteRenderer rend;
    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;

    #endregion

    /// <summary>
    /// The general things an enemy should do in his start or awake method
    /// </summary>
    protected virtual void GeneralInitialization()
    {
        // Find the renderer, the gui shader and the default sprite shader
        rend = gameObject.GetComponent<SpriteRenderer>();
        shaderGUItext = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Diffuse");

        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();

        cam = Camera.main;

        // Get the layerMask for collision
        int layer = LayerMask.NameToLayer("Ground");
        layersToCollideWith = 1 << layer;

        coll = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// The general things an enemy should do in his update method
    /// </summary>
    protected virtual void GeneralBehavior()
    {
        // Store the vector towards the player
        toPlayer = player.transform.position - transform.position;

        // Attacks the player if he within reach
        if (toPlayer.magnitude <= hitRange)
        {
            player.GetComponent<PlayerController>().TakeDamage(attack, CalculateKnockback());
        }
        //if (Health <= 0)
        //{
        //    Die();
        //}
    }

    #region Helper Functions

    /// <summary>
    /// This makes the enemy disappear and spawn juiceParticles for the player to fill up his Health Juice again
    /// </summary>
    protected virtual void Die()
    {
        bAlreadyDead = true;
        // Make time freeze for more frames
        StartCoroutine(StopTimeForFrames(amountFreezeFrames + 10));
        // Make the camera shake more
        cam.GetComponent<CameraShake>().shakeDuration = cameraShakeAmount * 10f;
        if (juiceParticle)
        {
            for (int i = 0; i < particleCountAtDeath; i++)
            {
                Instantiate(juiceParticle, transform.position, transform.rotation);
            }
        }
        player.GetComponent<PlayerController>().Exp += expToGive;
    }

    /// <summary>
    /// Subtract damage and apply knockback to the enemy
    /// </summary>
    /// <param name="damageToTake"></param>
    /// <param name="knockback"></param>
    public virtual void TakeDamage(int damageToTake, Vector3 knockback)
    {
        knockBackForce = knockback;
        // When the damage is greater than defense, do that remaining damage
        if (damageToTake - defense > 0)
        {
            Health -= damageToTake - defense;
        }
        // Otherwise deal just one damage
        else
        {
            Health--;
        }
        // Let the enemy sprite flash up white
        rend.material.shader = shaderGUItext;
        rend.color = Color.white;
        StartCoroutine(SetBackToDefaultShader(flashDuration));
        knockBackCounter = knockedBackDuration;
        if(health > 0)
        {
            // Make time freeze for some frames
            StartCoroutine(StopTimeForFrames(amountFreezeFrames));
            // Make the camera shake
            cam.GetComponent<CameraShake>().shakeDuration = cameraShakeAmount;
        }
    }

    protected void ApplyKnockBack()
    {
        if (!Physics2D.Raycast(transform.position, new Vector2(knockBackForce.x, 0f), knockBackForce.magnitude, layersToCollideWith))
        {
            transform.position += new Vector3(knockBackForce.x + knockBackForce.y, 0f);
            knockBackForce = new Vector3(knockBackForce.x * 0.9f, knockBackForce.y * 0.8f);
            if (Health > 0)
            {
                stunnedCounter = knockedBackDuration * 8;
            }
        }
        else
        {
            Health -= defense;
            knockBackCounter = 0f;
            if (Health > 0)
            {
                stunnedCounter = knockedBackDuration * 20;
            }
        }
    }

    /// <summary>
    /// Stop the time and make it run again after n frames
    /// </summary>
    /// <param name="frameAmount"></param>
    /// <returns></returns>
    IEnumerator StopTimeForFrames(int frameAmount)
    {
        Time.timeScale = 0f;
        for (int i = 0; i < frameAmount; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Make the Sprite show the normal colors again after a set amount of time
    /// </summary>
    /// <param name="sec"></param>
    /// <returns></returns>
    IEnumerator SetBackToDefaultShader(float sec)
    {
        yield return new WaitForSeconds(sec);
        rend.material.shader = shaderSpritesDefault;
        rend.color = Color.white;
    }

    /// <summary>
    /// Calculated the force which has to be given into the Take Damage function of the player, to cause the knockback for the player
    /// </summary>
    /// <returns></returns>
    protected Vector2 CalculateKnockback()
    {
        // Sets normal knockback
        Vector2 knockBack = toPlayer.normalized * knockBackStrength;
        // When the player is too close on the x-axis(most of the times because the player is right on top of the enemy) make the knockback stronger
        if(GetOnlyValue(toPlayer.x) <= 0.065f)
        {
            if(toPlayer.x > 0)
            {
                toPlayer.x = 0.1f;
            }
            else
            {
                toPlayer.x = -0.1f;
            }
        }
        return knockBack;
    }

    /// <summary>
    /// Returns only the value of a number given in. This gets rid of the algebraic sign before a number
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    protected float GetOnlyValue(float number)
    {
        if(number > 0)
        {
            return number;
        }
        else if(number < 0)
        {
            return -number;
        }
        else
        {
            return 0;
        }
    }

    #endregion

}
