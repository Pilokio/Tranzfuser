
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyStats : CharacterStats
{
    private void Update()
    {
        if (IsDead == true)
        {
            Die();
        }
    }

    public void SetAllAmmoCounts(int amount)
    {
        PistolAmmo = amount;
        SMGAmmo = amount;
        RifleAmmo = amount;
        ShotgunAmmo = amount;
        LauncherAmmo = amount;
        GrenadeCount = amount;
    }


    public override void Die()
    {
        base.Die();

        //Debug.Log("I am dead");
        //Ragdoll goes here
        //GetComponent<EnemyController>().enabled = false;
        //GetComponent<NavMeshAgent>().enabled = false;
        //GetComponent<WeaponController>().enabled = false;
        //GetComponent<Rigidbody>().freezeRotation = false;
        //GetComponent<Rigidbody>().isKinematic = false;
        //GetComponent<Rigidbody>().useGravity = true;
        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        //Destroy object
        // Destroy(gameObject);
    }
}
