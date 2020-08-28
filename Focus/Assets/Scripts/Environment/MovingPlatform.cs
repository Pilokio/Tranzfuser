using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public enum MoveAxis { x, y, z };

    //Control which axis the platform will move along (local)
    public MoveAxis Axis;
    //The speed at which it will move
    public float Speed = 5.0f;
    //The distance in each direction it will move before switching direction
    public float MoveRange = 5.0f;

    //Current direction of travel
    private Vector3 Direction = new Vector3();
    //Local cache of rigidbody component
    private Rigidbody rb;
    //The position of the platform on startup (ie the centre of its movement range)
    private Vector3 StartingPosition = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        //Init Rigidbody and starting position
        rb = GetComponent<Rigidbody>();
        StartingPosition = transform.position;

        //Determin the direction of travel and rigidbody constraints based on the chosen axis
        switch(Axis)
        {
            case MoveAxis.x:
                Direction = transform.right;
                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                break;
            case MoveAxis.y:
                Direction = transform.up;
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                break;
            case MoveAxis.z:
                Direction = transform.forward;
                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX| RigidbodyConstraints.FreezeRotation;
                break;
        }
    }

    private void FixedUpdate()
    {
        //Ensure the direction is inverted when reaching the edge of the movement range based on the axis of travel
        switch (Axis)
        {
            case MoveAxis.x:
                if (transform.position.x >= StartingPosition.x + MoveRange || transform.position.x <= StartingPosition.x - MoveRange)
                {
                    Direction.x *= -1;
                }
                break;
            case MoveAxis.y:
                if (transform.position.y >= StartingPosition.y + MoveRange || transform.position.y <= StartingPosition.y - MoveRange)
                {
                    Direction.y *= -1;
                }
                break;
            case MoveAxis.z:
                if (transform.position.z >= StartingPosition.z + MoveRange || transform.position.z <= StartingPosition.z - MoveRange)
                {
                    Direction.z *= -1;
                }
                break;
        }
        
        //Move the platform 
        rb.MovePosition(transform.position + Direction * Speed * Time.fixedDeltaTime);
    }

    private void OnValidate()
    {
        //Update the starting position in edit mode to update the gizmos
        StartingPosition = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        //Draw a line along the full extent of the platform's movement based on chosen axis
        Gizmos.color = Color.red;
        switch (Axis)
        {
            case MoveAxis.x:
                Gizmos.DrawLine(StartingPosition - (transform.right * MoveRange), StartingPosition + (transform.right * MoveRange));
                break;
            case MoveAxis.y:
                Gizmos.DrawLine(StartingPosition - (transform.up * MoveRange), StartingPosition + (transform.up * MoveRange));
                break;
            case MoveAxis.z:
                Gizmos.DrawLine(StartingPosition - (transform.forward * MoveRange), StartingPosition + (transform.forward * MoveRange));
                break;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //If a rigidbody touches this platform and remains, move it along with the platform
        if(collision.transform.GetComponent<Rigidbody>())
        {
            collision.transform.GetComponent<Rigidbody>().MovePosition(collision.transform.position + Direction * Speed * Time.fixedDeltaTime);
        }
    }
}
