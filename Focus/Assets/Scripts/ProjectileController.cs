using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileController : MonoBehaviour
{
    //The direction the projectile will travel
    public Vector3 Direction { get; set; }
    //The amount of force being applied to the projectile
    public Vector3 Force { get; set; }

    private Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    /// <summary>
    /// This function simply propels the projectile in the assigned direction using its assigned force value
    /// </summary>
    public void Fire()
    {
        rb.AddForce(new Vector3(Direction.x * Force.x, Direction.y * Force.y, Direction.z * Force.z) * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //TODO
        //Check the tag of the object it collides with and respond accordingly before destroying itself
        //ie Enemy would be ragdolled, surfaces would have an impact decal spawned, etc.


        if (collision.gameObject.tag != "Player" || collision.gameObject.layer != 9)
        {
            Debug.Log("I hit " + collision.gameObject.name);
            Destroy(this.gameObject);
        }
    }
}
