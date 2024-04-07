using Unity.VisualScripting;
using UnityEngine;
using MoreMountains.Feedbacks;


public class PhysicsGrabber : MonoBehaviour
{
    public float GrabLenght = 1f;
    public Vector3 HoldPos;
    [SerializeField]
    private LayerMask GrabLayer;

    public float rotationSpeed = 0.5f;
    public bool grabbing = false;
    Rigidbody rb;

    Outline outlineComponent;

    [Header("Spring Joint Configurations")]
    [SerializeField]
    private float springForce = 50f;
    [SerializeField]
    private float damper = 1f;
    [SerializeField]
    private float minDist = 0.1f;
    [SerializeField]
    private float maxDist = 1f;
    [SerializeField]
    private float tolarance = 0.02f;

    public Transform lookedAtTransform;
    private SpringJoint joint;
    private Vector3 grabOffset = Vector3.zero;

    [SerializeField]
    private MMF_Player grabFeedback;


    private void Start()
    {
        outlineComponent = GetComponent<Outline>();
    }

    private void Update()
    {
        HandleOutline();

        HandleGrabInput();

        Holding();

    }

    private void HandleGrabInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && lookedAtTransform != null && !grabbing)
        {
            if (lookedAtTransform.tag != "Interactable")
            {
                Grab(lookedAtTransform);
                grabFeedback.PlayFeedbacks();
            }
            else
            {
                //var act = lookedAtTransform.GetComponent<InteractableManager>();
                //act.Interact();
                
            }
        }

        if (grabbing && !Input.GetKey(KeyCode.Mouse0))
        {
            Drop();
        }
    }

    private Transform LookingAtTransform()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, GrabLenght, GrabLayer))
        {
            return hit.transform;
        }
        else
        {
            return null;

        }
    }

    private void EnableOutline(Transform transform)
    {
        //If transform dont have an outline component, add it, then fix the settings.
        if(transform.GetComponent<Outline>() == null)
        {
            var newOutline = transform.AddComponent<Outline>();
            newOutline.OutlineColor = outlineComponent.OutlineColor;
            newOutline.OutlineWidth = outlineComponent.OutlineWidth;
            newOutline.OutlineMode = outlineComponent.OutlineMode;
        }

        //Enable the outline, and store the transform to be turned off later.
        transform.GetComponent<Outline>().enabled = true;
        lookedAtTransform = transform;
    }

    private void DisableOutline(Transform transform)
    {
        //Dont need to check if it has outline component, since every transform inputted here has to have had it.
        transform.GetComponent<Outline>().enabled = false;
        lookedAtTransform = null;
    }

    private void HandleOutline()
    {
        //If im currently grabbing an object, i want only the grabbed object to get an outline.
        if(grabbing)
        {
            return;
        }

        //If im looking at a new transform, handle outline.
        if(LookingAtTransform() != null)
        {
            //If there was a previous object, check if the new object is the same.
            if (lookedAtTransform != null)
            {
                //if it's the same, do nothing, if it's a new object, remove the last object's outline, and add outline on the new object.
                if (LookingAtTransform() != lookedAtTransform)
                {
                    DisableOutline(lookedAtTransform);
                    EnableOutline(LookingAtTransform());
                }
            }
            else
            {
                //If no previous object, enable the new objects outline.
                EnableOutline(LookingAtTransform());
            }
        }
        //If im not looking at anything, remove previous outlines if there were any.
        else if(lookedAtTransform != null)
        {
            DisableOutline(lookedAtTransform);
        }
    }

    private void Grab(Transform obj)
    {
        rb = obj.GetComponent<Rigidbody>();
        rb.drag = 5;
        rb.angularDrag = 5f;
        //GetGrabOffset();
        AddSpringJoint(obj);
        grabbing = true;
    }

    private void GetGrabOffset()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, GrabLenght, GrabLayer))
        {
             grabOffset = hit.point;
        }
    }

    void Holding()
    {
        if (grabbing)
        {
            HoldPos = transform.position + transform.forward * GrabLenght / 1.5f;
            joint.connectedAnchor = HoldPos;

            //Rotate the object to align.
            Quaternion toRotation = Quaternion.FromToRotation(rb.transform.forward, transform.forward);

            // Apply rotation gradually using Lerp
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, toRotation * rb.rotation, rotationSpeed * Time.fixedDeltaTime));

            //Because unity has some bugs, i cant let the object you're holding stand still, so i apply some movement at all times.
            rb.AddForce(Vector3.up * 0.1f * Time.deltaTime);
        }
    }

    public void Drop()
    {
        rb.drag = 0f;
        rb.angularDrag = 0.1f;
        RemoveSpringJoint(rb.transform);
        grabbing = false;
    }

    private void AddSpringJoint(Transform obj)
    {
        //Add springjoint and all its variables
        joint = obj.AddComponent<SpringJoint>();

        joint.spring = springForce;
        joint.damper = damper;
        joint.minDistance = minDist;
        joint.maxDistance = maxDist;
        joint.tolerance = tolarance;

        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = grabOffset;
    }

    private void RemoveSpringJoint(Transform obj)
    {
        Destroy(obj.GetComponent<SpringJoint>());
        grabOffset = Vector3.zero;
        joint = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(255f, 0, 0, 0.2f);
    }
}
