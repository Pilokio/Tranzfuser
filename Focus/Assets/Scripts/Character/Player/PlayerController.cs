using Chronos;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(WeaponController))]
[RequireComponent(typeof(CharacterStats))]

public class PlayerController : MonoBehaviour
{
    PlayerMovement MyMovement;
    WeaponController MyWeaponController;
    WallRunning MyWallRunning;
    CharacterStats MyStats;
    TimeControl MyTimeController;
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

    public float wallRunSpeed = 8;
    private float minRange = 100f;

    public Animator animator;
    public AudioSource pistolShot;


    private Camera playerCamera;

    public Rigidbody MyRigidbody;

    private float smoothFactor = 10.0f;


    public float pushbackForce = 10.0f;
    [SerializeField] LayerMask GrappleMask;
    Hook TargetedHookPoint;
    float reachedHookshotPositionDistance = 2.5f;

    float GrappleSpeed = 1.0f;

    private enum State
    {
        Normal,
        HookshotThrown,
        HookShotFlyingPlayer
    }

    // Is the player using a ladder?
    public bool IsClimbing { get; private set; }

    // Is the player wall running?
    public bool IsWallRunning { get; private set; }

    public void SetIsClimbing(bool Param)
    {
        IsClimbing = Param;
        GetComponent<Timeline>().rigidbody.useGravity = !Param;
    }

    public void SetIsWallRunning(bool Param)
    {
        IsWallRunning = Param;
        GetComponent<Timeline>().rigidbody.useGravity = !Param;

        if (Param)
        {
            GetComponent<Timeline>().rigidbody.velocity = new Vector3(0,0,0);
        }
    }

