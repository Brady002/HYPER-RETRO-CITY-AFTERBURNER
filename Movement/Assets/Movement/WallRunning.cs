using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{

    [Header("Wallrunning")]
    public LayerMask isGround;
    public LayerMask isWall;
    public float wallRunForce;
    public float maxWallRunTime;
    private float wallRunTime;

    [Header("Inputs")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerController pc;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<PlayerController>();
        minHeight = pc.playerHeight;
    }

    void Update()
    {
        CheckForWall();
        StateMachine();
    }

    void FixedUpdate()
    {
        if(pc.wallRunning)
        {
            WallRunMovement();
        }
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, isWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, isWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minHeight, isGround);
    }
    

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //State 1
        if(wallLeft || wallRight && verticalInput > 0 && AboveGround() == true)
        {
            Debug.Log(AboveGround());
            if(!pc.wallRunning && verticalInput > 0)
            {
                StartWallRun();
                
            }
        } else
        {
            if(pc.wallRunning || verticalInput < 1)
            {
                StopWallRun();
            }
        }
    }

    private void StartWallRun()
    {
        pc.wallRunning = true;
    }

    private void WallRunMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if(Input.GetKey(pc.jumpKey))
        {
            //exitWallRun = true;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(wallNormal * (pc.jumpForce/2) + transform.up * (pc.jumpForce/2), ForceMode.Impulse);
        }
    }

    private void StopWallRun()
    {
        pc.wallRunning = false;
        rb.useGravity = true;
    }
}
