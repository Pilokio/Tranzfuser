using UnityEngine;
using UnityEngine.UI;

public class InputTester : MonoBehaviour
{
    public Text MoveTxt;
    public Text DPadTxt;
    public Text LookTxt;


    // Start is called before the first frame update
    void Start()
    {
        //Init the custom input manager and only track controllers connected on start
        CustomInputManager.InitialiseCustomInputManager();
        //Add bindings for a single player to the custom input manager
        CustomInputManager.CreateDefaultSinglePlayerInputManager();
        //Begin checking for controllers, to determine changes to the tracked controller list
        StartCoroutine(CustomInputManager.CheckForControllers());
    }

    // Update is called once per frame
    void Update()
    {
        if (CustomInputManager.GetButtonDown("ActionButton1"))
        {
            Debug.Log("ActionButton1 is pressed");
        }


        if (CustomInputManager.GetButtonDown("ActionButton2"))
        {
            Debug.Log("ActionButton2 is pressed");
        }

        if (CustomInputManager.GetButtonDown("ActionButton3"))
        {
            Debug.Log("ActionButton3 is pressed");
        }

        if (CustomInputManager.GetButtonDown("ActionButton4"))
        {
            Debug.Log("ActionButton4 is pressed");
        }

        if (CustomInputManager.GetButtonDown("LeftBumper"))
        {
            Debug.Log("Left Bumper is pressed");
        }

        if (CustomInputManager.GetButtonDown("RightBumper"))
        {
            Debug.Log("Right Bumper is pressed");
        }

        if (CustomInputManager.GetButtonDown("Menu"))
        {
            Debug.Log("Menu Button is pressed");
        }

        if (CustomInputManager.GetButtonDown("Share"))
        {
            Debug.Log("Share/Back Button is pressed");
        }

        if (CustomInputManager.GetButtonDown("LeftStick"))
        {
            Debug.Log("Left Stick is pressed");
        }

        if (CustomInputManager.GetButtonDown("RightStick"))
        {
            Debug.Log("Right Stick is pressed");
        }

        Vector2 Move = new Vector2(CustomInputManager.GetAxisRaw("LeftStickHorizontal"), CustomInputManager.GetAxisRaw("LeftStickVertical"));
        Vector2 Look = new Vector2(CustomInputManager.GetAxisRaw("RightStickHorizontal"), CustomInputManager.GetAxisRaw("RightStickVertical"));
        Vector2 Dpad = new Vector2(CustomInputManager.GetAxisRaw("DPadHorizontal"), CustomInputManager.GetAxisRaw("DPadVertical"));

        MoveTxt.text = "Move:" + Move.ToString();
        LookTxt.text = "Look:" + Look.ToString();
        DPadTxt.text = "DPad:" + Dpad.ToString();

        if (CustomInputManager.GetAxisAsButton("LeftTrigger"))
        {
            Debug.Log("Left Trigger Pressed");
        }

        if (CustomInputManager.GetAxisAsButton("RightTrigger"))
        {
            Debug.Log("Right Trigger Pressed");
        }
    }
}
