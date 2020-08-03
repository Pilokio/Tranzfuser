using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    #region Singleton

    public static EnemyManager Instance;

    private void Awake()
    {
        Instance = this;


        //Get a list of all the enemies in the current level
        GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");

        //Loop through each of them and add them to the enemy list
        //and add their stats component to the enemies stats list
        //This saves on processing later as we shouldnt have to call GetComponent multiple times a frame
        foreach (GameObject enemy in enemyList)
        {
            EnemiesStats.Add(enemy.GetComponent<EnemyStats>());
            EnemiesControllers.Add(enemy.GetComponent<EnemyController>());
        }

        TotalEnemyCount = enemyList.Length;
        Player = PlayerManager.Instance.Player;
        PlayerStats = Player.GetComponent<CharacterStats>();
    }

    #endregion

    List<EnemyStats> EnemiesStats = new List<EnemyStats>();
    List<EnemyController> EnemiesControllers = new List<EnemyController>();

    public GameObject Player { get; set; }
    public CharacterStats PlayerStats { get; set; }
    public int TotalEnemyCount { get; private set; }


    public List<Vector3> GetAllDeadEnemyPositions()
    {
        List<Vector3> DeadEnemies = new List<Vector3>();

        foreach(EnemyStats enemy in EnemiesStats)
        {
            if(enemy.IsDead)
            {
                //Debug.Log(enemy.gameObject.name + " is currently dead");
                DeadEnemies.Add(enemy.gameObject.transform.position);
            }
        }

        return DeadEnemies;
    }

    public List<GameObject> GetAllDeadEnemyGameObjects()
    {
        List<GameObject> DeadEnemies = new List<GameObject>();

        foreach (EnemyStats enemy in EnemiesStats)
        {
            if (enemy.IsDead)
            {
                //Debug.Log(enemy.gameObject.name + " is currently dead");
                DeadEnemies.Add(enemy.gameObject);
            }
        }

        if (DeadEnemies == null || DeadEnemies.Count < 1)
            return new List<GameObject>();

        return DeadEnemies;
    }

    private void Update()
    {
        //Debug Only
        //GetAllHostileEnemyPositions();
        //GetAllDeadEnemyPositions();
    }

    public List<Vector3> GetAllHostileEnemyPositions()
    {
        List<Vector3> HostileEnemies = new List<Vector3>();

        foreach (EnemyController enemy in EnemiesControllers)
        {
            if (enemy.AlertStatus == EnemyUtility.AlertState.Hostile)
            {
                //Debug.Log(enemy.gameObject.name + " is currently hostile");
                HostileEnemies.Add(enemy.gameObject.transform.position);
            }
        }

        return HostileEnemies;
    }
}
