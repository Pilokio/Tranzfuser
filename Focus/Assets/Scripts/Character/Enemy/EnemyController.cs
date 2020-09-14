using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Chronos;

public class EnemyController : MonoBehaviour
{

    #region ComponentCaches
   
    private WeaponController MyWeaponController;
    private NavMeshAgent MyNavMeshAgent;
    private EnemyStats MyEnemyStats;
    private HealthBar MyHealthBar;
    #endregion


    [SerializeField] Animator MyAnimator;
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int TargetPatrolPoint = 0;
    [SerializeField] Vector3 EyePosition = new Vector3();
    [SerializeField] private bool HeadshotInstantKill = false;
    [SerializeField] private float HeadshotDamagePercentage = 1.0f;
    [SerializeField] private float BodyShotDamagePercentage = 0.75f;
    //The detection angle of the cone of vision
    [SerializeField] float DetectionAngle = 35;
    //The detection range, ie the length of the cone of vision
    [SerializeField] float DetectionRange = 35;

    //The alert status determines whether the enemy is in open combat or not
    public EnemyUtility.AlertState AlertStatus { get; set; }
    public bool IsClimbing { get; set; }


    #region Detection
    //The direction of player is the direction from the enemy to the player
    //it is used to determine if they can see the player and what direction to shoot in
    private Vector3 DirectionOfPlayer = new Vector3();
    //This raycast hit is the result of the detection raycast
    //if it does not equal the player then, something is blocking LOS
    private RaycastHit hit;
    //Determine if a hostile has been sighted
    public bool TargetSighted = false;

    //Layer mask outlining what can block the raycast. (Typically everything apart from itself)
    private LayerMask DetectionMask = new LayerMask();

    #endregion


    #region Hitboxes
    //Hitbox Colliders
    private Collider HeadCollider;
    private Collider BodyCollider;
    //Hitbox used to detect incoming bullets even if 
    //they are a near-miss
    private Collider DetectionSphere;
    #endregion

    private GameObject PlayerRef;

    private int HitCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        MyWeaponController = GetComponent<WeaponController>();
        MyNavMeshAgent = GetComponent<NavMeshAgent>();
        MyEnemyStats = GetComponent<EnemyStats>();
        MyHealthBar = GetComponent<HealthBar>();


        //Store the hitboxes attached to this enemy
        HeadCollider = GetComponents<BoxCollider>()[0];
        BodyCollider = GetComponents<BoxCollider>()[1];
        DetectionSphere = GetComponent<SphereCollider>();

        //Create the layer mask in code to detect everything
        //to declutter inspector
        DetectionMask = ~0; //Sets the layer mask to everything


        PlayerRef = PlayerManager.Instance.Player;


        AlertStatus = EnemyUtility.AlertState.Idle;

        //Assign the max health of the enemy using the profile multiplied by the difficulty
        MyEnemyStats.MaxHealth = 100;
        MyEnemyStats.Health = 100;


        if (MyHealthBar != null)
        {
            MyHealthBar.SetMaxHP(MyEnemyStats.MaxHealth);
            MyHealthBar.UpdateHealthbar(MyEnemyStats.Health);
            MyHealthBar.UpdateStateText(AlertStatus.ToString());

        }

