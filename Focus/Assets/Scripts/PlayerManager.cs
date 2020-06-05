using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


    // Start is called before the first frame update
    void Start()
    {
        MyMovement = GetComponent<PlayerMovement>();
        MyWallRunning = GetComponent<WallRunning>();
        MyWeaponController = GetComponent<WeaponController>();
        MyStats = GetComponent<CharacterStats>();
        MyTimeManager = GetComponent<TimeManager>();

        ControllerSupport.InitialiseControllerSupport();
        StartCoroutine(ControllerSupport.CheckForControllers());
    }

    void HandleInput()
    {
        //Weapon Handling
        ///////////////////////////////////////////////////////////////////////

        //Fire the players currently equipped weapon 
        //Using either LMB, R2, or RT depending on input device
        if (ControllerSupport.Fire1.GetCustomButtonDown())
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
        if (ControllerSupport.Fire2.GetCustomButtonDown())
        {
            MyTimeManager.DoSlowmotion();
        }

        //Reload the currently equipped weapon
        //Using either the R key, Square, or X-button depending on input device
        if (ControllerSupport.ActionButton4.GetCustomButtonDown())
        {
            MyWeaponController.ReloadWeapon();
        }

        //Swap the currently equipped weapon
        //NB currently just iterates through the weapons list, but would be better with a weapon wheel
        //Using the V key, Triangle, or the Y-Button depending on input device
        if (ControllerSupport.ActionButton3.GetCustomButtonDown())
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

        if(ControllerSupport.ActionButton5.GetCustomButtonDown())
        {
            MyMovement.StartCrouch();
        }
        else if (ControllerSupport.ActionButton5.GetCustomButtonUp())
        {
            MyMovement.StopCrouch();
        }
        
        // Jump using the Spacebar, X-Button (PS4), or the A-Button (Xbox One)
        if (ControllerSupport.ActionButton1.GetCustomButtonDown())
        {
            MyMovement.Jump();
        }

        //Trigger the climb function
        //NB Currently not mapped to a controller
        if (Input.GetKey(KeyCode.C))
        {
            MyMovement.Climb();
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            MyStats.TakeDamage(25);
        }

    }

    void UpdateUI()
    {
        AmmoDisplayText.text = MyWeaponController.GetCurrentlyEquippedWeapon().WeaponName + ": " + MyWeaponController.GetCurrentlyEquippedWeapon().AmmoInCLip + "/" + MyStats.GetAmmoCount(MyWeaponController.GetCurrentlyEquippedWeapon().AmmoType);

        HealthBar.maxValue = MyStats.MaxHealth;
        HealthBar.value = MyStats.Health;
    }


    // Update is called once per frame
    void Update()
    {
        //Check for all player input
        HandleInput();




        //Update for wall running
        MyWallRunning.WallChecker();
        MyWallRunning.RestoreCamera();





        UpdateUI();
    }

    private void FixedUpdate()
    {
        //Core Player movement
        MyMovement.Look(new Vector2(ControllerSupport.RightHorizontal.GetAxis(), ControllerSupport.RightVertical.GetAxis()));
        MyMovement.Move(new Vector2(ControllerSupport.LeftHorizontal.GetAxis(), ControllerSupport.LeftVertical.GetAxis()));
    }
}
