using UnityEngine;

[CreateAssetMenu]
public class Weapon : ScriptableObject
{
    public enum WeaponType { Pistol, SMG, Rifle, Shotgun, Launcher };
    public enum AmmoSpawn { Always, SlowMoOnly, Never};


    [Header("Weapon Data")]
    public string WeaponName;
    public WeaponType Type;
    public GameObject WeaponObject;
    public int WeaponDamage;
    public int WeaponRange;
    public float ImpactForce = 100.0f;
    public bool CanAimDownSights = true;
    
    [Header("Audio Settings")]
    public AudioClip ReloadSound;
    public AudioClip FireSound;

    public GameObject MuzzleFlash;


    [Header("Ammunition Data")]
    [Range(0.1f, 1.0f)]
    public float WeaponFireRate = 0.1f;
    public AmmunitionType.AmmoType WeaponAmmoType;
    public int WeaponMagCapacity;
    public int WeaponAmmoLoaded;
    public AmmoSpawn SpawnAmmo;
    public GameObject AmmoObject;
    public float BulletForce = 1000.0f;
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