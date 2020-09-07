using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Chronos;




[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(WeaponController))]
[DisallowMultipleComponent]
public class EnemyController : BaseBehaviour
{
#pragma warning disable 0649


    #region AssignableInEditor
    [SerializeField] Animator MyAnimator;

    [Header("Enemy Base Settings")]
    [SerializeField] private EnemyProfile MyProfile;
    [Range(1, 5)]
    [SerializeField] private int Difficulty = 1;

   
    [Header("Patrol Settings")]
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int TargetPatrolPoint = 0;

    [Header("Search Settings")]
    //The position of the "eye". Where the cone of vision comes to a point. 
    [SerializeField] Vector3 EyePosition = new Vector3();

    [Header("Incoming Damage Multipliers")]
    [SerializeField] private bool HeadshotInstantKill = false;
    [SerializeField] private float HeadshotDamagePercentage = 1.0f;
    [SerializeField] private float BodyShotDamagePercentage = 0.75f;

    //This will allow the enemy to be paused for debug purposes
    //NB Likely not suitable for an actual in-game pause
    [Header("Debug Only")]
    [SerializeField] private bool IsPaused = false;
    #endregion

    #region ComponentCaches
    //The enemy manager allows this enemy to make decisions based on the other enemies in the level
    //It can track both dead enemies as well as those engaged in combat
    private EnemyManager MyManager;
    //Below are local stores of the components attached to the enemy
    //It will improve efficiency at runtime to store these on awake
    //instead of calling GetComponent multiple times each frame
    private WeaponController MyWeaponController;
    private NavMeshAgent MyNavMeshAgent;
    private EnemyStats MyEnemyStats;

    private HealthBar MyHealthBar;
    #endregion

#pragma warning restore 0649

    
    //The alert status determines whether the enemy is in open combat or not
    public EnemyUtility.AlertState AlertStatus { get; set; }
    public bool IsClimbing { get; set; }


    #region DetectionSetings
    //The direction of player is the direction from the enemy to the player
    //it is used to determine if they can see the player and what direction to shoot in
    private Vector3 DirectionOfPlayer = new Vector3();
    //This raycast hit is the result of the detection raycast
    //if it does not equal the player then, something is blocking LOS
    private RaycastHit hit;
    //Determine if a hostile has been sighted
    private bool TargetSighted = false;
    //Layer mask outlining what can block the raycast. (Typically everything apart from itself)
    private LayerMask DetectionMask = new LayerMask();
    //The detection angle of the cone of vision
    private float DetectionAngle = 35;
    //The detection range, ie the length of the cone of vision
    private float DetectionRange = 35;
    #endregion

    #region TakeCoverAttributes
    //The game object of the current piece of cover,
    //stored to continually check it is valid
    private GameObject CurrentCover;
    //Layer mask to identify what is cover
    private LayerMask CoverMask = new LayerMask();
    //If a piece of cover is found closer than this min threshold when iterating through 
    //all the cover within the search radius it will be immediately chosen for efficiency
    private float MinCoverThreshold = 10.0f;
    //The radius of the search sphere for suitable cover
    private float CoverSearchRadius = 100.0f;
    #endregion

    #region HitBoxes
    //Hitbox Colliders
    private Collider HeadCollider;
    private Collider BodyCollider;
    //Hitbox used to detect incoming bullets even if 
    //they are a near-miss
    private Collider DetectionSphere;
    #endregion

    #region DecisionMakingFlags
    //Flag to determine if the enemy is currently in cover or not
    private bool IsInCover = false;
    private bool IsAttacking = false;
    private bool IsMovingIn = false;
    private bool IsRetreating = false;
    private bool IsHoldingPosition = false;
    #endregion

    #region TimersAndCounters
    //The timer used to track searching when in the suspicious state
    private float SearchTimer;
    //The timer used to track how long LOS can be broken for before having to search
    private float LOS_Timer = 0.0f;
    //The amount of time the enemy will search for
    private float TimeForSearch = 10.0f;
    //The amount of time the enemy will wait before searching if LOS is broken
    private float TimeForLOS = 10.0f;
    private float DecisionTimer = 0;
    //Tracks the number of hits this enemy has recieved since last state change
    //For example, if attack out in the open, if being hit too much, take cover
    private int HitCounter = 0;
    private int HitThreshold = 2;
    #endregion


