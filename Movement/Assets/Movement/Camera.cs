using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public float sensitivityX = 1000;
    public float sensitivityY = 1000;
    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;
    private float zRotation = 0;
    public Transform orientation;


    private PlayerMover pc;
    private WallRunning wr;
    private float tiltRot;
    private float rotTime = 0;
    public float camTilt;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pc = GetComponentInParent<PlayerMover>();
        wr = GetComponentInParent<WallRunning>();
    }
    float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    void Update()
    {

        if (pc.wallRunning && wr.wallLeft)
        {
            rotTime += Time.deltaTime;
            if (tiltRot > -camTilt)
            {
                tiltRot += Mathf.Lerp(0, -camTilt, rotTime);
            }

        }
        else if (pc.wallRunning && wr.wallRight)
        {
            rotTime += Time.deltaTime;
            if (tiltRot < camTilt)
            {

                tiltRot += Mathf.Lerp(0, camTilt, rotTime);
            }

        }
        else
        {
            rotTime = 0f;

            tiltRot = 0f;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //GetComponent<Camera>().fieldOfView = map(gameObject.transform.parent.GetComponent<Rigidbody>().velocity.magnitude, 0, 50, 60, 80);

        zRotation = tiltRot;

        mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
        mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}