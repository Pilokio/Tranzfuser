using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Transform RespawnPoint;
#pragma warning restore 0649

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Player"))
        {
            other.transform.position = RespawnPoint.position;
            other.transform.rotation = RespawnPoint.rotation;
        }

        if(other.transform.CompareTag("Enemy"))
        {
            other.transform.GetComponent<EnemyStats>().TakeDamage(other.transform.GetComponent<EnemyStats>().MaxHealth);
        }
    }
}
