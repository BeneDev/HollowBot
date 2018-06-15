using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Script controls the Juice particles, spawning after an enemy is killed. It makes them fly up and float after reaching the highest point
/// </summary>
public class JuiceParticleController : MonoBehaviour {

    [SerializeField] LayerMask layersToCollide;
    [SerializeField] float upwardsVelocity = 0.003f; // The veloctiy of flying up
    private float actualUpwardsVeloctiy = 0f;
    [SerializeField] float maxHorizontalVelocity = 0.03f; // The max amount of drifting to the sides when flying up
    [SerializeField] float maxSecondsFlyingUp = 2f; // The max amount of seconds flying up before floating

    [SerializeField] float floatingVeloctiy = 0.001f; // How fast the particle travels when floating

    private Vector3 ankerPoint;
    private float actualHorizontalVelocity = 0f;
    private float actualSecondsFlyingUp = 0f;

	// Use this for initialization
	void Awake () {
        actualUpwardsVeloctiy = upwardsVelocity;
        actualHorizontalVelocity = Random.Range(-maxHorizontalVelocity, maxHorizontalVelocity);
        actualSecondsFlyingUp = Random.Range(0.5f, maxSecondsFlyingUp);
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 direction = new Vector2(actualHorizontalVelocity, actualUpwardsVeloctiy);
        if (!Physics2D.Raycast(transform.position, direction, direction.magnitude, layersToCollide))
        {
            transform.position += (Vector3)direction;
            if (actualSecondsFlyingUp > 0)
            {
                actualSecondsFlyingUp -= Time.deltaTime;
            }
        }
        // When the particle reached a collider, go immediately into floating mode
        else
        {
            actualSecondsFlyingUp = 0f;
        }
        // When flying up time is over, go into floating mode
        if(actualSecondsFlyingUp <= 0f)
        {
            if(ankerPoint == Vector3.zero)
            {
                ankerPoint = transform.position;
                actualHorizontalVelocity = 0f;
            }
            Floating();
        }
    }

    /// <summary>
    /// Make the Particle fly up and down to simulate some kind of floating in the air
    /// </summary>
    private void Floating() // TODO make this floating effect more realistic (maybe use lerp)
    {
        // When the particle is above the anker point build up negative velocity
        if (transform.position.y >= ankerPoint.y + floatingVeloctiy)
        {
            if (actualUpwardsVeloctiy > -floatingVeloctiy/20)
            {
                actualUpwardsVeloctiy -= floatingVeloctiy * Time.deltaTime;
            }
        }
        // When the particle is under the anker point build up positive velocity
        else if(transform.position.y <= ankerPoint.y - floatingVeloctiy)
        {
            if (actualUpwardsVeloctiy < floatingVeloctiy/20)
            {
                actualUpwardsVeloctiy += floatingVeloctiy * Time.deltaTime;
            }
        }
        transform.position += new Vector3(0f, actualUpwardsVeloctiy);
    }
}
