using System.Collections;
using UnityEngine;

/// <summary>
/// This script holds the relevant stats for each character in the game and is used to track their supplies
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("Core Stats")]
    [SerializeField] public int Health = 100;
    [SerializeField] public int Stamina = 100;
    [SerializeField] public int MaxHealth = 100;
    [SerializeField] public int MaxStamina = 100;

    [Space()]
    [SerializeField] public bool CanPassiveRegen = true;
    [SerializeField] public int HealthRegenRate = 1;
    [SerializeField] public int HealthRegenAmount = 5;
    [SerializeField] public int StaminaRegenRate = 5;
    [SerializeField] public int StaminaRegenAmount = 5;


    public bool IsDead { get; set; }

    [Header("Ammunition")]
    [SerializeField] public int PistolAmmo = 10;
    [SerializeField] public int ShotgunAmmo = 10;
    [SerializeField] public int SMGAmmo = 10;
    [SerializeField] public int RifleAmmo = 10;
    [SerializeField] public int LauncherAmmo = 10;

    [Header("Throwables")]
    [SerializeField] public int GrenadeCount = 10;

    private void Start()
    {
        IsDead = false;


        StartCoroutine(HealthRegen());

        StartCoroutine(StaminaRegen());
    }

    public void TakeDamage(int dmg)
    {
        Health -= dmg;

        if (Health <= 0)
        {
            Health = 0;
            IsDead = true;
        }
    }

    public void RestoreHealth(int amount)
    {
        Health += amount;

        if (Health >= MaxHealth)
        {
            Health = MaxHealth;
        }
    }

    public void RestoreStamina(int amount)
    {
        Stamina += amount;

        if (Stamina >= MaxStamina)
        {
            Stamina = MaxStamina;
        }
    }

    public void ConsumeStamina(int amount)
    {
        Stamina -= amount;

        if (Stamina <= 0)
        {
            Stamina = 0;
        }
    }

    /// <summary>
    /// This function is used to add ammo to the character of the desired type and amount
    /// </summary>
    public void AddAmmo(AmmunitionType ammo)
    {
        switch (ammo.Type)
        {
            case AmmunitionType.AmmoType.Pistol:
                PistolAmmo += ammo.Amount;
                break;
            case AmmunitionType.AmmoType.SMG:
                SMGAmmo += ammo.Amount;
                break;
            case AmmunitionType.AmmoType.Rifle:
                RifleAmmo += ammo.Amount;
                break;
            case AmmunitionType.AmmoType.Shotgun:
                ShotgunAmmo += ammo.Amount;
                break;
            case AmmunitionType.AmmoType.Launcher:
                LauncherAmmo += ammo.Amount;
                break;
            case AmmunitionType.AmmoType.Grenade:
                GrenadeCount += ammo.Amount;
                break;
            default:
                Debug.LogError("Invalid ammo type being added to " + transform.name);
                break;
        }
    }


    /// <summary>
    /// This function is used to subtract ammo to the character of the desired type and amount
    /// </summary>
    public void ConsumeAmmo(AmmunitionType ammo)
    {
        switch (ammo.Type)
        {
            case AmmunitionType.AmmoType.Pistol:
                PistolAmmo -= ammo.Amount;
                break;
            case AmmunitionType.AmmoType.SMG:
                SMGAmmo -= ammo.Amount;
                break;
            case AmmunitionType.AmmoType.Rifle:
                RifleAmmo -= ammo.Amount;
                break;
            case AmmunitionType.AmmoType.Shotgun:
                ShotgunAmmo -= ammo.Amount;
                break;
            case AmmunitionType.AmmoType.Launcher:
                LauncherAmmo -= ammo.Amount;
                break;
            case AmmunitionType.AmmoType.Grenade:
                GrenadeCount -= ammo.Amount;
                break;
            default:
                Debug.LogError("Invalid ammo type being consumed by " + transform.name);
                break;
        }
    }


    /// <summary>
    /// This function is used to get the current ammo count of the character of the desired type 
    /// </summary>
    public int GetAmmoCount(AmmunitionType.AmmoType type)
    {
        switch (type)
        {
            case AmmunitionType.AmmoType.Pistol:
                return PistolAmmo;
            case AmmunitionType.AmmoType.SMG:
                return SMGAmmo;
            case AmmunitionType.AmmoType.Rifle:
                return RifleAmmo;
            case AmmunitionType.AmmoType.Shotgun:
                return ShotgunAmmo;
            case AmmunitionType.AmmoType.Launcher:
                return LauncherAmmo;
            case AmmunitionType.AmmoType.Grenade:
                return GrenadeCount;
            default:
                Debug.LogError("Invalid ammo type being added to " + transform.name);
                return 0;
        }
    }


    IEnumerator HealthRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(HealthRegenRate);

            if (CanPassiveRegen)
            {
                RestoreHealth(HealthRegenAmount);
            }
        }
    }

    IEnumerator StaminaRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(StaminaRegenRate);

            RestoreStamina(StaminaRegenAmount);
        }
    }


    public virtual void Die()
    {
        Debug.Log(transform.name + " is dead");
    }
}

