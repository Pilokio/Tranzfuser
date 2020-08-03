using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyUtility
{
    /// <summary>
    /// This enum determines the state the enemy is in and what actions they can take.
    /// </summary>
    public enum AlertState
    {
        Idle = 0,
        Suspicious = 1,
        Hostile = 2
    }

    public enum DecisionType
    {
        TakeCover = 0,
        MoveTowardsTarget = 1,
        Retreat = 2,
        StopAndAttack = 3,
        RunAndGun = 4
    }

    /// <summary>
    /// This enum determines the type of combat this enemy will perform
    /// </summary>
    public enum EnemyType
    {
        Soldier = 0,
        Berzerker = 1,
        Tank = 2,
        Sniper = 3,
        Boss = 4,
        Custom = 5
    }

    /// <summary>
    /// This function creates a cone mesh for the cone of vision gizmo
    /// </summary>
    public static Mesh CreateViewCone(float aAngle, float aDistance, int aConeResolution = 30)
    {
        Vector3[] verts = new Vector3[aConeResolution + 1];
        Vector3[] normals = new Vector3[verts.Length];
        int[] tris = new int[aConeResolution * 3];
        Vector3 a = Quaternion.Euler(-aAngle, 0, 0) * Vector3.forward * aDistance;
        Vector3 n = Quaternion.Euler(-aAngle, 0, 0) * Vector3.up;
        Quaternion step = Quaternion.Euler(0, 0, 360f / aConeResolution);
        verts[0] = Vector3.zero;
        normals[0] = Vector3.back;
        for (int i = 0; i < aConeResolution; i++)
        {
            normals[i + 1] = n;
            verts[i + 1] = a;
            a = step * a;
            n = step * n;
            tris[i * 3] = 0;
            tris[i * 3 + 1] = (i + 1) % aConeResolution + 1;
            tris[i * 3 + 2] = i + 1;
        }
        Mesh m = new Mesh();
        m.vertices = verts;
        m.normals = normals;
        m.triangles = tris;
        m.RecalculateBounds();
        return m;
    }
}

[CreateAssetMenu]
public class EnemyProfile : ScriptableObject
{
    //The enemy type detemines the types of attacks this enemy can perform
    //as well as having an influence on their proficiency at performing actions
    [Header("Core Settings")]
    [Range(1, 3)]
    [SerializeField] public int Bravery = 1; //Determines how close the enemy can be before retreating
    [Range(1, 3)]
    [SerializeField] public int Aggresiveness = 1; //Determines whether the enemy will prioritise attacking over self preservation
    [Range(1, 3)]
    [SerializeField] public int Determination = 1; //Determines how long the enemy will search for before giving up
    [Range(1, 3)]
    [SerializeField] public int Awareness = 1; //Controls how well the enemy can see players (ie detection range) //NB May not be needed anymore FIXME
    [Range(1, 3)]
    [SerializeField] public int Swiftness = 1; //Controls the speed of the enemy (ie movement, climbing, etc.)
    [Range(1, 3)]
    [SerializeField] public int SuccessChance = 1; //Determines how likely is the enemy to be successful in their actions

    [Header("Combat Settings")]
    [SerializeField] public int BaseMaxHealth = 100; //The amount of health the enemy type will have before difficulty scaling
    [Space]
    [SerializeField] public bool HasTankBehaviour = false;
    [Space]
    [SerializeField] public EnemyUtility.DecisionType PassiveOption;
    [SerializeField] public EnemyUtility.DecisionType AggressiveOption;


    [Header("Detection Settings")]
    //The field of view angle where 180 is equivalent to full awareness
    [Range(25, 180)]
    [SerializeField] public float DefaultDetectionAngle = 35.0f; //The range at which hostiles can be seen (Multiplied by Awareness stat ?)
    [Range(20, 60)]
    [SerializeField] public float DefaultDetectionRange = 20.0f;

    //The amount of time before a new decision will be made
    //If equal to 0, new decisions will be made purely based on changes in circumstance
    [SerializeField] public float DecisionTime = 2.0f; 
}
