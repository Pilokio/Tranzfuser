using UnityEngine;

public class Ladder : MonoBehaviour
{
    // Speed the player will ascend/descend the ladder
    public float speed = 6;

    public GameObject player;

    private void OnTriggerStay(Collider other)
    {
        // Ascend ladder
        if (other.tag == "Player" && Input.GetKey(KeyCode.W))
        {
            //Debug.Log("Touching ladder");
            Physics.gravity = new Vector3(0, 0, 0); //Invert
            player.GetComponent<Rigidbody>().velocity = new Vector3(0, speed, 0);
        }

        // Descend ladder
        // Currently not working
        if (other.tag == "Player" && Input.GetKey(KeyCode.S))
        {
            Physics.gravity = new Vector3(0, 0, 0); //Invert
            player.GetComponent<Rigidbody>().velocity = new Vector3(0, -speed, 0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Physics.gravity = new Vector3(0, -30f, 0); //Invert
    }
}
