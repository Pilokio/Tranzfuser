public class EnemyStats : CharacterStats
{
    public override void Die()
    {
        base.Die();

        //Ragdoll goes here

        //Destroy object
        Destroy(gameObject);
    }
}
