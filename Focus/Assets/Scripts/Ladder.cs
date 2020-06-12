using UnityEngine;

public class Ladder : MonoBehaviour
{
    // Speed the player will ascend/descend the ladder
    public float speed = 6;

    public GameObject player;

    // Here we are disabling the player manager script so the player can use the ladder 
    // (may need to change in future if we are wanting the player to jump off the ladder etc)
    public void OnTriggerEnter(Collider other)
    {
        player.GetComponent<Rigidbody>().useGravity = false;
        player.GetComponent<PlayerManager>().enabled = false;
    }

    private void OnTriggerStay(Collider other)
    {
        // Ascend ladder
        if (other.tag == "Player" && Input.GetKey(KeyCode.W))
        {
            player.GetComponent<Rigidbody>().useGravity = false;
            player.GetComponent<Rigidbody>().velocity = new Vector3(0, speed, 0);
        }

        // Descend ladder
        if (other.tag == "Player" && Input.GetKey(KeyCode.S))
        {
            player.GetComponent<Rigidbody>().useGravity = false;
            player.GetComponent<Rigidbody>().velocity = new Vector3(0, -speed, 0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        player.GetComponent<Rigidbody>().useGravity = true;
        player.GetComponent<PlayerManager>().enabled = true;
    }
}
