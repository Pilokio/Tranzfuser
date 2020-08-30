using UnityEngine;
using System.Collections;
public class WallRunning : MonoBehaviour
{

    private Rigidbody rb;
    private PlayerController _PlayerController;
    private Camera MainCamera;

    [SerializeField] float wallJumpForce = 27.0f;
    [SerializeField] float TurnSpeed = 5.0f;
    [SerializeField] float TiltSpeed = 5.0f;
    [SerializeField] float MinAngle = 1.0f;
    public bool IsTurning = false;
    bool IsJumping = false;
    Vector3 JumpOffDirection = new Vector3();
    public float UpwardBias = 0.5f;


    private void Start()
    {
        MainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        _PlayerController = GetComponent<PlayerController>();
    }
    [SerializeField] LayerMask WallLayermask;

    bool WallToRight = false;
    bool WallToLeft = false;

    private void OnCollisionEnter(Collision collision)
    {
        WallToRight = false;
        WallToLeft = false;

        if (collision.gameObject.CompareTag("RunnableWall"))
        {
            StopAllCoroutines();
            IsJumping = false;

            //Use the wall normal to determine the jump off direction
            //This will not change regardless of move direction
            JumpOffDirection = collision.contacts[0].normal;
            //Adds some upward force to the wall jump
            JumpOffDirection += (Vector3.up * UpwardBias);


            if(Physics.Raycast(transform.position, transform.right, 1.0f, WallLayermask))
            {
                Debug.Log("Right");
                WallToRight = true;
                StartCoroutine(TiltCamera(35));
                GetComponent<PlayerMovement>().SetCameraClamps(-45, 0);
            }
            
            if (Physics.Raycast(transform.position, -transform.right, 1.0f, WallLayermask))
            {
                Debug.Log("Left");
                WallToLeft = true;
                StartCoroutine(TiltCamera(-35));
                GetComponent<PlayerMovement>().SetCameraClamps(0, 45);
            }


            //Determine which direction the player is looking at (ie where they are moving) relative to the wall's rotation

            //Calculate the difference between the player transform's y-rotation,
            //and the angle for moving along the positive and negative directions of the wall
            float distPositive = Mathf.DeltaAngle(transform.localEulerAngles.y, collision.transform.localEulerAngles.y);
            float distNegative = Mathf.DeltaAngle(transform.localEulerAngles.y, collision.transform.localEulerAngles.y + 180);

            //Use the absolute values as negatives are irrelevant here
            distPositive = Mathf.Abs(distPositive);
            distNegative = Mathf.Abs(distNegative);


            //Depending on which direction (+ or -) the player is mostly facing
            //Assign the TargetRote value to the desired rotation for the player object
            float TargetRote = transform.localEulerAngles.y;

            if (distPositive < distNegative)
            {
                TargetRote = collision.gameObject.transform.localEulerAngles.y;
            }
            else if (distPositive > distNegative)
            {
                TargetRote = collision.gameObject.transform.localEulerAngles.y + 180;
            }
            else
            {
                //This case is for if the player walks directly into the wall
                //where the resulting angle differences would be identical
                //In this event the positive direction will be used as default, however in gameplay
                //it is expected that this will be unlikely to occur unless actively attempting to trigger this
                TargetRote = collision.gameObject.transform.localEulerAngles.y;
            }


            //Start the coroutine to turn the player object to the target rotation
            StartCoroutine(TurnPlayer(TargetRote));

            //Set the player wall running bool to true
            GetComponent<PlayerController>().SetIsWallRunning(true);

        }
    }



    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("RunnableWall"))
        {
            StopAllCoroutines();


            StartCoroutine(TiltCamera(0));


            //MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, 0.0f, MainCamera.transform.localEulerAngles.z);


            StartCoroutine(RestoreCamera());


            GetComponent<PlayerController>().SetIsWallRunning(false);


        }
    }

    public void JumpOffWall()
    {
        rb.velocity += wallJumpForce * JumpOffDirection;
    }

    IEnumerator TurnPlayer(float TargetAngle)
    {
        IsTurning = true;

        while (transform.localEulerAngles.y != TargetAngle)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.localEulerAngles.y, TargetAngle)) <= MinAngle)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, TargetAngle, transform.rotation.eulerAngles.z);
            }
            else
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Mathf.LerpAngle(transform.rotation.eulerAngles.y, TargetAngle, Time.deltaTime * TurnSpeed), transform.rotation.eulerAngles.z);
            }

            yield return new WaitForEndOfFrame();
        }

        IsTurning = false;
    }

    IEnumerator TiltCamera(float TargetAngle)
    {
        while (MainCamera.transform.localEulerAngles.z != TargetAngle)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(MainCamera.transform.localEulerAngles.z, TargetAngle)) <= 1.0f)
            {
                MainCamera.transform.rotation = Quaternion.Euler(MainCamera.transform.rotation.eulerAngles.x, MainCamera.transform.rotation.eulerAngles.y, TargetAngle);
            }
            else
            {
                MainCamera.transform.rotation = Quaternion.Euler(MainCamera.transform.rotation.eulerAngles.x, MainCamera.transform.rotation.eulerAngles.y, Mathf.LerpAngle(MainCamera.transform.rotation.eulerAngles.z, TargetAngle, Time.deltaTime * TiltSpeed));
            }

            yield return null;
        }
    }

    IEnumerator RestoreCamera()
    {
        MainCamera.transform.Rotate(new Vector3(0, Mathf.DeltaAngle(MainCamera.transform.localEulerAngles.y, 0.0f)));

        yield return null;

        //while(MainCamera.transform.localEulerAngles.y != 0.0f)
        //{
        //    if (Mathf.Abs(Mathf.DeltaAngle(MainCamera.transform.localEulerAngles.y, 0.0f)) <= 5.0f)
        //    {
        //        Debug.Log("Close Enough");
        //        MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.rotation.eulerAngles.x, 0.0f, MainCamera.transform.rotation.eulerAngles.z);
        //        break;
        //    }
        //    else
        //    {
        //        MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.rotation.eulerAngles.x, Mathf.LerpAngle(MainCamera.transform.rotation.eulerAngles.y, 0.0f, Time.deltaTime * TiltSpeed), MainCamera.transform.rotation.eulerAngles.z);
        //    }

        //    yield return null;
        //}
    }
}