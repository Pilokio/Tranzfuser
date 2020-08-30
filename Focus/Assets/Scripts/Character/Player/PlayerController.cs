using Chronos;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(WeaponController))]
[RequireComponent(typeof(CharacterStats))]

public class PlayerController : MonoBehaviour
{
    //This disables the warning about uninitialised variables as they are assigned in the editor
#pragma warning disable 0649

    #region Component Caches

    //Local component caches
    PlayerMovement MyMovement;
    WeaponController MyWeaponController;
    WallRunning MyWallRunning;
    CharacterStats MyStats;
    TimeControl MyTimeController;
    CameraFov MyFOVController;
    WeaponSway MyWeaponSway;
    LineRenderer MyLineRenderer;
    //The player animator used for the fire and reload animations
    //Assigned in editor as animator is not on the parent object
    [SerializeField] Animator MyAnimator;
    //Local store of the main camera (ie the one used by the player)
    private Camera PlayerCamera;

    #endregion

    #region UI 
    [Header("User Interface")]
    //The text UI where the ammo counter will be displayed
    [SerializeField] Text AmmoDisplayText;
    //The player health bar
    [SerializeField] Slider HealthBar;
    [SerializeField] Slider FocusBar;

    [SerializeField] Image WeaponImage;
    #endregion

    #region Grapple
    [Header("Grapple Settings")]
    //Layermask to ensure the grapple raycast only interacts with suitable hook points
    [SerializeField] private LayerMask GrappleMask;
    //The particle system used when grappling
    [SerializeField] private ParticleSystem GrappleParticleSystem;
    //This is the range at which the grapple can be fired
    [SerializeField] private float GrappleRange = 100f;
    //The distance the player can be to the grapple point before disconnecting (ie close enough)
    [SerializeField] private float GrappleMinDistance = 2.5f;
    //Used when moving the player towards the grapple point 
    //Must be 1.0f 
    private float GrappleSpeed = 1.0f;
    //Local store of the targeted hook point 
    //used to toggle hook canvas without raycast hit
    Hook TargetedHookPoint;
    //The ray being used in the grapple raycast
    Ray GrappleRay;
    //The hit from the grapple raycast
    RaycastHit GrappleTarget;

    #endregion

    #region Combat
    [Header("Weapon Settings")]
    //This is the position the gun holder will be moved to when aiming down sights
    [SerializeField] Transform AimDownSightsPos;
    //This is the default position of the gun holder when not aiming down sights
    [SerializeField] Transform OriginalGunPos;
    //The gun holder object is where the currently equipped weapon will be spawned
    //This is what will be moved to move the gun to the aiming position
    [SerializeField] Transform GunHolder;
    //Toggle used in input callback to determine whether the gun holder
    //should be in the aim down sights position
    private bool IsAiming = false;
    //The smoothing factor used when aiming down the sights
    //Affects the interpolation of the gun holder transform
    private float AimingSmoothFactor = 10.0f;

    #endregion

    #region Traversal Variables
    public Vector3 characterVelocityMomentum;

    //The direction of movement based on player input
    private Vector2 MoveDirection = new Vector2();
    //The look direction based on player input
    private Vector2 LookDirection = new Vector2();
    //Is the player using a ladder?
    public bool IsClimbing { get; private set; }
    //Is the player wall running?
    public bool IsWallRunning { get; private set; }

    #endregion

#pragma warning restore 0649

    #region Input Callbacks
    //The following callback functions are used to update variables/activate functionality
    //based on user input from a connected input device. NB will work for any input device Unity can recognise.

