using Chronos;
using UnityEngine;

public class PlayerMovement : BaseBehaviour
{

    // Movement
    public float XRotation { get; set; }
    [Header("LookAt Parameters")]
    [SerializeField] float LookSensitivityKeyboardAndMouse = 100.0f;
    [SerializeField] float LookSensitivityController = 150.0f;

    [Header("Movement Parameters")]
    [SerializeField] float WalkSpeed = 5.0f;
    [SerializeField] float SprintSpeed = 10.0f;
    [SerializeField] float ClimbSpeed = 2.0f;
    [SerializeField] float WallRunSpeed = 10.0f;

    public bool IsSprinting { get; set; }

    private bool grounded = false;
    [SerializeField] float maxSlopeAngle = 35f;
    [SerializeField] LayerMask whatIsGround = new LayerMask();

    // Jumping Params
    [Space]
    [SerializeField] float jumpCooldown = 0.25f;
    [SerializeField] float jumpForce = 550f;
    private bool readyToJump = true;


    private bool cancellingGrounded = false;
    private Vector3 normalVector = Vector3.up;


    // Other
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Move(Vector3 MoveDirection)
    {
        if (MoveDirection.x < 0.25 && MoveDirection.x > -0.25)
            MoveDirection.x = 0.0f;

        if (MoveDirection.y < 0.25 && MoveDirection.y > -0.25)
            MoveDirection.y = 0.0f;

        // Create a Vector3 for the 3D move direction, making use of the inputs in relation to the transform
        Vector3 Move = (transform.right * MoveDirection.x) + (transform.forward * MoveDirection.y).normalized;

        if (IsSprinting)
        {
            Move *= SprintSpeed * Time.fixedDeltaTime;
        }
        else
        {
            Move *= WalkSpeed * Time.fixedDeltaTime;
        }

        if (Move.magnitude > 0.1f)
            rb.MovePosition(new Vector3(rb.position.x + Move.x, rb.position.y, rb.position.z + Move.z));

    }

    public void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            // Add jump forces
            rb.AddForce(Vector3.up * jumpForce * 3.5f);
            // rb.AddForce(normalVector * jumpForce * 0.5f);

            // If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void Look(Vector2 LookDirection)
    {
        float LookSensitivity = 0.0f;

        //LookDirection = new Vector2(Mathf.Clamp(LookDirection.x, -1, 1), Mathf.Clamp(LookDirection.y, -1, 1));

        LookSensitivity = LookSensitivityController;

        if (CustomInputManager.ControllersConnected == false)
            LookSensitivity = LookSensitivityKeyboardAndMouse;

        if (LookDirection.x < 0.25 && LookDirection.x > -0.25)
            LookDirection.x = 0.0f;

        if (LookDirection.y < 0.25 && LookDirection.y > -0.25)
            LookDirection.y = 0.0f;


        LookDirection *= LookSensitivity * Time.deltaTime;

        //Update the x rotation of the camera based on the new Look Direction
        XRotation -= LookDirection.y;

        //Clamp to prevent full 360 rotation around the x-axis
        XRotation = Mathf.Clamp(XRotation, -90, 90);

        //Update the camera's x rotation
        Camera.main.transform.localRotation = Quaternion.Euler(XRotation, Camera.main.transform.localRotation.eulerAngles.y, Camera.main.transform.localRotation.eulerAngles.z);

        //Rotate the entire player container transform around the y-axis based on the look direction
        transform.Rotate(transform.up * LookDirection.x);
    }

    public void ClimbLadder(Vector3 Direction)
    {
        GetComponent<Timeline>().rigidbody.useGravity = false;
        Direction *= ClimbSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + Direction);
    }

    public void WallRun(Vector3 Direction)
    {
        GetComponent<Timeline>().rigidbody.useGravity = false;
        Direction *= WallRunSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + Direction);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

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

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private void StopGrounded()
    {
        grounded = false;
    }
}
