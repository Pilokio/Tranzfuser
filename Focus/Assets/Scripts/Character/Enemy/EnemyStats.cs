
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyStats : CharacterStats
{
#pragma warning disable 0649


    [SerializeField] Animator MyAnimator;
#pragma warning restore 0649

    private void Start()
    {
        SetRigidbodyState(true);
        SetColliderState(true);
        
        if(MyAnimator)
            MyAnimator.enabled = true;

        if (MyAnimator == null)
            Debug.LogError("MyAnimator has not been assigned.", this);

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

        GetComponent<EnemyController>().DisableColliders();
       // Destroy(gameObject, 60.0f);

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
