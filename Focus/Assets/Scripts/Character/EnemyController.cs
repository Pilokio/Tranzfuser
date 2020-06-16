using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(WeaponController))]
public class EnemyController : MonoBehaviour
{
    private NavMeshAgent Agent;

    [Header("General Settings")]
    [SerializeField] AlertState AlertStatus = 0;
    
    [Header("Combat Settings")]
    //The minimum attack range
    [SerializeField] float AttackRange = 5.0f;
    [Range(1, 3)]
    [SerializeField] int Aggresiveness = 1;
    [Range(1, 3)]
    [SerializeField] int Bravery = 1;
    [SerializeField] CombatType EnemyType = 0;

    [Header("Detection Settings")]
    //The range at which hostiles can be seen
    [SerializeField] float DetectionRange = 10.0f;
    //The field of view angle
    [SerializeField] float VisionConeAngle = 45;
    //Layer mask outlining what can block the raycast. (Typically everything apart from itself)
    [SerializeField] LayerMask DetectionMask = new LayerMask();
    //The position of the "eye". Where the cone of vision comes to a point. 
    [SerializeField] Vector3 EyePosition = new Vector3();
    //The amount of time before a hostiles will be "lost" after losing line of sight
    [SerializeField] float TimeToLose = 2.0f;
    [SerializeField] float SearchTime = 2.0f;

    [Header("Patrol Settings")]
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int TargetPatrolPoint = 0;


   


  
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

    //Determine if a hostile has been sighted
    private bool TargetSighted = false;

    //The hostile's transform
    //Always store the player's position to perform raycast at them
    Transform Target;

    //Mesh used for drawing the wireframe gizmo for the cone of vision
    Mesh ConeOfVisionDebugMesh;


    //Used for debug
    public bool CanMove = false;

    // Start is called before the first frame update
    void Start()
    {
        //Get the wepon controller component
        MyWeaponController = GetComponent<WeaponController>();
        //Store the player transform to determine LOS at any point
        Target = PlayerManager.Instance.Player.transform;
        //Store the navmesh agent component for movement through the level
        Agent = GetComponent<NavMeshAgent>();
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
        ConeOfVisionDebugMesh = Utility.CreateViewCone(VisionConeAngle, DetectionRange, 10);
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
            ConeOfVisionDebugMesh = Utility.CreateViewCone(VisionConeAngle, DetectionRange, 10);
        }

        Gizmos.DrawWireMesh(ConeOfVisionDebugMesh, 0, transform.position + EyePosition, transform.rotation);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position + transform.TransformDirection( EyePosition), 0.5f);


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + EyePosition, Heading * DetectionRange);

    }


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
    // Update is called once per frame
    void Update()
    {
        //If the player is detected, move straight to the hostile state
        TargetSighted = DetectPlayer();

        if (TargetSighted)
        {
            AlertStatus = AlertState.Hostile;
        }



        switch (AlertStatus)
        {
            case AlertState.Idle:

                Debug.Log("I am Idle");
                // Patrol the designated patrol points

                if(PatrolPoints.Count > 1)
                {
                    //Move to the current target patrol point
                    if(CanMove)
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

                //This state could also be used to allow the enemies to interact with the world if desired
                //ie sit on chairs, lean on railings/walls, talk to each other
                break;
            case AlertState.Suspicious:
                //Investigate disturbances.
                Debug.Log("I am Suspicious");

                //If hostile was in combat and is no longer seen
                //Go to last known location and search

                //If dead body is found or gunfire is heard
                //Move to disturbance location and search
                break;
            case AlertState.Hostile:
                Debug.Log("I am Hostile");

                //Attack the target until they are dead, or line of sight is lost
                Combat();
                break;
            default:
                //Unreachable code
                Debug.LogError("Invalid alert status on enemy :" + transform.name);
                break;
        }






    }


    void Combat()
    {
        //Calculate the distance between the player and the enemy
        float distance = Vector3.Distance(Target.position, transform.position);

        //If the player cant be seen after the timer reaches zero then become suspicious
        if (!TargetSighted)
        {
            LineOfSightTimer -= Time.deltaTime;

            if (LineOfSightTimer <= 0.0f)
            {
                AlertStatus = AlertState.Suspicious;
            }
        }

        // Determine what action should be taken here.

        switch (EnemyType)
        {
            case CombatType.Grunt:
                //Take cover
                //Calc % chance to move or fire
                //move to new pos if compromised
                //take fire if desired
                //reload when necessary, behind cover
                break;
            case CombatType.Aggressor:
                //Move towards hostile
                //Open fire when possible
                //Reload if necessary
                //Take cover if low on health


                MoveInAndAttack(distance, false, true);





                break;
            case CombatType.Tank:
                //Move towards hostile
                //Open fire if possible
                //reload when necessary

                MoveInAndAttack(distance, true, true);

                break;
            case CombatType.Sniper:
                //Find a suitable vantage point
                //Move to vantage point
                //If hostile can be seen and you can shoot, open fire
                    //Calc %hit chance
                        //Either hit or miss
                //else hide behind cover
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


    float MinAttackDistance = 5.0f;

    /// <summary>
    /// This function tells the enemy to move towards the player and attack when in range
    /// </summary>
    void MoveInAndAttack(float distance, bool stopWhenInRange, bool SuccessfulHit)
    {
        //Move to the player 
        if(CanMove)
            Agent.SetDestination(Target.position);


        //If the target is within attack range, stop moving & attack
        if (distance <= AttackRange && TargetSighted)
        {
            if (stopWhenInRange || distance < MinAttackDistance)
            {
                //Stop moving
                if(CanMove)
                    Agent.SetDestination(transform.position);
            }

            Debug.Log("Bang");
            FaceTarget();

            //Calculate % chance of a successful hit

            //If hit is successful, fire at the hostile
            if (SuccessfulHit)
            {
                MyWeaponController.UseWeapon(Target.position);
            }
            else
            {
                Debug.Log("I missed");
            }
            //Else, fire try to miss
            //MyWeaponController.UseWeapon(Target.position + Small random vector);
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


    /// <summary>
    /// This enum describes the different enemy combat behaviours available.
    /// The primary differences in these behaviours are the distance they maintain from hostiles when attacking,
    /// the speed and frequency with which they move, the weapons available to them, the damage they deal,
    /// and the maximum health they have.
    /// </summary>
    enum CombatType
    {
        Grunt       = 0,  //Standard enemy. Medium range. Move somewhat frequently at standard speed. Takes cover just as much as they attack. Medium HP and DMG.
        Aggressor   = 1,  //Berserker-type enemy. Close range. Charge at the hostiles - Fast moving, doesnt take cover. Low DMG. Medium HP.
        Tank        = 2,  //High HP. Medium DMG. Medium - Close range. Doesnt take cover, but moves slowly.
        Sniper      = 3,  //Low HP. High DMG. Long range. Rarely moves from vantage point, almost always in cover.
    }

}
