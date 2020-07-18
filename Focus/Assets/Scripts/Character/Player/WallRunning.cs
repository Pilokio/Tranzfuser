using UnityEngine;
using System.Collections;
public class WallRunning : MonoBehaviour
{

    private Rigidbody rb;
    private PlayerController _PlayerController;
    private Camera MainCamera;

    public float wallJumpForce;


    private void Start()
    {
        MainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        _PlayerController = GetComponent<PlayerController>();
    }

    bool Restored = false;
    // This is used for when the player fires a hook while wall running
    public void RestoreCamera()
    {
        //if (MainCamera.transform.localEulerAngles.z != 0.0f)
        //{
        //    float angle = Mathf.LerpAngle(MainCamera.transform.localEulerAngles.z, 0.0f, Time.deltaTime * 5);
        //    MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, MainCamera.transform.localEulerAngles.y, angle);

        //    //Snap back to zero when the rotation is "close enough"
        //    if (MainCamera.transform.localEulerAngles.z > -0.5f && MainCamera.transform.localEulerAngles.z < 0.5f)
        //    {
        //        MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, MainCamera.transform.localEulerAngles.y, 0.0f);
        //    }
        //}

        //if(MainCamera.transform.localEulerAngles.y != 0 && !Restored)
        //{
        //    Debug.Log("normalising camera");          
        //    transform.rotation = Quaternion.Euler(transform.localEulerAngles.x, MainCamera.transform.localEulerAngles.y, transform.localEulerAngles.z);

        //    MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, 0.0f, MainCamera.transform.localEulerAngles.z);
        //    Restored = true;
        //}
    }

    public void TiltCamera()
    {
        //if(JumpOffDirection.x > 0)
        //{
        //    Debug.Log("Wall to left"); 
        //    if (MainCamera.transform.localEulerAngles.z != -45)
        //    {
        //        float angle = Mathf.LerpAngle(MainCamera.transform.localEulerAngles.z, -45.0f, Time.deltaTime * 5);
        //        MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, MainCamera.transform.localEulerAngles.y, angle);
        //    }
        //}


        //if(JumpOffDirection.x < 0)
        //{
        //    Debug.Log("Wall to right");
        //    if (MainCamera.transform.localEulerAngles.z != 45)
        //    {
        //        float angle = Mathf.LerpAngle(MainCamera.transform.localEulerAngles.z, 45.0f, Time.deltaTime * 5);
        //        MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, MainCamera.transform.localEulerAngles.y, angle);
        //    }
        //}
    }

    Vector3 JumpOffDirection = new Vector3();
    public float UpwardBias = 0.5f;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("RunnableWall"))
        {
            StopAllCoroutines();

            //Use the wall normal to determine the jump off direction
            //This will not change regardless of move direction
            JumpOffDirection = collision.contacts[0].normal;
            //Adds some upward force to the wall jump
            JumpOffDirection += (Vector3.up * UpwardBias);

            //Determine which direction the player is looking at (ie where they are moving) relative to the wall's rotation


            float distPositive = Mathf.DeltaAngle(transform.localEulerAngles.y, collision.transform.localEulerAngles.y);
            float distNegative = Mathf.DeltaAngle(transform.localEulerAngles.y, collision.transform.localEulerAngles.y + 180);

            distPositive = Mathf.Abs(distPositive);
            distNegative = Mathf.Abs(distNegative);

            float TargetRote = transform.localEulerAngles.y;

            if (distPositive < distNegative)
            {
                TargetRote = collision.gameObject.transform.localEulerAngles.y;
                //transform.rotation = Quaternion.Euler(transform.localEulerAngles.x, collision.gameObject.transform.localEulerAngles.y, transform.localEulerAngles.z);
            }
            else if (distPositive > distNegative)
            {
                TargetRote = collision.gameObject.transform.localEulerAngles.y + 180;
             //   transform.rotation = Quaternion.Euler(transform.localEulerAngles.x, collision.gameObject.transform.localEulerAngles.y + 180, transform.localEulerAngles.z);
            }
            else
            {
                //This case is for if the player walks directly into the wall
                //where the resulting angle differences would be identical
                //In this event the positive direction will be used as default, however in gameplay
                //it is expected that this will be unlikely to occur unless actively attempting to trigger this
                TargetRote = collision.gameObject.transform.localEulerAngles.y;
                //transform.rotation = Quaternion.Euler(transform.localEulerAngles.x, collision.gameObject.transform.localEulerAngles.y, transform.localEulerAngles.z);
            }

            StartCoroutine(TurnPlayer(TargetRote));


            Vector3 Direction = (transform.position - collision.gameObject.transform.position).normalized;
            Vector3 LeftorRight = Vector3.Cross(transform.forward, Direction);
            float dot = Vector3.Dot(LeftorRight, transform.up);

            if (dot > 0f)
            {
                Debug.Log("To the right");
                StartCoroutine(TiltCamera(35));
            }
            else if (dot < 0f)
            {
                Debug.Log("To the left");
                StartCoroutine(TiltCamera(-35));
            }
            else
            {
                Debug.Log("Neither left nor right. Defaulting to Right");
                StartCoroutine(TiltCamera(-35));
            }


            //Set the player wall running bool to true
            GetComponent<PlayerController>().SetIsWallRunning(true);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("RunnableWall"))
        {
            StopAllCoroutines();
            StartCoroutine(WallRunCleanup());
            GetComponent<PlayerController>().SetIsWallRunning(false);
        }
    }
    private void Update()
    {
        if (GetComponent<PlayerController>().IsWallRunning && CustomInputManager.GetButtonDown("ActionButton1"))
        {
            rb.velocity += wallJumpForce * JumpOffDirection;
        }
    }


    IEnumerator TurnPlayer(float TargetAngle)
    {
        while (transform.localEulerAngles.y != TargetAngle)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.localEulerAngles.y, TargetAngle)) <= 1.0f)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, TargetAngle, transform.rotation.eulerAngles.z);
            }
            else
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Mathf.LerpAngle(transform.rotation.eulerAngles.y, TargetAngle, Time.deltaTime), transform.rotation.eulerAngles.z);
            }
            yield return new WaitForEndOfFrame();
        }
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
                MainCamera.transform.rotation = Quaternion.Euler(MainCamera.transform.rotation.eulerAngles.x, MainCamera.transform.rotation.eulerAngles.y, Mathf.LerpAngle(MainCamera.transform.rotation.eulerAngles.z, TargetAngle, Time.deltaTime));
            }

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator WallRunCleanup()
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, MainCamera.transform.localEulerAngles.y, transform.localEulerAngles.z);
        yield return new WaitForEndOfFrame();
        MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, 0.0f, MainCamera.transform.localEulerAngles.z);
        StartCoroutine(TiltCamera(0.0f));
    }

}