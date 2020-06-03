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
    }

    //The currently equipped weapon
    [SerializeField] WeaponType CurrentlyEquippedWeapon;

    //A list of the stats for each possible weapon
    [SerializeField] List<WeaponStats> WeaponsList = new List<WeaponStats>();

    public float CurrentAmmoCount { get; private set; }

    private RaycastHit Target;
    [SerializeField] LayerMask WeaponMask;

    private int CurrentWeaponIndex = -1;

    [SerializeField] GameObject GunHolder;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetWeaponIndex()
    {
        for(int i = 0; i < WeaponsList.Count; i++)
        {
            if(WeaponsList[i].Type == CurrentlyEquippedWeapon)
            {
                CurrentWeaponIndex = i;
                return;
            }
        }
        return;
    }
    //This function handles the use of the currently equipped weapon
    void UseWeapon()
    {
        //If there is still ammo in the magazine
        if (CurrentAmmoCount > 0 && CurrentWeaponIndex != -1)
        {
            //Calculate the direction the player is aiming at
            //FIXME change origin to the tip of the weapon
            Vector3 origin = Camera.main.transform.position;
            Vector3 destination = Camera.main.transform.forward * WeaponsList[CurrentWeaponIndex].Range;
            Vector3 FireDirection = (destination - origin).normalized;

            Physics.Raycast(origin, FireDirection, out Target, WeaponsList[CurrentWeaponIndex].Range, WeaponMask);


            GameObject projectile = Instantiate(WeaponsList[CurrentWeaponIndex].ProjectilePrefab, transform.position, transform.rotation);
            projectile.GetComponent<ProjectileController>().Force = new Vector3(10, 10, 10);
            projectile.GetComponent<ProjectileController>().Direction = FireDirection;
            projectile.GetComponent<ProjectileController>().Fire();


        }
        else if(CurrentWeaponIndex == -1)
        {
            Debug.LogError("Invalid weapon selected in WeaponController component of " + transform.name);
        }
    }
}
