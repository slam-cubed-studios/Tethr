using UnityEngine;

namespace Tethr.PathVisualiser.Examples
{
    public class TargetVisualisationExample : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void OnDrawGizmosSelected()
        {
            if (target)
            {
                PathVisualiserUtility.DrawTargetLine(transform.position, target.position);
            }
        }
    }
}
