using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    /*
      As a physics based character this will need to reference the Rigidbody (RB), the sprite for visuals and the player controller

        Standard Movement
        Achieved by applying a set force on the player left or right non local to the player's orientation.
        A resistance force should also always be applied unless the player is on a frictionless surface or airborne (otherwise just a reduced force)

        Instead it might be better to give the player values based on their "terrain state"
        Sliding reduces friction but the player can't increase or decrease speed
        Running has everything at a standard 
        Jumping/Falling allows for a small amount of aerial strafing but otherwise has no friction

        To limit the players speed a force is always applied in the opposite direction of their movement 
        
        A raycast will be needed to see whether or not the player can jump and what surface they are currently on



        Logic:

        Functions:
            CalcualateRun();
            CalculateJump();
            
            MoveCharacter();


        Calculate Run:
        Vars: 
            float moveClamp
            float deAcceleration
            float Input.X
            bool isGrounded

        Func:
            CalcGroundedMove{
                If(Input.X != 0) {
                 currentSpeed += Input.X * accelaration * Time.deltaTime

                currentSpeed = Clamp(currentspeed, -moveclamp, moveclamp)
                }
            
     */


    [SerializeField] private float accelaration = 5f;
    [SerializeField] private float deAccelaration = 5f;
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float moveClamp = 20f;

     private float inputDirection = 0;
    [SerializeField] private Rigidbody2D rb;

    public InputActionReference move;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TryGetComponent<Rigidbody2D>(out rb);
    }

    // Update is called once per frame
    void Update()
    {
        inputDirection = move.action.ReadValue<Vector2>().x;
        CalcGroundedMove();
        Move();
    }

    void CalcGroundedMove()
    {
        if(inputDirection != 0)
        {
            currentSpeed += inputDirection * accelaration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -moveClamp, moveClamp);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deAccelaration * Time.deltaTime);
        }
        
    }

    void Move()
    {
        rb.linearVelocityX = currentSpeed;
    }
}