        //Extra check at the end of startup to ensure everything is initialised correctly 
        //otherwise this enemy will default to dead
        if (
            MyWeaponController == null ||
            MyNavMeshAgent == null ||
            MyEnemyStats == null ||
            HeadCollider == null ||
            BodyCollider == null ||
            DetectionSphere == null ||
            DetectionAngle <= 0.0f ||
            DetectionRange <= 1.0f ||
            MyEnemyStats.Health <= 0 ||
            PlayerManager.Instance == null ||
            PlayerManager.Instance.Player == null)
        {
            Debug.LogError("Enemy Initialisation of " + transform.name + " was unsuccessful.");
            MyEnemyStats.IsDead = true;
        }
    }

    public LayerMask Enemies;

    private void FixedUpdate()
    {
        if (!MyEnemyStats.IsDead)
        {
            if (!MyEnemyStats.IsDead && !TargetSighted && AlertStatus == EnemyUtility.AlertState.Idle)
            {
                TargetSighted = DetectPlayer();
            }

            if (AlertStatus == EnemyUtility.AlertState.Idle)
            {
                Collider[] nearbyenemies = Physics.OverlapSphere(transform.position, 50.0f, Enemies);

                foreach (Collider collider in nearbyenemies)
                {
                    if (collider.GetComponent<EnemyController>().AlertStatus == EnemyUtility.AlertState.Hostile)
                    {
                        SwitchToHostile();
                        break;
                    }
                }
            }
        }
    }

    public void UpdateHealthbar()
    {
        if(MyHealthBar != null)
        {
            MyHealthBar.UpdateHealthbar(MyEnemyStats.Health);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!MyEnemyStats.IsDead)
        {
            switch (AlertStatus)
            {
                case EnemyUtility.AlertState.Idle:
                    if (PatrolPoints.Count > 0)
                    {
                        PlayWalkAnimation();
                        Patrol();
                    }

                    if (HitCounter > 0)
                    {
                        SwitchToHostile();
                    }

                    break;
                case EnemyUtility.AlertState.Hostile:

                    float Distance = Vector3.Distance(transform.position, PlayerRef.transform.position);

                    if (Distance > MyWeaponController.GetCurrentlyEquippedWeapon().WeaponRange)
                    {
                        PlayRunAnimation();
                        MoveIn();
                    }
                    else if (Distance < 10.0f)
                    {
                        PlayRunAnimation();
                        Retreat();
                    }
                    else
                    {
                        PlayShootAnimation();
                        HoldPosition();
                        FaceTarget();
                        Attack();
                    }
                    break;
            }
        }
        else
        {
            MyHealthBar.DisableHealthbar();
        }
    }

    private void SwitchToHostile()
    {
        AlertStatus = EnemyUtility.AlertState.Hostile;
        TargetSighted = true;
        if (MyHealthBar != null)
        {
            MyHealthBar.UpdateStateText(AlertStatus.ToString());
        }
    }

    //This function determines if the enemy can see the player
    private bool DetectPlayer()
    {

        Vector3 DirectionOfTarget = ((PlayerRef.transform.position + (Vector3.up * 2.0f)) - (transform.position + EyePosition)).normalized;

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
                if (hit.transform.CompareTag("Player"))
                {
                    SwitchToHostile();
                    return true;
                }
            }
        }

        return false;
    }

    //This function makes the enemy move through its list of patrol points
    private void Patrol()
    {
        if (PatrolPoints.Count > 1)
        {
            MyAnimator.SetBool("IsWalk", true);
            MyAnimator.SetBool("IsRun", false);
            MyAnimator.SetBool("IsIdle", false);
            MyAnimator.SetBool("IsSearch", false);
            MyAnimator.SetBool("IsShoot", false);

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

    //This function determines which hitbox a physical bullet object intersects
    public void IsHit(Collider bullet, int damageAmount)
    {
        //If the collider intersects the D-Sphere, increment the hit counter and move to suspicious if not already hostile
        if (DetectionSphere.bounds.Intersects(bullet.bounds))
        {
            HitCounter++;
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

            UpdateHealthbar();
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

            UpdateHealthbar();


            return;
        }
    }

    // This function determines which hitbox the player raycast intersects
    public void IsHit(Ray ray, int damageAmount)
    {
        //If the ray intersects the D-Sphere, increment the hit counter and move to suspicious if not already hostile
        if (DetectionSphere.bounds.IntersectRay(ray))
        {
            HitCounter++;
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
            UpdateHealthbar();


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

            UpdateHealthbar();


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

    //This function will fire the currently equipped weapon at the player
    private void Attack()
    {
        MyWeaponController.UseWeapon(transform.position + EyePosition, DirectionOfPlayer);
    }

    //This function will tell the enemy to stop moving
    private void HoldPosition()
    {
        MyNavMeshAgent.SetDestination(transform.position);
    }

    //This function will make the enemy flee from the player
    private void Retreat()
    {
        Vector3 direction = (PlayerRef.transform.position - transform.position).normalized;

        MyNavMeshAgent.SetDestination(transform.position -(direction * 10.0f));
    }

    //This function will tell the enemy to move towards the player's position
    private void MoveIn()
    {
        MyNavMeshAgent.SetDestination(PlayerRef.transform.position);
    }

    //This function rotates the enemy to face the given point (usually the player)
    void FaceTarget()
    {
        Vector3 direction = (PlayerRef.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
    }

    public void DisableColliders()
    {
        DetectionSphere.enabled = false;
        HeadCollider.enabled = false;
        BodyCollider.enabled = false;
    }


    void PlayWalkAnimation()
    {
        MyAnimator.SetBool("IsRun", false);
        MyAnimator.SetBool("IsShoot", false);
        MyAnimator.SetBool("IsIdle", false);
        MyAnimator.SetBool("IsWalk", true);
        MyAnimator.SetBool("IsSearch", false);
    }

    void PlayRunAnimation()
    {
        MyAnimator.SetBool("IsRun", true);
        MyAnimator.SetBool("IsShoot", false);
        MyAnimator.SetBool("IsIdle", false);
        MyAnimator.SetBool("IsWalk", false);
        MyAnimator.SetBool("IsSearch", false);
    }

    void PlayShootAnimation()
    {
        MyAnimator.SetBool("IsRun", false);
        MyAnimator.SetBool("IsShoot", true);
        MyAnimator.SetBool("IsIdle", false);
        MyAnimator.SetBool("IsWalk", false);
        MyAnimator.SetBool("IsSearch", false);
    }
}