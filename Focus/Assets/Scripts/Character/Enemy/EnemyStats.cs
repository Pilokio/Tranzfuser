
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyStats : CharacterStats
{
    [SerializeField] Animator MyAnimator;
    private void Start()
    {
        SetRigidbodyState(true);
        SetColliderState(true);
        MyAnimator.enabled = true;

    }

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
        GetComponent<NavMeshAgent>().SetDestination(transform.position);
        MyAnimator.StopPlayback();
        MyAnimator.enabled = false;
        SetRigidbodyState(false);
        SetColliderState(true);
        //Destroy(gameObject, 3f);



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

    void SetRigidbodyState(bool state)
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = state;
        }

        GetComponent<Rigidbody>().isKinematic = !state;
    }

    void SetColliderState(bool state)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = state;
        }

        GetComponent<Collider>().enabled = !state;
    }
}
