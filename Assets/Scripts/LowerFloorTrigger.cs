using UnityEngine;

public class LowerFloorTrigger : MonoBehaviour
{
    public bool isPlayerPresent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerPresent = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerPresent = false;
        }
    }
}