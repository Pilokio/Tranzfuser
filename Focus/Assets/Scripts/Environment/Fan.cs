using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public class Fan : BaseBehaviour
{

    [SerializeField] float speed = 500.0f;
    [SerializeField] int DamageAmount = 25;
    //[SerializeField] float KnockbackForce = 50.0f;

    
    private void Start()
    {
        //store non rote angles and  freeze in update ?
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Rotate(transform.right * speed * time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.GetComponent<CharacterStats>().TakeDamage(DamageAmount);

            //Vector3 FanPos = transform.position;
            //FanPos.y = collision.transform.position.y;

            //Vector3 Direction = FanPos - collision.transform.position;
            //Direction.Normalize();
            //Direction.z = 0;
            //Direction *= -1;

            //collision.transform.position += Direction * KnockbackForce;
            //collision.transform.GetComponent<Rigidbody>().AddForce(Direction * KnockbackForce, ForceMode.Impulse);
        }
    }
}