    private void Start()
    {
        MyManager = EnemyManager.Instance;
        MyWeaponController = GetComponent<WeaponController>();
        MyNavMeshAgent = GetComponent<NavMeshAgent>();
        MyEnemyStats = GetComponent<EnemyStats>();

        MyHealthBar = GetComponent<HealthBar>();


        //Store the hitboxes attached to this enemy
        HeadCollider = GetComponents<BoxCollider>()[0];
        BodyCollider = GetComponents<BoxCollider>()[1];
        DetectionSphere = GetComponent<SphereCollider>();

        //Create the layer masks in code to detect everything
        //to declutter inspector
        DetectionMask = ~0; //Sets the layer mask to everything
        CoverMask = LayerMask.GetMask("Cover"); //Sets the layer mask to cover only




        //Assign the starting detection angle and range to the default values
        DetectionAngle = MyProfile.DefaultDetectionAngle;
        DetectionRange = MyProfile.DefaultDetectionRange;


        //Assign the max health of the enemy using the profile multiplied by the difficulty
        MyEnemyStats.MaxHealth = MyProfile.BaseMaxHealth * Difficulty;
        MyEnemyStats.Health = MyEnemyStats.MaxHealth;


        if (MyHealthBar != null)
        {
            MyHealthBar.SetMaxHP(MyEnemyStats.MaxHealth);
            MyHealthBar.UpdateHealthbar(MyEnemyStats.Health);
        }


        //Initialise the timers and counters
        TimeForSearch *= MyProfile.Determination;
        SearchTimer = TimeForSearch;
        DecisionTimer = 0.0f; //Set to 0 so we can immediately make a decision


        //Extra check at the end of startup to ensure everything is initialised correctly 
        //otherwise this enemy will default to dead
        if(MyManager == null || 
            MyWeaponController == null ||
            MyNavMeshAgent == null || 
            MyEnemyStats == null || 
            HeadCollider == null || 
            BodyCollider == null ||
            DetectionSphere == null ||
            DetectionAngle <= 0.0f ||
            DetectionRange <= 1.0f ||
            MyEnemyStats.Health <= 0 ||
            MyProfile == null ||
            PlayerManager.Instance == null ||
            PlayerManager.Instance.Player == null)
        {
            Debug.LogError("Enemy Initialisation of " + transform.name + " was unsuccessful.");
            MyEnemyStats.IsDead = true;
        }

    }

   
    private void Update()
    {
        if (MyHealthBar != null)
        {
            MyHealthBar.UpdateHealthbar(MyEnemyStats.Health);
           // MyHealthBar.UpdateStateText(AlertStatus.ToString());

        }

        if (!MyEnemyStats.IsDead)
        {
            #region StateIndependentBehaviours
            //Check if the player can be seen
            TargetSighted = DetectTarget(PlayerManager.Instance.Player.transform, true);

            //If the target is sighted then become hostile
            if (TargetSighted)
            {
                //If moving from the idle state then heighten detection mode
                if (AlertStatus == EnemyUtility.AlertState.Idle)
                    ChangeDetectionMode(true);

                LOS_Timer = TimeForLOS;
                AlertStatus = EnemyUtility.AlertState.Hostile;
            }

            if(HitCounter >= HitThreshold && AlertStatus != EnemyUtility.AlertState.Hostile)
            {
                LOS_Timer = TimeForLOS;
                AlertStatus = EnemyUtility.AlertState.Hostile;
            }

         
           
           
            #endregion


            



            switch (AlertStatus)
            {
                case EnemyUtility.AlertState.Idle:
                    #region IdleBehaviours

                    //If there is a direct LOS to any dead enemies
                    if(DetectDeadEnemies())
                    { 
                        //Move to suspicious mode and heighten detection cone
                        ChangeDetectionMode(true);
                        SearchTimer = TimeForSearch;
                        
                        AlertStatus = EnemyUtility.AlertState.Suspicious;
                        StartCoroutine(FindSearchPoint());

                        break;
                    }

                    List<GameObject> HostileEnemies = MyManager.GetHostileEnemies();

                    if (HostileEnemies.Count > 0)
                    {
                        foreach (GameObject item in HostileEnemies)
                        {
                            if (Vector3.Distance(item.transform.position, transform.position) < 30.0f)
                            {
                                ChangeDetectionMode(true);
                                SearchOrigin = item.transform.position;
                                StartCoroutine(FindSearchPoint());
                                AlertStatus = EnemyUtility.AlertState.Suspicious;
                                break;
                            }
                        }
                    }

                    //Patrol through the patrol points
                    Patrol();
                    #endregion
                    break;
                case EnemyUtility.AlertState.Suspicious:
                    #region SuspiciousBehaviours

                    SearchTimer -= time.deltaTime;

                    if(SearchTimer <= 0.0f)
                    {
                        SearchTimer = TimeForSearch;
                        ChangeDetectionMode(false);
                        AlertStatus = EnemyUtility.AlertState.Idle;
                        break;
                    }


                    if(MyNavMeshAgent.remainingDistance < 5.0f && !FindingSP)
                    {
                        StopAllCoroutines();
                        StartCoroutine(FindSearchPoint());
                    }
                    
                    //Increase angle and depth of cone of vision as the enemy is on high alert (x2 to both?)


                    //Being Shot At OR Player is shooting ////////////////////////
                    //If being shot at, look to player and if LOS is found then move to hostile
                    //Otherwise move to location where they shot from

                    //Dead Enemy Found OR Nearby Enemy Hostile ///////////////////////////////////////////
                    //Using the source of the suspicion (location of dead enemy, approx location of nearest gunfire, etc.)
                    //Define a search radius around that point

                    //Begin by moving to the source if not already close to it
                    //Assuming enemy is still in this state

                    //Repeat for a defined number of times (persistence rating? determination?)
                    //Pick a random point within the search radius and move to it
                    //This provides an opportunity for this enemy to find the player by looking around the area to move into the hostile state


                    //Meanwhile a serach timer will decrement and when it reaches 0 this enemy will return to the idle state
                    #endregion
                    break;
                case EnemyUtility.AlertState.Hostile:
                    #region HostileBehaviours


                    //If LOS is broken, a timer will decrement and when it reaches 0,
                    //this enemy will search for the player by moving to the suspicious state
                    if (!TargetSighted)
                    {
                        LOS_Timer -= Time.deltaTime;


                        if(LOS_Timer <= 0.0f)
                        {
                            LOS_Timer = TimeForLOS;
                            SearchTimer = TimeForSearch;
                            AlertStatus = EnemyUtility.AlertState.Suspicious;
                        }
                    }



                    //If this enemy does not have tank behaviours proceed with normal decision making
                    if (!MyProfile.HasTankBehaviour)
                    {
                        if(!IsInCover || (IsInCover && Vector3.Distance(transform.position, CurrentCover.GetComponent<CoverData>().CoverPosition.position) <= 5.0f))
                            DecisionTimer -= Time.deltaTime;

                        //If the timer is up, the enemy has been hit the threshold number of times, or the cover the enemy is currently in is no longer valid
                        if (DecisionTimer <= 0.0f || HitCounter >= HitThreshold || (IsInCover && !IsCurrentCoverValid()))
                        {
                            DecisionTimer = MyProfile.DecisionTime;

                            switch (CombatDecisionAorB(MyProfile.PassiveOption, MyProfile.AggressiveOption))
                            {
                                case EnemyUtility.DecisionType.TakeCover:
                                    IsAttacking = false;
                                    IsRetreating = false;
                                    IsInCover = true;
                                    IsMovingIn = false;
                                    IsHoldingPosition = false;
                                    //Find a suitable cover spot and move to it
                                    MyNavMeshAgent.SetDestination(FindCover());
                                    break;
                                case EnemyUtility.DecisionType.RunAndGun:
                                    IsAttacking = true;
                                    IsRetreating = false;
                                    IsInCover = false;
                                    IsMovingIn = true; 
                                    MoveIn();
                                    break;
                                case EnemyUtility.DecisionType.MoveTowardsTarget:
                                    IsAttacking = false;
                                    IsRetreating = false;
                                    IsHoldingPosition = false;
                                    IsInCover = false;
                                    IsMovingIn = true;
                                    MoveIn();
                                    break;
                                case EnemyUtility.DecisionType.Retreat:
                                    IsAttacking = false;
                                    IsRetreating = true;
                                    IsInCover = false;
                                    IsMovingIn = false;
                                    IsHoldingPosition = false;
                                    break;
                                case EnemyUtility.DecisionType.StopAndAttack:
                                    IsAttacking = true;
                                    IsRetreating = false;
                                    IsInCover = false;
                                    IsMovingIn = false;
                                    IsHoldingPosition = true;
                                    HoldPosition();
                                    break;
                                default:
                                    Debug.LogError("Unexpected result from combat decision making by " + transform.name);
                                    break;
                            }
                        }

                        if(IsAttacking)
                        {
                            Attack();
                        }

                        if (IsMovingIn)
                        {
                            
                            if (Vector3.Distance(transform.position, PlayerManager.Instance.Player.transform.position) <= 5.0f)
                            {
                                IsHoldingPosition = true;
                                HoldPosition();
                            }
                            else
                            {
                                IsHoldingPosition = false;
                                MoveIn();
                            }
                        }
                        
                        if(IsRetreating)
                        {
                            Retreat();
                        }

                        if (IsInCover && (CurrentCover != null && Vector3.Distance(transform.position, CurrentCover.GetComponent<CoverData>().CoverPosition.position) <= 5.0f))
                        {
                            IsHoldingPosition = true;
                        }

                        if (IsHoldingPosition)
                        {
                            FaceTarget(PlayerManager.Instance.Player.transform.position);
                        }

                    }
                    else
                    {
                        DecisionTimer -= Time.deltaTime;

                        if (DecisionTimer <= 0.0f)
                        {
                            DecisionTimer = Random.Range(MyProfile.DecisionTime, MyProfile.DecisionTime * 2);

                            if (!IsAttacking)
                            {
                                HoldPosition();
                                IsAttacking = true;
                                IsHoldingPosition = true;
                            }
                            else
                            {
                                MoveIn();
                                IsAttacking = false;
                                IsHoldingPosition = false;
                            }
                        }

                        //TODO
                        //if distance between player and this enemy is less than 1 then perform knockback
                        //Add knockback force attribute to enemy profiles?
                        if (Vector3.Distance(transform.position, PlayerManager.Instance.Player.transform.position) <= 5.0f)
                        {
                            IsHoldingPosition = true;
                            HoldPosition();
                        }

                        if(IsHoldingPosition)
                        {
                            FaceTarget(PlayerManager.Instance.Player.transform.position);
                        }
                    }
                    #endregion
                    break;
            }
        }
        else
        {
            MyHealthBar.DisableHealthbar();
        }
    }

