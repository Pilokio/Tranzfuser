using UnityEngine;

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
        Debug.Log("Restoring camera");

        if(MainCamera.transform.localEulerAngles.y != 0 && !Restored)
        {
            Debug.Log("normalising camera");          
            transform.rotation = Quaternion.Euler(transform.localEulerAngles.x, MainCamera.transform.localEulerAngles.y, transform.localEulerAngles.z);

            MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, 0.0f, MainCamera.transform.localEulerAngles.z);
            Restored = true;
        }
    }

    public void TiltCamera()
    {
        Restored = false;
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
        Debug.Log("Tilting camera");
    }

    Vector3 JumpOffDirection = new Vector3();

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("RunnableWall"))
        {
            JumpOffDirection = collision.contacts[0].normal;
            GetComponent<PlayerController>().SetIsWallRunning(true);

            //transform.rotation = collision.gameObject.transform.rotation;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("RunnableWall"))
        {
            GetComponent<PlayerController>().SetIsWallRunning(false);
        }
    }
    private void Update()
    {
        if(GetComponent<PlayerController>().IsWallRunning && CustomInputManager.GetButtonDown("ActionButton1"))
        {
            rb.velocity += wallJumpForce * JumpOffDirection;
        }
    }

}
