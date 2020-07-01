using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ladder : MonoBehaviour
{
    public bool IsOccupied = false;
    bool PlayerOccupied = false;

    //The player gameobject
    private GameObject player;

    private void Start()
    {
        //Use the player manager to access the player object from anywhere without editor assign
        //Useful for if the player is to be instantiated into the scene at runtime
        player = PlayerManager.Instance.Player;
    }


    private void OnCollisionEnter(Collision collision)
    {
        //Check the collider belongs to the player and not the enemy etc
        if (collision.transform.tag == "Player" && !player.GetComponent<PlayerController>().IsClimbing && !IsOccupied)
        {
            PlayerOccupied = true;
            IsOccupied = true;
           //Make the player turn to face the ladder
           Vector3 target = transform.rotation.eulerAngles;
            target.x = 0;
            target.z = 0;
            target.y += 180;
            player.transform.rotation = Quaternion.Euler(target);

            //Set the x and z position of the player to match the ladder climbing positions
            player.transform.position = new Vector3(LadderPath[1].position.x, player.transform.position.y, LadderPath[1].position.z);

            //Set the player to climb
            player.GetComponent<PlayerController>().SetIsClimbing(true);
        }
    }

    private void Update()
    {
        if (PlayerOccupied)
        {

            if (CustomInputManager.GetAxisRaw("LeftStickHorizontal") < CustomInputManager.GetAxisNeutralPosition("LeftStickHorizontal"))
            {
                player.GetComponent<PlayerController>().SetIsClimbing(false);
                IsOccupied = false;
                PlayerOccupied = false;
            }


            //If the player is close to the bottom of the ladder and still moving down
            if (Vector3.Distance(player.transform.position, LadderPath[2].position) < 0.5f && CustomInputManager.GetAxisRaw("LeftStickVertical") < CustomInputManager.GetAxisNeutralPosition("LeftStickVertical"))
            {
                IsOccupied = false;
                PlayerOccupied = false;
                //Stop climbing and resume normal movement
                player.GetComponent<PlayerController>().SetIsClimbing(false);
            }

            //If the player is near or above the top of the ladder and still moving up
            if (player.transform.position.y > LadderPath[1].position.y && CustomInputManager.GetAxisRaw("LeftStickVertical") > CustomInputManager.GetAxisNeutralPosition("LeftStickVertical"))
            {
                IsOccupied = false;
                PlayerOccupied = false;
                //Apply slight push to ensure they clear the top of the ladder
                player.transform.position = LadderPath[0].position;//+= Vector3.up + (player.transform.forward * 2.0f);
                                                                                //Stop climbing and resume normal movement
                player.GetComponent<PlayerController>().SetIsClimbing(false);
            }

        }
    }

    public List<Transform> LadderPath = new List<Transform>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && !IsOccupied)
        {
            if (!other.gameObject.GetComponent<EnemyController>().IsClimbing)
            {
                IsOccupied = true;
               other.gameObject.GetComponent<EnemyController>().IsClimbing = true;
                other.gameObject.GetComponent<Rigidbody>().useGravity = false;

                float distanceTop = Vector3.Distance(other.gameObject.transform.position, LadderPath[0].position);
                float distanceBottom = Vector3.Distance(other.gameObject.transform.position, LadderPath[2].position);

                //Make the player turn to face the ladder
                Vector3 target = transform.rotation.eulerAngles;
                target.x = 0;
                target.z = 0;
                target.y += 180;
                other.transform.rotation = Quaternion.Euler(target);

                if (distanceTop > distanceBottom)
                {
                    StartCoroutine(UseLadderNPC(other.gameObject, true));
                }
                else
                {
                    StartCoroutine(UseLadderNPC(other.gameObject, false));
                }
            }
        }
    }

    IEnumerator UseLadderNPC(GameObject NPC, bool climbingUp)
    {
        NPC.GetComponent<NavMeshAgent>().enabled = false;
        NPC.GetComponent<EnemyController>().enabled = false;
        float speed = 10.0f;
        while (NPC.GetComponent<EnemyController>().IsClimbing)
        {

            if (climbingUp)
            {
                while (NPC.transform.position != LadderPath[2].position)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, LadderPath[2].position, speed * Time.deltaTime);
                    yield return null;
                }

                while (NPC.transform.position != LadderPath[1].position)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, LadderPath[1].position, speed * Time.deltaTime);
                    yield return null;
                }

                while (NPC.transform.position != LadderPath[0].position)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, LadderPath[0].position, speed * Time.deltaTime);
                    yield return null;
                }
            }
            else
            {
                while (NPC.transform.position != LadderPath[0].position)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, LadderPath[0].position, speed * Time.deltaTime);
                    yield return null;
                }

                while (NPC.transform.position != LadderPath[1].position)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, LadderPath[1].position, speed * Time.deltaTime);
                    yield return null;
                }

                while (NPC.transform.position != LadderPath[2].position)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, LadderPath[2].position, speed * Time.deltaTime);
                    yield return null;
                }


            }

            IsOccupied = false;
            NPC.GetComponent<EnemyController>().IsClimbing = false;
        }
        NPC.GetComponent<NavMeshAgent>().enabled = true;
        NPC.GetComponent<EnemyController>().enabled = true;
    }


}
