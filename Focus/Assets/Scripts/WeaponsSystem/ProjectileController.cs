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
