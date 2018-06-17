using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

    public event System.Action OnSomethingHit;

    PlayerController owner;

    GameObject player;

    [SerializeField] int damage;
    [SerializeField] float knockBackStrength;

    bool equipped = false;
    bool isAttacking = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        print("im here");
        if(equipped && isAttacking)
        {
            print("attacking");
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
        else
        {
            if(collision.gameObject.Equals(player))
            {
                // TODO Show stats of weapon and how to equip it overlay menu
            }
        }
    }

    public void Equip()
    {
        owner = player.GetComponent<PlayerController>();
        owner.Equip(gameObject);
        owner.OnAttack += OnAttack;
    }

    void OnAttack(float duration)
    {
        isAttacking = true;
        StartCoroutine(EndAttackAferSeconds(duration));
    }

    IEnumerator EndAttackAferSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isAttacking = false;
    }
}
