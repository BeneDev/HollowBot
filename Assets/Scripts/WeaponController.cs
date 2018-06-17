using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

    public event System.Action OnSomethingHit;

    PlayerController owner;

    GameObject player;

    Rigidbody2D rb;

    [SerializeField] int damage;
    [SerializeField] int thrownDamageMultiplier = 4;
    [SerializeField] float knockBackStrength;
    [SerializeField] int thrownKnockBackMultiplier = 2;
    [SerializeField] float thrownForce = 3f;
    [SerializeField] int hitsUntilBreak = 5;

    [SerializeField] ParticleSystem attackParticle;
    ParticleSystem.EmissionModule attackEmission;
    ParticleSystem.MinMaxCurve standardEmissionRate;

    BoxCollider2D coll;

    bool equipped = false;

    bool isFlying = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        attackEmission = attackParticle.emission;
        standardEmissionRate = attackEmission.rateOverDistance;
        attackEmission.rateOverDistance = 0f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(equipped)
        {
            if(OnSomethingHit != null)
            {
                OnSomethingHit();
            }
            if (collision.gameObject.GetComponent<BaseEnemy>())
            {
                if (owner)
                {
                    Vector3 knockBackDirection = collision.transform.position - owner.gameObject.transform.position;
                    knockBackDirection.y = 0f;
                    collision.gameObject.GetComponent<BaseEnemy>().TakeDamage(owner.Damage + damage, knockBackDirection.normalized * knockBackStrength);
                    hitsUntilBreak--;
                }
            }
            if (hitsUntilBreak <= 0)
            {
                GameManager.Instance.BreakWeapon(transform.position);
                owner.OnAttack -= OnAttack;
                owner.OnWeaponThrown -= OnThrown;
                Destroy(gameObject);
            }
        }
        else if(!isFlying)
        {
            if(collision.gameObject.Equals(player))
            {
                // TODO Show stats of weapon and how to equip it overlay menu
            }
        }
        else if(isFlying)
        {
            if (collision.gameObject.GetComponent<BaseEnemy>())
            {
                Vector3 knockBackDirection = collision.transform.position - transform.position;
                knockBackDirection.y = 0f;
                collision.gameObject.GetComponent<BaseEnemy>().TakeDamage(damage * thrownDamageMultiplier, knockBackDirection.normalized * (knockBackStrength * thrownKnockBackMultiplier));
                hitsUntilBreak -= 2;
            }
            if (hitsUntilBreak <= 0)
            {
                GameManager.Instance.BreakWeapon(transform.position);
                owner.OnAttack -= OnAttack;
                owner.OnWeaponThrown -= OnThrown;
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(isFlying)
        {
            isFlying = false;
        }
    }

    public void Equip()
    {
        owner = player.GetComponent<PlayerController>();
        owner.Equip(gameObject);
        equipped = true;
        rb.isKinematic = true;
        rb.angularVelocity = 0f;
        rb.velocity = Vector2.zero;
        owner.OnAttack += OnAttack;
        owner.OnWeaponThrown += OnThrown;
        coll.enabled = false;
    }

    void OnThrown(float YVelocity, float direction)
    {
        rb.isKinematic = false;
        equipped = false;
        coll.enabled = true;
        transform.parent = null;
        owner.OnAttack -= OnAttack;
        owner.OnWeaponThrown -= OnThrown;
        Vector2 force;
        if (YVelocity > 0.5f)
        {
            force = Vector2.up * thrownForce;
        }
        else if(YVelocity < -0.5f)
        {
            force = Vector2.down * thrownForce;
        }
        else
        {
            force = new Vector2(direction * thrownForce, YVelocity);
        }
        rb.velocity += force;
        isFlying = true;
    }

    void OnAttack(float duration)
    {
        attackEmission.rateOverDistance = standardEmissionRate;
        coll.enabled = true;
        StartCoroutine(EndAttackAferSeconds(duration));
    }

    IEnumerator EndAttackAferSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if(equipped)
        {
            coll.enabled = false;
        }
        attackEmission.rateOverDistance = 0f;
    }
}
