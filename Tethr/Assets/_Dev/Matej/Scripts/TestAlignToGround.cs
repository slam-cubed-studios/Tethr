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
            transform.AlignToGround(Mathf.Infinity, groundLayer);
        }
        else
        {
            transform.AlignToGround2D(Mathf.Infinity, groundLayer);
        }
    }
    private void Update()
    {
        
    }
}
