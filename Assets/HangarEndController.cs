using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class HangarEndController : MonoBehaviour
{
    [SerializeField] private GameObject endCamera;
    [SerializeField] private Volume volume;
    [SerializeField] private GameObject player;

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
            fadeTimer += Time.deltaTime;
            volume.weight = fadeTimer / 2f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ending)
        {
            if (other.CompareTag("Player") && !ended)
            {
                // End the game
                endEvent.Invoke();
                ended = true;
                StartCoroutine(EndCamera());
            }
        }
    }

    private IEnumerator EndCamera()
    {
        yield return new WaitForSeconds(2f);
        endCamera.SetActive(true);
        player.SetActive(false);
        volume.weight = 0f;
    }
}
