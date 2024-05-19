using Cinemachine;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region References and Variables
    [Space(20)]
    [Header("Character Size Variables")]
    [SerializeField, Range(0.0f, 2.0f)]
    private float characterHeight = 1.8f;
    [SerializeField, Range(0.0f, 2.0f)]
    private float characterWidth = 1.0f;
    [SerializeField, Range(0.1f, 0.8f)]
    private float stepHeight = 0.5f;
    [SerializeField]
    private Transform capsuleHolder;

    private float startedCharacterHeight;

    [Space(20)]
    [Header("Movement Variables")]
    [SerializeField]
    private float moveForce = 5.0f;
    [SerializeField]
    private float maxSpeed = 10.0f;
    [SerializeField]
    private float stopForce = 5.0f;
    [SerializeField, Range(0.0f, 1.0f)]
    private float airMultiplier = 0.5f;
    [SerializeField]
    private float rotationSpeed = 720.0f;
    [SerializeField]
    private float springStrenght = 50.0f;
    [SerializeField]
    private float springDampener = 10.0f;

    private float startMoveForce;
    private float startMaxSpeed;

    [Space(20)]
    [Header("Counter Movement")]
    [SerializeField]
    private float rightShiftMultiplier = 10.0f;
    [SerializeField, Range(0.9000f, 0.9999f)]
    private float counterDotThreshold = 0.995f;
    [SerializeField, Range(0.1f, 1.0f)]
    private float counterMovementDirVelMultiplier = 1.0f;

    private Vector3 right;
    private bool crouch;
    private float crouchTimer;

    [Space(20)]
    [Header("Jump Variables")]
    [SerializeField]
    private float jumpForce = 10.0f;
    [SerializeField]
    float jumpDelay = 0.2f;
    [SerializeField]
    float jumpLeway = 1.0f;

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
    private float checkerRadius = 1.0f;

    [Space(20)]
    [Header("Camera Variables")]
    [SerializeField]
    private Transform aimPoint;
    [SerializeField]
    private CinemachineVirtualCamera cinCamera;
    [SerializeField]
    private Transform cameraPivotTransform;
    [SerializeField]
    private float pivotSpeed = 1.0f;
    [SerializeField]
    private float lookSpeed = 1.0f;
    [Space(5)]
    [SerializeField]
    private float cameraShakeAmplitudeMax = 2f;
    [SerializeField]
    private float cameraShakeFrequencyMax = 2f;
    public Transform cameraHolder;
    [SerializeField]
    private float maxTilt = 10.0f;


    private float lookAngle;
    private float pivotAngle;

    [Space(20)]
    [Header("Interactions")]
    [SerializeField] private Transform npcLookAt;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialogueChoicesUI;
    [SerializeField] private GameObject[] choiceButtons;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private Animator anim;

    private float npcDist;

    public void InitializeNPC(NPCInteraction npc)
    {
        npc.Initialize(dialogueUI, dialogueChoicesUI, choiceButtons, nameText, dialogueText, continueText, interactText, anim);
    }

    bool interacting;

    [HideInInspector]
    public Rigidbody rb;

    [Space(20)]
    [Header("Sound")]
    [SerializeField]
    private StudioEventEmitter footstepEmitter;
    [SerializeField]
    private StudioEventEmitter landingEmitter;
    [SerializeField]
    private StudioEventEmitter jumpEmitter;
    [SerializeField]
    private EventReference SelectJumpAudio;
    private EventInstance jumpAudio;
    [SerializeField]
    float maxFallDistance = 2.0f;

    private float footstepTimer;
    private Vector3 lastPosition;
    private Vector3 fallFromPosition;
    private bool falling;

    [Space(20)]
    [Header("Debugging")]
    [SerializeField]
    private TextMeshProUGUI moveForceText;
    [SerializeField]
    private TextMeshProUGUI stopForceText;
    [SerializeField]
    private TextMeshProUGUI magText;

    #endregion

    #region Runtime Functions
    private void Awake()
    {
        InstantiateCharacterSize();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        startedCharacterHeight = characterHeight;
        startMoveForce = moveForce;
        startMaxSpeed = maxSpeed;


        jumpAudio = RuntimeManager.CreateInstance(SelectJumpAudio);
        RuntimeManager.AttachInstanceToGameObject(jumpAudio, landingEmitter.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (!interacting)
        {
            HandleRotation();
            CheckForInteractable();
        }
        HandleCameraShake();
        HandleFootsteps();
        HandleLandingSound();
        HandleCameraTilt();
        CrouchHandler();
    }

    public void LateUpdate()
    {
        if (!interacting)
        {
            Jump();
        }
    }

    public void FixedUpdate()
    {
        HandleMovement();
    }
    #endregion

    #region Movement and Rotation

    private void HandleMovement()
    {
        if (MoveVector() != Vector3.zero && !interacting)
        {
            Moving();
        }
        else if (IsGrounded())
        {
            Stopping();
        }

        // Should be implemented as a sliding function instead
        if (IsGrounded() && crouch && rb.velocity.magnitude > maxSpeed)
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
        RaycastHit hit = RaycastGround(2f);
        if (hit.point != Vector3.zero)
        {
            Vector3 vel = rb.velocity;

            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = hit.rigidbody;
            if (hitBody != null)
            {
                otherVel = hitBody.velocity;
            }

            float relDirVel = Vector3.Dot(Vector3.down, vel);
            float otherDirVel = Vector3.Dot(Vector3.down, otherVel);

            float relVel = relDirVel - otherDirVel;

            float x = hit.distance - characterHeight;

            float springForce = (x * springStrenght) - (relVel * springDampener);
            rb.AddForce(Vector3.down * springForce);

            // Add force to the hit rigidbody
            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(Vector3.up * springForce, hit.point);
            }
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
        }
    }

    // Small method to calculate counter movement direction
    private Vector3 CalculateCounterMovement(Vector3 moveDir, Vector3 rbVel, float dot, float dirVel)
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
                Vector3 gradualAngleShift = right * (counterDot * rightShiftMultiplier) * ((dirVel / maxSpeed) / counterMovementDirVelMultiplier);
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
            if (jumpTimer >= jumpDelay)
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

            // Add force to the rigidbody below the player when jumping
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, characterHeight + stepHeight, groundLayer))
            {
                Rigidbody hitBody = hit.rigidbody;

                // Add force to the hit rigidbody
                if (hitBody != null)
                {
                    hitBody.AddForceAtPosition(Vector3.up * jumpForce, hit.point);
                }
            }

            PlayJumpEmitter();
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

    private void HandleRotation()
    {
        lookAngle += (MouseInputVector().x * lookSpeed);
        pivotAngle -= (MouseInputVector().y * pivotSpeed);
        pivotAngle = Mathf.Clamp(pivotAngle, -85, 85);

        Vector3 rotation = Vector3.zero;
        rotation.y = lookAngle;
        Quaternion targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;

        targetRotation = Quaternion.Euler(rotation);
        cameraPivotTransform.localRotation = targetRotation;
    }

    #endregion

    #region Variable Methods

    private Vector3 MoveVector()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        return new Vector3(moveX, 0, moveZ);
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
        if (Physics.Raycast(transform.position, Vector3.down, characterHeight + (characterHeight * 0.1f), groundLayer))
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
        if (IsGrounded())
        {
            return 1;
        }
        else
        {
            return multiplier;
        }
    }

    private Vector3 MouseInputVector()
    {
        var mouseY = Input.GetAxis("Mouse Y");
        var mouseX = Input.GetAxis("Mouse X");

        return new Vector3(mouseX, mouseY, 0);
    }

    // Could be implemented in many functions using raycasts
    // Also modifiable
    private RaycastHit RaycastGround(float lengthMultiplier)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterHeight + (characterHeight * lengthMultiplier), groundLayer))
        {
            return hit;
        }

        return hit;
    }

    #endregion

    #region Interactions

    public void ToggleInteraction(bool _int, Transform lookAt)
    {
        if (_int)
        {
            Cursor.lockState = CursorLockMode.None;
            interacting = true;
            cinCamera.LookAt = lookAt;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            cinCamera.LookAt = aimPoint;
            interacting = false;
        }
    }

    // Function that checks if player is looking at an interactable object
    private void CheckForInteractable()
    {

        Collider[] colliders = Physics.OverlapCapsule(cinCamera.transform.position + (cameraPivotTransform.forward * 0.75f),
            (cameraPivotTransform.forward * 1.5f) + cinCamera.transform.position, 0.60f);

        List<Collider> npcCollidersList = new List<Collider>();

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("NPC"))
            {
                npcCollidersList.Add(col);
            }
        }

        // Check if collider has npc tag, then find the closest amongst the colliders
        if (npcCollidersList.Count > 0)
        {
            ResetClosest();
            SortClosest(npcCollidersList);
        }
        else
        {
            ResetClosest();
            return;
        }

        if (npcLookAt != null)
        {
            if (!npcLookAt.GetComponent<NPCInteraction>().endedDialogue)
            {
                interactText.gameObject.SetActive(true);
                interactText.text = "Press E to interact with " + npcLookAt.GetComponent<NPCInteraction>().dialogue._name;
                npcLookAt.GetComponent<NPCInteraction>().canInteract = true;
            }
        }

    }

    private void SortClosest(List<Collider> _colliders)
    {
        foreach (Collider col in _colliders)
        {
            if (col.CompareTag("NPC"))
            {
                if (npcLookAt == null)
                {
                    npcLookAt = col.transform;
                    npcDist = Vector3.Distance(transform.position, npcLookAt.position);
                }
                else
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < npcDist)
                    {
                        npcLookAt = col.transform;
                        npcDist = dist;
                    }
                }
            }
        }
    }

    private void ResetClosest()
    {
        npcDist = 0;
        interactText.gameObject.SetActive(false);
        if (npcLookAt != null)
        {
            npcLookAt.GetComponent<NPCInteraction>().canInteract = false;
            npcLookAt = null;
        }
    }

    #endregion

    #region Sound Systems

    private void HandleFootsteps()
    {
        if (IsGrounded() && MoveVector() != Vector3.zero)
        {
            // Place the footstep emitter at the position of our feet
            // This would make more sense if i could fully develope the player controller
            RaycastHit hit = RaycastGround(0.1f);
            if (hit.point != Vector3.zero)
            {
                footstepEmitter.transform.position = hit.point;
            }

            // Then decide if we can play the footstep sound
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= 0.5f && !crouch)
            {
                footstepEmitter.Play();
                footstepTimer = 0;
            }
            else if(footstepTimer >= 0.75f && crouch)
            {
                footstepEmitter.Play();
                footstepTimer = 0;
            }
        }
        else
        {
            //footstepTimer = 0;
        }
    }

    private void HandleLandingSound()
    {
        // If we're not grounded, Start fall checking
        if (!IsGrounded())
        {
            // As long as we're moving upwards, we raise the fall from position
            // When moving downwards, this will calculate the fall distance
            if (rb.velocity.y > 0)
            {
                fallFromPosition = transform.position;
            }
            falling = true;
        }
        else
        {
            // If we were falling, play the landing sound, then make sure we're not falling anymore
            if (falling)
            {
                // Calculate the fall distance to determine falling sound intensity
                float fallDistance = fallFromPosition.y - transform.position.y;

                // Stops it from playing landing sound when crouching
                if (fallDistance > 0.2f)
                {
                    // Place the landing emitter at the position of our feet
                    RaycastHit hit = RaycastGround(0.1f);
                    if (hit.point != Vector3.zero && fallDistance > 0.5f)
                    {
                        landingEmitter.transform.position = hit.point;
                    }

                    // Set the intensity, and play the landing sound
                    // FMOD IS WEIRD
                    float lerpedDistance = Mathf.Lerp(0.5f, 0.9f, fallDistance / maxFallDistance);
                    RuntimeManager.AttachInstanceToGameObject(jumpAudio, landingEmitter.transform);
                    jumpAudio.setParameterByName("FallHeight", lerpedDistance);
                    jumpAudio.start();
                }

            }
            else
            {
                // If we were not falling, in case of us never going upwards before falling
                // We set the fall from position to the current position
                fallFromPosition = transform.position;
            }
            falling = false;
        }
    }

    private void PlayJumpEmitter()
    {
        jumpEmitter.Play();
    }

    #endregion

    #region Other Functions

    private void HandleCameraShake()
    {
        // Get values for shake amplitude and frequency based on current speed
        float shakeAmplitude = Mathf.Lerp(1, cameraShakeAmplitudeMax, rb.velocity.magnitude / (maxSpeed * 2));
        float shakeFrequency = Mathf.Lerp(1, cameraShakeFrequencyMax, rb.velocity.magnitude / (maxSpeed * 2));

        // Set the shake values to the camera
        cinCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = shakeAmplitude;
        cinCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = shakeFrequency;
    }

    private void HandleCameraTilt()
    {
        float input = Input.GetAxis("Horizontal");
        if(input > 0)
        {
            cinCamera.m_Lens.Dutch = Mathf.Lerp(0, -maxTilt, input);
        }else if (input < 0)
        {
            cinCamera.m_Lens.Dutch = Mathf.Lerp(0, maxTilt, input * -1);
        }
        else
        {
            cinCamera.m_Lens.Dutch = 0;
        }

    }

    private void CrouchHandler()
    {
        float crouchMultiplier = 1.3f;

        // Input and single impulse changes
        // Not the best
        if (Input.GetKeyDown(KeyCode.C))
        {
            crouch = true;
            maxSpeed = startMaxSpeed / 2;
            moveForce = startMoveForce / 2;
            crouchTimer = 0;
        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            crouch = false;
            maxSpeed = startMaxSpeed;
            moveForce = startMoveForce;
            crouchTimer = 0;
        }

        // What actually runs
        if (crouch)
        {
            crouchTimer += Time.deltaTime;

            characterHeight = Mathf.Lerp(characterHeight, startedCharacterHeight / crouchMultiplier, crouchTimer);
            capsuleHolder.localScale = new(characterWidth, (characterHeight / 2 - ((stepHeight / 2) + 0.02f)), characterWidth);
        }
        else
        {
            crouchTimer += Time.deltaTime;

            characterHeight = Mathf.Lerp(characterHeight, startedCharacterHeight , crouchTimer);
            capsuleHolder.localScale = new(characterWidth, (characterHeight / 2 - ((stepHeight / 2) + 0.02f)), characterWidth);
        }
    }

    private void InstantiateCharacterSize()
    {
        RaycastHit hit = RaycastGround(0.5f);
        float y = hit.point.y;
        transform.position = new(transform.position.x, characterHeight + y, transform.position.z);

        capsuleHolder.localScale = new(characterWidth, (characterHeight / 2 - ((stepHeight / 2) + 0.02f)), characterWidth);
        transform.localScale = new(characterWidth, 1, characterWidth);

        rb = GetComponent<Rigidbody>();
    }

    #endregion
}
