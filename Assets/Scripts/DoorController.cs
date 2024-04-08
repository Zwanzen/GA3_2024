using System.Collections;
using System.Collections.Generic;
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
        // Check for input to open the door only if the player is in range
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Toggle the door state
            isOpen = !isOpen;

            // Update the Animator parameter to trigger the door animation
            doorAnimator.SetBool("doorActivate", isOpen);

            // Start the coroutine to handle light deactivation/activation
            StartCoroutine(ToggleLightCoroutine(isOpen));
        }
    }

    IEnumerator ToggleLightCoroutine(bool openState)
    {
        if (openState)
        {
            // Wait for 0.15 seconds before deactivating the light
            yield return new WaitForSeconds(0.15f);

            // Deactivate the light when the door opens if there is a light component
            if (doorLight != null)
            {
                doorLight.enabled = false;
            }
        }
        else
        {
            // Wait for 0.85 seconds before reactivating the light
            yield return new WaitForSeconds(0.85f);

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