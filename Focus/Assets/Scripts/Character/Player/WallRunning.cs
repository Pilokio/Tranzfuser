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

  
   
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("RunnableWall"))
        {
            StopAllCoroutines();
            IsJumping = false;

            //Use the wall normal to determine the jump off direction
            //This will not change regardless of move direction
            JumpOffDirection = collision.contacts[0].normal;
            //Adds some upward force to the wall jump
            JumpOffDirection += (Vector3.up * UpwardBias);

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


            //Vector3 Direction = (transform.position - collision.gameObject.transform.position).normalized;
            //Vector3 LeftorRight = Vector3.Cross(transform.forward, Direction);
            //float dot = Vector3.Dot(LeftorRight, transform.up);
            ////Debug.Log("Dot = " + dot);

            Vector3 relativePoint = MainCamera.transform.InverseTransformPoint(collision.transform.position);

            //if (relativePoint.x > 0f)
            //{
            //    Debug.Log("To the right");
            //    StartCoroutine(TiltCamera(35));
            //}
            //else if (relativePoint.x < 0f)
            //{
            //    Debug.Log("To the left");
            //    StartCoroutine(TiltCamera(-35));
            //}
            //else
            //{
            //    Debug.Log("Neither left nor right. Defaulting to Right");
            //    StartCoroutine(TiltCamera(-35));
            //}

            //Set the player wall running bool to true
            GetComponent<PlayerController>().SetIsWallRunning(true);

        }
    }



    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("RunnableWall"))
        {
            StopAllCoroutines();
            WallRunCleanup();
            //StartCoroutine(Cleanup());

            if (IsJumping)
                Invoke("StopWallRun",0.25f);
            else
                Invoke("StopWallRun", 0.0f);


        }
    }

    public void JumpOffWall()
    {
        rb.velocity += wallJumpForce * JumpOffDirection;
        IsJumping = true;
    }
 
  

    void StopWallRun()
    {
        GetComponent<PlayerController>().SetIsWallRunning(false);
    }

    IEnumerator TurnPlayer(float TargetAngle)
    {
        IsTurning = true;

        while (transform.localEulerAngles.y != TargetAngle)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.localEulerAngles.y, TargetAngle)) <= MinAngle)// || !IsTurning)
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

            yield return new WaitForEndOfFrame();
        }
    }


    void WallRunCleanup()
    {
        float angle = MainCamera.transform.localEulerAngles.y;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, angle, transform.rotation.eulerAngles.z);
        Invoke("FinalCheck", 0.25f);
    }

    //IEnumerator Cleanup()
    //{
    //    float angle = MainCamera.transform.localEulerAngles.y;
    //    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, angle, transform.rotation.eulerAngles.z);

    //    yield return new WaitForEndOfFrame();
    //    FinalCheck();
    //    //Invoke("FinalCheck", 0.5f);
    //}


    void FinalCheck()
    {
        if(MainCamera.transform.localEulerAngles.y != 0.0f)
        {
            //Debug.LogWarning("Standard Wall Run Cleanup failed to correct camera rotation. Backup fix in place to prevent movement errors");
            MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, 0.0f, MainCamera.transform.localEulerAngles.z);
        }
    }
}