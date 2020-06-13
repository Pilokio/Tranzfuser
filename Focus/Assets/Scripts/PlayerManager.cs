﻿using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(WallRunning))]
[RequireComponent(typeof(WeaponController))]
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(TimeManager))]
public class PlayerManager : MonoBehaviour
{
    PlayerMovement MyMovement;
    WeaponController MyWeaponController;
    WallRunning MyWallRunning;
    CharacterStats MyStats;
    TimeManager MyTimeManager;

    [Header("User Interface")]
    [SerializeField] Text AmmoDisplayText;
    [SerializeField] Slider HealthBar;
    [SerializeField] Text FPS;

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
        FPS.text = "FPS: " + ((int)(1.0f / Time.deltaTime)).ToString();

        //Check for all player input
        HandleInput();




        //Update for wall running
        MyWallRunning.WallChecker();
        MyWallRunning.RestoreCamera();

        MyMovement.Look(new Vector2(CustomInputManager.GetAxisRaw("RightStickHorizontal"), CustomInputManager.GetAxisRaw("RightStickVertical")));




        UpdateUI();
    }

    private void FixedUpdate()
    {
        //Core Player movement
        MyMovement.Move(new Vector2(CustomInputManager.GetAxisRaw("LeftStickHorizontal"), CustomInputManager.GetAxisRaw("LeftStickVertical")));
        
     
    }

    private void LateUpdate()
    {

    }
}
