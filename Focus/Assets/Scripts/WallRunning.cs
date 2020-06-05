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
            if (distFromRight < 3f)
            {
                isRight = true;
                isLeft = false;
            }
        }

        if (Physics.Raycast(transform.position, -transform.right, out leftWall))
        {
            distFromLeft = Vector3.Distance(transform.position, leftWall.point);
            if (distFromLeft < 3f)
            {
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

            if (isRight)
            {
                // FIXME Need to find out why player is not tilting when wall running (also in OnCollisionExit function)
                float angle = Mathf.LerpAngle(minAngle, maxAngle, Time.time);
                Camera.main.transform.eulerAngles = new Vector3(0f, angle, 0f);
            }
            if (isLeft)
            {
                float angle = Mathf.LerpAngle(minAngle, maxAngle, Time.time);
                Camera.main.transform.eulerAngles = new Vector3(0f, angle, 0f);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (isLeft)
                {
                    rb.AddForce(Vector3.up * upForce * Time.deltaTime);
                    rb.AddForce(transform.right * sideForce * Time.deltaTime);
                }
                if (isRight)
                {
                    rb.AddForce(Vector3.up * upForce * Time.deltaTime);
                    rb.AddForce(-transform.right * sideForce * Time.deltaTime);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            rb.useGravity = true;
            Camera.main.transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}
