using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Chronos;


//Debug Only Remove later
using UnityEngine.UI;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(WeaponController))]
public class EnemyController : BaseBehaviour
{
    [Header("Enemy Stats")]
    [Range(1, 5)]
    [SerializeField] int Difficulty = 1;
    [SerializeField] CombatType EnemyType = 0;
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

    [Header("Detection Settings")]
    [SerializeField] AlertState AlertStatus = 0;

    //The field of view angle
    [SerializeField] float VisionConeAngle = 45;
    //The minimum detection range of the cone of vision
    [SerializeField] float MinDetectionRange = 10;
    //Layer mask outlining what can block the raycast. (Typically everything apart from itself)
    [SerializeField] LayerMask DetectionMask = new LayerMask();
    //The position of the "eye". Where the cone of vision comes to a point. 
    [SerializeField] Vector3 EyePosition = new Vector3();
    //The amount of time before a hostiles will be "lost" after losing line of sight
    private float TimeToLose = 2.0f;
    private float SearchTime = 2.0f;
    //The timer for losing a hostile after LOS is lost
    private float LineOfSightTimer = 0.0f; 
    //The range at which hostiles can be seen
    float DetectionRange = 10.0f;
    //The timer tracking how long the enemy has been searching for
    private float SearchTimer = 0.0f;

    [Header("Patrol Settings")]
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int TargetPatrolPoint = 0;
   
    [Header("Combat Settings")]
    //The minimum attack range
    float AttackRange = 5.0f;
    float RetreatDistance = 2.5f;
    [Header("Incoming Damage Multipliers")]
    [SerializeField] float HeadshotDamagePercentage = 1.0f;
    [SerializeField] float BodyShotDamagePercentage = 0.75f;

    [Header("DEBUG ONLY")]
    //Debug Only Remove later
    public Slider DebugSlider;
    public bool CanMove = false;


    //The navmesh agent applied to this game object
    private NavMeshAgent Agent;
    //Default percentage chance of a successful hit
    private int PercentageHitChance = 100;

    //Stores the hit from the raycase. 
    //If it is not the player then LOS is lost
    private RaycastHit hit;

    //The target direction of the enemy. 
    //Used to determine if the player is in their cone of vision
    private Vector3 Heading = new Vector3();


    //Store the weapon controller to allow the enemy to use their weapons
    private WeaponController MyWeaponController;

    //The component storing the health, stamina, ammo, etc. of this enemy
    private EnemyStats EnemyStats;

    //Determine if a hostile has been sighted
    private bool TargetSighted = false;

    //The hostile's transform
    //Always store the player's position to perform raycast at them
    Transform Target;

    //Mesh used for drawing the wireframe gizmo for the cone of vision
    Mesh ConeOfVisionDebugMesh;

    public bool IsClimbing = false;
    private bool StopWhenInRange = true;
    float TurnSpeed = 5.0f;

    Collider HeadCollider;
    Collider BodyCollider;
    Collider DetectionSphere;

    //How long will the enemy stay in cover for
    [SerializeField] float CoverTime = 5.0f;
    public float CoverSearchRadius = 50.0f;
    public LayerMask CoverMask;
    public float MinCoverThreshold = 10.0f;

    //How long will the enemy stay in combat for
    [SerializeField] float AttackTime = 5.0f;
    private float DecisionTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        //Initialise the enemy parameters using the assigned stats
        Init();


        //Store the player transform to determine LOS at any point
        Target = PlayerManager.Instance.Player.transform;


