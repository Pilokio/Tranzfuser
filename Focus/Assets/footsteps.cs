using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class footsteps : MonoBehaviour
{
    PlayerMovement playerMovement;
    PlayerController playerController;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (playerMovement.grounded == true && playerMovement.IsMoving == true && playerMovement.GetComponent<AudioSource>().isPlaying == false)
        {
           // playerMovement.GetComponent<AudioSource>().volume = Random.Range(0.0f, 1);
            playerMovement.GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.1f);
            playerMovement.GetComponent<AudioSource>().Play();
        }
        else if (playerMovement.IsMoving == false)
        {
            playerMovement.GetComponent<AudioSource>().Stop();
        }

        if (playerController.IsWallRunning == true && playerMovement.GetComponent<AudioSource>().isPlaying == false)
        {
            //playerMovement.GetComponent<AudioSource>().volume = Random.Range(0.0f, 1);
            playerMovement.GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.1f);
            playerMovement.GetComponent<AudioSource>().Play();
        }
        else if (playerController.IsWallRunning == false && playerMovement.grounded == false)
        {
            playerMovement.GetComponent<AudioSource>().Stop();
        }
    }
}
