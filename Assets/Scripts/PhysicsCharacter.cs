using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCharacter : MonoBehaviour {

    protected struct PlayerRaycasts // To store the informations of raycasts around the player to calculate physics
    {
        public RaycastHit2D bottomLeft;
        public RaycastHit2D bottomRight;
        public RaycastHit2D upperLeft;
        public RaycastHit2D lowerLeft;
        public RaycastHit2D upperRight;
        public RaycastHit2D lowerRight;
        public RaycastHit2D top;
    }
    protected PlayerRaycasts raycasts; // Stores the actual information of the raycasts to calculate physics

    protected LayerMask groundMask;

    [SerializeField] protected float gravity;

    protected Vector3 velocity;
    [SerializeField] protected float veloYLimit = 1f;

    [SerializeField] protected float drag = 1f;

    protected bool bGrounded = false; // Stores if the player is on the ground or not
    protected bool bOnWall = false; // Stores if the player is on a wall or not

    #region Unity Messages

    protected virtual void Awake()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        groundMask = 1 << groundLayer;
    }

    protected virtual void FixedUpdate()
    {
        UpdateRaycasts();
        CheckGrounded();
        // Apply gravity
        if (!bGrounded)
        {
            velocity += new Vector3(0, -gravity * Time.fixedDeltaTime);
        }
        CheckForValidVelocity();
        // Apply the velocity to the transform
        transform.position += velocity;
    }

    #endregion

    #region HelperMethods

    /// <summary>
    /// Make sure the velocity does not violate the laws of physics in this game
    /// </summary>
    protected void CheckForValidVelocity()
    {
        // Check for ground under the player
        if (bGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        // Checking for colliders to the sides
        if (WallInWay())
        {
            velocity.x = 0f;
        }

        // Make sure, velocity in y axis does not get over limit
        if(velocity.y >= 0 && velocity.y > veloYLimit)
        {
            velocity.y = veloYLimit;
        }
        if (velocity.y <= 0 && velocity.y < -veloYLimit)
        {
            velocity.y = -veloYLimit;
        }

        // Check if something is above the player and let him bounce down again relative to the force he went up with
        if (raycasts.top && velocity.y > 0)
        {
            velocity.y = -velocity.y / 2;
        }

        // Apply drag to velocity
        velocity = velocity * (1 - Time.fixedDeltaTime * drag);
    }

    /// <summary>
    /// Checks if there are walls in the direction the player is facing
    /// </summary>
    /// <returns> True if there is a wall. False when there is none</returns>
    protected bool WallInWay()
    {
        if (transform.localScale.x < 0)
        {
            if (raycasts.upperLeft || raycasts.lowerLeft)
            {
                bOnWall = true;
                if (raycasts.upperLeft.distance < 0.45f)
                {
                    transform.position += Vector3.right * ((0.45f - (raycasts.upperLeft.distance)) / 5f);
                }
                else if (raycasts.lowerLeft.distance < 0.45f)
                {
                    transform.position += Vector3.right * ((0.45f - (raycasts.lowerLeft.distance)) / 5f);
                }
                return true;
            }
        }
        else if (transform.localScale.x > 0)
        {
            if (raycasts.upperRight || raycasts.lowerRight)
            {
                bOnWall = true;
                if (raycasts.upperRight.distance < 0.2f)
                {
                    transform.position += Vector3.left * ((0.25f - (raycasts.upperRight.distance)) / 5f);
                }
                else if (raycasts.lowerRight.distance < 0.2f)
                {
                    transform.position += Vector3.left * ((0.25f - (raycasts.lowerRight.distance)) / 5f);
                }
                return true;
            }
        }
        bOnWall = false;
        return false;
    }

    /// <summary>
    /// Checks if the player is on the ground or not
    /// </summary>
    protected virtual void CheckGrounded()
    {
        // When the bottom raycasts hit ground
        if (raycasts.bottomLeft || raycasts.bottomRight)
        {
            bGrounded = true;
            velocity.y = 0f;
            if(raycasts.bottomLeft.distance < 0.7f)
            {
                transform.position += Vector3.up * ((0.75f - (raycasts.bottomLeft.distance)) / 5f);
            }
            else if (raycasts.bottomRight.distance < 0.7f)
            {
                transform.position += Vector3.up * ((0.75f - (raycasts.bottomRight.distance)) / 5f);
            }
        }
        // Otherwise the player is not grounded
        else
        {
            bGrounded = false;
        }
    }

    /// <summary>
    /// Check every raycast from the raycasts struct and return the first one, which found an object which matched the tag 
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="rayArray"></param>
    /// <returns> The first raycast who hit an object with the right tag</returns>
    protected RaycastHit2D? WhichRaycastForTag(string tag, params RaycastHit2D[] rayArray)
    {
        for (int i = 0; i < rayArray.Length; i++)
        {
            if (rayArray[i].collider != null)
            {
                if (rayArray[i].collider.tag == tag)
                {
                    return rayArray[i];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Update all the different raycast hit values to calculate physics
    /// </summary>
    protected virtual void UpdateRaycasts()
    {
        raycasts.bottomRight = Physics2D.Raycast(transform.position + Vector3.right * 0.1f + Vector3.down * 0.4f, Vector2.down, 0.75f, groundMask);
        raycasts.bottomLeft = Physics2D.Raycast(transform.position + Vector3.right * -0.2f + Vector3.down * 0.4f, Vector2.down, 0.75f, groundMask);

        raycasts.upperRight = Physics2D.Raycast(transform.position + Vector3.up * 0.75f + Vector3.right * 0.4f, Vector2.right, 0.25f, groundMask);
        raycasts.lowerRight = Physics2D.Raycast(transform.position + Vector3.up * -0.4f + Vector3.right * 0.4f, Vector2.right, 0.25f, groundMask);

        raycasts.upperLeft = Physics2D.Raycast(transform.position + Vector3.up * 0.75f + Vector3.right * -0.4f, Vector2.left, 0.25f, groundMask);
        raycasts.lowerLeft = Physics2D.Raycast(transform.position + Vector3.up * -0.4f + Vector3.right * -0.4f, Vector2.left, 0.25f, groundMask);

        raycasts.top = Physics2D.Raycast(transform.position + Vector3.up * 0.75f, Vector2.up, 0.25f, groundMask);
    }

    //private void OnDrawGizmos()
    //{
    //    Debug.DrawRay(transform.position + Vector3.right * 0.1f + Vector3.down * 0.4f, Vector2.down * 0.75f);
    //    Debug.DrawRay(transform.position + Vector3.right * -0.2f + Vector3.down * 0.4f, Vector2.down * 0.75f);
    //}

    #endregion

}
