using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Chronos;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(WeaponController))]
public class EnemyController : BaseBehaviour
{
    private NavMeshAgent Agent;

    [Header("General Settings")]
    AlertState AlertStatus = 0;

    [Header("Combat Settings")]
    //The minimum attack range
    float AttackRange = 5.0f;
    float RetreatDistance = 2.5f;
    //[SerializeField] CombatType EnemyType = 0;

    [Header("Detection Settings")]
    //The range at which hostiles can be seen
    float DetectionRange = 10.0f;
    //The field of view angle
    [SerializeField] float VisionConeAngle = 45;
    //Layer mask outlining what can block the raycast. (Typically everything apart from itself)
    [SerializeField] LayerMask DetectionMask = new LayerMask();
    //The position of the "eye". Where the cone of vision comes to a point. 
    [SerializeField] Vector3 EyePosition = new Vector3();
    //The amount of time before a hostiles will be "lost" after losing line of sight
    float TimeToLose = 2.0f;
    float SearchTime = 2.0f;




    [Header("Enemy B.A.D.A.S.S. Stats")]
    [Range(1, 3)]
    [SerializeField] int Bravery = 1; //Determines how close the enemy can be before retreating
    [Range(1, 3)]
    [SerializeField] int Aggresiveness = 1; //Determines whether the enemy will prioritise attacking over self preservation
    [Range(1, 3)]
    [SerializeField] int Determination = 1; //Determines how long the enemy will search for before giving up
    [Range(1, 3)]
    [SerializeField] int Awareness = 1; //Controls how well the enemy can see players (ie detection range)
    [Range(1, 3)]
    [SerializeField] int Swiftness = 1; //Controls the speed of the enemy (ie movement, climbing, etc.)
    [Range(1, 3)]
    [SerializeField] int SuccessChance = 1; //Determines how likely is the enemy to be successful in their actions

    [Header("Patrol Settings")]
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int TargetPatrolPoint = 0;



    private int PercentageHitChance = 100;


    //The timer for losing a hostile after LOS is lost
    float LineOfSightTimer = 0.0f;

    //The timer tracking how long the enemy has been searching for
    float SearchTimer;

    //Stores the hit from the raycase. 
    //If it is not the player then LOS is lost
    RaycastHit hit;

    //The target direction of the enemy. 
    //Used to determine if the player is in their cone of vision
    Vector3 Heading = new Vector3();


    //Store the weapon controller to allow the enemy to use their weapons
    WeaponController MyWeaponController;

    CharacterStats EnemyStats;

    //Determine if a hostile has been sighted
    private bool TargetSighted = false;

    //The hostile's transform
    //Always store the player's position to perform raycast at them
    Transform Target;

    //Mesh used for drawing the wireframe gizmo for the cone of vision
    Mesh ConeOfVisionDebugMesh;


    //Used for debug
    public bool CanMove = false;

    public bool IsClimbing = false;


    // Start is called before the first frame update
    void Start()
    {
        //Get the wepon controller component
        MyWeaponController = GetComponent<WeaponController>();
        //Store the player transform to determine LOS at any point
        Target = PlayerManager.Instance.Player.transform;
        //Store the navmesh agent component for movement through the level
        Agent = GetComponent<NavMeshAgent>();

        Init();
    }

    private void Awake()
    {
        AlertStatus = AlertState.Idle;
        //Init timers
        LineOfSightTimer = TimeToLose;
        SearchTimer = SearchTime;
    }

    private void OnValidate()
    {
        //DEBUG ONLY
        //
        //float minRetreatDist = 10 + (((3 - Bravery) - 1) * 5) - ((Aggresiveness - 1) * 2);
        //float maxRetreatDist = 15 + (((3 - Bravery) - 1) * 5) - ((Aggresiveness - 1) * 2);
        //result = (AttackRange / 4) + Random.Range(minRetreatDist, maxRetreatDist);
        //TimeToLose = Determination * Random.Range(5, 10);
        //DetectionRange = AttackRange * Awareness;
        //if(Agent != null)
        //    Agent.speed = Swiftness * 1.5f;

        ConeOfVisionDebugMesh = Utility.CreateViewCone(VisionConeAngle, DetectionRange, 10);
    }

