using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyKillCheck : MonoBehaviour
{
    [SerializeField] List<EnemyStats> EnemiesInRoom = new List<EnemyStats>();
    [SerializeField] GameObject LaserGrid;

    // Update is called once per frame
    void Update()
    {
        if (IsAllDead())
        {
            Debug.Log("All enemies dead");
            LaserGrid.SetActive(false);
        }
    }

    bool IsAllDead()
    {
        foreach (EnemyStats enemy in EnemiesInRoom)
        {
            if (!enemy.IsDead)
            {
                return false;
            }
        }

        return true;
    }
}
