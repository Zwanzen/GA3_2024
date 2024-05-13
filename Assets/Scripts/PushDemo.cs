using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushDemo : MonoBehaviour
{
    [SerializeField]
    private LayerMask groundLayer;
    private Rigidbody rb;
    private ForceMode forceMode = ForceMode.Acceleration;
    [SerializeField]
    private float springStrenght = 50f;
    [SerializeField]
    private float springDampener = 5f;
    private const float targetHeight = 1.8f;
    private const float gravity = 9.81f;
    private float currentHeight;
    private float heightDifference;
    private bool isGrounded;
    private float accel;

    RaycastHit hit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2, groundLayer))
        {
            currentHeight = transform.position.y - hit.point.y;
            isGrounded = true;
            rb.useGravity = false;
        }
        else
        {
            isGrounded = false;
            rb.useGravity = true;
        }
    }

    private void FixedUpdate()
    {

        if(isGrounded)
        {
            Vector3 vel = rb.velocity;
            float relVel = Vector3.Dot(Vector3.down,vel);

            float x = hit.distance - targetHeight;
            Debug.Log("X: " + x);

            float springForce = (x * springStrenght) - (relVel * springDampener);
            rb.AddForce(Vector3.down * springForce);

        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Velocity: " + rb.velocity);
        GUI.Label(new Rect(10, 30, 200, 20), "Current Height: " + currentHeight);
        GUI.Label(new Rect(10, 50, 200, 20), "Current Differance: " + heightDifference);
        GUI.Label(new Rect(10, 70, 200, 20), "Current Acceleration: " + accel);

    }
}
