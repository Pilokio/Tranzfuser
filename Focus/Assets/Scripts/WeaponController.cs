using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This component is used to determine the weapons that can be used by the character it is placed on
/// as well as the usage of them
/// </summary>
public class WeaponController : MonoBehaviour
{

    public TimeManager timeManager;

    [Header("Weapon Details")]
    //A list of the stats for each possible weapon
    [SerializeField] List<Weapon> WeaponsList = new List<Weapon>();

    //The gun holder object, the parent of the gun after instantiation
    //ie where it will be placed in relation to the player object
    [SerializeField] GameObject GunHolder;

    //The index in the weapon list array
    private int CurrentWeaponIndex = -1;

    //The currently equipped gun, stored after instantiation to allow for deletion on weapon change
    private GameObject CurrentGun;


    [Header("Debug")]
    //For Debugging to show currently equipped weapon and ammo counters
    [SerializeField] Text AmmoCounter;

    // Start is called before the first frame update
    void Start()
    {
        //Error handling for if the gun holder is not assigned in the editor
        if (GunHolder == null)
        {
            //Manual search for the GunHolder object
            foreach (Transform child in transform)
            {
                if (child.tag == "GunHolder")
                {
                    GunHolder = child.gameObject;
                    break;
                }
                else if (child.transform.childCount > 0)
                {
                    foreach (Transform grandchild in child.transform)
                    {
                        if (grandchild.tag == "GunHolder")
                        {
                            GunHolder = grandchild.gameObject;
                            break;
                        }
                    }
                }
            }

            if (GunHolder != null)
            {
                Debug.LogWarning("Gun Holder object has not been assigned in the editor on the WeaponController component of " + transform.name + ". Successfully found via manual search");
            }
            else
            {
                Debug.LogError("No Gun Holder object has been assigned on the Weapon Controller Component of " + transform.name + ". Manual searches have been unsuccessful.");
            }
        }

        //Init the currently equipped weapon
        ChangeWeapon(0);
    }

    // Update is called once per frame
    void Update()
    {
        //All of the folllowing should be moved to the Player controller script along with a reference to this component
        //This will allow Enemy AI to utilise the same code for weapon usage
        AmmoCounter.text = WeaponsList[CurrentWeaponIndex].WeaponName + ": " + WeaponsList[CurrentWeaponIndex].AmmoInCLip + "/" + WeaponsList[CurrentWeaponIndex].SpareAmmoCount;
        if (Input.GetMouseButtonDown(0))
        {
            UseWeapon();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (CurrentWeaponIndex + 1 < WeaponsList.Count)
            {
                Debug.Log("Changing Weapon");
                ChangeWeapon(CurrentWeaponIndex + 1);
            }
            else
            {

                ChangeWeapon(0);
            }
        }
    }

    /// <summary>
    /// This function changes the currently equipped weapon to the new ID.
    ///It swaps out the weapon prefabs to show the currently equipped one.
    /// </summary>
    public void ChangeWeapon(int index)
    {
        //Check ID is not out of bounds
        if (index >= 0 && index < WeaponsList.Count)
        {
            CurrentWeaponIndex = index;

            //If a current gun object is present then delete the previously equipped weapon
            if (CurrentGun != null)
            {
                Destroy(CurrentGun.gameObject);
            }

            //Instantiate the new gun object
            CurrentGun = Instantiate(WeaponsList[CurrentWeaponIndex].WeaponObject, GunHolder.transform);

        }
        else
        {
            Debug.LogError("Invalid weapon ID presented when changing weapon in WeaponController component of " + transform.name);
        }
    }

