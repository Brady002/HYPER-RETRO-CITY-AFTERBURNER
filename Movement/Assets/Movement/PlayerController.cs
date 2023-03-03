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

    public float baseMoveSpeed;
    private float moveSpeed;
    private float maxSpeed;
    public float wallRunSpeed;
    public Transform orientation;
    public float playerHeight;
    public LayerMask Ground;
    public float normalMaxSpeed = 30f;
    public float groundDrag;
    public bool grounded;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private bool exitWallRun = false;

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

    [Header("Energy")]
    public float energy;
    public float maxEnergy;

    [Header("Unity Setup")]

    public Rigidbody rb;

    public PlayerState state;

    public enum PlayerState
    {
        onGround,
        inAir,
        dashing,
        crouched,
        sliding,
        wallRunning
    }

    public bool wallRunning;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        crouchYStart = transform.localScale.y;

        maxSpeed = normalMaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {

        moveSpeed = baseMoveSpeed + (energy * 1f);

        Debug.Log(GetSlopeMoveDirection());

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, Ground); //Checks to see if player is on ground

        PlayerInput();
        ControlSpeed();
        StateHandler();

        if (grounded == true && !Input.GetKey(crouchKey)) //Drag is used to slow and stop player on ground. Set to 0 in air for momentum.
        {
            rb.drag = groundDrag;

        }
        else
        {
            rb.drag = 0;
        }

        if (Input.GetKey(crouchKey) && (horizontalInput != 0 || verticalInput != 0) && !sliding) //Attempt at sliding. Not working currently
        {
            StartSliding();

        }

        if (sliding == true)
        {
            SlidingMovement();
        }
        else
        {
            Move();
        }
    }

    void FixedUpdate()
    {


    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //Jumping

        if (Input.GetKey(jumpKey) && canJump == true && grounded == true)
        {
            canJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Dashing

        if (Input.GetKey(dashKey) && canDash == true && energy > 0)
        {

            rb.velocity = transform.forward * dashForce;
            canDash = false;
            maxSpeed = 50;
            groundDrag = 0;
            Invoke(nameof(EndDash), 0.3f);
        }

        //Crouching

        if (Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (grounded) //Adds a downward force if on ground so player doesn't appear to 'fall' when crouching. 
            {
                rb.AddForce(Vector3.down * 0.1f, ForceMode.Impulse);
            }
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYStart, transform.localScale.z);
        }
    }

    private void StateHandler() //Dictates speed variables. Mess with these if you want to change values.
    {


        //Grounded and Air


        if (grounded)
        {
            state = PlayerState.onGround;
            maxSpeed = normalMaxSpeed;
        }
        else if (wallRunning)
        {
            state = PlayerState.wallRunning;
            moveSpeed = wallRunSpeed;
            maxSpeed = normalMaxSpeed + 2f;
            if (rb.velocity.magnitude > 20)
            {
                float mult = 20 / rb.velocity.magnitude;
                Vector3 update = new Vector3(rb.velocity.x * mult, rb.velocity.y * mult, rb.velocity.z * mult);
                rb.velocity = update;
            }
        }

        else if (!grounded)
        {

            state = PlayerState.inAir;
            maxSpeed = 50;
        }
        else if (canDash == true && Input.GetKey(dashKey) && energy > 0)
        {

            state = PlayerState.dashing;
            maxSpeed = 50;
        }

        //Crouching & Sliding

        if (Input.GetKey(crouchKey) && moveSpeed < crouchSpeed + 1)
        {
            state = PlayerState.crouched;
            rb.drag = groundDrag;
        }
        else if (Input.GetKey(crouchKey) && moveSpeed > crouchSpeed + 1)
        {
            //state = PlayerState.sliding;
            //rb.drag = 0;
        }


    }

    private void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded == true && !sliding)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        if (grounded == false)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (state == PlayerState.crouched)
        {
            rb.AddForce(moveDirection.normalized * crouchSpeed * 10f, ForceMode.Force);
        }

        if (sliding == true)
        {
            rb.AddForce(moveDirection.normalized * 2f, ForceMode.Force);
        }

        if (OnSlope()) //Normalize the movement direction on a slope.
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 2f, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Force);
            }

        }

        rb.useGravity = !OnSlope(); //Turns gravity off if player is on slope to avoid unintentional sliding

    }

    private void ControlSpeed() //Keeps player speed capped
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > maxSpeed && wallRunning)
        {
            Vector3 limitVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }


        //Grounded

        if (flatVel.magnitude > maxSpeed && grounded)
        {
            Vector3 limitVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }

        //In Air

        if (flatVel.magnitude > maxSpeed && !grounded)
        {
            Vector3 limitVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }

        //Grounded on slope

        if (OnSlope() && !exitSlope)
        {
            /*if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }*/
        }
    }

    private void Jump()
    {
        if (wallRunning)
        {

        }
        else
        {
            exitSlope = true;

            rb.velocity = new Vector3(rb.velocity.x * 1.4f, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

    }

    private void StartSliding()
    {
        sliding = true;
        rb.mass = 100f;
    }

    private void SlidingMovement()
    {
        horizontalInput = 0;
        verticalInput = 0;
        if (rb.velocity.x == 0 && rb.velocity.z == 0)
        {
            EndSlide();
        }
        if (Input.GetKeyUp(crouchKey))
        {
            EndSlide();
        }
    }

    private void EndSlide()
    {
        rb.mass = 1f;
        sliding = false;
    }

    private void ResetJump()
    {
        canJump = true;
        exitSlope = false;
    }

    private void EndDash()
    {
        groundDrag = 5f;
        energy--;
        maxSpeed = normalMaxSpeed;
        Invoke(nameof(ResetDash), dashCooldown);
    }

    private void ResetDash()
    {
        canDash = true;
    }

    private bool OnSlope() //Checks to see if player is on slope.
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
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
