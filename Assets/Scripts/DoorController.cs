using UnityEngine;
using System.Collections;
using FMODUnity;
using System.Collections.Generic;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;
    private Light doorLight;
    private bool isPlayerInRange = false;
    private Transform doorTransform; // Reference to the door's transform

    [Header("Door Settings")]
    public GameObject doorObject; // Reference to the door GameObject
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool automaticDoor = false; // Boolean for whether the door automatically opens or closes
    [SerializeField] private bool isLocked = false; // Boolean for whether the door is locked
    public KeyCode keyToOpen = KeyCode.E; // Key required to open the door, default is 'E'

    [Header("FMOD Sound Events")]
    public string doorOpenSoundEvent;
    public string doorCloseSoundEvent;

    [Header("Messages")]
    public string lockedMessage = "The door is locked.";

    void Start()
    {

        // Get the Animator component attached to the door if it exists
        doorAnimator = doorObject.GetComponent<Animator>();

        // Check if the Animator component is attached to the door
        if (doorAnimator == null)
        {
            Debug.LogError("Animator component not found on the door GameObject.");
            return;
        }

        // Get the door's transform
        doorTransform = doorObject.transform;

        // Check if the door has a light component
        doorLight = doorObject.GetComponentInChildren<Light>();

        // Close the door at the start
        // If it should be closed - Bias
        //doorAnimator.SetBool("doorActivate", false);
        //isOpen = false;
        if (!isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    void Update()
    {
        if (automaticDoor)
        {
            HandleAutomaticDoor();
        }
        else
        {
            HandleManualDoor();
        }
    }

    void HandleAutomaticDoor()
    {
        if(inTrigger.Count > 0)
        {
            isPlayerInRange = true;
        }
        else
        {
            isPlayerInRange = false;
        }

        if (isPlayerInRange && !isOpen && !isLocked)
        {
            OpenDoor();
        }
        else if (!isPlayerInRange && isOpen)
        {
            CloseDoor();
        }
    }

    void HandleManualDoor()
    {
        if (isPlayerInRange && Input.GetKeyDown(keyToOpen))
        {
            if (isLocked)
            {
                BroadcastLockedMessage();
            }
            else if (!isOpen)
            {
                OpenDoor();
            }
            else
            {
                CloseDoor();
            }
        }
    }

    public void OpenDoor()
    {
        // Update the Animator parameter to trigger the door animation
        doorAnimator.SetBool("doorActivate", true);
        isOpen = true;

        // Start the coroutine to handle light deactivation
        StartCoroutine(ToggleLightCoroutine(true));

        // Play the door open sound from the door's position
        FMODUnity.RuntimeManager.PlayOneShot(doorOpenSoundEvent, doorTransform.position);
    }

    public void CloseDoor()
    {
        // Update the Animator parameter to trigger the door animation
        doorAnimator.SetBool("doorActivate", false);
        isOpen = false;

        // Start the coroutine to handle light reactivation
        StartCoroutine(ToggleLightCoroutine(false));

        // Play the door close sound from the door's position
        FMODUnity.RuntimeManager.PlayOneShot(doorCloseSoundEvent, doorTransform.position);
    }

    void BroadcastLockedMessage()
    {
        Debug.Log(lockedMessage); // You can replace this with whatever method you use to display messages to the player
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

    private List<Collider> inTrigger = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("NPC")) && !inTrigger.Contains(other))
        {
            inTrigger.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (inTrigger.Contains(other))
        {
            inTrigger.Remove(other);
        }
    }
}
