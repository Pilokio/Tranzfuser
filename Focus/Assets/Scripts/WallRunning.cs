using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [SerializeField] private float upForce;
    [SerializeField] private float sideForce;

    private Rigidbody rb;

    // public Transform head;
    // public Transform cam;

    private float minAngle = 0.0f;
    private float maxAngle = 30.0f;

    private float WallRunRoteLeft = -30.0f;
    private float WallRunRoteRight = 30.0f;

    // Is the player touching the wall on the left or the right?
    private bool isLeft;
    private bool isRight;

    private float distFromLeft;
    private float distFromRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        WallChecker();

    }

    // Checks if the player is colliding with a wall
    private void WallChecker()
    {
        RaycastHit leftWall;
        RaycastHit rightWall;

        if (Physics.Raycast(transform.position, transform.right, out rightWall))
        {
            distFromRight = Vector3.Distance(transform.position, rightWall.point);
            if (distFromRight < 3f && rightWall.transform.tag == "RunnableWall")
            {
                float angle = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, WallRunRoteRight, Time.deltaTime);
                Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, angle);
                isRight = true;
                isLeft = false;
            }
        }

        if (Physics.Raycast(transform.position, -transform.right, out leftWall))
        {
            distFromLeft = Vector3.Distance(transform.position, leftWall.point);
           
            if (distFromLeft < 3f && leftWall.transform.tag == "RunnableWall")
            {
                float angle = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, WallRunRoteLeft, Time.deltaTime);
                Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, angle);
                isRight = false;
                isLeft = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            rb.useGravity = false;
            Debug.Log("Wall Running!");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            if (Input.GetKey(KeyCode.Space))
            {
                //if (isLeft)
                //{
                //    rb.AddForce(Vector3.up * upForce * Time.deltaTime);
                //    rb.AddForce(transform.right * sideForce * Time.deltaTime);
                //}
                //if (isRight)
                //{
                //    rb.AddForce(Vector3.up * upForce * Time.deltaTime);
                //    rb.AddForce(-transform.right * sideForce * Time.deltaTime);
                //}
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            rb.useGravity = true;
         
               //float angle = Mathf.LerpAngle(Camera.main.transform.eulerAngles.z, 0.0f, Time.deltaTime);
               // Camera.main.transform.eulerAngles = new Vector3(0f, 0.0f, angle);
            
        }
    }
}
