using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxZoomOutMultiplier = 1.5f;
    [SerializeField] private float maxZoomSpeed;

    private float initialCamFov;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        initialCamFov = playerCamera.orthographicSize;
        print(initialCamFov);
    }

    // Update is called once per frame
    void Update()
    {
        print(rb.linearVelocity.magnitude);
        //float lerpValue = Mathf.Clamp01(rb.linearVelocity.magnitude / maxZoomSpeed);
        playerCamera.orthographicSize = Mathf.Lerp(initialCamFov, initialCamFov * maxZoomOutMultiplier, rb.linearVelocity.magnitude / maxZoomSpeed);
    }
}
