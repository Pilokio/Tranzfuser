using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private NavMeshAgent Agent;


    public enum EnemyState
    {
        Patrol = 0,
        Run = 1,
        Shoot = 2
    }

    [SerializeField] EnemyState CurrentState = EnemyState.Patrol;


    [SerializeField] float DetectionRange = 10.0f;
    [SerializeField] float AttackRange = 5.0f;

    private bool TargetSighted = false;

    Transform Target;

    Mesh ConeOfVisionDebugMesh;

    // Start is called before the first frame update
    void Start()
    {
        Target = PlayerManager.Instance.Player.transform;
        Agent = GetComponent<NavMeshAgent>();
        LineOfSightTimer = TimeToLose;
    }

    Mesh CreateViewCone(float aAngle, float aDistance, int aConeResolution = 30)
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
    private void OnValidate()
    {
        ConeOfVisionDebugMesh = CreateViewCone(VisionConeAngle, DetectionRange, 10);
    }

    private void OnDrawGizmosSelected()
    {
         Gizmos.color = Color.red;
        foreach(Transform target in PatrolPoints)
        {
            Gizmos.DrawWireSphere(target.position, 2.0f);
        }

        if(ConeOfVisionDebugMesh == null)
        {
            ConeOfVisionDebugMesh = CreateViewCone(VisionConeAngle, DetectionRange, 10);
        }

        Gizmos.DrawWireMesh(ConeOfVisionDebugMesh, 0, transform.position + EyePosition, transform.rotation);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position + transform.TransformDirection( EyePosition), 0.5f);


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + EyePosition, Heading * DetectionRange);

    }
    [SerializeField] LayerMask DetectionMask;

    [SerializeField] float VisionConeAngle = 45;
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int TargetPatrolPoint = 0;
    RaycastHit hit;
    [SerializeField] Vector3 EyePosition = new Vector3();
    [SerializeField] float TimeToLose = 2.0f;
    float LineOfSightTimer = 0.0f;

    Vector3 Heading = new Vector3();
    // Update is called once per frame
    void Update()
    {   
        //Calculate the distance between the player and the enemy
        float distance = Vector3.Distance(Target.position, transform.position);
        
        //Calculate the direction of the player in relation to the enemy
        Heading = Target.position - (transform.position + EyePosition);
        //Using the heading, calculate the angle between it and the current Look Direction (the forward vector)
        float angle = Vector3.Angle(transform.forward, Heading);

        //If the calculated angle is less than the defined threshold
        //Then the player is within the edges of the cone of vision
        if(angle < VisionConeAngle)
        {
            //Raycast to determine if the enemy can see the player from its current position
            //This accounts for the detection range and line of sight
            if (Physics.Raycast(transform.position + EyePosition, Heading, out hit, DetectionRange, DetectionMask))
            {
                if(hit.transform.gameObject.layer != 8)
                    Debug.Log("I see " + hit.transform.name);
                
                //If the raycast hits the player then they must be in range, with a clear line of sight
                if (hit.transform.tag == "Player")
                {
                    Debug.Log("I see you");
                    //Reset line of sight timer
                    LineOfSightTimer = TimeToLose;
                    TargetSighted = true;
                }
                else
                {
                    TargetSighted = false;
                }
            }
        }




        switch (CurrentState)
        {
            case EnemyState.Patrol: //move between the different patrol points until a target is detected
                if(PatrolPoints.Count > 1)
                {
                    //Move to the current target patrol point
                    Agent.SetDestination(PatrolPoints[TargetPatrolPoint].position);

                    //If the enemy arrives at the patrol point, move to the next one
                    if(Vector3.Distance(transform.position, PatrolPoints[TargetPatrolPoint].position) < Agent.stoppingDistance)
                    {
                        TargetPatrolPoint++;
                        if (TargetPatrolPoint >= PatrolPoints.Count)
                        {
                            TargetPatrolPoint = 0;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Not enough patrol points for " + transform.name + ". Unable to patrol.");
                }

                //If the player is seen while patrolling, chase them
                if(TargetSighted)
                {
                    CurrentState = EnemyState.Run;
                }
                break;
            case EnemyState.Run: //Chase the target until they are within attack range (determined by currently equipped weapon)

                //Move to the player 
                Agent.SetDestination(Target.position);

                if(!TargetSighted)
                {
                    LineOfSightTimer -= Time.deltaTime;
                }


                if(LineOfSightTimer <= 0.0f)
                {
                    Debug.Log("Must've been the wind");
                    CurrentState = EnemyState.Patrol;
                }

                //If the target is within attack range, attack
                if (distance <= AttackRange && TargetSighted)
                {
                    //Stop moving
                    Agent.SetDestination(transform.position);
                    //Move to attack state
                    CurrentState = EnemyState.Shoot;
                }

                break;
            case EnemyState.Shoot: //Attack the target until they are dead, or line of sight is lost

                if (TargetSighted)
                {
                    //If the target is within attack range, attack
                    if (distance <= AttackRange)
                    {
                        Debug.Log("Bang");
                        FaceTarget();
                    }
                    else
                    {
                        CurrentState = EnemyState.Run;
                    }
                }
                else
                {
                    Debug.Log("I have lost the target.");
                    CurrentState = EnemyState.Run;
                }
                break;
            default:
                break;
        }






    }

    float TurnSpeed = 5.0f;

    void FaceTarget()
    {
        Vector3 direction = (Target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * TurnSpeed);
    }
}