    /// <summary>
    /// This function is called to use the currently equipped weapon if possible
    /// </summary>    
    void UseWeapon()
    {
        //Error handling for if required parameters are null or invalid

        if (CurrentGun == null || CurrentGun.transform.childCount == 0)
        {
            Debug.LogError("Error when trying to use weapon. Check the object is instantiated correctly and a child object for the tip of the barrel is present.");
            return;
        }

        if (CurrentWeaponIndex == -1)
        {
            Debug.LogError("Invalid Weapon is equipped. Unable to find suitable ID in WeaponController component of " + transform.name);
            return;
        }

        //Check that the currently equipped weapon is not a melee weapon
        if (WeaponsList[CurrentWeaponIndex].IsRanged)
        {
            //If there is still ammo in the magazine and a suitable weapon is equipped
            if (WeaponsList[CurrentWeaponIndex].AmmoInCLip > 0)
            {
                // Start slowmotion
                timeManager.DoSlowmotion();
                // Slow motion end

                //Find the tip of the gun's barrel
                Transform BarrelEnd = CurrentGun.transform.GetChild(0).transform;
                //Instantiate the bullet at the tip of the gun
                GameObject projectile = Instantiate(WeaponsList[CurrentWeaponIndex].ProjectilePrefab, BarrelEnd.position, BarrelEnd.rotation);
                //Set its projectile force based on the stats in the weaponList and its direction based on the forward vector of the barrel tip
                projectile.GetComponent<ProjectileController>().Force = Vector3.one * WeaponsList[CurrentWeaponIndex].ProjectileForce;
                projectile.GetComponent<ProjectileController>().Direction = BarrelEnd.forward;
                //Fire the projectile
                projectile.GetComponent<ProjectileController>().Fire();
                //Decrement the ammo count
                WeaponsList[CurrentWeaponIndex].AmmoInCLip--;
                WeaponsList[CurrentWeaponIndex].SpareAmmoCount--;
            }
            else if (WeaponsList[CurrentWeaponIndex].SpareAmmoCount > 0)
            {
                //Automatically reload if the player attempts to fire the weapon 
                //provided they have more ammunition
                ReloadWeapon();
            }
        }
        else
        {
            //Add Melee stuff here if we decide to add it
        }
    }

    /// <summary>
    /// This function reloads the weapon if there is enough spare ammo for it
    /// </summary>
    void ReloadWeapon()
    {
        //Ensure there is spare ammo to load into the weapon
        if (WeaponsList[CurrentWeaponIndex].SpareAmmoCount > 0)
        {
            //Take the ammo from the clip and add it to the spare ammo counter (prevents the current mag being lost in the reload)
            WeaponsList[CurrentWeaponIndex].SpareAmmoCount += WeaponsList[CurrentWeaponIndex].AmmoInCLip;
            WeaponsList[CurrentWeaponIndex].AmmoInCLip = 0;

            //If the player has more ammo than can be loaded into the magazine
            //Add the total magazine capacity to ammo in clip, and subtract it from the spare ammo counter
            if (WeaponsList[CurrentWeaponIndex].SpareAmmoCount > WeaponsList[CurrentWeaponIndex].MagazineCapacity)
            {
                WeaponsList[CurrentWeaponIndex].AmmoInCLip = WeaponsList[CurrentWeaponIndex].MagazineCapacity;
                WeaponsList[CurrentWeaponIndex].SpareAmmoCount -= WeaponsList[CurrentWeaponIndex].MagazineCapacity;
            }
            else
            {
                //If the player has less than the magazine capacity, add all their spare ammo into the clip
                //and set the spare counter to 0
                WeaponsList[CurrentWeaponIndex].AmmoInCLip = WeaponsList[CurrentWeaponIndex].SpareAmmoCount;
                WeaponsList[CurrentWeaponIndex].SpareAmmoCount = 0;
            }
        }
        else
        {
            //Used for Debug Purposes
            //Debug.Log("No Ammo to load");
        }
    }
}

/// <summary>
/// This class represents the Weapons in the game. 
/// It contains the relevant information regarding its identifiers, ammo supplies, and usage. 
/// All of which can be assigned in the editor
/// </summary>
[System.Serializable]
public class Weapon
{
    public enum WeaponType { Pistol };

    public string WeaponName;
    public string WeaponID;
    public WeaponType Type;
    public GameObject ProjectilePrefab;
    public GameObject WeaponObject;
    public int MagazineCapacity;
    public int SpareAmmoCount;
    public int AmmoInCLip;
    public float ProjectileForce;
    public bool IsRanged;
}

//TODO?
//Have spare ammo count stored seperately to allow for multiples of the same weapon type to share an ammo pool
