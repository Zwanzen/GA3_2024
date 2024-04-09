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

    [Space(20)]
    [Header("Counter Movement")]

    [Space(20)]
    [Header("Jump Variables")]
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
    private Vector3 right;

    [Space(20)]
    [Header("Debugging")]
    [SerializeField]
    private TextMeshProUGUI moveForceText;
    [SerializeField]
    private TextMeshProUGUI stopForceText;
    [SerializeField]
    private TextMeshProUGUI magText;
    
    private Vector3 debugMoveDir;
    private Vector3 debugMoveInput;
    private Vector3 storedRBVel;
    private Vector3 counterMoveDir;

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
        else if(IsGrounded())
        {
            Stopping();
        }
    }

    private void Moving()
    {
        // Get dir & vel
        Vector3 moveDir = MoveVector();
        Vector3 rbVel = new(rb.velocity.x, 0, rb.velocity.z);

        // Rotate dir based on camera, then find the relative velocity in that direction
        // We use this to determine how fast we are moving in the wanted direction
        moveDir = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * moveDir;
        dirVel = Vector3.Dot(rbVel, moveDir.normalized);

        // Find the dot product between the moveDir and the rbVel to determine if we are moving forward or backward
        // This is used to calculate the counter movement direction
        float dot = Vector3.Dot(moveDir.normalized, rbVel.normalized);
        moveDir = CalculateCounterMovement(moveDir, rbVel, dot);

        // Normalize and project onto the surface normal
        // This is to make sure we are moving in the direction of the surface we are standing on
        // This is to make going up or down slopes more natural & decide if we can go up the slope
        moveDir.Normalize();
        moveDir = Vector3.ProjectOnPlane(moveDir, GetSurfaceNormal());

        // Calculate gradual force depending on current speed
        // Dot multiplier is temporary, it tries to increase the speed when hard turning
        float dotMultiplier = Mathf.Lerp(1, 1, dot);
        float gradualForce = Mathf.Lerp(moveForce * dotMultiplier, 0, dirVel / maxSpeed);

        // Apply force only if we are not at max speed
        if (dirVel < maxSpeed)
        {
            rb.AddForce(moveDir * gradualForce * AirMultiplier(airMultiplier));
        }
        else
        {
            // Can be removed, but useful for determening if we apply too much force
            Debug.Log("over max speed: " + (dirVel - maxSpeed));
        }

        // Remove or make debug variables if we want to see them in the inspector
        debugMoveDir = moveDir;
        debugMoveInput = MoveVector();
        debugMoveInput = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * debugMoveInput;
        debugMoveInput.Normalize();
    }

    // Small method to calculate counter movement direction
    private Vector3 CalculateCounterMovement (Vector3 moveDir, Vector3 rbVel, float dot)
    {
        // Counter vel for better control only when moving forward to not confuse the counter movement
        if (dot > 0.1f)
        {
            // By using the dot of right and rbVel we can determine if we are moving left or right
            // Because by shifting it 90 degrees, we get left and right instead of forward and backward
            right = Vector3.Cross(Vector3.up, moveDir);
            right.Normalize();

            if (Vector3.Dot(right, rbVel.normalized) > 0)
            {
                right *= -1;
            }

            // Mirroring the rbVel to get the exact opposite direction
            // Then setting the movement direction to the reflected direction
            // This will quickly alight the player with the wanted direction the larger the difference
            // Between the rbVel and the wanted direction
            moveDir = -Vector3.Reflect(rbVel.normalized, moveDir.normalized);

            // To make the player align faster when the diffrerence is small
            // We gradually shift angle of counterDir to faster align with input direction the
            // Smaller the difference is the stronger the shift until the difference is acceptable
            float counterDot = Vector3.Dot(moveDir.normalized, rbVel.normalized);
            if (counterDot < 0.995f)
            {
                Vector3 gradualAngleShift = right * counterDot;
                //Vector3 gradualAngleShift = right * Mathf.Lerp(0, 1, counterDot);
                moveDir += gradualAngleShift;
                
            }

            return moveDir;
        }

        // If not moving forward, return the original moveDir to avoid bugs
        return moveDir;
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
        /*
        Gizmos.DrawSphere(transform.position + checkerPosition, checkerRadius);
        */
        if (rb.velocity.magnitude > 0.1f)
        {
            storedRBVel = rb.velocity;
        }

        Gizmos.DrawLine(transform.position, transform.position + storedRBVel.normalized * 1.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + debugMoveDir * 1.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + debugMoveInput * 1.5f);
    }
}
