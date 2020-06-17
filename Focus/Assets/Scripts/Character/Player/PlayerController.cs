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

#pragma warning disable 0649
    [Header("User Interface")]
    [SerializeField] Text AmmoDisplayText;
    [SerializeField] Slider HealthBar;
#pragma warning restore 0649


    public bool IsClimbing = false;

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

    void HandleInput()
    {
        //Weapon Handling
        ///////////////////////////////////////////////////////////////////////

        //Fire the players currently equipped weapon 
        //Using either LMB, R2, or RT depending on input device
        if (CustomInputManager.GetAxisAsButton("RightTrigger"))
        {
            //If using a controller, activate cooldown as Fire1 and 2 are axis being treated like buttons
            if (!ControllerSupport.NoControllersConnected)
            {
                StartCoroutine(ControllerSupport.Fire1.ResetAxisButton());
            }

            //Use the equipped weapon
            MyWeaponController.UseWeapon();
        }


        //Slow time for the player
        //using either RMB, L2, or LT depending on input device
        if (CustomInputManager.GetAxisAsButton("LeftTrigger"))
        {
            MyTimeManager.DoSlowmotion();
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

    void UpdateUI()
    {
        AmmoDisplayText.text = MyWeaponController.GetCurrentlyEquippedWeapon().WeaponName + ": "
            + MyWeaponController.GetCurrentlyEquippedWeapon().AmmoInCLip + "/"
            + MyStats.GetAmmoCount(MyWeaponController.GetCurrentlyEquippedWeapon().AmmoType);

        HealthBar.maxValue = MyStats.MaxHealth;
        HealthBar.value = MyStats.Health;
    }


    // Update is called once per frame
    void Update()
    {
        // Check for all player input
        HandleInput();




        // Update for wall running
        MyWallRunning.WallChecker();
        MyWallRunning.RestoreCamera();

        if (!IsClimbing)
            MyMovement.Look(new Vector2(CustomInputManager.GetAxisRaw("RightStickHorizontal"), CustomInputManager.GetAxisRaw("RightStickVertical")));
        else
            MyMovement.Look(new Vector2(0.0f, CustomInputManager.GetAxisRaw("RightStickVertical")));




        UpdateUI();
    }

    private void FixedUpdate()
    {

        Vector2 MoveDirection = new Vector2(CustomInputManager.GetAxisRaw("LeftStickHorizontal"), CustomInputManager.GetAxisRaw("LeftStickVertical"));

        //Core Player movement
        if (!IsClimbing)
        {
            //GetComponent<Rigidbody>().useGravity = true; // This line was causing the wall run to not work
            MyMovement.Move(MoveDirection);
        }
        else
        {
            //GetComponent<Rigidbody>().useGravity = false;
            MyMovement.Move(new Vector2(MoveDirection.x, 0));
            MyMovement.ClimbLadder(new Vector3(0, MoveDirection.y, 0));
        }
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

    private void LateUpdate()
    {

    }
}
