using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

    [SerializeField] PlayerController owner;

    [SerializeField] int damage;
    [SerializeField] float knockBackStrength;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<BaseEnemy>())
        {
            if(owner)
            {
                Vector3 knockBackDirection = collision.transform.position - owner.gameObject.transform.position;
                knockBackDirection.y = 0f;
                collision.gameObject.GetComponent<BaseEnemy>().TakeDamage(owner.Damage + damage, knockBackDirection.normalized * knockBackStrength);
            }
        }
    }
}
