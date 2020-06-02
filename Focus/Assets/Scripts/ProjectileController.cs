using System.Collections;
using System.Collections.Generic;
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
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    public void Fire()
    {
        rb.AddForce(new Vector3(Direction.x * Force.x, Direction.y * Force.y, Direction.z * Force.z));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("I hit something");
        Destroy(this.gameObject);
    }
}
