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

    // Start is called before the first frame update
    void Start()
    {
        Target = PlayerManager.Instance.Player.transform;
        Agent = GetComponent<NavMeshAgent>();
        LineOfSightTimer = TimeToLose;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }
    [SerializeField] LayerMask DetectionMask;

    [SerializeField] float VisionConeAngle = 45;
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int TargetPatrolPoint = 0;
    RaycastHit hit;

    [SerializeField] float TimeToLose = 2.0f;
    float LineOfSightTimer = 0.0f;


    // Update is called once per frame
    void Update()
    {   
        //Calculate the distance between the player and the enemy
        float distance = Vector3.Distance(Target.position, transform.position);
        
        //Calculate the direction of the player in relation to the enemy
        Vector3 Heading = Target.position - transform.position;
        //Using the heading, calculate the angle between it and the current Look Direction (the forward vector)
        float angle = Vector3.Angle(transform.forward, Heading);

        //If the calculated angle is less than the defined threshold
        //Then the player is within the edges of the cone of vision
        if(angle < VisionConeAngle)
        {
            //Raycast to determine if the enemy can see the player from its current position
            //This accounts for the detection range and line of sight
            if (Physics.Raycast(transform.position + new Vector3(0, 1.25f, 0), Heading, out hit, DetectionRange, DetectionMask))
            {
                //If the raycast hits the player then they must be in range, with a clear line of sight
                if (hit.transform.tag == "Player")
                {
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
                    Debug.Log("Searching");
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
                        Debug.Log("You can run but you cant hide");
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
