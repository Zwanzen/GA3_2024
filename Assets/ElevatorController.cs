using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    public UpperFloorTrigger upperFloorTriggerScript;
    public LowerFloorTrigger lowerFloorTriggerScript;

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
                }
                // Check if the elevator is in LiftUpperFloor state
                else if (currentState.IsName("LiftUpperFloor"))
                {
                    Debug.Log("Lift is going down.");
                    animator.SetTrigger("Descend");
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
                }
                else
                {
                    Debug.Log("Lift is already on this floor.");
                    return;
                }
            }
        }
    }
}