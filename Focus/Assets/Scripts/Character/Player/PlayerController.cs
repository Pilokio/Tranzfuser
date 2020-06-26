using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(WallRunning))]
[RequireComponent(typeof(WeaponController))]
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(TimeManager))]

public class PlayerController : MonoBehaviour
{
    PlayerMovement MyMovement;
    WeaponController MyWeaponController;
    WallRunning MyWallRunning;
    CharacterStats MyStats;
    TimeManager MyTimeManager;
    CameraFov cameraFov;
    private ParticleSystem speedLinesParticleSystem;

    private const float NORMAL_FOV = 60f;
    private const float HOOKSHOT_FOV = 100f;
#pragma warning disable 0649
    [Header("User Interface")]
    [SerializeField] Text AmmoDisplayText;
    [SerializeField] Slider HealthBar;
    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform hookshotTransform;
#pragma warning restore 0649

    public Transform AimDownSightsPos;
    public Transform GunHolder;
    public Transform OriginalGunPos;
    public Vector3 characterVelocityMomentum;

    private Camera playerCamera;

    public bool IsClimbing = false;

    private float smoothFactor = 10.0f;

    private State state;
    private Vector3 hookshotPosition;
    private float hookshotSize;

    private enum State
    {
        Normal,
        HookshotThrown,
        HookShotFlyingPlayer
    }

    private void Awake()
    {
        playerCamera = transform.Find("Main Camera").GetComponent<Camera>();
        cameraFov = playerCamera.GetComponent<CameraFov>();
        speedLinesParticleSystem = transform.Find("Main Camera").Find("SpeedLinesParticleSystem").GetComponent<ParticleSystem>();
        speedLinesParticleSystem.Stop();
        state = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;

        //Init the custom input manager and only track controllers connected on start
        CustomInputManager.InitialiseCustomInputManager();
        //Add bindings for a single player to the custom input manager
        CustomInputManager.CreateDefaultSinglePlayerInputManager();
        //Begin checking for controllers, to determine changes to the tracked controller list
        StartCoroutine(CustomInputManager.CheckForControllers());

        MyMovement = GetComponent<PlayerMovement>();
        MyWallRunning = GetComponent<WallRunning>();
        MyWeaponController = GetComponent<WeaponController>();
        MyStats = GetComponent<CharacterStats>();
        MyTimeManager = GetComponent<TimeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // When the hook fires, grant the player some access to movement such as looking around, jumping etc
        switch (state)
        {
            default:
            case State.Normal:
                // Check for all player input
                HandleInput();
                HandleLook();
                HandleHookshotStart();

                MyWallRunning.WallChecker();
                MyWallRunning.RestoreCamera();
                break;

            case State.HookshotThrown:
                HandleHookshotThrow();
                HandleInput();
                break;

            case State.HookShotFlyingPlayer:
                HandleLook();
                HandleHookshotMovement();
                break;
        }

        UpdateUI();
    }

    private void FixedUpdate()
    {
        Vector2 MoveDirection = new Vector2(CustomInputManager.GetAxisRaw("LeftStickHorizontal"), CustomInputManager.GetAxisRaw("LeftStickVertical"));

        //Core Player movement
        if (!IsClimbing)
        {
            // Apply momentum
            MyMovement.Move(characterVelocityMomentum);

            //GetComponent<Rigidbody>().useGravity = true; // This line was causing the wall run to not work
            MyMovement.Move(MoveDirection);

            // Dampen momentum
            if (characterVelocityMomentum.magnitude >= 0f)
            {
                float momentumDrag = 3f;
                characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
                if (characterVelocityMomentum.magnitude < .0f)
                {
                    characterVelocityMomentum = Vector3.zero;
                }
            }
        }
        else
        {
            //GetComponent<Rigidbody>().useGravity = false;
            MyMovement.Move(new Vector2(MoveDirection.x, 0));
            MyMovement.ClimbLadder(new Vector3(0, MoveDirection.y, 0));
        }
    }

