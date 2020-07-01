using UnityEngine;
using Chronos;

public class WallRunning : MonoBehaviour
{
    [SerializeField] private float upForce;
    [SerializeField] private float sideForce;
    //The minimum distance for the wall run to engage
    [SerializeField] float MinWallRunDistance = 3.0f;

    private Rigidbody rb;

    private float WallRunRoteLeft = -30.0f;
    private float WallRunRoteRight = 30.0f;

    // Is the player touching the wall on the left or the right?
    private bool isLeft;
    private bool isRight;

    private float distFromLeft;
    private float distFromRight;

    float defaultMass = 1.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        defaultMass = rb.mass;
    }


    public void RestoreCamera()
    {
        //If not touching a wall, and the camera z-rotation is not 0 then lerp back
        if (!isRight && !isLeft && Camera.main.transform.localEulerAngles.z != 0.0f)
        {
            float angle = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, 0.0f, Time.deltaTime * 5);
            Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, angle);

            //Snap back to zero when the rotation is "close enough"
            if (Camera.main.transform.localEulerAngles.z > -0.5f && Camera.main.transform.localEulerAngles.z < 0.5f)
            {
                Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, 0.0f);
            }
        }
    }

    // Checks if the player is colliding with a wall
    public void WallChecker()
    {
        RaycastHit leftWall;
        RaycastHit rightWall;

        if (Physics.Raycast(transform.position, transform.right, out rightWall))
        {
            distFromRight = Vector3.Distance(transform.position, rightWall.point);
            if (distFromRight < MinWallRunDistance && rightWall.transform.tag == "RunnableWall")
            {
                float angle = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, WallRunRoteRight, Time.deltaTime * 5);
                Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, angle);
                isRight = true;
                isLeft = false;
            }
        }
        else
        {
            isRight = false;
        }

        if (Physics.Raycast(transform.position, -transform.right, out leftWall))
        {
            distFromLeft = Vector3.Distance(transform.position, leftWall.point);

            if (distFromLeft < MinWallRunDistance && leftWall.transform.tag == "RunnableWall")
            {
                float angle = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, WallRunRoteLeft, Time.deltaTime * 5);
                Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, angle);
                isRight = false;
                isLeft = true;
            }
        }
        else
        {
            isLeft = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            GetComponent<Timeline>().rigidbody.useGravity = false;

            //rb.useGravity = false;
            //rb.mass = 0;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            GetComponent<Timeline>().rigidbody.useGravity = true;

            //rb.mass = defaultMass;
            //  rb.useGravity = true;
        }
    }

    //TODO move this calculation into seperate function for use in the player manager
    //private void OnCollisionStay(Collision collision)
    //{
    //    if (collision.transform.CompareTag("RunnableWall"))
    //    {
    //        if (Input.GetKey(KeyCode.Space))
    //        {
    //            //if (isLeft)
    //            //{
    //            //    rb.AddForce(Vector3.up * upForce * Time.deltaTime);
    //            //    rb.AddForce(transform.right * sideForce * Time.deltaTime);
    //            //}
    //            //if (isRight)
    //            //{
    //            //    rb.AddForce(Vector3.up * upForce * Time.deltaTime);
    //            //    rb.AddForce(-transform.right * sideForce * Time.deltaTime);
    //            //}
    //        }
    //    }
    //}


}
