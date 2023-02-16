using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Keybinds")]

    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement")]

    public float moveSpeed = 5;
    private float maxSpeed = 7;
    public Transform orientation;
    public float playerHeight;
    public LayerMask Ground;
    public float groundDrag;
    private bool grounded;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    [Header("Crouching")]
    public float crouchSpeed = 3f;
    public float crouchYScale;
    public float crouchYStart;

    [Header("Sliding")]
    private bool sliding = false;

    [Header("Jumping")]

    public float jumpForce;
    public float jumpCooldown = 0.2f;
    public float airMultiplier = 0.1f;
    private bool canJump = true;

    [Header("Dash")]
    public float dashForce;
    public float dashCooldown = 1f;
    private bool canDash = true;

    [Header("Slopes")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitSlope = false;

    [Header("Unity Setup")]

    public Rigidbody rb;

    public PlayerState state;

    public enum PlayerState
    {
        onGround,
        inAir,
        dashing,
        crouched,
        sliding
    }
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        crouchYStart = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInput();
        ControlSpeed();

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, Ground);
        
        if (grounded == true)
        {
            rb.drag = groundDrag;

        } else
        {
            rb.drag = 0;
        }

        if(Input.GetKey(crouchKey) && (horizontalInput != 0 || verticalInput !=0 ) && rb.velocity.x > crouchSpeed + 1)
        {
            StartSliding();
        } else 
        Debug.Log(rb.velocity);
    }

    void FixedUpdate()
    {
        Move();
        if(sliding == true)
        {
            SlidingMovement();
        }
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //Jumping

        if(Input.GetKey(jumpKey) && canJump == true && grounded == true)
        {
            canJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Dashing

        if (Input.GetKey(dashKey) && canDash == true)
        {
            rb.velocity = transform.forward * dashForce;
            canDash = false;
            maxSpeed = 10;
            groundDrag = 0;
            Invoke(nameof(ResetDash), dashCooldown);
        }

        //Crouching

        if(Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if(grounded)
            {
                rb.AddForce(Vector3.down * 0.1f, ForceMode.Impulse);
            }
        } else
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYStart, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        if(grounded)
        {
            state = PlayerState.onGround;
            maxSpeed = 10;
        } else if (!grounded)
        {
            state = PlayerState.inAir;
            maxSpeed = 20;
        } else if(canDash == true && Input.GetKey(dashKey))
        {
            state = PlayerState.dashing;
            maxSpeed = 50;
        }

        if(Input.GetKey(crouchKey) && moveSpeed < crouchSpeed + 1)
        {
            state = PlayerState.crouched;
            rb.drag = groundDrag;
        } else if (Input.GetKey(crouchKey) && moveSpeed > crouchSpeed + 1)
        {
            state = PlayerState.sliding;
            rb.drag = 0;
        }
    }

    private void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(grounded == true && !sliding)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        if (grounded == false)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if(state == PlayerState.crouched)
        {
            rb.AddForce(moveDirection.normalized * crouchSpeed * 10f, ForceMode.Force);
        }

        if(sliding == true)
        {
            rb.AddForce(moveDirection.normalized * 2f, ForceMode.Force);
        }

        if(OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Force);
            }
            
        }

        rb.useGravity = !OnSlope();

    }

    private void ControlSpeed()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVel.magnitude > maxSpeed && grounded == true)
        {
            Vector3 limitVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }

        if (flatVel.magnitude > maxSpeed && grounded == false)
        {
            Vector3 limitVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }

        if(OnSlope() && exitSlope == false)
        {
            if(rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
    }

    private void Jump()
    {
        exitSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void StartSliding()
    {
        sliding = true;
    }

    private void SlidingMovement()
    {

    }

    private void EndSlide()
    {
        sliding = false;
    }

    private void ResetJump()
    {
        canJump = true;
        exitSlope = false;
    }

    private void ResetDash()
    {
        canDash = true;
        groundDrag = 5;

    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