    private void OnDrawGizmosSelected()
    {


        if (ConeOfVisionDebugMesh == null)
        {
            ConeOfVisionDebugMesh = Utility.CreateViewCone(VisionConeAngle, DetectionRange, 10);
        }

        Gizmos.DrawWireMesh(ConeOfVisionDebugMesh, 0, transform.position + EyePosition, transform.rotation);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(EyePosition), 0.5f);


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + EyePosition, Heading * DetectionRange);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Transform target in PatrolPoints)
        {
            Gizmos.DrawWireSphere(target.position, 2.0f);
        }
    }

    /// <summary>
    /// This function returns true if the player can be seen within the enemy's cone of vision
    /// </summary>
    /// <returns></returns>
    private bool DetectPlayer()
    {
        //Calculate the direction of the player in relation to the enemy
        Heading = Target.position - (transform.position + EyePosition);
        //Using the heading, calculate the angle between it and the current Look Direction (the forward vector)
        float angle = Vector3.Angle(transform.forward, Heading);

        //If the calculated angle is less than the defined threshold
        //Then the player is within the edges of the cone of vision
        if (angle < VisionConeAngle)
        {
            //Raycast to determine if the enemy can see the player from its current position
            //This accounts for the detection range and line of sight
            if (Physics.Raycast(transform.position + EyePosition, Heading, out hit, DetectionRange, DetectionMask))
            {
                //If the raycast hits the player then they must be in range, with a clear line of sight
                if (hit.transform.tag == "Player")
                {
                    //Reset line of sight timer
                    LineOfSightTimer = TimeToLose;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// This function makes use of the BADASS stats to randomise the enemy's behaviours
    /// </summary>
    private void Init()
    {

        //Set the attack range to the currently equipped weapon's range
        AttackRange = MyWeaponController.GetCurrentlyEquippedWeapon().WeaponRange;

        //Using the aggressiveness and bravery stat, determine a suitable range of values for the retreat distance
        float minRetreatDist = 10 + (((3 - Bravery) - 1) * 5) - ((Aggresiveness - 1) * 2);
        float maxRetreatDist = 15 + (((3 - Bravery) - 1) * 5) - ((Aggresiveness - 1) * 2);
        //Use a quarter of the weapon range combined with a random value within the calculated range
        //to determine the retreat distance
        RetreatDistance = Random.Range(minRetreatDist, maxRetreatDist);

        //Use the sight quality attribute to determine the detection range
        DetectionRange = AttackRange * Awareness;

        //Use the swiftness attribute to determine the enemy's move speed
        //TODO use this to alter climb speed/ weapons reload speed etc.
        Agent.speed = Swiftness * 1.5f;


        PercentageHitChance = SuccessChance * Random.Range(20, 30);

        TimeToLose = Determination * Random.Range(5, 10);

    }

    private void Update()
    {
        TargetSighted = DetectPlayer();

        switch (AlertStatus)
        {
            case AlertState.Idle:

                if (TargetSighted)
                {
                    Debug.Log("Hostile Located");
                    AlertStatus = AlertState.Hostile;
                }

                Patrol();
                break;
            case AlertState.Hostile:

                //If the player cannot be seen while hostile
                //Start timer
                //If timer gets below zero, give up and return to idle
                //If visual is regained while still hostile, resume combat and reset timer
                if (!TargetSighted)
                {
                    Debug.Log("Lost Visual of Hostile");
                    LineOfSightTimer -= Time.deltaTime;


                    if (LineOfSightTimer <= 0.0f)
                    {
                        Debug.Log("I have lost the hostile.");
                        AlertStatus = AlertState.Idle;
                    }
                }
                else if (LineOfSightTimer != TimeToLose)
                {
                    Debug.Log("Regained visual. Resetting timer.");
                    LineOfSightTimer = TimeToLose;
                }



                Combat();
                break;
        }
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    //If the player is detected, move straight to the hostile state
    //    TargetSighted = DetectPlayer();

    //    if (TargetSighted)
    //    {
    //        Debug.Log("Player found");
    //        LineOfSightTimer = TimeToLose;
    //        AlertStatus = AlertState.Hostile;
    //    }
    //    else
    //    {
    //        LineOfSightTimer -= Time.deltaTime;

    //        if (LineOfSightTimer <= 0.0f)
    //        {
    //            Debug.Log("Player Lost");
    //            LineOfSightTimer = TimeToLose;

    //            AlertStatus = AlertState.Idle; //Change to suspicious for stealth gameplay
    //        }
    //    }

    //    switch (AlertStatus)
    //    {
    //        case AlertState.Idle:

    //            // Patrol the designated patrol points

    //            if(PatrolPoints.Count > 1)
    //            {
    //                //Move to the current target patrol point
    //                if(CanMove)
    //                    Agent.SetDestination(PatrolPoints[TargetPatrolPoint].position);

    //                //If the enemy arrives at the patrol point, move to the next one
    //                if(Vector3.Distance(transform.position, PatrolPoints[TargetPatrolPoint].position) < Agent.stoppingDistance)
    //                {
    //                    TargetPatrolPoint++;
    //                    if (TargetPatrolPoint >= PatrolPoints.Count)
    //                    {
    //                        TargetPatrolPoint = 0;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                Debug.LogWarning("Not enough patrol points for " + transform.name + ". Unable to patrol.");
    //            }

    //            //This state could also be used to allow the enemies to interact with the world if desired
    //            //ie sit on chairs, lean on railings/walls, talk to each other
    //            break;
    //        case AlertState.Suspicious:
    //            //Investigate disturbances.

    //           //Get Search location (ie player last known location, origin of gunfire, dead body)
    //           //Pick a random position in a radius around the disturbance location
    //           //Move to this location.
    //           //Repeat until search timer is 0 or hostile found
    //           //Return to idle if not found in time limit, move to hostile if found
    //            break;
    //        case AlertState.Hostile:
    //            //Attack the target until they are dead, or line of sight is lost
    //            Combat();
    //            break;
    //        default:
    //            //Unreachable code
    //            Debug.LogError("Invalid alert status on enemy :" + transform.name);
    //            break;
    //    }
    //}

    void Patrol()
    {
        if (PatrolPoints.Count > 1)
        {
            //Move to the current target patrol point
            if (CanMove)
                Agent.SetDestination(PatrolPoints[TargetPatrolPoint].position);

            //If the enemy arrives at the patrol point, move to the next one
            if (Vector3.Distance(transform.position, PatrolPoints[TargetPatrolPoint].position) < Agent.stoppingDistance)
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
    }

    public bool StopWhenInRange = true;
    void Combat()
    {
        //Calculate the distance between the player and the enemy
        float distance = Vector3.Distance(Target.position, transform.position);

        StopMoving();

        if (distance > AttackRange)
        {
            MoveToTarget();
        }
        else if (distance < RetreatDistance)
        {
            TakeCover();
        }
        else
        {
            if (StopWhenInRange)
                StopMoving();

            Attack();
        }
    }


    float TurnSpeed = 5.0f;

    void FaceTarget()
    {
        Vector3 direction = (Target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * TurnSpeed);
    }




    void TakeCover()
    {
        //This allows the enemy to retreat when the player gets too close
        //TODO have the option to take cover here
        Agent.SetDestination(transform.position - (5 * transform.forward));
    }


    void MoveToTarget()
    {
        //Move to the player 
        if (CanMove)
            Agent.SetDestination(Target.position);
    }

    void StopMoving()
    {
        //Stop moving
        if (CanMove)
            Agent.SetDestination(transform.position);
    }


    void Attack()
    {
        FaceTarget();

        //Calculate % chance of a successful hit
        float chance = Random.Range(0, 100);
        bool SuccessfulHit = false;

        if (chance <= PercentageHitChance)
        {
            SuccessfulHit = true;
        }

        //If hit is successful, fire at the hostile
        if (SuccessfulHit)
        {
            // MyWeaponController.UseWeapon(EyePosition, (Target.position - EyePosition));
        }
        else
        {
            // MyWeaponController.UseWeapon(EyePosition, (Target.position - EyePosition) + Small random offset to fire near the target);
        }
    }


    /// <summary>
    /// This enum determines the state the enemy is in and what actions they can take.
    /// </summary>
    enum AlertState
    {
        Idle = 0,
        Suspicious = 1,
        Hostile = 2
    }
}
