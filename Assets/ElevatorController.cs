using UnityEngine;
using FMODUnity;

public class ElevatorController : MonoBehaviour
{
    [Header("FMOD Sound Events")]
    public string elevatorActivationEvent;

    public UpperFloorTrigger upperFloorTriggerScript;
    public LowerFloorTrigger lowerFloorTriggerScript;

    public GameObject elevatorSoundSource; // Reference to the sound source GameObject
    public Animator animator;
    public bool isPlayerInside;

    private GameObject playerObject; // Reference to the Player GameObject

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            // Store reference to the root GameObject of the Player
            playerObject = other.transform.root.gameObject;
            // Make the Player GameObject a child of the elevator
            playerObject.transform.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            // Reset player's parent to null to detach it from the elevator
            playerObject.transform.parent = null;
        }
    }

    private void Update()
    {
        // Player is inside the elevator
        if (isPlayerInside)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

                // Check if the elevator is in LiftLowerFloor state
                if (currentState.IsName("LiftLowerFloor"))
                {
                    Debug.Log("Lift is going up.");
                    animator.SetTrigger("Ascend");
                    PlayElevatorActivationSound();
                }
                // Check if the elevator is in LiftUpperFloor state
                else if (currentState.IsName("LiftUpperFloor"))
                {
                    Debug.Log("Lift is going down.");
                    animator.SetTrigger("Descend");
                    PlayElevatorActivationSound();
                }
            }
        }
        // Player is outside the elevator but inside a floor trigger
        else if (upperFloorTriggerScript.isPlayerPresent)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

                // Check if the elevator is in LiftLowerFloor state
                if (currentState.IsName("LiftLowerFloor"))
                {
                    Debug.Log("Lift is coming up.");
                    animator.SetTrigger("Ascend");
                    PlayElevatorActivationSound();
                }
                else
                {
                    Debug.Log("Lift is already on this floor.");
                    return;
                }
            }
        }
        else if (lowerFloorTriggerScript.isPlayerPresent)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

                // Check if the elevator is in LiftUpperFloor state
                if (currentState.IsName("LiftUpperFloor"))
                {
                    Debug.Log("Lift is coming down.");
                    animator.SetTrigger("Descend");
                    PlayElevatorActivationSound();
                }
                else
                {
                    Debug.Log("Lift is already on this floor.");
                    return;
                }
            }
        }

        void PlayElevatorActivationSound()
        {
            // Update the position of the sound source to match the elevator's position
            elevatorSoundSource.transform.position = transform.position;
            // Play elevator activation sound event
            FMODUnity.RuntimeManager.PlayOneShot(elevatorActivationEvent, elevatorSoundSource.transform.position);
        }
    }
}