using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component is used to determine the weapons that can be used by the character it is placed on
/// as well as the usage of them
/// </summary>
[DisallowMultipleComponent]
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

    //The object hit when firing the weapon
    RaycastHit hit;

   
   
    //Timer variables to determine if weapon can be fired again
    private float FireDelay = 0;
    private float FireTimer = 0;
    public  bool CanFire { get; private set; }

    private ParticleSystem muzzleFlash;

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
        muzzleFlash = transform.Find("Main Camera").Find("HandPos").Find("GunHolder").Find("Pistol(Clone)").GetComponentInChildren<ParticleSystem>();
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

            FireDelay = WeaponsList[CurrentWeaponIndex].WeaponFireRate;
            FireTimer = FireDelay;
        }
        else
        {
            Debug.LogError("Invalid weapon ID presented when changing weapon in WeaponController component of " + transform.name);
        }
    }

    private void Update()
    {
        //Decrement timer
        FireTimer -= Time.deltaTime;

        //If timer elapses then allow the weapon to be fired
        if (FireTimer <= 0.0f)
        {
            CanFire = true;
        }
    }

    /// <summary>
    /// This function is called to use the currently equipped weapon if possible
    /// </summary>    
    public void UseWeapon(Vector3 Origin, Vector3 Direction)
    {
        //Don't attempt to use weapon if invalid
        if (CurrentGun == null || CurrentWeaponIndex == -1)
        {
            Debug.LogError("Error when firing weapon on " + transform.name + " object.");
            return;
        }

        //If the weapon is ready to be fired again
        if (CanFire)
        {
            FireTimer = FireDelay;
            CanFire = false;

            //If there is still ammo in the magazine and a suitable weapon is equipped
            if (WeaponsList[CurrentWeaponIndex].WeaponAmmoLoaded > 0)
            {
                //Spawn a physical bullet object (as per the current weapon) if any of the following conditions are met
                //1. The weapon's ammo spawn is set to always
                //2. The weapon's ammo spawn is set to slow motion only AND slow motion is currently active
                //Else fire weapon via raycast
                if ((WeaponsList[CurrentWeaponIndex].SpawnAmmo == Weapon.AmmoSpawn.Always) || (WeaponsList[CurrentWeaponIndex].SpawnAmmo == Weapon.AmmoSpawn.SlowMoOnly && PlayerManager.Instance.Player.transform.GetComponent<TimeControl>().IsSlowMo))
                {
                    SpawnBullet();
                }
                else
                {
                    Ray ray = new Ray(Origin, Direction);


                    muzzleFlash.Play();

                    if (Physics.Raycast(ray, out hit, WeaponsList[CurrentWeaponIndex].WeaponRange))
                    {
                        if (hit.transform.CompareTag("Enemy") && !transform.CompareTag("Enemy"))
                        {
                           hit.transform.GetComponent<EnemyController>().IsHit(ray, WeaponsList[CurrentWeaponIndex].WeaponDamage);
                        }

                        if (hit.transform.CompareTag("Player") && !transform.CompareTag("Player"))
                        {
                            hit.transform.GetComponent<CharacterStats>().TakeDamage(WeaponsList[CurrentWeaponIndex].WeaponDamage);
                        }


                        if (WeaponsList[CurrentWeaponIndex].Type == Weapon.WeaponType.Launcher)
                        {
                            Vector3 hitLocation = hit.point;
                            Collider[] cols = Physics.OverlapSphere(hitLocation, 10.0f);
                            foreach (Collider c in cols)
                            {
                                if (c.gameObject.GetComponent<CharacterStats>())
                                {
                                    c.gameObject.GetComponent<CharacterStats>().TakeDamage(WeaponsList[CurrentWeaponIndex].WeaponDamage);
                                }

                                if (c.gameObject.GetComponent<Rigidbody>())
                                {
                                    c.gameObject.GetComponent<Rigidbody>().AddExplosionForce(WeaponsList[CurrentWeaponIndex].ImpactForce, hitLocation, 10.0f);
                                }
                            }
                        }
                        else if (hit.rigidbody != null)
                        {
                            hit.rigidbody.AddForce(-hit.normal * WeaponsList[CurrentWeaponIndex].ImpactForce);
                        }

                    }

                }
                
                //Decrement the ammo count
                WeaponsList[CurrentWeaponIndex].WeaponAmmoLoaded--;
            }
            else if (MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].WeaponAmmoType) > 0)
            {
                //Automatically reload if the player attempts to fire the weapon 
                //provided they have more ammunition
                ReloadWeapon();
            }
        }

    }

    private void SpawnBullet()
    {
        GameObject bulletTemp = Instantiate(WeaponsList[CurrentWeaponIndex].AmmoObject, CurrentGun.transform.GetChild(0).transform.position + CurrentGun.transform.GetChild(0).transform.forward, CurrentGun.transform.GetChild(0).transform.rotation);
        bulletTemp.GetComponent<ProjectileController>().DamageAmount = WeaponsList[CurrentWeaponIndex].WeaponDamage;
        bulletTemp.GetComponent<ProjectileController>().Fire(CurrentGun.transform.GetChild(0).transform.forward, WeaponsList[CurrentWeaponIndex].BulletForce, WeaponsList[CurrentWeaponIndex].WeaponRange);
    }


    /// <summary>
    /// This function reloads the weapon if there is enough spare ammo for it
    /// </summary>
    public void ReloadWeapon()
    {
        //Ensure there is spare ammo to load into the weapon
        if (MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].WeaponAmmoType) > 0)
        {
            //Take the ammo from the clip and add it to the spare ammo counter (prevents the current mag being lost in the reload)

            MyStats.AddAmmo(new AmmunitionType(WeaponsList[CurrentWeaponIndex].WeaponAmmoType, WeaponsList[CurrentWeaponIndex].WeaponAmmoLoaded));
            WeaponsList[CurrentWeaponIndex].WeaponAmmoLoaded = 0;

            //If the player has more ammo than can be loaded into the magazine
            //Add the total magazine capacity to ammo in clip, and subtract it from the spare ammo counter
            if (MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].WeaponAmmoType) > WeaponsList[CurrentWeaponIndex].WeaponMagCapacity)
            {
                WeaponsList[CurrentWeaponIndex].WeaponAmmoLoaded = WeaponsList[CurrentWeaponIndex].WeaponMagCapacity;
                MyStats.ConsumeAmmo(new AmmunitionType(WeaponsList[CurrentWeaponIndex].WeaponAmmoType, WeaponsList[CurrentWeaponIndex].WeaponMagCapacity));
            }
            else
            {
                //If the player has less than the magazine capacity, add all their spare ammo into the clip
                //and set the spare counter to 0
                WeaponsList[CurrentWeaponIndex].WeaponAmmoLoaded = MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].WeaponAmmoType);
                MyStats.ConsumeAmmo(new AmmunitionType(WeaponsList[CurrentWeaponIndex].WeaponAmmoType, MyStats.GetAmmoCount(WeaponsList[CurrentWeaponIndex].WeaponAmmoType)));
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

   
}



