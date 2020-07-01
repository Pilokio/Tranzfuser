using UnityEngine;

public class CharacterController3D : MonoBehaviour
{
    //The speed at which the character moves on the x and z axis
    [SerializeField] float MoveSpeed = 5.0f;

    //The height at which the character can jump to (influenced by gravity)
    [SerializeField] float JumpHeight = 100.0f;
    //The amount of downward velocity the character will recieve due to gravity
    [SerializeField] float Gravity = -1.0f;
    //The characters velocity on the y-axis (jumping and gravity)
    float Yvelocity = 0.0f;

    //Flag tracking whether the character should jump or not
    public bool Jump { get; set; }

    public bool RotatePlayerObject { get; set; }

    public void MoveCharacter(Vector2 MoveDirection, Vector2 LookDirection)
    {
        //Stop accumulating downward velocity due to gravity if the character is touching the ground
        if (GetComponent<CharacterController>().isGrounded && Yvelocity < 0)
        {
            Yvelocity = -2.0f;
        }

        if (RotatePlayerObject)
        {
            //Rotate the character based on the current look direction
            transform.Rotate(Vector3.up * LookDirection.x);
        }

        //Update the character's position based on the current move direction
        Vector3 move = (transform.right * MoveDirection.x) + (transform.forward * MoveDirection.y);

        //Increase the y-velocity allowing the character to 
        //jump if the flag is true and they are touching the ground
        if (Jump && GetComponent<CharacterController>().isGrounded)
        {
            //Calculate the jump velocity
            Yvelocity = Mathf.Sqrt(JumpHeight * -2.0f * Gravity);
            //Set the flag to false
            Jump = false; //FIXME alter to allow multiple jumps?
        }

        //Multiply the move vector by the move speed before applying gravity and assigning the yVelocity
        move *= MoveSpeed;
        Yvelocity += Gravity;
        move.y = Yvelocity;

        //Move the character using Unity's built-in character controller
        GetComponent<CharacterController>().Move(move * Time.deltaTime);
    }


}
