using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
  
        
        InitEnemyList();
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

        for (int i = 0; i < EnemyList.Count; i++)
        {
            if (EnemyList[i].Enemy.transform.GetComponent<EnemyStats>().IsDead)
            {
                EnemyList[i].IsDead = true;
            }

            if (EnemyList[i].Enemy.transform.GetComponent<EnemyController>().AlertStatus == EnemyUtility.AlertState.Hostile)
            {
                EnemyList[i].IsHostile = true;
            }
            else
            {
                EnemyList[i].IsHostile = false;
            }
        }
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


    public void InitEnemyList()
    {
        //Get a list of all the enemies in the current level
        GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");

        foreach(GameObject g in enemyList)
        {
            
        }
    }



    List<EnemyData> EnemyList = new List<EnemyData>();

    public List<GameObject> GetDeadEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        EnemyData[] data = EnemyList.Where(x => x.IsDead == true).ToArray();

        foreach(EnemyData d in data)
        {
            enemies.Add(d.Enemy);
        }
       
        return enemies;
    }
}

class EnemyData
{
    public GameObject Enemy;
    public bool IsDead;
    public bool IsFound;
    public bool IsHostile;

    private EnemyStats Stats;
    public EnemyData(GameObject gameObject, EnemyStats stats)
    {

    }
}