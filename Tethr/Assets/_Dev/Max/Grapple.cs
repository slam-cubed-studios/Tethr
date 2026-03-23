using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class Grapple : MonoBehaviour
{
    [SerializeField] private float grappleStrength = 1f;
    [SerializeField] private GameObject ropeObj;
    private int ignoreLayers;
    private SpringJoint2D distanceJoint;
    private Vector2 anchorPoint;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        distanceJoint = GetComponent<SpringJoint2D>();
    }

    void Start()
    {
        int layerToIgnore = LayerMask.NameToLayer("Player");
        ignoreLayers = ~(1 << layerToIgnore);
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue() - new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 mouseDir = mousePos.normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, mouseDir, 100f, ignoreLayers);
            if (hit.distance == 0)
            {
                return;
            }
            Debug.DrawLine(transform.position, hit.point, Color.red, 2f);

            distanceJoint.connectedAnchor = hit.point;
            //distanceJoint.distance = hit.distance;

            anchorPoint = hit.point;
        }

        RopeVisual();

        Vector2 anchorDir = (anchorPoint - (Vector2)transform.position).normalized;
        Vector2 veloDir = rb.linearVelocity.normalized;
        Vector2 midPoint = (anchorDir + veloDir).normalized;

        Debug.DrawRay(transform.position, midPoint, Color.green, 2f);

        rb.AddForce(anchorDir * grappleStrength);
    }

    void RopeVisual()
    {
        Vector2 direction = anchorPoint - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        ropeObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        //float distance = (anchorPoint - (Vector2)transform.position).magnitude;
        ropeObj.transform.localScale = new Vector3(Vector2.Distance(anchorPoint, transform.position), 1, 1);
    }
}
