using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The script, controlling the camera arm, holding the camera with a certain offset and following the player
/// </summary>
public class CameraController : MonoBehaviour {

    GameObject player;
    [SerializeField] float lerpSpeed = 1f;
    
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	void Update () {
        if(!player) { return; }
        // Follow the player. Due to the fact that the camera is childed to this Camera Arm with an offset, the camera follows the player in the right view angle
        transform.position = Vector3.Lerp(transform.position, player.transform.position, lerpSpeed);
	}
}