    public void Move(InputAction.CallbackContext context)
    {
        MoveDirection = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        if(context.control.name == "delta")
        {
            MyMovement.SetLookSensitivity(true);
        }
        else
        {
            MyMovement.SetLookSensitivity(false);
        }

        LookDirection = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if(context.ReadValueAsButton())
        {
            MyMovement.IsSprinting = true;
        }
        else
        {
            MyMovement.IsSprinting = false;
        }
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && MyWeaponController.GetCurrentlyEquippedWeapon().CanAimDownSights)
        {
            IsAiming = true;
        }
        else
        {
            IsAiming = false;
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            //Use the equipped weapon
            if (MyWeaponController.UseWeapon(Camera.main.transform.position, Camera.main.transform.forward))
            {
                MyAnimator.SetBool("isFiring", true);
            }
        }
        else
        {
            MyAnimator.SetBool("isFiring", false);
        }
    }

    public void Focus(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            MyTimeController.ToggleSlowMo(); 
        }
    }

    public void Grapple(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            GrappleToHook();
        }
    }

    public void WeaponSwap(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
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
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            if (IsWallRunning)
            {
                MyWallRunning.JumpOffWall();
            }
            else
            {
                MyMovement.Jump();
            }
        }
    }
    public void Reload(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            if (MyWeaponController.GetCurrentlyEquippedWeapon().WeaponAmmoLoaded < MyWeaponController.GetCurrentlyEquippedWeapon().WeaponMagCapacity)
            {
                MyAnimator.SetBool("isReloading", true);
                MyWeaponController.ReloadWeapon();
            }
        }
        else
        {
            MyAnimator.SetBool("isReloading", false);
        }
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            //Toggle pause menu here
        }
    }

    #endregion

    #region Getters & Setters

    //Used by the ladder script to determine the current move input
    public Vector2 GetMoveDirection()
    {
        return MoveDirection;
    }

    //Used by the weapon sway script to determine the current look input
    public Vector2 GetLookDirection()
    {
        return LookDirection;
    }

    //Determines whether the player should use climbing controls
    public void SetIsClimbing(bool Param)
    {
        IsClimbing = Param;
        GetComponent<Timeline>().rigidbody.useGravity = !Param;
    }

    //Determines whether the player should use wall running controls
    public void SetIsWallRunning(bool Param)
    {
        IsWallRunning = Param;
        GetComponent<Timeline>().rigidbody.useGravity = !Param;

        if (Param)
        {
            GetComponent<Timeline>().rigidbody.velocity = new Vector3(0,0,0);
        }
    }

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;

        //Get and store the main camera, used by the player
        PlayerCamera = Camera.main;

        //Store local caches of all the relevant components on the player
        MyMovement = GetComponent<PlayerMovement>();
        MyWallRunning = GetComponent<WallRunning>();
        MyWeaponController = GetComponent<WeaponController>();
        MyStats = GetComponent<CharacterStats>();
        MyTimeController = GetComponent<TimeControl>();
        MyLineRenderer = GetComponent<LineRenderer>();
        MyWeaponSway = GetComponentInChildren<WeaponSway>();
        MyFOVController = PlayerCamera.GetComponent<CameraFov>();

        //Ensure the grapple particle system doesnt play by default
        GrappleParticleSystem.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        //Lock the x-axis of the look direction when climbing ladders
        //otherwise pass the look direction to the movement script as is
        if (IsClimbing)
        {
            MyMovement.Look(new Vector2(0.0f, LookDirection.y));
        }
        else if (IsWallRunning)
        {
            MyMovement.LookOnWall(LookDirection);
        }
        else
        {
            MyMovement.Look(LookDirection);
        }

        //Create the ray for the grapple raycast
        GrappleRay = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
        //Perform the raycast for the grapple
        if (Physics.Raycast(GrappleRay, out GrappleTarget, GrappleRange, GrappleMask))
        {
            //if aiming at a different grapple point from the one stored
            //hide the canvas on the old one before proceeding
            if(TargetedHookPoint != GrappleTarget.transform.GetComponent<Hook>() && TargetedHookPoint != null)
            {
                TargetedHookPoint.HideCanvas();
            }

            //Store the result of the raycast and display the canvas
            TargetedHookPoint = GrappleTarget.transform.GetComponent<Hook>();
            TargetedHookPoint.DisplayCanvas();
        }
        else
        {
            //If the raycat fails to hit anything and there is still a hook stored
            //Hide its canvas and set the local store to null
            if (TargetedHookPoint != null)
            {
                TargetedHookPoint.HideCanvas();
                TargetedHookPoint = null;
            }
        }

        //While holding the aim button, disable weapon sway and move the gun to the aim position
        //otherwise return to default position and resume sway
        if(IsAiming)
        {
            MyWeaponSway.enabled = false;
            MyAnimator.SetBool("isAiming", true);
            GunHolder.transform.position = Vector3.Lerp(GunHolder.transform.position, AimDownSightsPos.transform.position, Time.deltaTime * AimingSmoothFactor);
        }
        else
        {
            MyWeaponSway.enabled = true;
            MyAnimator.SetBool("isAiming", false);
            GunHolder.transform.position = Vector3.Lerp(GunHolder.position, OriginalGunPos.position, Time.deltaTime * AimingSmoothFactor);
        }

        UpdateUI();
    }

    private void FixedUpdate()
    {
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
        
        //If climbing a ladder, use the climb ladder function to move up/down using the y component of the move direction
        //while also using the x component to dismount early if desired using the default move function
        if(IsClimbing)
        {
            MyMovement.Move(new Vector2(MoveDirection.x, 0));
            MyMovement.ClimbLadder(new Vector3(0, MoveDirection.y, 0));
        }

        //Use the wall running scripts move function when wall running instead of the default movement
        if (IsWallRunning)
        {
            if (MyWallRunning.IsTurning && MoveDirection.y != 0.0f)
            {
                MyWallRunning.IsTurning = false;
            }

            MyMovement.MoveOnWall(MoveDirection.y);
        }
    }

    //This function is used to update the healthbar and ammo counter in the UI
    //NB The focus bar is updated elsewhere
    void UpdateUI()
    {
        AmmoDisplayText.text = MyWeaponController.GetCurrentlyEquippedWeapon().WeaponAmmoLoaded + "/"
            + MyStats.GetAmmoCount(MyWeaponController.GetCurrentlyEquippedWeapon().WeaponAmmoType);

        HealthBar.maxValue = MyStats.MaxHealth;
        HealthBar.value = MyStats.Health;

        FocusBar.maxValue = MyTimeController.MaxFocus;
        FocusBar.value = MyTimeController.FocusMeter;

        if (MyWeaponController.GetCurrentlyEquippedWeapon().WeaponImage != null && WeaponImage != null)
            WeaponImage.sprite = MyWeaponController.GetCurrentlyEquippedWeapon().WeaponImage;
    }

   //This function begins the process of grappling to the targeted hook point
    void GrappleToHook()
    {
        if (TargetedHookPoint != null)
        {
            StartCoroutine(MoveGrapple(TargetedHookPoint.transform.position));
        }
    }

    //This coroutine handles the grappling mechanic itself
    IEnumerator MoveGrapple(Vector3 destination)
    {
        //Enable the line renderer, play the particle effect and update the FOV
        MyLineRenderer.enabled = true;
        GrappleParticleSystem.Play();
        MyFOVController.UseGrappleFOV();

        //Use the line renderer to draw a line from the player to the hook
        GetComponent<LineRenderer>().SetPositions(new Vector3[] { MyWeaponController.CurrentGun.transform.GetChild(0).transform.position, destination });

        yield return null;

        //While the player is more than the min distance away from the hook point, move towards it and update the line renderer
        while (Vector3.Distance(transform.position, destination) > GrappleMinDistance)
        {
            yield return null;
            transform.position = Vector3.MoveTowards(transform.position, destination, GrappleSpeed);
            GetComponent<LineRenderer>().SetPosition(0, MyWeaponController.CurrentGun.transform.GetChild(0).transform.position);
        }

        //After reaching the hook point, disable the line renderer, stop the particle effect, and return to normal FOV
        MyLineRenderer.enabled = false;
        MyFOVController.UseNormalFOV();
        GrappleParticleSystem.Stop();
    }

    public void OnCollisionEnter(Collision collision)
    {
        //If the player is hit by a physical bullet object, take damage
        if (collision.transform.CompareTag("Bullet"))
        {
            MyStats.TakeDamage((int)collision.gameObject.GetComponent<ProjectileController>().DamageAmount);
        }
    }
}
