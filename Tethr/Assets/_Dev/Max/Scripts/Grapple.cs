using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;
using System.Collections.Generic;

public class Grapple : MonoBehaviour
{
    [SerializeField] private float grappleStrength = 1f;
    [SerializeField] private float fireGrappleDistance = 100f;
    [SerializeField] private float minDistanceBetweenPoints = 0.1f;
    [SerializeField] private GameObject ropeObject;
    private int ignoreLayers;
    private DistanceJoint2D distanceJoint;
    private Vector2 anchorPoint;
    private Rigidbody2D playerRigidbody;
    private float ropeTotalDistance;
    private List<Vector2> ropePoints = new List<Vector2>();
    private List<GameObject> ropeSegmentObjects = new List<GameObject>();
    private bool canGrapple;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        distanceJoint = GetComponent<DistanceJoint2D>();
    }

    private void Start()
    {
        int layerToIgnore = LayerMask.NameToLayer("Player");
        ignoreLayers = ~(1 << layerToIgnore);
    }

    private void Update()
    {
        PlayerInput();

        

        //distanceJoint.distance = Vector2.Distance(transform.position, anchorPoint);
    }

    private void PlayerInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) //initial mouse press
        {
            canGrapple = PlayerRopeControl();
        }

        if (canGrapple && Mouse.current.leftButton.isPressed) //mouse is pressed
        {
            ropeObject.SetActive(true);
            distanceJoint.enabled = true;

            MoveRope();

            PullPlayerOnRope();

            DetectRopeCollision();
        }
        else
        {
            ropeObject.SetActive(false);
            distanceJoint.enabled = false;
        }
    }

    private bool PlayerRopeControl()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue() - new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mouseDirection = mousePosition.normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, mouseDirection, fireGrappleDistance, ignoreLayers);
        if (hit == false)
        {
            return false;
        }
        Debug.DrawLine(transform.position, hit.point, Color.red, 2f);
        ropeTotalDistance = hit.distance;
        FireRope(hit.point);
        return true;
    }

    private void FireRope(Vector2 ropeHitPoint)
    {
        distanceJoint.connectedAnchor = ropeHitPoint;

        distanceJoint.distance = ropeTotalDistance;

        anchorPoint = ropeHitPoint;
    }

    private void MoveRope()
    {
        Vector2 direction = anchorPoint - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        ropeObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        //float distance = (anchorPoint - (Vector2)transform.position).magnitude;
        ropeObject.transform.localScale = new Vector3(Vector2.Distance(anchorPoint, transform.position), 1, 1);
    }

    private void PullPlayerOnRope()
    {
        Vector2 anchorDirection = (anchorPoint - (Vector2)transform.position).normalized;
        Vector2 velocityDirection = playerRigidbody.linearVelocity.normalized;
        Vector2 midpoint = (anchorDirection + velocityDirection).normalized;

        Debug.DrawRay(transform.position, midpoint, Color.green, 2f);
        //Vector2 dirToPoint = (anchorPoint - (Vector2)transform.position).normalized;

        //Vector2 perpA = new Vector2(-dirToPoint.y, dirToPoint.x);
        //Vector2 perpB = new Vector2(dirToPoint.y, -dirToPoint.x);

        //Vector2 velocityDir = playerRigidbody.linearVelocity.normalized;

        //Vector2 chosenDirection = (Vector2.Dot(velocityDir, perpA) > Vector2.Dot(velocityDir, perpB)) ? perpA : perpB;

        playerRigidbody.AddForce(midpoint * grappleStrength);
    }

    private void DetectRopeCollision()
    {
        RaycastHit2D hit = Physics2D.Linecast(transform.position, anchorPoint, ignoreLayers);
        if (hit.point == Vector2.zero || Vector2.Distance(anchorPoint, hit.point) < minDistanceBetweenPoints)
        {
            return;
        }
        ropeTotalDistance -= Vector2.Distance(anchorPoint, hit.point);
        FireRope(hit.point);
    }

    private void DetectRopeUnrwap()
    {

    }

    private void AddNewRope(Vector2 ropeHit)
    {
        ropePoints.Add(ropeHit);
        //ropeObjects.Add(Instantiate())
    }

    //detect rope collision should add the point to the list and inst a new rope obj
    //detect rope unwrap should FireRope on the previous rope point before deleting the old rope point and obj in list
    //redo MoveRope to use latest in RopeObjects list

}
