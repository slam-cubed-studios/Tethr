using UnityEngine;

/// <summary>
/// TEST SCRIPT
/// </summary>
public class TestAlignToGround : MonoBehaviour
{
    [SerializeField] private bool is3D = false;
    [SerializeField] private LayerMask groundLayer;

    private void Start()
    {
        if (is3D)
        {
            transform.AlignToGround(Mathf.Infinity, groundLayer, new Vector3(0.0f, 0.5f));
        }
        else
        {
            transform.AlignToGround2D(Mathf.Infinity, groundLayer, new Vector2(0.0f, 0.5f));
        }
    }
    private void Update()
    {
        
    }
}
