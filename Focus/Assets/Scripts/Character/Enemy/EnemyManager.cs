using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyManager : MonoBehaviour
{

    #region Singleton

    public static EnemyManager Instance;

    private void Start()
    {
        Instance = this;

        Player = PlayerManager.Instance.Player;
        PlayerStats = Player.GetComponent<CharacterStats>();
        InitEnemyList();
        TotalEnemyCount = EnemyList.Count();
    }

    #endregion


    public GameObject Player { get; set; }
    public CharacterStats PlayerStats { get; set; }

    public int TotalEnemyCount { get; private set; }

    List<EnemyData> EnemyList = new List<EnemyData>();



    public void InitEnemyList()
    {
        //Get a list of all the enemies in the current level
        GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");

        foreach(GameObject g in enemyList)
        {
            EnemyList.Add(new EnemyData(g));
        }
    }




    public List<GameObject> GetDeadEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        //This will get all the enemies from the list that are both dead and not already checked
        EnemyData[] data = EnemyList.Where(x => x.IsDead() == true && x.Checked == false).ToArray();
        //Add their game objects to the list to be returned
        foreach(EnemyData d in data)
        {
            enemies.Add(d.GetGameObject());
        }
       
        return enemies;
    }

    public List<GameObject> GetHostileEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        EnemyData[] data = EnemyList.Where(x => x.IsHostile() == true).ToArray();

        foreach (EnemyData item in data)
        {
            enemies.Add(item.GetGameObject());
        }

        return enemies;
    }

    public void SetDeadEnemyAsDiscovered(GameObject enemy)
    {
        EnemyList.Where(x => x.GetGameObject() == enemy).First().Checked = true;
    }
}

class EnemyData
{
    private GameObject Enemy;
    private EnemyController Controller;
    private EnemyStats Stats;

    public bool Checked = false;
    public EnemyData(GameObject gameObject)
    {
        Enemy = gameObject;
        Controller = gameObject.GetComponent<EnemyController>();
        Stats = gameObject.GetComponent<EnemyStats>();
    }

    public GameObject GetGameObject()
    {
        return Enemy;
    }

    public bool IsDead()
    {
        return Stats.IsDead;
    }

    public bool IsHostile()
    {
        if(Controller.AlertStatus == EnemyUtility.AlertState.Hostile)
        {
            return true;
        }

        return false;
    }
}