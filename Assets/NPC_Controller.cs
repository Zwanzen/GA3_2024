using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class NPC_Controller : MonoBehaviour
{
    [Space(20)]
    [Header("Agent Variables")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField, Range(0.1f,1.0f)] private float endRotationSpeed = 0.5f;
    [SerializeField] private float delayTime = 1f;

    private float rotationTimer = 0f;

    [Space(20)]
    [Header("Story")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Dialogue[] dialogues;

    private int currentWaypoint = -1;
    private bool isWaiting = false;
    private bool canRotate = false;

    private bool rotateToPlayer = false;
    private float rotateToPlayerTimer = 0f;


    // Refrences
    private void Initialize()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        npcInteraction = GetComponent<NPCInteraction>();
    }


    private NavMeshAgent agent;
    private Animator anim;
    private NPCInteraction npcInteraction;


    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (agent.remainingDistance <= 0.1f)
        {
            if (canRotate)
            {
                RotateToWaypoint();
                npcInteraction.endedDialogue = false;
            }
        }

        if (rotateToPlayer)
        {
            RotateTowardsPlayer();
        }
    }


    public void MoveToNextWaypoint()
    {
        isWaiting = false;
        currentWaypoint++;
        if (currentWaypoint > waypoints.Length)
        {
            return;
        }
        else
        {
            StartCoroutine(StartMove());

        }
    }

    private IEnumerator StartMove()
    {
        yield return new WaitForSeconds(delayTime);

        // Start moving to the next waypoint
        // Give the npc the next dialogue
        rotationTimer = 0f;
        agent.SetDestination(waypoints[currentWaypoint].position);
        npcInteraction.dialogue = dialogues[currentWaypoint];
        canRotate = true;
        isWaiting = false;
    }

    private void RotateToWaypoint()
    {
        // Change npc rotation to the waypoints rotation
        float dir = waypoints[currentWaypoint].localEulerAngles.y;

        // Slowly rotate the npc to the direction of the waypoint
        rotationTimer += Time.deltaTime * endRotationSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, dir, 0), rotationTimer);
        if (rotationTimer >= 1)
        {
            canRotate = false;
        }
    }

    public void IsInteracting()
    {
        canRotate = false;
        rotateToPlayerTimer = 0f;
        rotateToPlayer = true;
    }

    private void RotateTowardsPlayer()
    {
        rotateToPlayerTimer += Time.deltaTime * endRotationSpeed;

        // slowy rotate the npc to the player
        Vector3 dir = npcInteraction.player.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotateToPlayerTimer).eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if(rotateToPlayerTimer >= 1 || npcInteraction.endedDialogue)
        {
            rotateToPlayer = false;
        }

    }

}
