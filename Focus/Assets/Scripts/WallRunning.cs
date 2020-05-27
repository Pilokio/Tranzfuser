using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [SerializeField] private float upForce;
    [SerializeField] private float sideForce;

    private Rigidbody rb;

    public Transform head;
    public Transform cam;

    // Is the player touching the wall on the left or the right?
    private bool isLeft;
    private bool isRight;

    private float distFromLeft;
    private float distFromRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        WallChecker();
    }

    // Checks if the player is colliding with a wall
    private void WallChecker()
    {
        RaycastHit leftWall;
        RaycastHit rightWall;

        if(Physics.Raycast(head.position, head.right, out rightWall))
        {
            distFromRight = Vector3.Distance(head.position, rightWall.point);
            if(distFromRight < 3f)
            {
                isRight = true;
                isLeft = false;
            }
        }

        if (Physics.Raycast(head.position, -head.right, out leftWall))
        {
            distFromLeft = Vector3.Distance(head.position, leftWall.point);
            if (distFromLeft < 3f)
            {
                isRight = false;
                isLeft = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            rb.useGravity = false;
            Debug.Log("Wall Running!");

            if (isRight)
            {
                // Can use Lerp for smooth action
                cam.localEulerAngles = new Vector3(0f, 0f, 10f);
            }
            if (isLeft)
            {
                cam.localEulerAngles = new Vector3(0f, 0f, -10f);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (isLeft)
                {
                    rb.AddForce(Vector3.up * upForce * Time.deltaTime);
                    rb.AddForce(head.right * sideForce * Time.deltaTime);
                }
                if (isRight)
                {
                    rb.AddForce(Vector3.up * upForce * Time.deltaTime);
                    rb.AddForce(-head.right * sideForce * Time.deltaTime);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            rb.useGravity = true;
            cam.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
    }
}
