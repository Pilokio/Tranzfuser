using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] private Transform RespawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Player"))
        {
            Debug.Log("Respawning Player");

            other.transform.position = RespawnPoint.position;
            other.transform.rotation = RespawnPoint.rotation;
        }

        if(other.transform.CompareTag("Enemy"))
        {
            other.transform.GetComponent<EnemyStats>().TakeDamage(other.transform.GetComponent<EnemyStats>().MaxHealth);
        }
    }
}
