using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This Script controls the general Panel for showing the gameplay UI
/// </summary>
public class GameplayOverlayController : MonoBehaviour {

    #region Fields
    [SerializeField] Text healthText;
    [SerializeField] Text healthJuiceText;
    [SerializeField] Text expText;
    [SerializeField] Text levelText;

    private GameObject player;
    #endregion

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Sign up for the players delegates
        //player.GetComponent<PlayerController>().OnHealthChanged += UpdateHealthText;
        //player.GetComponent<PlayerController>().OnHealthJuiceChanged += UpdateHealthJuiceText;
        //player.GetComponent<PlayerController>().OnExpChanged += UpdateExpText;
        //player.GetComponent<PlayerController>().OnLevelChanged += UpdateLevelText;
    }

    /// <summary>
    /// Updates the health count overlay
    /// </summary>
    /// <param name="newHealth"></param>
    private void UpdateHealthText(int newHealth)
    {
        healthText.text = "Health: " + newHealth;
    }

    /// <summary>
    /// Updates the health juice count overlay
    /// </summary>
    /// <param name="newHealthJuice"></param>
    private void UpdateHealthJuiceText(int newHealthJuice)
    {
        healthJuiceText.text = "Health Juice: " + newHealthJuice;
    }

    private void UpdateExpText(int exp, int expToLevel)
    {
        expText.text = "Exp: " + exp + "/" + expToLevel;
    }

    private void UpdateLevelText(int level)
    {
        levelText.text = "Level: " + level;
    }
}
