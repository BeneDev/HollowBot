using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The Script, controlling the canvas which holds the text, showing the damage number over the enemies
/// </summary>
public class DamageNumberController : MonoBehaviour {

    #region Fields
    [SerializeField] Text damageNumberText;

    [SerializeField] float upwardsSpeed = 0.1f; // How fast the text flys up
    [SerializeField] float secondsToLast = 1f; // How long the text lasts on screen

    private float offsetX = 0f;
    [SerializeField] float horizontalSpeed = 0.1f; // This sets the Range of the random calculation for the offset grouwth for the x-axis
    private float offsetXGrouwth = 0;

    private GameObject objectToFollow; // The object to follow regarding the x-axis

    [SerializeField] float scalingMultiplier = 0.003f; // The Multiplier with which the text will be scaled up depending on the damage value
    #endregion

    /// <summary>
    /// Setting the object to follow on the x-axis and the number to show
    /// </summary>
    /// <param name="number"></param>
    /// <param name="obj"></param>
    public void Init(int number, GameObject obj)
    {
        damageNumberText.text = number.ToString();
        // Make the text dependent on the damage value
        transform.localScale = new Vector3(transform.localScale.x + number * scalingMultiplier, transform.localScale.y + number * scalingMultiplier, transform.localScale.z + number * scalingMultiplier);
        objectToFollow = obj;
        transform.position = obj.transform.position;
        offsetXGrouwth = Random.Range(-horizontalSpeed, horizontalSpeed);
    }

    private void Update()
    {
        // Start the counter to destroy the object
        StartCoroutine(WaitForEnd());
        offsetX += offsetXGrouwth;
        // Follow the given object on the x-axis
        if (objectToFollow)
        {
            transform.position = new Vector3(objectToFollow.transform.position.x + offsetX, transform.position.y);
        }
        // Make the canvas fly upwards
        transform.position += new Vector3(0f, upwardsSpeed);
        // Make the text shrink every frame
        damageNumberText.transform.localScale -= new Vector3(damageNumberText.transform.localScale.x * Time.deltaTime, damageNumberText.transform.localScale.y * Time.deltaTime);
        // Make text fade out
        damageNumberText.CrossFadeAlpha(0f, secondsToLast/2, true);
    }

    /// <summary>
    /// Waiting a set amount of time before destroying itself
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForEnd()
    {
        yield return new WaitForSeconds(secondsToLast);
        Destroy(gameObject);
    }
}