    void HandleInput()
    {
        //Weapon Handling
        ///////////////////////////////////////////////////////////////////////

        //Fire the players currently equipped weapon 
        //Using either LMB, R2, or RT depending on input device
        if (CustomInputManager.GetAxis("RightTrigger") != CustomInputManager.GetAxisNeutralPosition("RightTrigger"))
        {
            //Use the equipped weapon
            MyWeaponController.UseWeapon(Camera.main.transform.position, Camera.main.transform.forward);
        }


        //Slow time for the player
        //using either RMB, L2, or LT depending on input device
        if (CustomInputManager.GetAxis("LeftTrigger") != CustomInputManager.GetAxisNeutralPosition("LeftTrigger"))
        {
            GunHolder.transform.position = Vector3.Lerp(GunHolder.transform.position, AimDownSightsPos.transform.position, Time.deltaTime * smoothFactor);

            //MyTimeManager.DoSlowmotion();
        }
        else
        {
            GunHolder.transform.position = Vector3.Lerp(GunHolder.position, OriginalGunPos.position, Time.deltaTime * smoothFactor);
        }

        //Reload the currently equipped weapon
        //Using either the R key, Square, or X-button depending on input device
        if (CustomInputManager.GetButtonDown("ActionButton4"))
        {
            MyWeaponController.ReloadWeapon();
        }

        //Swap the currently equipped weapon
        //NB currently just iterates through the weapons list, but would be better with a weapon wheel
        //Using the V key, Triangle, or the Y-Button depending on input device
        if (CustomInputManager.GetButtonDown("ActionButton3"))
        {
            //if the next increment is beyond the bounds of the weapons list, reset to zero
            if (MyWeaponController.CurrentWeaponIndex + 1 < MyWeaponController.GetWeaponListSize())
            {
                MyWeaponController.ChangeWeapon(MyWeaponController.CurrentWeaponIndex + 1);
            }
            else
            {
                MyWeaponController.ChangeWeapon(0);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        // Crouch using X
        if (ControllerSupport.ActionButton5.GetCustomButtonDown())
        {
            // MyMovement.StartCrouch();
        }
        else if (ControllerSupport.ActionButton5.GetCustomButtonUp())
        {
            // MyMovement.StopCrouch();
        }

        // Jump using the Spacebar, X-Button (PS4), or the A-Button (Xbox One)
        if (CustomInputManager.GetButtonDown("ActionButton1"))
        {
            MyMovement.Jump();
        }

        if (CustomInputManager.GetButtonDown("Menu"))
        {
            MyStats.TakeDamage(25);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            MyMovement.IsSprinting = true;
        }
        else
        {
            MyMovement.IsSprinting = false;
        }

    }

    void HandleLook()
    {
        if (!IsClimbing)
            MyMovement.Look(new Vector2(CustomInputManager.GetAxisRaw("RightStickHorizontal"), CustomInputManager.GetAxisRaw("RightStickVertical")));
        else
            MyMovement.Look(new Vector2(0.0f, CustomInputManager.GetAxisRaw("RightStickVertical")));
    }

    void UpdateUI()
    {
        AmmoDisplayText.text = MyWeaponController.GetCurrentlyEquippedWeapon().WeaponName + ": "
            + MyWeaponController.GetCurrentlyEquippedWeapon().WeaponAmmoLoaded + "/"
            + MyStats.GetAmmoCount(MyWeaponController.GetCurrentlyEquippedWeapon().WeaponAmmoType);

        HealthBar.maxValue = MyStats.MaxHealth;
        HealthBar.value = MyStats.Health;
    }

    // Fixes for the gravity
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            GetComponent<Rigidbody>().useGravity = false;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            GetComponent<Rigidbody>().useGravity = true;
        }
    }

    /// <summary>
    /// WIP - Hook mechanic, press Q to fire hook
    /// </summary>
    private void HandleHookshotStart()
    {
        if (TestInputDownHookshot())
        {
           if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit))
            {
                // Hit something
                debugHitPointTransform.position = raycastHit.point;
                hookshotPosition = raycastHit.point;
                hookshotSize = 0f;
                hookshotTransform.gameObject.SetActive(true);
                hookshotTransform.localScale = Vector3.zero;
                state = State.HookshotThrown;
            }
        }
    }

    private void HandleHookshotThrow()
    {
        hookshotTransform.LookAt(hookshotPosition);

        float hookshotThrowSpeed = 500f;
        hookshotSize += hookshotThrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if (hookshotSize >= Vector3.Distance(transform.position, hookshotPosition))
        {
            state = State.HookShotFlyingPlayer;
            cameraFov.SetCameraFov(HOOKSHOT_FOV);
            speedLinesParticleSystem.Play();
        }
    }
     
    private void HandleHookshotMovement()
    {
        transform.position = Vector3.MoveTowards(transform.position, hookshotPosition, 0.5f);



        //hookshotTransform.LookAt(hookshotTransform);

        //Vector3 hookshotDir = (hookshotPosition - transform.position).normalized;

        //// FIXEME Not working - probably the cause of the weird movement
        //float hookshotSpeedMin = 10f;
        //float hookshotSpeedMax = 40f;
        //float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotPosition), hookshotSpeedMin, hookshotSpeedMax);
        //float hookshotSpeedMultiplier = 2f;

        //MyMovement.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        float reachedHookshotPositionDistance = 1f;
        if (Vector3.Distance(transform.position, hookshotPosition) < reachedHookshotPositionDistance)
        {
            StopHookshot();
        }

        if (TestInputDownHookshot())
        {
            StopHookshot();
        }

        //if (TestInputJump())
        //{
        //    float momentumExtraSpeed = 7f;
        //    characterVelocityMomentum = hookshotDir * hookshotSpeed * momentumExtraSpeed;
        //    float jumpSpeed = 40f;
        //    characterVelocityMomentum += Vector3.up * jumpSpeed;
        //    StopHookshot();
        //}
    }

    private void StopHookshot()
    {
        state = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
        cameraFov.SetCameraFov(NORMAL_FOV);
        speedLinesParticleSystem.Stop();
    }

    private bool TestInputDownHookshot()
    {
        return Input.GetKeyDown(KeyCode.Q);
    }

    private bool TestInputJump()
    {
        return CustomInputManager.GetButtonDown("ActionButton1");
    }
    ///
    // End of hook mechanic
    ///
}
