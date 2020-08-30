using Chronos;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileController : MonoBehaviour
{
    [SerializeField] bool DebugMode = false;
    public float DamageAmount { get; set;}
    Vector3 StartingPosition = new Vector3();
    float DistanceTravelled = 0;   
    float MaxTravelDistance = 100;

    public bool IsExplosive = false;
    public ParticleSystem Explosion;
    public float ImpactForce = 0.0f;

    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<TrailRenderer>().enabled = DebugMode;

        StartingPosition = transform.position;
    }

    public void Fire(Vector3 Direction, float Force, float MaxRange)
    {
        MaxTravelDistance = MaxRange;
        GetComponent<Timeline>().rigidbody.AddForce(Direction * Force * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(DebugMode)
            Debug.Log("I have hit something. Destroying self.");
        
        if(IsExplosive)
        {
            GetComponent<MeshRenderer>().enabled = false;
            Explosion.GetComponent<ParticleSystem>();

            if(Explosion != null)
                Explosion.Play();

            Collider[] cols = Physics.OverlapSphere(transform.position, 10.0f);
            foreach (Collider c in cols)
            {
                if (c.gameObject.GetComponent<CharacterStats>())
                {
                    c.gameObject.GetComponent<CharacterStats>().TakeDamage((int)DamageAmount);
                }

                if (c.gameObject.GetComponent<Rigidbody>())
                {
                    c.gameObject.GetComponent<Rigidbody>().AddExplosionForce(ImpactForce, transform.position, 10.0f);
                }
            }   
            
            Invoke("Cleanup", 2.0f);
        }
        else
        {
            Cleanup();
        }

    }

    void Cleanup()
    {
        Destroy(this.gameObject);
    }

    private void Update()
    {
        DistanceTravelled = Vector3.Distance(StartingPosition, transform.position);

        if (DistanceTravelled >= MaxTravelDistance)
        {
            if (DebugMode)
                Debug.Log("Reached Max Range on weapon and hit nothing. Destroying self.");
           
            Destroy(this.gameObject);
            GetComponent<Timeline>().rigidbody.velocity = Vector3.zero;
        }
    }
}
