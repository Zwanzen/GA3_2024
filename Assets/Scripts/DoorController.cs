using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;
    private Light doorLight;
    private bool isOpen = false;
    private bool isPlayerInRange = false;

    void Start()
    {
        // Get the Animator component attached to the door
        doorAnimator = GetComponent<Animator>();

        // Check if the door has a light component
        doorLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        // Check if the door should open or close based on the player's proximity
        if (isPlayerInRange)
        {
            if (!isOpen)
            {
                // Update the Animator parameter to trigger the door animation
                doorAnimator.SetBool("doorActivate", true);
                isOpen = true;

                // Start the coroutine to handle light deactivation
                StartCoroutine(ToggleLightCoroutine(true));
            }
        }
        else
        {
            if (isOpen)
            {
                // Update the Animator parameter to trigger the door animation
                doorAnimator.SetBool("doorActivate", false);
                isOpen = false;

                // Start the coroutine to handle light reactivation
                StartCoroutine(ToggleLightCoroutine(false));
            }
        }
    }

    IEnumerator ToggleLightCoroutine(bool openState)
    {
        if (openState)
        {
            // Wait for 0.2 seconds before deactivating the light
            yield return new WaitForSeconds(0.2f);

            // Deactivate the light when the door opens if there is a light component
            if (doorLight != null)
            {
                doorLight.enabled = false;
            }
        }
        else
        {
            // Wait for 0.8 seconds before reactivating the light
            yield return new WaitForSeconds(0.8f);

            // Reactivate the light when the door closes if there is a light component
            if (doorLight != null)
            {
                doorLight.enabled = true;
            }
        }
    }

    // OnTriggerEnter is called when the Collider other enters the trigger
    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    // OnTriggerExit is called when the Collider other has stopped touching the trigger
    void OnTriggerExit(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}