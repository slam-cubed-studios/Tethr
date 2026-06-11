using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;
using System.Collections.Generic;
using UnityEditor;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DistanceJoint2D))]
public class Grapple : MonoBehaviour
{
    [SerializeField] private bool isPlayer;
    [SerializeField] private float grappleStrength = 1f;
    [SerializeField] private float minDistanceBetweenPoints = 0.1f;
    [SerializeField] private GameObject ropeObject;
    [SerializeField] private int autoAimTraceCount = 1;
    [SerializeField] private float autoAimRadius = 20f;
    [SerializeField] private float autoAimRange = 100f;
    [SerializeField] private float nonPlayerRopeDistance = 50f;
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
        if (!ropeObject.scene.IsValid()) //check if object is in scene
        {
            ropeObject = Instantiate(ropeObject, transform);
        }
        int layerToIgnore = LayerMask.NameToLayer("Player");
        ignoreLayers = ~(1 << layerToIgnore);

        if (!isPlayer)
        {
            ropeTotalDistance = nonPlayerRopeDistance;
            FireRope(transform.position);
            playerRigidbody.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 5000f);
        }
    }

    private void Update()
    {
        if (isPlayer)
        {
            PlayerInput();
        }
        else
        {
            ropeObject.SetActive(true);
            distanceJoint.enabled = true;

            MoveRope();
            DetectRopeSegmentCollision();
        }
        
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

            DetectRopeSegmentCollision();
        }
        else
        {
            ropeObject.SetActive(false);
            distanceJoint.enabled = false;
            ClearRopeSegments();
        }
    }

    private bool PlayerRopeControl()
    {
        Vector2 autoAimResult = HookAutoAim();
        
        if (autoAimResult == Vector2.zero)
        {
            return false;
        }

        Debug.DrawLine(transform.position, autoAimResult, Color.green, 5f);

        ropeTotalDistance = Vector2.Distance(autoAimResult, transform.position);
        FireRope(autoAimResult);
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

        ropeObject.transform.localScale = new Vector3(Vector2.Distance(anchorPoint, transform.position), 1, 1);
    }

    private void PullPlayerOnRope()
    {
        Vector2 anchorDirection = (anchorPoint - (Vector2)transform.position).normalized;
        Vector2 velocityDirection = playerRigidbody.linearVelocity.normalized;
        Vector2 midpoint = (anchorDirection + velocityDirection).normalized;

        Debug.DrawRay(transform.position, midpoint, Color.blue, 2f);

        playerRigidbody.AddForce(midpoint * grappleStrength);
    }

    private void DetectRopeSegmentCollision()
    {
        RaycastHit2D hit = Physics2D.Linecast(transform.position, anchorPoint, ignoreLayers);
        if (hit.point == Vector2.zero || Vector2.Distance(anchorPoint, hit.point) < minDistanceBetweenPoints)
        {
            return;
        }
        AddNewRope(hit.point, anchorPoint);
        ropeTotalDistance -= Vector2.Distance(anchorPoint, hit.point);
        FireRope(hit.point);
    }

    private void DetectRopeUnrwap()
    {

    }

    private void AddNewRope(Vector2 ropeHit, Vector2 ropeAnchor)
    {
        //ropePoints.Add(ropeHit);

        Vector2 direction = ropeAnchor - ropeHit;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject ropeSegmentObj = Instantiate(ropeObject, ropeHit, Quaternion.Euler(0f, 0f, angle));
        ropeSegmentObj.transform.localScale = new Vector3(Vector2.Distance(ropeHit, ropeAnchor), 1, 1);

        ropeSegmentObjects.Add(ropeSegmentObj);
    }

    private void ClearRopeSegments()
    {
        //make sure to clear rope points when doing unwrapping

        foreach (GameObject ropeObj in ropeSegmentObjects)
        {
            Destroy(ropeObj);
        }
        ropeSegmentObjects.Clear();
    }

    //detect rope collision should add the point to the list and inst a new rope obj
    //detect rope unwrap should FireRope on the previous rope point before deleting the old rope point and obj in list
    //redo MoveRope to use latest in RopeObjects list

    private Vector2 HookAutoAim()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue() - new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mouseDirection = mousePosition.normalized;

        float startAngle = -autoAimRadius * 0.5f;
        float angleStep = autoAimTraceCount > 1 ? autoAimRadius / (autoAimTraceCount - 1) : 0f;

        float autoAimSmallestDist = Mathf.Infinity;
        Vector2 autoAimSmallestLoc = Vector2.zero;
        for (int i = 0; i < autoAimTraceCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 rayDir = Quaternion.Euler(0, 0, angle) * mouseDirection;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, autoAimRange, ignoreLayers);
            Debug.DrawRay(transform.position, rayDir * autoAimRange, Color.red, 1f);

            if (!hit)
            {
                continue;
            }

            if (hit.distance < autoAimSmallestDist)
            {
                autoAimSmallestDist = hit.distance;
                autoAimSmallestLoc = hit.point;
            }
        }

        return autoAimSmallestLoc;
    }

}
