using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensitivityX = 1000;
    public float sensitivityY = 1000;
    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;

    public float camTilt;
    private float tiltRotation;
    private float zRotation;
    private float rotTime = 0;

    public Transform orientation;

    private WallRunning wr;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        wr = GetComponentInParent<WallRunning>();
        zRotation = 0;
    }
    float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    void Update()
    {

        if (gameObject.transform.parent.GetComponent<PlayerController>().wallRunning && wr.wallLeft)
        {
            if (tiltRotation >= -camTilt)
            {
                if(rotTime < 1)
                {
                    rotTime += Time.deltaTime * 0.1f;
                }
                
                
                tiltRotation += Mathf.Lerp(0, -camTilt, rotTime);
                Debug.Log(rotTime);
            }

            

        }
        else if (gameObject.transform.parent.GetComponent<PlayerController>().wallRunning && wr.wallRight)
        {
            if (tiltRotation <= camTilt)
            {
                if (rotTime < 1)
                {
                    rotTime += Time.deltaTime * 0.1f;
                }

                tiltRotation += Mathf.Lerp(0, camTilt, rotTime);
                Debug.Log(rotTime);
            }
        }
        else
        {
            
            rotTime = 0;
            if(tiltRotation != 0)
            {

                if(tiltRotation < 0)
                {
                    tiltRotation += Mathf.Lerp(-camTilt)
                } else if (tiltRotation > 0)
                {

                }
            }
            tiltRotation = 0;
            
            
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        GetComponent<Camera>().fieldOfView = map(gameObject.transform.parent.GetComponent<Rigidbody>().velocity.magnitude, 0, 50, 60, 80);

        zRotation = tiltRotation;
        
        mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
        mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
