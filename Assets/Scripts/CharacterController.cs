using System.Collections;
using TMPro;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    [Space(20)]
    [Header("Movement Variables")]
    [SerializeField]
    private float moveForce = 5f;
    [SerializeField]
    private float maxSpeed = 10f;
    [SerializeField]
    private float stopForce = 5f;
    [SerializeField, Range(0,1)]
    private float airMultiplier = 0.5f;
    [SerializeField]
    private float rotationSpeed = 720;
    [SerializeField]
    private float jumpForce = 10f;
    [SerializeField]
    float jumpDelay = 0.2f;
    [SerializeField]
    float jumpLeway = 1f;

    public bool jumping;
    public bool canJump;

    private float jumpTimer;

    [Space(20)]
    [Header("Ground Check")]
    [SerializeField]
    private Vector3 checkerPosition;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private float checkerRadius = 1f;

    [Space(20)]
    [Header("Camera Variables")]
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private Transform cameraPivotTransform;
    [SerializeField]
    private float followSpeed = 1.0f;
    [SerializeField]
    private float pivotSpeed = 1f;
    [SerializeField]
    private float lookSpeed = 1f;

    public float lookAngle;
    private float pivotAngle;

    [HideInInspector]
    public Rigidbody rb;
    public Transform cameraHolder;

    private float dirVel;

    [Space(20)]
    [Header("Debugging")]
    [SerializeField]
    private TextMeshProUGUI moveForceText;
    [SerializeField]
    private TextMeshProUGUI stopForceText;
    [SerializeField]
    private TextMeshProUGUI magText;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
        HandleRotation();
    }

    public void LateUpdate()
    {
        Jump();
    }

    public void FixedUpdate()
    {
        HandleMovement();
    }

    private Vector3 MoveVector()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        return new Vector3(moveX, 0, moveZ);
    }

    private void HandleMovement()
    {
        if(MoveVector() != Vector3.zero)
        {
            Moving();
        }
        else
        {
            Stopping();
        }
    }

    private void Moving()
    {
        // Get dir & vel
        Vector3 moveDir = MoveVector();
        Vector3 rbVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Rotate dir based on camera
        moveDir = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * moveDir;

        // If move, counter vel for better control
        if (MoveVector().normalized.magnitude <= 1 && Vector3.Dot(moveDir, rbVel) > 0.2f)
        {
            moveDir = -Vector3.Reflect(rbVel, moveDir);
        }

        // Normalize, get dirVel, then project onto normal
        moveDir.Normalize();
        dirVel = Vector3.Dot(rbVel, moveDir);
        moveDir = Vector3.ProjectOnPlane(moveDir, GetSurfaceNormal());

        // Calculate gradual force depending on current speed
        float gradualForce = Mathf.Lerp(moveForce, 0, rbVel.magnitude / maxSpeed);

        // Apply force only if we are not at max speed
        if (dirVel < maxSpeed)
        {
            rb.AddForce(moveDir * gradualForce * AirMultiplier(airMultiplier));
        }
    }

    // When not inputting any movement, and on the ground, apply a gradual force to stop the player
    // This is intended to make the player feel more responsive and less floaty
    private void Stopping()
    {
        // Get vel, mag, dir
        Vector3 rbVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float rbMag = rbVel.magnitude;
        Vector3 stopDir = -rbVel.normalized;

        // Only apply stopping force if the player is moving
        if (rbMag > 0.1f)
        {
            // Calculate gradual stop force depending on current speed
            float stopVel = Mathf.Lerp(0, stopForce, rbMag / maxSpeed);

            // Decide if we should add airMultiplier, currently not
            if (dirVel < maxSpeed)
            {
                rb.AddForce(AirMultiplier(1) * stopVel * stopDir);
            }
        }
    }

    private void Jump()
    {
        if (IsGrounded() && !jumping)
        {
            jumpTimer += Time.deltaTime;
            if(jumpTimer >= jumpDelay)
            {
                canJump = true;
            }
        }
        else
        {
            StartCoroutine(JumpLeway());
        }

        if (Input.GetKey(KeyCode.Space) && canJump)
        {
            jumpTimer = 0;
            jumping = true;
            canJump = false;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            StartCoroutine(JumpDelay());
        }
    }

    private IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(jumpDelay);
        jumping = false;
    }

    private IEnumerator JumpLeway()
    {
        yield return new WaitForSeconds(jumpLeway);
        canJump = false;
    }

    private Vector3 GetSurfaceNormal()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, groundLayer))
        {
            return hit.normal;
        }

        return Vector3.up; // Default to returning upward direction if no surface is found
    }

    private bool IsGrounded()
    {
        Collider[] colliders = Physics.OverlapSphere(checkerPosition + transform.position, checkerRadius, groundLayer);

        if (colliders.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Find Better Name
    private float AirMultiplier(float multiplier)
    {
        if(IsGrounded())
        {
            return 1;
        }
        else
        {
            return multiplier;
        }
    }

    //WTF is this?
    private void FollowPlayer()
    {
        Vector3 targetPosition = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime / followSpeed);
        transform.position = targetPosition;
    }

    private Vector3 MouseInputVector()
    {
        var mouseY = Input.GetAxis("Mouse Y");
        var mouseX = Input.GetAxis("Mouse X");

        return new Vector3(mouseX, mouseY, 0);
    }

    private void HandleRotation()
    {
        lookAngle += (MouseInputVector().x * lookSpeed);
        pivotAngle -= (MouseInputVector().y * pivotSpeed);
        pivotAngle = Mathf.Clamp(pivotAngle, -90, 90);

        Vector3 rotation = Vector3.zero;
        rotation.y = lookAngle;
        Quaternion targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;

        targetRotation = Quaternion.Euler(rotation);
        cameraPivotTransform.localRotation = targetRotation;
    }

    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(transform.position + checkerPosition, checkerRadius);

        /*
        Gizmos.DrawLine(transform.position, transform.position + rb.velocity);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + MoveVector().normalized * 1.5f);
        */
    }
}