    #region NonCombatMethods
    //This function determines whether the enemy can see the player from their current position and heading
    private bool DetectTarget(Transform Target, bool isPlayer)
    {
        //Calculate the direction of the player in relation to the enemy
        Vector3 DirectionOfTarget = ((Target.position + (Vector3.up * 2.0f)) - (transform.position + EyePosition)).normalized;

        if (isPlayer)
            DirectionOfPlayer = DirectionOfTarget;

        //Using the heading, calculate the angle between it and the current Look Direction (the forward vector)
        float angle = Vector3.Angle(transform.forward, DirectionOfTarget);
        //If the calculated angle is less than the defined threshold
        //Then the player is within the edges of the cone of vision
        if (angle < DetectionAngle)
        {
            //Raycast to determine if the enemy can see the player from its current position
            //This accounts for the detection range and line of sight
            if (Physics.Raycast(transform.position + EyePosition, DirectionOfTarget, out hit, DetectionRange, DetectionMask))
            {
                //If the raycast hits the player then they must be in range, with a clear line of sight
                if (hit.transform.CompareTag("Player") && isPlayer)
                {
                    //Reset line of sight timer
                    LOS_Timer = TimeForLOS;
                    return true;
                }
                
                if(hit.transform.CompareTag("Enemy") && hit.transform == Target && !isPlayer)
                {
                    return true;
                }

                return false;
            }
        }

        return false;
    }

