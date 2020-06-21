using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ladder : MonoBehaviour
{
    //Transforms for the top and bottom of the ladder
    public Transform Top;
    public Transform Bottom;

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
        if (collision.transform.tag == "Player")
        {

            //Make the player turn to face the ladder
            Vector3 target = transform.rotation.eulerAngles;
            target.x = 0;
            target.z = 0;
            target.y += 180;
            player.transform.rotation = Quaternion.Euler(target);


            //Set the x and z position of the player to match the ladder climbing positions
            player.transform.position = new Vector3(Bottom.position.x, player.transform.position.y, Bottom.position.z);

            //Set the player to climb
            player.GetComponent<PlayerController>().IsClimbing = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //Check the collider belongs to the player and not the enemy etc
        if (collision.transform.tag == "Player")
        {
            //If the player is close to the bottom of the ladder and still moving down
            if (Vector3.Distance(player.transform.position, Bottom.position) < 1 && CustomInputManager.GetAxisRaw("LeftStickVertical") < 0)
            {
                //Stop climbing and resume normal movement
                player.GetComponent<PlayerController>().IsClimbing = false;
            }

            //If the player is near or above the top of the ladder and still moving up
            if (player.transform.position.y > Top.position.y - 0.5f && CustomInputManager.GetAxisRaw("LeftStickVertical") > 0)
            {
                //Apply slight push to ensure they clear the top of the ladder
                player.transform.position += Vector3.up + (player.transform.forward * 2.0f);
                //Stop climbing and resume normal movement
                player.GetComponent<PlayerController>().IsClimbing = false;
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        //Check that the exiting collider is the enemy
        if (collision.transform.tag == "Player")
        {
            //Resume normal player movement
            player.GetComponent<PlayerController>().IsClimbing = false;
        }
    }

    //The path the NPC will follow when using the ladder
    public List<Transform> LadderPath = new List<Transform>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {

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
                Debug.Log("NPC is climbing up");
                StartCoroutine(UseLadderNPC(other.gameObject, true));
            }
            else
            {
                Debug.Log("NPC is climbing down");
                StartCoroutine(UseLadderNPC(other.gameObject, false));
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

           
            NPC.GetComponent<EnemyController>().IsClimbing = false;
        }
        NPC.GetComponent<NavMeshAgent>().enabled = true;
        NPC.GetComponent<EnemyController>().enabled = true;
        Debug.Log("Finished");
    }


}
