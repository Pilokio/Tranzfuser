using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    //The different possible weapons types
    //Add more here for additional weapons
    public enum WeaponType { Pistol };
    
    //Struct containing the details regarding each possible weapon
    [System.Serializable]
    public struct WeaponStats
    {
        public WeaponType Type;
        public GameObject ProjectilePrefab;
        public GameObject WeaponObject;
        public bool IsRanged;
        public float Range;
        public float MagSize;
        public float ProjectileForce;
    }

    //The currently equipped weapon
    [SerializeField] WeaponType CurrentlyEquippedWeapon;

    //A list of the stats for each possible weapon
    [SerializeField] List<WeaponStats> WeaponsList = new List<WeaponStats>();

    //The current ammo count, should only be edited within this script when firing a weapon
    //however may need to be accessed from elsewhere
    public float CurrentAmmoCount { get; private set; }

    //The index in the weapon list array
    private int CurrentWeaponIndex = -1;

    //The gun holder object, the parent of the gun after instantiation
    //ie where it will be placed in relation to the player object
    [SerializeField] GameObject GunHolder;

    //The currently equipped gun, stored after instantiation to allow for deletion on weapon change
    private GameObject CurrentGun;

    // Start is called before the first frame update
    void Start()
    {
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

        ChangeWeapon(CurrentlyEquippedWeapon);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UseWeapon();
        }
    }

    //This function changes the currently equipped weapon
    //It changes the appropriate parameters and instantiates the new gun object in place of the previously equipped one
    public void ChangeWeapon(WeaponType newWeapon)
    {
        //Check the desired item type is contained in the current weapon list
        if (WeaponsList.FindAll(weap => weap.Type == newWeapon).Count > 0)
        {
            CurrentlyEquippedWeapon = newWeapon;

            //This will update the current weapon index for use later
            GetWeaponIndex();

            //Update the ammo count
            //FIXME if changing weapons the ammo will be fully replenished, this should be altered to
            //store the ammo count on a weapon by weapon basis to prevent an exploit
            CurrentAmmoCount = WeaponsList[CurrentWeaponIndex].MagSize;

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
            Debug.LogError("Unable to change weapon as weapon type does not exist in the weapon list of " + transform.name + " in the WeaponController component");
        }
    }

    //This function acts as an overload for the change weapon function above to allow
    //for a weapon change via index (ie iterate through different weapons on keypress)
    public void ChangeWeapon(int newID)
    {
        //Check ID is not out of bounds
        if (newID >= 0 && newID < WeaponsList.Count)
        {
            CurrentWeaponIndex = newID;

            CurrentlyEquippedWeapon = WeaponsList[CurrentWeaponIndex].Type;

            //Update the ammo count
            //FIXME if changing weapons the ammo will be fully replenished, this should be altered to
            //store the ammo count on a weapon by weapon basis to prevent an exploit
            CurrentAmmoCount = WeaponsList[CurrentWeaponIndex].MagSize;

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

    //This function will update the current weapon index
    //FIXME ? This script only works if there are only one weapon of each type
    //To fix this a unique ID for each weapon would need to be generated to allow for multiples
    void GetWeaponIndex()
    {
        //Loop through the weapons list and find the corresponding weapon id
        for(int i = 0; i < WeaponsList.Count; i++)
        {
            if(WeaponsList[i].Type == CurrentlyEquippedWeapon)
            {
                CurrentWeaponIndex = i;
                return;
            }
        }

        //Set to -1 if the appropriate ID cannot be found
        CurrentWeaponIndex = -1;
        return;
    }


    //This function handles the use of the currently equipped weapon
    void UseWeapon()
    {
        if(CurrentGun == null || CurrentGun.transform.childCount == 0)
        {
            Debug.LogError("Error when trying to use weapon. Check the object is instantiated correctly and a child object for the tip of the barrel is present.");
        }

        if(CurrentWeaponIndex == -1)
        {
            Debug.LogError("Invalid Weapon is equipped. Unable to find suitable ID in WeaponController component of " + transform.name);
            return;
        }

        //If there is still ammo in the magazine and a suitable weapon is equipped
        if (CurrentAmmoCount > 0)
        {      
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
            CurrentAmmoCount--;
        }
    }
}
