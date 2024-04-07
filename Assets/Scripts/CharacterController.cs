using System.Collections;
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
    private float rotationSpeed = 720;
    [SerializeField]
    private float jumpForce = 10f;
    [SerializeField]
    float jumpDelay = 0.2f;
    [SerializeField]
    float jumpLeway = 1f;

    public bool jumping;
    public bool canJump;

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
    private Vector3 dir;

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
        Walking();
    }

    private Vector3 MoveVector()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        return new Vector3(moveX, 0, moveZ);
    }

    private void Walking()
    {
        //get dir
        Vector3 moveDir = MoveVector();
        //get vel
        Vector3 rbVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        moveDir = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * moveDir;

        //if move, counter vel for better control
        if (MoveVector().normalized.magnitude <= 1 && Vector3.Dot(moveDir, rbVel) > 0.2f)
        {
            moveDir = -Vector3.Reflect(rbVel, moveDir);
        }

        //normalize before force
        moveDir.Normalize();

        //get vel based on dir
        dirVel = Vector3.Dot(rbVel, moveDir);

        //Project onto normal
        moveDir = Vector3.ProjectOnPlane(moveDir, GetSurfaceNormal());

        //Store something for later
        dir = moveDir;

        //decide if i can apply force in air vs grounded
        if (dirVel < maxSpeed && !IsGrounded())
        {
            rb.AddForce(moveDir * moveForce / 5);
        }
        else if (dirVel < maxSpeed)
        {
            rb.AddForce(moveDir * moveForce);
        }
    }

    private void Jump()
    {
        if (IsGrounded() && !jumping)
        {
            canJump = true;
        }
        else
        {
            StartCoroutine(JumpLeway());
        }

        if (Input.GetKey(KeyCode.Space) && canJump)
        {
            jumping = true;
            canJump = false;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            StartCoroutine(JumpDelay());
        }
    }

    private IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(jumpDelay * 2.8f);
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
        /*
        Gizmos.color = Color.yellow;

        //Gizmos.DrawSphere(transform.position + checkerPosition, checkerRadius);

        Gizmos.DrawLine(transform.position, transform.position + rb.velocity);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + MoveVector().normalized * 1.5f);
        */
    }
}
