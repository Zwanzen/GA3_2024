using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HangarEndController : MonoBehaviour
{
    [SerializeField] private GameObject endCamera;

    [Space(10)]
    private bool ending = false;
    private bool ended = false;
    public UnityEvent endEvent;

    private float fadeTimer = 0f;

    public void StartDecom()
    {
        ending = true;
    }

    private void Update()
    {
        if (ended)
        {
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ending)
        {
            if (other.CompareTag("Player"))
            {
                // End the game
                endEvent.Invoke();
                ended = true;

            }
        }
    }

    private IEnumerator EndCamera()
    {
        yield return new WaitForSeconds(2f);
        endCamera.SetActive(true);
    }
}
