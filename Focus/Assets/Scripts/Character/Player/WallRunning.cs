using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [SerializeField] private float upForce;
    [SerializeField] private float sideForce;
    //The minimum distance for the wall run to engage
    [SerializeField] float MinWallRunDistance = 3.0f;

    private Rigidbody rb;

    PlayerController playerController;

    private float WallRunRoteLeft = -30.0f;
    private float WallRunRoteRight = 30.0f;

    public float wallJumpForce;

    // Is the player touching the wall on the left or the right?
    private bool isLeft;
    private bool isRight;

    public bool IsOccupied = false;
    bool PlayerOccupied = false;
    public bool isOnWall;

    private float distFromLeft;
    private float distFromRight;

    float defaultMass = 1.0f;

    //The player gameobject
    private GameObject player;

    private void Start()
    {
        //Use the player manager to access the player object from anywhere without editor assign
        //Useful for if the player is to be instantiated into the scene at runtime
        player = PlayerManager.Instance.Player;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        defaultMass = rb.mass;
    }

    // Restore camera when jumping off wall
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

    // This is used for when the player fires a hook while wall running
    public void WallRunCameraRestore()
    {
        float angle = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, 0.0f, Time.deltaTime * 5);
        Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, angle);

        //Snap back to zero when the rotation is "close enough"
        if (Camera.main.transform.localEulerAngles.z > -0.5f && Camera.main.transform.localEulerAngles.z < 0.5f)
        {
            Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, 0.0f);
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
            if (distFromRight < MinWallRunDistance && rightWall.transform.CompareTag("RunnableWall"))
            {
                isOnWall = true;

                float angle = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, WallRunRoteRight, Time.deltaTime * 5);
                Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, angle);
                isRight = true;
                isLeft = false;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    rb.velocity += wallJumpForce * Vector3.left;
                }
            }
        }
        else
        {
            isRight = false;
            isOnWall = false;
        }

        if (Physics.Raycast(transform.position, -transform.right, out leftWall))
        {
            distFromLeft = Vector3.Distance(transform.position, leftWall.point);

            if (distFromLeft < MinWallRunDistance && leftWall.transform.CompareTag("RunnableWall"))
            {
                isOnWall = true;

                float angle = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, WallRunRoteLeft, Time.deltaTime * 5);
                Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, angle);
                isRight = false;
                isLeft = true;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    rb.velocity += wallJumpForce * Vector3.right;
                }
            }
        }
        else
        {
            isLeft = false;
            isOnWall = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Check the collider belongs to the player and not the enemy etc
        if (collision.transform.tag == "Player" && !player.GetComponent<PlayerController>().IsWallRunning && !IsOccupied)
        {
            PlayerOccupied = true;
            IsOccupied = true;
            //Make the player turn to face the ladder
            Vector3 target = transform.rotation.eulerAngles;
            target.x = 0;
            target.z = 0;
            target.y += 180;
            player.transform.rotation = Quaternion.Euler(target);


            //Set the player to climb
            player.GetComponent<PlayerController>().SetIsWallRunning(true);
        }
    }

    private void Update()
    {
        if (PlayerOccupied)
        {
            if (CustomInputManager.GetAxisRaw("LeftStickHorizontal") < CustomInputManager.GetAxisNeutralPosition("LeftStickHorizontal"))
            {
                player.GetComponent<PlayerController>().SetIsWallRunning(false);
                IsOccupied = false;
                PlayerOccupied = false;
            }

            /*
            //If the player is close to the bottom of the ladder and still moving down
            if (Vector3.Distance(player.transform.position, LadderPath[2].position) < 0.5f && CustomInputManager.GetAxisRaw("LeftStickVertical") < CustomInputManager.GetAxisNeutralPosition("LeftStickVertical"))
            {
                IsOccupied = false;
                PlayerOccupied = false;
                //Stop climbing and resume normal movement
                player.GetComponent<PlayerController>().SetIsClimbing(false);
            }

            //If the player is near or above the top of the ladder and still moving up
            if (player.transform.position.y > LadderPath[1].position.y && CustomInputManager.GetAxisRaw("LeftStickVertical") > CustomInputManager.GetAxisNeutralPosition("LeftStickVertical"))
            {
                IsOccupied = false;
                PlayerOccupied = false;
                //Apply slight push to ensure they clear the top of the ladder
                player.transform.position = LadderPath[0].position;//+= Vector3.up + (player.transform.forward * 2.0f);
                                                                   //Stop climbing and resume normal movement
                player.GetComponent<PlayerController>().SetIsClimbing(false);
            }
            */
        }

        if (isOnWall)
        {
            player.GetComponent<PlayerController>().SetIsWallRunning(true);
        }
        else
        {
            player.GetComponent<PlayerController>().SetIsWallRunning(false);
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
