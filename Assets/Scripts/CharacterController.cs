using System.Collections;
using TMPro;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Space(20)]
    [Header("Character Size Variables")]
    [SerializeField, Range(0,2)]
    private float characterHeight = 1.8f;
    [SerializeField, Range(0,2)]
    private float characterWidth = 1f;
    [SerializeField, Range(0.1f,0.8f)]
    private float stepHeight = 0.5f;
    [SerializeField]
    private Transform capsuleHolder;

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
    private float springStrenght = 50f;
    [SerializeField]
    private float springDampener = 10f;

    [Space(20)]
    [Header("Counter Movement")]
    [SerializeField]
    private float rightShiftMultiplier = 10f;
    [SerializeField, Range(0.9000f,0.9999f)]
    private float counterDotThreshold = 0.995f;
    [SerializeField, Range(0.1f, 1.0f)]
    private float counterMovementDirVelMultiplier = 1f;

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
    private float debugDirVel;

    private void Awake()
    {
        InstantiateCharacterSize();
    }

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

        if (IsGrounded() && !jumping)
        {
            Floating();
        }
    }

    private void Floating()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterHeight + stepHeight, groundLayer))
        {
            Vector3 vel = rb.velocity;
            float relVel = Vector3.Dot(Vector3.down, vel);

            float x = hit.distance - characterHeight;

            float springForce = (x * springStrenght) - (relVel * springDampener);
            rb.AddForce(Vector3.down * springForce);
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
        float dirVel = Vector3.Dot(rbVel, moveDir.normalized);

        // Find the dot product between the moveDir and the rbVel to determine if we are moving forward or backward
        // This is used to calculate the counter movement direction
        float dot = Vector3.Dot(moveDir.normalized, rbVel.normalized);
        moveDir = CalculateCounterMovement(moveDir, rbVel, dot, dirVel);

        // Normalize and project onto the surface normal
        // This is to make sure we are moving in the direction of the surface we are standing on
        // This is to make going up or down slopes more natural & decide if we can go up the slope
        moveDir.Normalize();
        moveDir = Vector3.ProjectOnPlane(moveDir, GetSurfaceNormal());

        // Calculate gradual force depending on current speed
        float gradualForce = Mathf.Lerp(moveForce, 0, dirVel / maxSpeed);
        float counterDot = Vector3.Dot(moveDir.normalized, rbVel.normalized);
        if (counterDot < counterDotThreshold)
        {
            gradualForce = Mathf.Lerp(moveForce, moveForce * 0.1f, dirVel / maxSpeed);
        }

        // Apply force only if we are not at max speed
        if (dirVel < maxSpeed)
        {
            moveDir = moveDir * gradualForce * AirMultiplier(airMultiplier);
            rb.AddForce(moveDir);
            debugMoveDir = moveDir;
            debugDirVel = dirVel;
        }
        else
        {
            // Can be removed, but useful for determening if we apply too much force
            Debug.Log("over max speed: " + (dirVel - maxSpeed));
        }

        // Remove or make debug variables if we want to see them in the inspector
        
        debugMoveInput = MoveVector();
        debugMoveInput = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * debugMoveInput;
        debugMoveInput.Normalize();
    }

    // Small method to calculate counter movement direction
    private Vector3 CalculateCounterMovement (Vector3 moveDir, Vector3 rbVel, float dot, float dirVel)
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

            //moveDir = -Vector3.Reflect(rbVel.normalized, moveDir.normalized);

            // To make the player align faster when the diffrerence is small
            // We gradually shift angle of counterDir to faster align with input direction the
            // Smaller the difference is the stronger the shift until the difference is acceptable
            float counterDot = Vector3.Dot(moveDir.normalized, rbVel.normalized);
            if (counterDot < counterDotThreshold && counterDot > 0.001f)
            {
                Vector3 gradualAngleShift = right * (counterDot * rightShiftMultiplier) * ((dirVel/maxSpeed) / counterMovementDirVelMultiplier);
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

        }

        // Calculate gradual stop force depending on current speed
        float stopVel = Mathf.Lerp(0, stopForce, rbMag / maxSpeed);

        // Decide if we should add airMultiplier, currently not
        rb.AddForce(AirMultiplier(1) * stopVel * stopDir);
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
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterHeight + (characterHeight * 0.1f), groundLayer))
        {
            return hit.normal;
        }

        return Vector3.up; // Default to returning upward direction if no surface is found
    }

    private bool IsGrounded()
    {
        if(Physics.Raycast(transform.position, Vector3.down, characterHeight + (characterHeight * 0.1f), groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }


/*        Collider[] colliders = Physics.OverlapSphere(checkerPosition + transform.position, checkerRadius, groundLayer);

        if (colliders.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }*/
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

    private void InstantiateCharacterSize()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterHeight + (characterHeight * 0.1f), groundLayer))
        {
            float y = hit.point.y;
            transform.position = new(transform.position.x, characterHeight + y, transform.position.z);
        }

        capsuleHolder.localScale = new(characterWidth, characterHeight / 2 - ((stepHeight / 2) + 0.02f), characterWidth);
        transform.localScale = new(characterWidth, 1, characterWidth);

        rb = GetComponent<Rigidbody>();
    }

    private void OnValidate()
    {
        InstantiateCharacterSize();

    }

    private void OnDrawGizmos()
    {
        
        /*
        Gizmos.DrawSphere(transform.position + checkerPosition, checkerRadius);
        */
        if (rb.velocity.magnitude > 0.1f)
        {
            storedRBVel = rb.velocity;
        }
        Gizmos.color = Color.Lerp(Color.yellow, Color.red, debugDirVel / maxSpeed);
        Gizmos.DrawLine(transform.position, transform.position + storedRBVel);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + debugMoveDir);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + debugMoveInput * 1.5f);
    }
}
