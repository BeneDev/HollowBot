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

    [SerializeField] ParticleSystem attackParticle;
    ParticleSystem.EmissionModule attackEmission;
    ParticleSystem.MinMaxCurve standardEmissionRate;

    bool equipped = false;
    bool isAttacking = false;

    bool isFlying = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        attackEmission = attackParticle.emission;
        standardEmissionRate = attackEmission.rateOverDistance;
        attackEmission.rateOverDistance = 0f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(equipped && isAttacking)
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
                }
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
            }
            else
            {
                isFlying = false;
            }
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
    }

    void OnThrown(float YVelocity)
    {
        rb.isKinematic = false;
        transform.parent = null;
        owner.OnAttack -= OnAttack;
        owner.OnWeaponThrown -= OnThrown;
        Vector2 Force = -((Vector2)player.transform.position - (Vector2)transform.position).normalized * thrownForce;
        Force.y *= YVelocity;
        rb.velocity += Force;
        isFlying = true;
    }

    void OnAttack(float duration)
    {
        attackEmission.rateOverDistance = standardEmissionRate;
        isAttacking = true;
        StartCoroutine(EndAttackAferSeconds(duration));
    }

    IEnumerator EndAttackAferSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isAttacking = false;
        attackEmission.rateOverDistance = 0f;
    }
}