    private void Awake()
    {
        playerCamera = transform.Find("Main Camera").GetComponent<Camera>();
        cameraFov = playerCamera.GetComponent<CameraFov>();

        animator = transform.Find("Main Camera").Find("HandPos").GetComponent<Animator>();

        speedLinesParticleSystem = transform.Find("Main Camera").Find("SpeedLinesParticleSystem").GetComponent<ParticleSystem>();
        speedLinesParticleSystem.Stop();
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
        MyTimeController = GetComponent<TimeControl>();
        MyRigidbody = GetComponent<Rigidbody>();

       // pistolShot = transform.Find("Main Camera").Find("HandPos").Find("GunHolder").Find("Pistol(Clone)").GetComponentInChildren<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        HandleLook();
          
        /// Trying to get raycast to constantly draw from player in order
        /// to play particle system on hook points
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit, minRange, GrappleMask))
        {
            if(TargetedHookPoint != raycastHit.transform.GetComponent<Hook>() && TargetedHookPoint != null)
            {
                TargetedHookPoint.HideCanvas();
            }
            
            TargetedHookPoint = raycastHit.transform.GetComponent<Hook>();
            TargetedHookPoint.DisplayCanvas();
           
            //raycastHit.transform.GetComponent<ParticleSystem>().Play();

            // Play particle system
            // Play UI element that displays "Hook" with hook image?
        }
        else
        {
            if(TargetedHookPoint != null)
                TargetedHookPoint.HideCanvas();
        }
        UpdateUI();
    }

    private void FixedUpdate()
    {
        Vector2 MoveDirection = new Vector2(CustomInputManager.GetAxisRaw("LeftStickHorizontal"), CustomInputManager.GetAxisRaw("LeftStickVertical"));

        //Core Player movement
        if (!IsClimbing && !IsWallRunning)
        {
            // Apply momentum
            MyMovement.Move(characterVelocityMomentum);

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
        

        if(IsClimbing)
        {
            //GetComponent<Rigidbody>().useGravity = false;
            MyMovement.Move(new Vector2(MoveDirection.x, 0));
            MyMovement.ClimbLadder(new Vector3(0, MoveDirection.y, 0));
        }

        if (IsWallRunning)
        {
            if (MyWallRunning.IsTurning && MoveDirection.y != 0.0f)
            {
                MyWallRunning.IsTurning = false;
            }

            MyMovement.MoveOnWall(MoveDirection.y);
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
            if (!pistolShot.isPlaying) pistolShot.Play();
            animator.SetBool("isFiring", true);
            //Use the equipped weapon
            MyWeaponController.UseWeapon(Camera.main.transform.position, Camera.main.transform.forward);
        }
        else
        {
            animator.SetBool("isFiring", false);
        }


        //Slow time for the player
        //using either RMB, L2, or LT depending on input device
        if (CustomInputManager.GetAxis("LeftTrigger") != CustomInputManager.GetAxisNeutralPosition("LeftTrigger") && MyWeaponController.GetCurrentlyEquippedWeapon().CanAimDownSights)
        {
            GetComponentInChildren<WeaponSway>().enabled = false;
            animator.SetBool("isAiming", true);
            GunHolder.transform.position = Vector3.Lerp(GunHolder.transform.position, AimDownSightsPos.transform.position, Time.deltaTime * smoothFactor);

            //MyTimeManager.DoSlowmotion();
        }
        else
        {
            GetComponentInChildren<WeaponSway>().enabled = true;
            animator.SetBool("isAiming", false);
            GunHolder.transform.position = Vector3.Lerp(GunHolder.position, OriginalGunPos.position, Time.deltaTime * smoothFactor);
        }

        //Reload the currently equipped weapon
        //Using either the R key, Square, or X-button depending on input device
        if (CustomInputManager.GetButtonDown("ActionButton4"))
        {
            if (MyWeaponController.GetCurrentlyEquippedWeapon().WeaponAmmoLoaded < MyWeaponController.GetCurrentlyEquippedWeapon().WeaponMagCapacity)
            {
                animator.SetBool("isReloading", true);
                MyWeaponController.ReloadWeapon();
            }
        }
        else
        {
            animator.SetBool("isReloading", false);
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


        if (Input.GetKeyDown(KeyCode.Q))
        {
            GrappleToHook();
        }

        // Jump using the Spacebar, X-Button (PS4), or the A-Button (Xbox One)
        if (CustomInputManager.GetButtonDown("ActionButton1") && !IsWallRunning)
        {
            MyMovement.Jump();
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            MyMovement.IsSprinting = true;
        }
        else
        {
            MyMovement.IsSprinting = false;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            MyTimeController.ToggleSlowMo();
        }
    }

    public void HandleLook()
    {
        if (!IsClimbing && !IsWallRunning)
            MyMovement.Look(new Vector2(CustomInputManager.GetAxisRaw("RightStickHorizontal"), CustomInputManager.GetAxisRaw("RightStickVertical")));
        else if(IsClimbing)
            MyMovement.Look(new Vector2(0.0f, CustomInputManager.GetAxisRaw("RightStickVertical")));
        else if(IsWallRunning)
            MyMovement.LookOnWall(new Vector2(CustomInputManager.GetAxisRaw("RightStickHorizontal"), CustomInputManager.GetAxisRaw("RightStickVertical")));   
    }

    void UpdateUI()
    {
        AmmoDisplayText.text = MyWeaponController.GetCurrentlyEquippedWeapon().WeaponName + ": "
            + MyWeaponController.GetCurrentlyEquippedWeapon().WeaponAmmoLoaded + "/"
            + MyStats.GetAmmoCount(MyWeaponController.GetCurrentlyEquippedWeapon().WeaponAmmoType);

        HealthBar.maxValue = MyStats.MaxHealth;
        HealthBar.value = MyStats.Health;
    }

   
    void GrappleToHook()
    {
        if (TargetedHookPoint != null)
        {
            StartCoroutine(MoveGrapple(TargetedHookPoint.transform.position));
        }
    }

    IEnumerator MoveGrapple(Vector3 destination)
    {
        GetComponent<LineRenderer>().enabled = true;
        speedLinesParticleSystem.Play();
        cameraFov.SetCameraFov(HOOKSHOT_FOV);

        GetComponent<LineRenderer>().SetPositions(new Vector3[] { MyWeaponController.CurrentGun.transform.GetChild(0).transform.position, destination });

        yield return null;
        while (Vector3.Distance(transform.position, destination) > reachedHookshotPositionDistance)
        {
            yield return new WaitForEndOfFrame();
            transform.position = Vector3.MoveTowards(transform.position, destination, GrappleSpeed);
            GetComponent<LineRenderer>().SetPosition(0, MyWeaponController.CurrentGun.transform.GetChild(0).transform.position);
        }

        GetComponent<LineRenderer>().enabled = false;
        cameraFov.SetCameraFov(NORMAL_FOV);
        speedLinesParticleSystem.Stop();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Bullet"))
        {
            MyStats.TakeDamage((int)collision.gameObject.GetComponent<ProjectileController>().DamageAmount);
        }
    }
}
