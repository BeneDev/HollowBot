using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Singleton Script, handling the general game related tasks
/// </summary>
public class GameManager : Singleton<GameManager> {

    public Vector3 currentCheckpoint; // Stores the Checkpoint which is currently activated, to send the player there if he dies
    
    [SerializeField] int weaponBreakParticleCount = 100;
    [SerializeField] GameObject weaponBreakParticlePrefab;
    [SerializeField] GameObject weaponBreakParticleParent;
    Stack<GameObject> freeWeaponBreakParticles = new Stack<GameObject>();

    // Make the GameManger Instance a Singleton 
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        // Set the first checkpoint to the starting point of the player
        currentCheckpoint = Vector3.zero;

        // Instantiate the Weapon Break Particles
        // Instantiate a defined number of balls into the free cannon balls stack (Preparation for object pooling)
        for (int i = 0; i < weaponBreakParticleCount; i++)
        {
            GameObject particle = Instantiate(weaponBreakParticlePrefab, Vector3.zero, transform.rotation);
            particle.SetActive(false);
            particle.transform.SetParent(weaponBreakParticleParent.transform);
            freeWeaponBreakParticles.Push(particle);
        }
    }

    // Return a free to use cannonball to shoot 
    public void BreakWeapon(Vector3 positionToSpawn)
    {
        GameObject particle = freeWeaponBreakParticles.Pop();
        particle.transform.position = positionToSpawn;
        particle.SetActive(true);
        particle.GetComponent<ParticleSystem>().Play();
        StartCoroutine(GetParticleBackAfterSeconds(particle.GetComponent<ParticleSystem>().main.duration, particle));
    }

    IEnumerator GetParticleBackAfterSeconds(float seconds, GameObject particle)
    {
        yield return new WaitForSeconds(seconds);
        freeWeaponBreakParticles.Push(particle);
        particle.SetActive(false);
    }
}