    //This function allows the enemy to move between it's patrol points
    void Patrol()
    {
        if (PatrolPoints.Count > 1)
        {
            MyAnimator.SetBool("IsWalk", true);
            MyAnimator.SetBool("IsRun", false);
            MyAnimator.SetBool("IsIdle", false);
            MyAnimator.SetBool("IsSearch", false);
            MyAnimator.SetBool("IsShoot", false);

            //Move to the current target patrol point
            if (!IsPaused)
                MyNavMeshAgent.SetDestination(PatrolPoints[TargetPatrolPoint].position);

            //If the enemy arrives at the patrol point, move to the next one
            if (Vector3.Distance(transform.position, PatrolPoints[TargetPatrolPoint].position) < MyNavMeshAgent.stoppingDistance)
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

    //This function rotates the enemy to face the given point (usually the player)
    void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
    }
    
    //This function will change the detection range and angle to reflect the current alert status
    private void ChangeDetectionMode(bool Alert)
    {
        if (Alert)
        {
            DetectionRange = MyProfile.DefaultDetectionRange * 2;
            DetectionAngle = MyProfile.DefaultDetectionAngle + 90;
        }
        else
        {
            DetectionRange = MyProfile.DefaultDetectionRange;
            DetectionAngle = MyProfile.DefaultDetectionAngle;
        }
    }

    //This function loops through the enemy manager's list of dead enemies (if there are any)
    //And detects if this enemy has a LOS to any of them. If they do, then return true, otherwise return false
    private bool DetectDeadEnemies()
    {
        List<GameObject> deadEnemies = MyManager.GetDeadEnemies();

        if (deadEnemies != null)
        {
            if (deadEnemies.Count > 0)
            {
                foreach (GameObject enemy in deadEnemies)
                {
                    if (DetectTarget(enemy.transform, false))
                    {
                        SearchOrigin = enemy.transform.position;
                        MyManager.SetDeadEnemyAsDiscovered(enemy);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    #endregion



    Vector3 SearchOrigin = new Vector3();
    float SearchRadius = 10.0f;
    float WaitTime = 2.0f;
    bool FindingSP = false;



    IEnumerator FindSearchPoint()
    {
        FindingSP = true;
        MyAnimator.SetBool("IsRun", false);
        MyAnimator.SetBool("IsIdle", false);
        MyAnimator.SetBool("IsWalk", false);
        MyAnimator.SetBool("IsSearch", true);
        MyAnimator.SetBool("IsShoot", false);

        yield return new WaitForSeconds(WaitTime);

        MyNavMeshAgent.SetDestination(new Vector3(Random.Range(SearchOrigin.x - SearchRadius, SearchOrigin.x + SearchRadius), SearchOrigin.y, Random.Range(SearchOrigin.z - SearchRadius, SearchOrigin.z + SearchRadius)));
        
        MyAnimator.SetBool("IsSearch", false);
        MyAnimator.SetBool("IsWalk", true);

        while (MyNavMeshAgent.path.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("Finding valid path");
            MyNavMeshAgent.SetDestination(new Vector3(Random.Range(SearchOrigin.x - SearchRadius, SearchOrigin.x + SearchRadius), SearchOrigin.y, Random.Range(SearchOrigin.z - SearchRadius, SearchOrigin.z + SearchRadius)));
        }


        FindingSP = false;
    }


    #region CombatMethods

    //This function will check if the current piece of cover is still valid
    bool IsCurrentCoverValid()
    {
        if (CurrentCover == null)
            return false;


        //Determine suitability of cover based on player current position
        //ie is the player in front of the cover position (will the piece of cover actually provide protection)
        Vector3 heading = PlayerManager.Instance.Player.transform.position - CurrentCover.GetComponent<CoverData>().CoverPosition.position;
        float dot = Vector3.Dot(heading, CurrentCover.GetComponent<CoverData>().CoverPosition.forward);

        //If the dot product is greater than zero, then the player is roughly in front of the cover
        if (dot > 0)
        {
            return true;
        }

        return false;
    }

    //This function will search the nearby area for cover
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
                Vector3 heading = PlayerManager.Instance.Player.transform.position - Cover.CoverPosition.position;
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

    //This function will fire the currently equipped weapon at the player
    private void Attack()
    {
        MyAnimator.SetBool("IsIdle", false);

        MyAnimator.SetBool("IsShoot", true);
        MyWeaponController.UseWeapon(transform.position + EyePosition, DirectionOfPlayer);
    }

    //This function will tell the enemy to stop moving
    private void HoldPosition()
    {
        MyAnimator.SetBool("IsRun", false);
        MyAnimator.SetBool("IsIdle", true);
        MyAnimator.SetBool("IsWalk", false);
        MyAnimator.SetBool("IsSearch", false);
        MyAnimator.SetBool("IsShoot", false);

        MyNavMeshAgent.SetDestination(transform.position);
    }
    
    //This function will make the enemy flee from the player
    private void Retreat()
    {
        MyAnimator.SetBool("IsRun", true);
        MyAnimator.SetBool("IsIdle", false);
        MyAnimator.SetBool("IsWalk", false); 
        MyAnimator.SetBool("IsSearch", false);

        //TODO
        //Change this to instead look for a random position away from the player?
        MyNavMeshAgent.SetDestination(transform.position - (DirectionOfPlayer * 5.0f));
    }

    //This function will tell the enemy to move towards the player's position
    private void MoveIn()
    {
        MyAnimator.SetBool("IsRun", true);
        MyAnimator.SetBool("IsIdle", false);
        MyAnimator.SetBool("IsWalk", false);
        MyAnimator.SetBool("IsSearch", false);

        MyNavMeshAgent.SetDestination(PlayerManager.Instance.Player.transform.position);
    }

    //This function will be used to make combat decisions, typically, option A will be the more passive option
    //while option B will be more aggressive. The exception being the Tank enemy type, where both options are fairly aggressive
    private EnemyUtility.DecisionType CombatDecisionAorB(EnemyUtility.DecisionType optionA, EnemyUtility.DecisionType optionB)
    {
        //This is the boundary in the 0-100 scale which will be used to decide between option A and B.
        //The decision making process will move this boundary up or down, in the direction of the favoured option
        //After which, a randomly generated number will be used to determine the action to be taken, based on which side of the 
        //boundary it falls on.
        //By default the bravery and aggressiveness stat is used where a very brave and aggressive enemy will prioritise the aggressuve option
        int SplitPoint = 100 - ((MyProfile.Aggressiveness + MyProfile.Bravery) * 10);

        if (MyProfile.HasTankBehaviour)
        {
            //The tank enemy type will alternate between moving towards the player for x seconds and stopping to shoot
            //As they have such a large health pool, very little decision making is neccessary, as such this will always return optionA
            Debug.LogWarning("The function: CombatDecisionAorB(), is incompatible with the Tank enemy type. By Default a 50-50 decision will be made.");
            SplitPoint = 50;
        }
        else
        {
            //This is where the decision making process will occur
            //enviromental factors will influence the split point value
            //this will change the liklihood of a particular option being chosen
            //while still retaining some randomness to the behaviours
            //Smaller split point means higher chance of option B (Aggressive)
            //Larger split point means higher chance of option A (Passive)




            //The enemy's bravery stat is used to determine the percentage of dead enemies in the level where cowardice will be prioritised
            if (MyManager.GetDeadEnemies().Count/MyManager.TotalEnemyCount > ((MyProfile.Bravery) * 25) / MyManager.TotalEnemyCount)
            {
                //Passive
                SplitPoint += 10;
            }

            //If the enemy's health is below a percentage defined by the bravery stat, then prioritise self preservation
            if (MyEnemyStats.Health <= (MyEnemyStats.MaxHealth/ 100) * (4 - MyProfile.Bravery) * 20)
            {
                //Passive
                SplitPoint += 15;
            }

            
            //If the enemy is being hit by the players attacks too frequently then they are exposed
            if (HitCounter >= HitThreshold)
            {
                //Agressiveness rating will decrease split
                //bravery will increase split


                //Using the aggressiveness rating, this will increase the chances of an aggressive action
                //being taken proportional to the stat
                SplitPoint -= (MyProfile.Aggressiveness) * 10;

                //Using the bravery rating, the chances of cowardice being prioritised will
                //be increased proportionally to the stat
                SplitPoint += (3 - MyProfile.Bravery) * 10;

                HitCounter = 0;
            }

            //If the enemy is stronger than the player (in terms of health %) then aggro
            if ((MyEnemyStats.Health / MyEnemyStats.MaxHealth) * 100 >= (MyManager.PlayerStats.Health/ MyManager.PlayerStats.MaxHealth) * 100)
            {
              //Aggressive
              SplitPoint -= 10;
            }

            //If the enemy can one-shot the player, take the extra risk of attacking 
            if(MyWeaponController.GetCurrentlyEquippedWeapon().WeaponDamage >= MyManager.PlayerStats.Health)
            {
                //Aggressive
                SplitPoint -= 20;
            }


        }

        int Percent = Random.Range(1, 101);

        if (Percent < SplitPoint)
            return optionA;
        else
            return optionB;
    }

    // This function determines which hitbox the player raycast intersects
    public void IsHit(Ray ray, int damageAmount)
    {
        //If the ray intersects the D-Sphere, increment the hit counter and move to suspicious if not already hostile
        if (DetectionSphere.bounds.IntersectRay(ray))
        {
            HitCounter++;

            if (AlertStatus == EnemyUtility.AlertState.Idle)
                AlertStatus = EnemyUtility.AlertState.Suspicious;
        }

        //If the ray intersects the head hitbox, use the increased damage multiplier
        if (HeadCollider.bounds.IntersectRay(ray))
        {
            if (HeadshotInstantKill)
            {
                MyEnemyStats.TakeDamage((int)(MyEnemyStats.MaxHealth));
            }
            else
            {
                MyEnemyStats.TakeDamage((int)(damageAmount * HeadshotDamagePercentage));
            }
            return;
        }

        //If the ray intersects the body hitbox, use the standard damage multiplier
        if (BodyCollider.bounds.IntersectRay(ray))
        {
            if (HeadshotInstantKill)
            {
                MyEnemyStats.TakeDamage((int)(damageAmount));
            }
            else
            {
                MyEnemyStats.TakeDamage((int)(damageAmount * BodyShotDamagePercentage));
            }
            return;
        }
    }

    public void DisableColliders()
    {
        DetectionSphere.enabled = false;
        HeadCollider.enabled = false;
        BodyCollider.enabled = false;
    }
    //This function determines which hitbox a physical bullet object intersects
    public void IsHit(Collider bullet, int damageAmount)
    {
        //If the collider intersects the D-Sphere, increment the hit counter and move to suspicious if not already hostile
        if (DetectionSphere.bounds.Intersects(bullet.bounds))
        {
            HitCounter++;

            if (AlertStatus == EnemyUtility.AlertState.Idle)
                AlertStatus = EnemyUtility.AlertState.Suspicious;
        }

        //If the bullet intersects the head hitbox, use the increased damage multiplier
        if (HeadCollider.bounds.Intersects(bullet.bounds))
        {
            if (HeadshotInstantKill)
            {
                MyEnemyStats.TakeDamage((int)(MyEnemyStats.MaxHealth));
            }
            else
            {
                MyEnemyStats.TakeDamage((int)(damageAmount * HeadshotDamagePercentage));
            }
            return;
        }

        //If the bullet intersects the body hitbox, use the standard damage multiplier
        if (BodyCollider.bounds.Intersects(bullet.bounds))
        {
            if (HeadshotInstantKill)
            {
                MyEnemyStats.TakeDamage((int)(damageAmount));
            }
            else
            {
                MyEnemyStats.TakeDamage((int)(damageAmount * BodyShotDamagePercentage));
            }
            return;
        }
    }

    //This is used to check if a physical bullet object collides with this enemy
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Bullet") && !MyEnemyStats.IsDead)
        {
            IsHit(collision.gameObject.GetComponent<Collider>(), (int)collision.gameObject.GetComponent<ProjectileController>().DamageAmount);
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        foreach (Transform point in PatrolPoints)
        {
            Gizmos.DrawWireSphere(point.position, 1.0f);
        }

        Gizmos.DrawLine(transform.position + EyePosition,(transform.position + EyePosition) +  DirectionOfPlayer * 10.0f);


        Gizmos.color = Color.white;
        Gizmos.DrawWireMesh(EnemyUtility.CreateViewCone(DetectionAngle, DetectionRange, 8), 0, transform.position + EyePosition);

        Gizmos.color = Color.blue;
        if(MyNavMeshAgent != null)
            Gizmos.DrawSphere(MyNavMeshAgent.destination, 1.0f);
    }
}