        //Debug Only Remove later
        DebugSlider.maxValue = GetComponent<CharacterStats>().MaxHealth;
        DebugSlider.value = GetComponent<CharacterStats>().Health;
    }

    private void Awake()
    {
        //Default to idle state
        AlertStatus = AlertState.Idle;
        //Init timers
        LineOfSightTimer = TimeToLose;
        SearchTimer = SearchTime; 
    }

    int SelfPreservationThreshold = 50;
    int ProximityThreshold = 50;
    /// <summary>
    /// This function makes use of the enemy's stats to randomise the enemy's behaviours
    /// </summary>
    private void Init()
    {

        //Get the wepon controller component
        MyWeaponController = GetComponent<WeaponController>();
        //Store the navmesh agent component for movement through the level
        Agent = GetComponent<NavMeshAgent>();
        //Get the character stats component
        EnemyStats = GetComponent<EnemyStats>();

        //Store the hitboxes attached to this enemy
        HeadCollider = GetComponents<BoxCollider>()[0];
        BodyCollider = GetComponents<BoxCollider>()[1];
        DetectionSphere = GetComponent<SphereCollider>();

        switch (EnemyType)
        {
            case CombatType.Soldier:
                EnemyStats.MaxHealth = 100 * Difficulty;
                EnemyStats.Health = EnemyStats.MaxHealth;
                StopWhenInRange = true;

                Bravery = 2;
                Aggresiveness = 2;
                Determination = 2;
                Awareness = 2;
                Swiftness = 2;
                SuccessChance = 2;
                break;
            case CombatType.Berzerker:
                EnemyStats.MaxHealth = 20 * Difficulty;
                EnemyStats.Health = EnemyStats.MaxHealth;
                StopWhenInRange = false;

                Bravery = 3;
                Aggresiveness = 3;
                Determination = 3;
                Awareness = 1;
                Swiftness = 1;
                SuccessChance = 1;
                break;
            case CombatType.Tank:
                EnemyStats.MaxHealth = 500 * Difficulty;
                EnemyStats.Health = EnemyStats.MaxHealth;
                StopWhenInRange = false;

                Bravery = 3;
                Aggresiveness = 3;
                Determination = 3;
                Awareness = 1;
                Swiftness = 1;
                SuccessChance = 1;
                break;
            case CombatType.Sniper:
                EnemyStats.MaxHealth = 50 * Difficulty;
                EnemyStats.Health = EnemyStats.MaxHealth;
                StopWhenInRange = true;

                Bravery = 1;
                Aggresiveness = 1;
                Determination = 3;
                Awareness = 3;
                Swiftness = 3;
                SuccessChance = 3;
                break;
            case CombatType.Boss:
                EnemyStats.MaxHealth = 1000 * Difficulty;
                EnemyStats.Health = EnemyStats.MaxHealth;
                StopWhenInRange = false;

                Bravery = 3;
                Aggresiveness = 3;
                Determination = 3;
                Awareness = 3;
                Swiftness = 3;
                SuccessChance = 3;
                break;
        }

        float temp = ((3.0f - (float)Bravery) + (3.0f - (float)Aggresiveness))/6.0f;
        SelfPreservationThreshold = (int)(temp * 100);

        float temp2 = ((3.0f - (float)Bravery) + (3.0f - (float)Aggresiveness)) / 6.0f;
        ProximityThreshold = (int)(temp2 * 100);

        //Set the attack range to the currently equipped weapon's range
        AttackRange = MyWeaponController.GetCurrentlyEquippedWeapon().WeaponRange;

        //Using the aggressiveness and bravery stat, determine a suitable range of values for the retreat distance
        float minRetreatDist = 10 + (((3 - Bravery) - 1) * 5) - ((Aggresiveness - 1) * 2);
        float maxRetreatDist = 15 + (((3 - Bravery) - 1) * 5) - ((Aggresiveness - 1) * 2);
        //Use a quarter of the weapon range combined with a random value within the calculated range
        //to determine the retreat distance
        RetreatDistance = Random.Range(minRetreatDist, maxRetreatDist);

        //Use the sight quality attribute to determine the detection range
        DetectionRange = MinDetectionRange + AttackRange * Awareness;

        //Use the swiftness attribute to determine the enemy's move speed
        //TODO use this to alter climb speed/ weapons reload speed etc.
        Agent.speed = Swiftness * 1.5f;

        //Use the success chance stat to assign the percentage hit chance 
        //(ie how likely is it that this enemy's shots will successfully hit the target)
        PercentageHitChance = SuccessChance * Random.Range(20, 30);

        //Assign how long it will take for the enemy to lose the player after LOS is lost 
        //using the determination stat
        TimeToLose = Determination * Random.Range(10, 20);

        
        EnemyStats.SetAllAmmoCounts(10000);

      
    }

    public Text StateDebug;


    //void DecisionMaking()
    //{
    //    int combatVotes = 0;
    //    int coverVotes = 0;

    //    //Calculate the percentages for the HP of the enemy and target, as well as the distance of the target
    //    //relative to the max attack range of the current weapon
    //    float enemyHP = ((float)EnemyStats.Health / (float)EnemyStats.MaxHealth);
    //    float targetHP = (float)Target.GetComponent<CharacterStats>().Health / (float)Target.GetComponent<CharacterStats>().MaxHealth;

    //    //Convert the floating point percentages to ints and set between 0 and 100 for easier calculations
    //    int enemyPercentHP = (int)(enemyHP * 100);
    //    int targetPercentHP = (int)(targetHP * 100);

    //    //If the current health % falls below threshold
    //    //One vote for take cover
    //    if (enemyPercentHP < SelfPreservationThreshold)
    //    {
    //        coverVotes++;
    //    }
    //    else
    //    {
    //        combatVotes++;
    //    }

    //    //If the enemy health % is greater than the target health %
    //    //One vote for combat otherwise one vote to take cover
    //    //This allows the enemy to prioritise their action based on the HP difference between themself and the target
    //    //if the enemy is winning, they may be more aggressive, while if the target is winning they may be more defensive
    //    if (enemyPercentHP >= targetPercentHP)
    //    {
    //        combatVotes++;
    //    }
    //    else
    //    {
    //       coverVotes++;
    //    }

    //    //If the player HP% is less than the damage of a single shot from the current weapon
    //    //One vote for combat to finish them off
    //    //No need for an else here as it would present unrealistic behaviour to take cover is you cant one shot the target
    //    //Instead this acts to tip the scales in favour of combat if the target is weak
    //    if (targetPercentHP <= MyWeaponController.GetCurrentlyEquippedWeapon().WeaponDamage)
    //    {
    //        combatVotes++;
    //    }


    //    //This unbalances the votes and handles tie breakers
    //    //It reduces the liklihood of the enemy repeating behaviours multiple times in a row
    //    if (AlertStatus == AlertState.TakingCover)
    //    {
    //        combatVotes++;
    //    }
    //    else if (AlertStatus == AlertState.Hostile)
    //    {
    //        coverVotes++;
    //    }

    //    if(combatVotes > coverVotes)
    //    {
    //        DecisionTimer = AttackTime;
    //        AlertStatus = AlertState.Hostile;
    //    }
    //    else if(coverVotes > combatVotes)
    //    {
    //        DecisionTimer = CoverTime;
    //        TakeCover();
    //       // AlertStatus = AlertState.TakingCover;
    //    }
    //    else
    //    {
    //        Debug.Log("50-50");
    //    }

    //    StateDebug.text = combatVotes.ToString() + " : " + coverVotes.ToString() + " = " +  AlertStatus.ToString();

    //}



    AttackType CurrentAttackType = 0;

    private void Update()
    {
        //Debug Only Remove later
        DebugSlider.value = GetComponent<CharacterStats>().Health;

        //Determine if the enemy has a LOS to the player
        TargetSighted = DetectPlayer();


        //Control this enemy based on the current alert status
        switch (AlertStatus)
        {
            case AlertState.Idle:
                if (IsHitCounter > 2 || TargetSighted)
                {
                    IsHitCounter = 0;
                    AlertStatus = AlertState.Hostile;
                }
                Patrol();
                break;
            case AlertState.Suspicious:

                //Increase angle and depth of cone of vision as the enemy is on high alert (x2 to both?)

                //Using the source of the suspicion (location of dead enemy, approx location of nearest gunfire, etc.)
                //Define a search radius around that point

                //Begin by moving to the source if not already close to it
                //Assuming enemy is still in this state

                //Repeat for a defined number of times (persistence rating? determination?)
                //Pick a random point within the search radius and move to it
                //This provides an opportunity for this enemy to find the player by looking around the area to move into the hostile state


                //Meanwhile a serach timer will decrement and when it reaches 0 this enemy will return to the idle state


                break;
            case AlertState.Hostile:

                //if this enemy is a tank or a berserker
                //choose between attack and move towards player
                //or do both at same time? set destination then attack while moving?
                //When moving towards player, a relatively small max distance will be observed to ensure they are always close to the player when not actively moving

                //If the enemy is a sniper or soldier/grunt
                //choose between move towards player, take cover and attack
                //this decision will be made based on the stats and environmental factors
                //when moving towards player a reasonably large min distance will be observed to ensure they dont get too close 

                //If LOS is broken, a timer will decrement and when it reaches 0, this enemy will search for the player by moving to the suspicious state

                switch (CurrentAttackType)
                {
                    case AttackType.Charge:
                        break;
                    case AttackType.Attack:

                        Combat();

                        DecisionTimer -= Time.deltaTime;

                        //If being hit when in combat, set the timer to 0 to make a new decision
                        if (IsHitCounter > 5)
                        {
                            IsHitCounter = 0;
                            DecisionTimer = 0;
                        }

                        break;
                    case AttackType.TakeCover:

                        //Upon arriving at the cover, begin the timer
                        if (Agent.remainingDistance < Agent.stoppingDistance)
                        {
                            DecisionTimer -= Time.deltaTime;


                            if (!IsCurrentCoverValid())
                            {
                                Debug.Log("This cover is compromised");
                                TakeCover();
                            }


                            //If being hit when in cover, set the timer to 0 to make a new decision
                            if (IsHitCounter > 0)
                            {
                                IsHitCounter = 0;
                                DecisionTimer = 0;
                            }
                        }
                        break;
                }
                break;
        }
    }

  
    Vector3 FindCover()
    {
        //Find all the possible cover points scattered around the level in the assigned radius
        Collider[] CoverPoints = Physics.OverlapSphere(transform.position, CoverSearchRadius, CoverMask);
        int TargetIndex = -1;
        int CurrentIndex = 0;
        float MinDistance = Mathf.Infinity;

        foreach (Collider col in CoverPoints)
        {
            CoverData Cover = col.gameObject.GetComponent<CoverData>();
            //Check if cover is already occupied
            if (!Cover.IsOccupied)
            {
                //Determine suitability of cover based on player current position
                //ie is the player in front of the cover position (will the piece of cover actually provide protection)
                Vector3 heading = Target.position - Cover.CoverPosition.position;
                float dot = Vector3.Dot(heading, Cover.CoverPosition.forward);

                //If the dot product is greater than zero, then the player is roughly in front of the cover
                if (dot > 0)
                {
                    //Determine the distance from current position
                    float CurrentDistance = Vector3.Distance(transform.position, col.transform.position);
                    //Compare with current target
                    if (CurrentDistance < MinDistance)
                    {
                        //Store the current index
                        TargetIndex = CurrentIndex;
                        MinDistance = CurrentDistance;

                        //If the targeted cover distance is below the threshold, 
                        //then it is 'good enough' and will be used
                        if (CurrentDistance <= MinCoverThreshold)
                        {
                            CurrentCover = CoverPoints[TargetIndex].gameObject;
                            return Cover.CoverPosition.position;
                        }
                    }
                }
            }

            CurrentIndex++;
        }

        if (TargetIndex < 0)
        {
            //If the target index is still its default value of -1
            //then return the current transform position
            //This will be checked before assigning a new destination as we dont want the enemy to deviate from its existing
            //orders if there is no suitable cover 
            CurrentCover = null;
            return transform.position;
        }
        else
        {
            //If all cover points were looked at and the most suitable one is not below the minimum threshold
            //Use the best one that could be found
            CurrentCover = CoverPoints[TargetIndex].gameObject;
            return CoverPoints[TargetIndex].gameObject.GetComponent<CoverData>().CoverPosition.position;
        }
    }

    private GameObject CurrentCover;
    /// <summary>
    /// This function handles the patrol behaviours of the enemy
    /// </summary>
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

   
    bool IsCurrentCoverValid()
    {
        if (CurrentCover == null)
            return false;


        //Determine suitability of cover based on player current position
        //ie is the player in front of the cover position (will the piece of cover actually provide protection)
        Vector3 heading = Target.position - CurrentCover.GetComponent<CoverData>().CoverPosition.position;
        float dot = Vector3.Dot(heading, CurrentCover.GetComponent<CoverData>().CoverPosition.forward);

        //If the dot product is greater than zero, then the player is roughly in front of the cover
        if (dot > 0)
        {
            return true;
        }



            return false;
    }

    /// <summary>
    /// This function handles the combat behaviours of the enemy
    /// </summary>
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
            if(StopWhenInRange)
                Retreat();
        }
        else
        {
            if (StopWhenInRange)
                StopMoving();

            Attack();
        }
    }



    void FaceTarget()
    {
        Vector3 direction = (Target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * TurnSpeed);
    }




    bool TakeCover()
    {
        //This allows the enemy to retreat when the player gets too close
        //TODO have the option to take cover here also
        Vector3 Destination = FindCover();

        if (Destination != transform.position)
        {
            Agent.SetDestination(Destination);
            return true;
        }

        return false;
    }

    void Retreat()
    {
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

        if (MyWeaponController.CanFire)
        {
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
                //PlayerManager.Instance.Player.GetComponent<CharacterStats>().TakeDamage(10);
                MyWeaponController.UseWeapon(transform.position + EyePosition, Heading);
            }
            else
            {
                Vector3 Offset = new Vector3(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
                MyWeaponController.UseWeapon(EyePosition, ((Target.position + Offset) - EyePosition).normalized);
            }
        }
    }

    private int IsHitCounter = 0;
   

    /// <summary>
    /// This function determines which hitbox the player raycast intersects
    /// </summary>
    public void IsHit(Ray ray, int damageAmount)
    {
        if(DetectionSphere.bounds.IntersectRay(ray))
        {
            IsHitCounter++;
            AlertStatus = AlertState.Hostile;
        }

        if(HeadCollider.bounds.IntersectRay(ray))
        {
            AlertStatus = AlertState.Hostile;
            GetComponent<CharacterStats>().TakeDamage((int)(damageAmount * HeadshotDamagePercentage));
            return;
        }

        if(BodyCollider.bounds.IntersectRay(ray))
        {
            AlertStatus = AlertState.Hostile;
            GetComponent<CharacterStats>().TakeDamage((int)(damageAmount * BodyShotDamagePercentage));
            return;
        }
    }

    /// <summary>
    /// This function returns true if the player can be seen within the enemy's cone of vision
    /// </summary>
    /// <returns></returns>
    private bool DetectPlayer()
    {
        //Calculate the direction of the player in relation to the enemy
        Heading = (Target.position + Vector3.up) - (transform.position + EyePosition);
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


    private CombatType previous;
    //Debug Only - gizmos for patrol points, cone of vision, etc.
    private void OnValidate()
    {

        Init();

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
        if (Target != null)
            Gizmos.DrawLine(transform.position + EyePosition, Target.position);



        Gizmos.color = Color.red;
        if (Target != null)
            Gizmos.DrawSphere(Target.position, 1.0f);

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
    /// This enum determines the state the enemy is in and what actions they can take.
    /// </summary>
    enum AlertState
    {
        Idle = 0,
        Suspicious = 1,
        Hostile = 2
    }

    enum AttackType
    {
        Charge = 0,
        Attack = 1,
        TakeCover = 2
    }

    /// <summary>
    /// This enum determines the type of combat this enemy will perform
    /// </summary>
    public enum CombatType
    {
    Soldier = 0,
    Berzerker = 1,
    Tank = 2,
    Sniper = 3,
    Boss = 4,
    Custom = 5
    }

}
