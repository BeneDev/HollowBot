using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Script, returning the input, used to the CharController Script, which converts the given information into actions the player can do
/// </summary>
public class PlayerInput : MonoBehaviour, IInput {

    #region Axis

    // The input for horizontal movement
    public float Horizontal
    {
        get
        {
            return Input.GetAxis("Horizontal");
        }
    }

    // The input for vertical movement
    public float Vertical
    {
        get
        {
            return Input.GetAxis("Vertical");
            return 0f;
        }
    }

    #endregion

    #region Actions

    // Check if jump button is pressed or holded
    public int Jump
    {
        get
        {
            if (Input.GetButtonDown("Jump"))
            {
                return 1;
            }
            else if(Input.GetButton("Jump"))
            {
                return 2;
            }
            return 0;
        }
    }

    // Looks for Input for Dodge
    public bool Dodge
    {
        get
        {
            if(Input.GetButtonDown("Dodge"))
            {
                return true;
            }
            return false;
        }
    }

    // Looks for Input for Attack
    public bool Attack
    {
        get
        {
            if(Input.GetButtonDown("Attack"))
            {
                return true;
            }
            return false;
        }
    }

    // Looks for Input for Picking up
    public bool Pickup
    {
        get
        {
            if (Input.GetButtonDown("Pickup"))
            {
                return true;
            }
            return false;
        }
    }

    // Looks for Input for Throwing
    public bool Throw
    {
        get
        {
            if (Input.GetButtonDown("Throw"))
            {
                return true;
            }
            return false;
        }
    }

    public bool Heal
    {
        get
        {
            if(Input.GetButton("Heal"))
            {
                return true;
            }
            return false;
        }
    }

    #endregion
}
