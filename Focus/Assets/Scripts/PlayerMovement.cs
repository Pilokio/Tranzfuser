using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    // Assingables
    //public Transform playerCam;
    //public Transform orientation;

    // Other
    private Rigidbody rb;

    // Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    // Movement
    public float moveSpeed;
    public float maxSpeed;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    // Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    // Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;

    // Input
    float x, y;
    bool jumping, sprinting, crouching;

    // Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;


    private Vector2 LookDirection = new Vector2();
    private Vector2 MoveDirection = new Vector2();
    private float XRotation = 0.0f;
    [SerializeField] float LookSensitivity = 100.0f;
    float YVelocity = 0.0f;
    [SerializeField] float JumpHeight = 100.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerScale = transform.localScale;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(CheckForBullets());
    }


    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        MyInput();
        Look();
    }

    // Find user input. Should put this in its own class
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }

    private void Movement()
    {
        // Below are the changes made to the core movement functionality
        ///////////////////////////////////////////////////////////////////////////////////////////////

        // Get the input axis and place them into 2 new vectors
        MoveDirection = new Vector2(-Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Clamp the manual gravity when touching the ground
        if (grounded && YVelocity < 0)
        {
            YVelocity = -2.0f;
        }

        // Create a Vector3 for the 3D move direction, making use of the inputs in relation to the transform
        Vector3 Move = (transform.right * MoveDirection.y) + (transform.forward * MoveDirection.x);

        // If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump();

        // Using the 3D move direction, apply the speed factor
        Move *= moveSpeed;
        Move.y = YVelocity;

        // Apply the final movement force to the rigidbody, making use of fixed delta time
        rb.AddForce(Move * Time.fixedDeltaTime);

        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y,
            Mathf.Clamp(rb.velocity.z, -maxSpeed, maxSpeed));

        rb.angularVelocity = new Vector3(Mathf.Clamp(rb.angularVelocity.x, -maxSpeed, maxSpeed), rb.angularVelocity.y,
            Mathf.Clamp(rb.angularVelocity.z, -maxSpeed, maxSpeed));

        ///////////////////////////////////////////////////////////////////////////////////////////////


        /* Old core movement functionality:

        // If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        // Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }
        Movement while sliding
        if (grounded && crouching) multiplierV = 0f;

        */


        ///////////////////////////////////////////////////////////////////////////////////////////////
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            // Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 3.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            // If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private float desiredX;
    private void Look()
    {
        LookDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * LookSensitivity * Time.deltaTime;

        //Update the x rotation of the camera based on the new Look Direction
        XRotation -= LookDirection.y;
        //Clamp to prevent full 360 rotation around the x-axis
        XRotation = Mathf.Clamp(XRotation, -90, 90);
        //Update the camera's x rotation
        Camera.main.transform.localRotation = Quaternion.Euler(XRotation, Camera.main.transform.localRotation.eulerAngles.y, 0);
        //Rotate the entire player container transform around the y-axis based on the look direction
        transform.Rotate(transform.up * LookDirection.x);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        // Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        // Counter movement
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        // Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        // Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        // Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            // FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        // Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }


    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }




    [Header("Bullet Cleanup Parameters")]
    [SerializeField] private float TimeForBulletCheck = 1.0f;
    [SerializeField] private int MaxBulletsInSceneCount = 10;


    //Coroutine for optimisation by removing bullets if there are too many in the scene
    //Currently just for debug as bullets will typically destroy themselves and spawn an impact decal or something?
    //Could be adapted for shell casings later on though?
    IEnumerator CheckForBullets()
    {
        while (true)
        {
            yield return new WaitForSeconds(TimeForBulletCheck);

            // Debug.Log("Checking Bullets");

            //Find all the bullets in the scene
            GameObject[] AllBulletsInScene = GameObject.FindGameObjectsWithTag("Bullet");

            //If there are more than the set number of bullets in the scene
            if (AllBulletsInScene.Length > MaxBulletsInSceneCount)
            {
                // Debug.Log("Too many Bullets. Removing some.");
                //Delete the bullets found up to the desired amount
                for (int i = 0; i < AllBulletsInScene.Length - MaxBulletsInSceneCount; i++)
                {
                    Destroy(AllBulletsInScene[i]);
                }
            }
        }
    }


}
