using UnityEngine;
using System.Collections;
using FMODUnity;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;
    private Light doorLight;

    [Header("Door Settings")]
    public GameObject doorObject; // Reference to the door GameObject
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool isPlayerInRange = false;

    [Header("FMOD Sound Events")]
    public string doorOpenSoundEvent;
    public string doorCloseSoundEvent;

    void Start()
    {
        // Get the Animator component attached to the door
        doorAnimator = doorObject.GetComponent<Animator>();

        // Check if the Animator component is attached to the door
        if (doorAnimator == null)
        {
            Debug.LogError("Animator component not found on the door GameObject.");
            return;
        }

        // Check if the door has a light component
        doorLight = doorObject.GetComponentInChildren<Light>();
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

                // Play the door open sound
                FMODUnity.RuntimeManager.PlayOneShot(doorOpenSoundEvent, GetComponent<Transform>().position);
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

                // Play the door close sound
                FMODUnity.RuntimeManager.PlayOneShot(doorCloseSoundEvent, GetComponent<Transform>().position);
            }
        }
    }

    IEnumerator ToggleLightCoroutine(bool openState)
    {
        if (openState)
        {
            // Wait for 0.2 seconds before deactivating the light (to hinder light through the wall)
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