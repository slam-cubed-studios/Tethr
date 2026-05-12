using Tethr;
using UnityEngine;

public class TestScriptMovement : MonoBehaviour
{
    private MovementComponent movementComponent;

    private void Awake()
    {
        movementComponent = GetComponent<MovementComponent>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(HasReachedDestination), 0.0f, 0.25f);
    }

    private void Update()
    {
        movementComponent.MoveToDestination();
    }

    private void HasReachedDestination()
    {
        if (movementComponent.HasReachedDestination())
        {
            if (movementComponent.GetMovementType() != MovementType.Manual)
            {
                movementComponent.UpdateDestination();
            }
            else
            {
                Vector3 offset = new(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f));
                movementComponent.SetDestination(transform.position + offset);
            }
        }
    }
}
