using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component is used to determine the weapons that can be used by the character it is placed on
/// as well as the usage of them
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Details")]
    //A list of the stats for each possible weapon
    [SerializeField] List<Weapon> WeaponsList = new List<Weapon>();

    //The gun holder object, the parent of the gun after instantiation
    //ie where it will be placed in relation to the player object
    [SerializeField] GameObject GunHolder;

    //The index in the weapon list array
    public int CurrentWeaponIndex { get; private set; }

    //The currently equipped gun, stored after instantiation to allow for deletion on weapon change
    public GameObject CurrentGun { get; private set; }

    //Reference to the character's stats which are used to determine if reloading is possible
    CharacterStats MyStats;

    private bool CanFire = true;

    // Start is called before the first frame update
    void Start()
    {
        // Error handling for if the gun holder is not assigned in the editor
        if (GunHolder == null)
        {
            // Manual search for the GunHolder object
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

        MyStats = GetComponent<CharacterStats>();
        // Init the currently equipped weapon
        ChangeWeapon(0);
    }

    /// <summary>
    /// This function changes the currently equipped weapon to the new ID.
    ///It swaps out the weapon prefabs to show the currently equipped one.
    /// </summary>
    public void ChangeWeapon(int index)
    {
        // Check ID is not out of bounds
        if (index >= 0 && index < WeaponsList.Count)
        {
            CurrentWeaponIndex = index;

            // If a current gun object is present then delete the previously equipped weapon
            if (CurrentGun != null)
            {
                Destroy(CurrentGun.gameObject);
            }

            // Instantiate the new gun object
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
    public void UseWeapon(Vector3 Target)
    {
        // Error handling for if required parameters are null or invalid

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

        // Check that the currently equipped weapon is not a melee weapon
        if (WeaponsList[CurrentWeaponIndex].IsRanged)
        {
            //If there is still ammo in the magazine and a suitable weapon is equipped
            if (WeaponsList[CurrentWeaponIndex].AmmoInCLip > 0 && CanFire)
            {
                CanFire = false;
                //Find the tip of the gun's barrel
                Transform BarrelEnd = CurrentGun.transform.GetChild(0).transform;
                //Instantiate the bullet at the tip of the gun
                GameObject projectile = Instantiate(WeaponsList[CurrentWeaponIndex].ProjectilePrefab, BarrelEnd.position, Quaternion.Euler(Target - BarrelEnd.position));
                //Set its projectile force based on the stats in the weaponList and its direction based on the forward vector of the barrel tip
                projectile.GetComponent<ProjectileController>().Force = Vector3.one * WeaponsList[CurrentWeaponIndex].ProjectileForce;
                projectile.GetComponent<ProjectileController>().Direction = BarrelEnd.forward;

                if (transform.tag == "Player")
                {
                    projectile.GetComponent<ProjectileController>().BelongsToPlayer = true;
                }
                else
                {
                    projectile.GetComponent<ProjectileController>().BelongsToPlayer = false;
                }

                projectile.GetComponent<ProjectileController>().DamageAmount = WeaponsList[CurrentWeaponIndex].Damage;

                //Fire the projectile
                projectile.GetComponent<ProjectileController>().Fire();
                //Decrement the ammo count
                WeaponsList[CurrentWeaponIndex].AmmoInCLip--;
                StartCoroutine(StartWeaponCooldown());
            }
            else if (MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].AmmoType) > 0)
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
    public void ReloadWeapon()
    {
        //Ensure there is spare ammo to load into the weapon
        if (MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].AmmoType) > 0)
        {
            //Take the ammo from the clip and add it to the spare ammo counter (prevents the current mag being lost in the reload)

            MyStats.AddAmmo(new AmmunitionType(WeaponsList[CurrentWeaponIndex].AmmoType, WeaponsList[CurrentWeaponIndex].AmmoInCLip));
            WeaponsList[CurrentWeaponIndex].AmmoInCLip = 0;

            //If the player has more ammo than can be loaded into the magazine
            //Add the total magazine capacity to ammo in clip, and subtract it from the spare ammo counter
            if (MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].AmmoType) > WeaponsList[CurrentWeaponIndex].MagazineCapacity)
            {
                WeaponsList[CurrentWeaponIndex].AmmoInCLip = WeaponsList[CurrentWeaponIndex].MagazineCapacity;
                MyStats.ConsumeAmmo(new AmmunitionType(WeaponsList[CurrentWeaponIndex].AmmoType, WeaponsList[CurrentWeaponIndex].MagazineCapacity));
            }
            else
            {
                //If the player has less than the magazine capacity, add all their spare ammo into the clip
                //and set the spare counter to 0
                WeaponsList[CurrentWeaponIndex].AmmoInCLip = MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].AmmoType);
                MyStats.ConsumeAmmo(new AmmunitionType(WeaponsList[CurrentWeaponIndex].AmmoType, MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].AmmoType)));
            }
        }
        else
        {
            // Used for Debug Purposes
            // Debug.Log("No Ammo to load");
        }
    }


    //Returns the size of the weapons list for use elsewhere
    public int GetWeaponListSize()
    {
        return WeaponsList.Count;
    }

    //Returns the currently selected weapon for use elsewhere
    public Weapon GetCurrentlyEquippedWeapon()
    {
        return WeaponsList[CurrentWeaponIndex];
    }

    IEnumerator StartWeaponCooldown()
    {
        yield return new WaitForSeconds(WeaponsList[CurrentWeaponIndex].CoolDownBetweenShots);
        CanFire = true;
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
    public AmmunitionType.AmmoType AmmoType;
    public int AmmoInCLip;
    public float ProjectileForce;
    public bool IsRanged;
    public float CoolDownBetweenShots;
    public float Damage;
    public float Range;
}

[System.Serializable]
public class AmmunitionType
{
    public enum AmmoType { Pistol, SMG, Rifle, Shotgun, Launcher, Grenade };

    public AmmoType Type;
    public int Amount;

    public AmmunitionType(AmmoType type, int amount)
    {
        this.Type = type;
        this.Amount = amount;
    }
}
