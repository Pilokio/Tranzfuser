using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int DamageAmount;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.GetComponent<CharacterStats>())
        {
            collision.transform.GetComponent<CharacterStats>().TakeDamage(DamageAmount);
        }
    }
}
