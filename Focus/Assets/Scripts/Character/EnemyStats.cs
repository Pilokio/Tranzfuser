public class EnemyStats : CharacterStats
{

    private void Update()
    {

        if (IsDead == true)
        {
            Die();
        }
    }

    public override void Die()
    {
        base.Die();

        //Ragdoll goes here

        //Destroy object
        Destroy(gameObject);
    }
}
