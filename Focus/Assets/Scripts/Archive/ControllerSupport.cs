using System.Collections;
using UnityEngine;


//How To Use
//Simply start the CheckForControllers coroutine once at the beginning of the application
//And when handling input, check the relevant CustomButtons and axis etc. in the appropriate scripts

/// <summary>
/// This script handles the detection of whether a controller is connected,
/// and if so, what controller type it is. Additional controllers can be added by creating a bool to track their connection status,
/// and by adding their joystick name to the array of supported names
/// </summary>
static class ControllerSupport
{
    //The 4 action buttons on the right hand side of the controller starting with the bottom
    //and working anti-clockwise
    public static CustomButton ActionButton1 = new CustomButton();
    public static CustomButton ActionButton2 = new CustomButton();
    public static CustomButton ActionButton3 = new CustomButton();
    public static CustomButton ActionButton4 = new CustomButton();



    public static CustomButton Fire2 = new CustomButton();
    public static CustomButton ActionButton5 = new CustomButton();
    public static CustomButton Fire1 = new CustomButton();
    public static CustomButton ActionButton6 = new CustomButton();


    //The standard horizontal and vertical axis. Mapped to the left joystick on controllers
    public static CustomAxis LeftHorizontal = new CustomAxis();
    public static CustomAxis LeftVertical = new CustomAxis();

    //The Horizontal and vertical axis, mapped to the right joystick on controllers, and the mouse X & Y 
    public static CustomAxis RightHorizontal = new CustomAxis();
    public static CustomAxis RightVertical = new CustomAxis();

    //The D-Pad on controllers
    public static CustomAxis DirectionHorizontal = new CustomAxis();
    public static CustomAxis DirectionVertical = new CustomAxis();

    //The Left & Right Triggers on the controllers 
    //Used only by their CustomButton counterparts
    private static CustomAxis LeftTriggerAxis = new CustomAxis();
    private static CustomAxis RightTriggerAxis = new CustomAxis();




    public static bool ControllerSupportInitialised = false;

    /// <summary>
    ///This function initialises the 4 action buttons on the controller with default values
    ///NB each action button can be individually initialised elsewhere if preferred 
    /// </summary>
    public static void InitialiseControllerSupport()
    {
        LeftHorizontal = LeftHorizontal.Create(CustomAxis.AxisName.LeftHorizontal);
        LeftVertical = LeftVertical.Create(CustomAxis.AxisName.LeftVertical);
        RightHorizontal = RightHorizontal.Create(CustomAxis.AxisName.RightHorizontal);
        RightVertical = RightVertical.Create(CustomAxis.AxisName.RightVertical);
        DirectionHorizontal = DirectionHorizontal.Create(CustomAxis.AxisName.DirectionHorizontal);
        DirectionVertical = DirectionVertical.Create(CustomAxis.AxisName.DirectionVertical);
        LeftTriggerAxis = LeftTriggerAxis.Create(CustomAxis.AxisName.LeftTrigger);
        RightTriggerAxis = RightTriggerAxis.Create(CustomAxis.AxisName.RightTrigger);

        ActionButton1 = ActionButton1.Create("PS4CrossButton", "XBOXAButton", KeyCode.Space);
        ActionButton2 = ActionButton2.Create("PS4CircleButton", "XBOXBButton", KeyCode.Tab);
        ActionButton3 = ActionButton3.Create("PS4TriangleButton", "XBOXYButton", KeyCode.V);
        ActionButton4 = ActionButton4.Create("PS4SquareButton", "XBOXXButton", KeyCode.R);


        Fire2 = Fire2.Create("LeftTriggerPS4", "LeftTriggerXBOX", KeyCode.Mouse1, true);
        ActionButton5 = ActionButton5.Create("LeftButtonPS4", "LeftBumperXBOX", KeyCode.X);
        Fire1 = Fire1.Create("RightTriggerPS4", "RightTriggerXBOX", KeyCode.Mouse0, true);
        ActionButton6 = ActionButton6.Create("RightButtonPS4", "RightBumperXBOX", KeyCode.Z);


        ControllerSupportInitialised = true;
    }


    /// <summary>
    ///List of supported joystick names
    /// </summary>
    public static string[] SupportedJoystickNames =
    {
        "Wireless Controller"
    };

    //Flags to track the types of controllers connected
    public static bool PS4Win = false;
    public static bool XBoxWin = false;
    public static bool NoControllersConnected = true;


    /// <summary>
    /// Coroutine to check for connected controllers every 2 seconds realtime 
    /// </summary>
    public static IEnumerator CheckForControllers()
    {
        if (!ControllerSupportInitialised)
        {
            InitialiseControllerSupport();
        }


        while (true)
        {
            yield return new WaitForSecondsRealtime(2.0f);

            //If controllers are detected, loop through the list
            for (int i = 0; i < Input.GetJoystickNames().Length; i++)
            {
                //Double check that the string is not null or empty
                //Otherwise there are no controllers connected
                if (!string.IsNullOrEmpty(Input.GetJoystickNames()[i]))
                {
                    //Set the various controller flags where appropriate

                    NoControllersConnected = false;

                    if (Input.GetJoystickNames()[i] == SupportedJoystickNames[0])
                    {
                        Debug.Log("PS4 Controller Connected");
                        PS4Win = true;
                    }
                    else
                    {
                        Debug.LogError("Unsupported Joystick Connected. Controller Support script must be modified to allow additional controller types.");
                        NoControllersConnected = true;
                    }

                    i = Input.GetJoystickNames().Length;
                }
                else
                {
                    //No Controller connected/Controller Disconnected
                    if (PS4Win)
                    {
                        Debug.Log("Controller Disconnected");
                    }

                    i = Input.GetJoystickNames().Length;

                    PS4Win = false;
                    NoControllersConnected = true;
                }
            }
        }
    }
}

/// <summary>
/// This class is used to represent custom buttons, with controller support as well as keyboard and mouse
/// </summary>
public class CustomButton
{
    private string PS4ButtonName;
    private string XBOX1ButtonName;
    private KeyCode KeyboardButtonName = KeyCode.Tilde;
    private bool IsAxis = false;
    private bool AxisButtonReady = true;


    /// <summary>
    /// Initialiser for the custom button 
    /// </summary>
    public CustomButton Create(string PS4Map, string XBOX1Map, KeyCode keyboardKey)
    {
        CustomButton newButton = new CustomButton();
        newButton.PS4ButtonName = PS4Map;
        newButton.XBOX1ButtonName = XBOX1Map;
        newButton.KeyboardButtonName = keyboardKey;
        return newButton;
    }


    /// <summary>
    /// Overload for the custom button initialiser to handle axis being used as buttons (ie the triggers on a PS4 controller)
    /// </summary>
    public CustomButton Create(string PS4Map, string XBOX1Map, KeyCode keyboardKey, bool axis)
    {
        CustomButton newButton = new CustomButton();
        newButton.PS4ButtonName = PS4Map;
        newButton.XBOX1ButtonName = XBOX1Map;
        newButton.KeyboardButtonName = keyboardKey;
        newButton.IsAxis = axis;
        return newButton;
    }


    /// <summary>
    /// It is not recommended to use this function. Instead manually assign the desired keycode in ControllerSupport.InitialiseControllerSupport.
    ///IT IS NOT ADVISED TO CHANGE AT RUNTIME
    /// </summary>
    public void SetKeyboardKey(KeyCode newCode)
    {
        KeyboardButtonName = newCode;
    }


    /// <summary>
    /// This function acts the same as the standard Input.GetButtonDown() and will return true if the button is pressed
    /// For Axis that are being treated as buttons, the ResetAxisButton() coroutine must be called after detecting its press as true
    ///  to prevent it acting like Input.GetButton() as a cooldown is applied to prevent multiple triggers
    /// </summary>
    public bool GetCustomButtonDown()
    {
        //If there are any controllers connected check controller buttons, rather than mouse&keyboard
        if (!ControllerSupport.NoControllersConnected)
        {
            //PS4 Controller 
            if (ControllerSupport.PS4Win)
            {
                //Used for the triggers on the controller as they are categorised as axis rather than buttons
                if (IsAxis)
                {
                    //Check the button is ready as well as pressed, as axis would trigger this multiple times otherwise
                    if (Input.GetAxisRaw(PS4ButtonName) == -1.0f && AxisButtonReady)
                    {
                        AxisButtonReady = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                //If it is not an axis just return the GetButtonDown as normal
                return Input.GetButtonDown(PS4ButtonName);
            }

            //XBOX One Controller
            if (ControllerSupport.XBoxWin)
            {
                //Used for the triggers on the controller as they are categorised as axis rather than buttons
                if (IsAxis)
                {
                    //Check the button is ready as well as pressed, as axis would trigger this multiple times otherwise
                    if (Input.GetAxisRaw(XBOX1ButtonName) == 0.0f && AxisButtonReady)
                    {
                        AxisButtonReady = false;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                //If it is not an axis just return the GetButtonDown as normal
                return Input.GetButtonDown(XBOX1ButtonName);
            }
        }
        else
        {
            //If no controllers are connected then return the assigned KeyCode state
            return Input.GetKeyDown(KeyboardButtonName);
        }

        //if for some reason this code is reached, return false to be safe
        return false;
    }


    /// <summary>
    /// This function acts the same as the standard Input.GetButton(), it will return true if the button is held down
    /// </summary>
    public bool GetCustomButton()
    {
        //If there are any controllers connected check controller buttons, rather than mouse&keyboard
        if (!ControllerSupport.NoControllersConnected)
        {
            //PS4 Controller 
            if (ControllerSupport.PS4Win)
            {
                //Used for the triggers on the controller as they are categorised as axis rather than buttons
                if (IsAxis)
                {
                    if (Input.GetAxisRaw(PS4ButtonName) == -1.0f)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                //If it is not an axis just return the GetButton as normal
                return Input.GetButton(PS4ButtonName);
            }

            if (ControllerSupport.XBoxWin)
            {
                //Used for the triggers on the controller as they are categorised as axis rather than buttons
                if (IsAxis)
                {
                    if (Input.GetAxisRaw(XBOX1ButtonName) == 0.0f)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                //If it is not an axis just return the GetButton as normal
                return Input.GetButton(XBOX1ButtonName);
            }
        }
        else
        {
            return Input.GetKey(KeyboardButtonName);
        }

        return false;
    }

    /// <summary>
    /// This function acts as the Input.GetButtonUp for this custom button
    /// </summary>
    public bool GetCustomButtonUp()
    {
        //If there are any controllers connected check controller buttons, rather than mouse&keyboard
        if (!ControllerSupport.NoControllersConnected)
        {
            if (ControllerSupport.PS4Win)
            {
                //Used for the triggers on the controller as they are categorised as axis rather than buttons
                if (IsAxis)
                {
                    if (Input.GetAxis(PS4ButtonName) == -1.0f)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                //If it is not an axis just return the GetButtonUp as normal
                return Input.GetButtonUp(PS4ButtonName);
            }

            if (ControllerSupport.XBoxWin)
            {
                //Used for the triggers on the controller as they are categorised as axis rather than buttons
                if (IsAxis)
                {
                    if (Input.GetAxis(XBOX1ButtonName) >= 0.5f)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                //If it is not an axis just return the GetButtonUp as normal
                return Input.GetButtonUp(XBOX1ButtonName);
            }
        }
        else
        {
            return Input.GetKeyUp(KeyboardButtonName);
        }

        return false;
    }


    /// <summary>
    /// This coroutine is used to reset the button if it is an axis. It is necessary to start this
    /// after detecting the button press if it has been pressed to prevent multiple presses per frame. 
    /// NB not necessary for GetButton(), only GetButtonDown()
    /// </summary>
    public IEnumerator ResetAxisButton()
    {
        yield return new WaitForSeconds(0.5f);
        AxisButtonReady = true;
    }
}



/// <summary>
/// Exactly the same as the CustomButton class, however for axis instead
/// </summary>
public class CustomAxis
{
    public enum AxisName { LeftHorizontal, LeftVertical, RightHorizontal, RightVertical, DirectionHorizontal, DirectionVertical, LeftTrigger, RightTrigger };

    private AxisName MyAxisName;

    /// <summary>
    /// Initialiser for the custom axis
    /// </summary>
    public CustomAxis Create(AxisName name)
    {
        CustomAxis newAxis = new CustomAxis();
        newAxis.MyAxisName = name;
        return newAxis;
    }


    /// <summary>
    /// Returns the GetAxis value for the custom axis depending on the input being used 
    /// </summary>
    public float GetAxis()
    {
        switch (MyAxisName)
        {
            case AxisName.LeftHorizontal:
                return Input.GetAxis("Horizontal");
            case AxisName.LeftVertical:
                return Input.GetAxis("Vertical");
            case AxisName.RightHorizontal:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxis("HorizontalRightStickPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxis("HorizontalRightStickXBOX");
                    }
                }
                else
                {
                    return Input.GetAxis("Mouse X");
                }
                break;
            case AxisName.RightVertical:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxis("VerticalRightStickPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxis("VerticalRightStickXBOX");
                    }
                }
                else
                {
                    return Input.GetAxis("Mouse Y");
                }
                break;
            case AxisName.DirectionHorizontal:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxis("D-PadHorizontalPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxis("D-PadHorizontalXBOX");
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        return -1;
                    }

                    if (Input.GetKey(KeyCode.RightArrow))
                    {
                        return 1;
                    }

                    return 0;
                }
                break;
            case AxisName.DirectionVertical:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxis("D-PadVerticalPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxis("D-PadVerticalXBOX");
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                    {
                        return 1;
                    }

                    if (Input.GetKey(KeyCode.DownArrow))
                    {
                        return -1;
                    }

                    return 0;
                }
                break;
            case AxisName.LeftTrigger:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxis("LeftTriggerPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxis("LeftTriggerXBOX");
                    }
                }
                else
                {
                    return 0;
                }
                break;
            case AxisName.RightTrigger:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxis("RightTriggerPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxis("RightTriggerXBOX");
                    }
                }
                else
                {
                    return 0;
                }
                break;
            default:
                Debug.LogError("Invalid Axis Type");
                return 0;
        }
        return 0;
    }

    /// <summary>
    /// Returns the GetAxisRaw value for the custom axis depending on the input being used 
    /// </summary>
    public float GetAxisRaw()
    {
        switch (MyAxisName)
        {
            case AxisName.LeftHorizontal:
                return Input.GetAxisRaw("Horizontal");
            case AxisName.LeftVertical:
                return Input.GetAxisRaw("Vertical");
            case AxisName.RightHorizontal:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxisRaw("HorizontalRightStickPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxisRaw("HorizontalRightStickXBOX");
                    }
                }
                else
                {
                    return Input.GetAxisRaw("Mouse X");
                }
                break;
            case AxisName.RightVertical:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxisRaw("VerticalRightStickPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxisRaw("VerticalRightStickXBOX");
                    }
                }
                else
                {
                    return Input.GetAxisRaw("Mouse Y");
                }
                break;
            case AxisName.DirectionHorizontal:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxisRaw("D-PadHorizontalPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxisRaw("D-PadHorizontalXBOX");
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        return -1;
                    }

                    if (Input.GetKey(KeyCode.RightArrow))
                    {
                        return 1;
                    }

                    return 0;
                }
                break;
            case AxisName.DirectionVertical:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxisRaw("D-PadVerticalPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxisRaw("D-PadVerticalXBOX");
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                    {
                        return 1;
                    }

                    if (Input.GetKey(KeyCode.DownArrow))
                    {
                        return -1;
                    }

                    return 0;
                }
                break;
            case AxisName.LeftTrigger:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxisRaw("LeftTriggerPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxisRaw("LeftTriggerXBOX");
                    }
                }
                else
                {
                    return 0;
                }
                break;
            case AxisName.RightTrigger:
                if (!ControllerSupport.NoControllersConnected)
                {
                    if (ControllerSupport.PS4Win)
                    {
                        return Input.GetAxisRaw("RightTriggerPS4");
                    }

                    if (ControllerSupport.XBoxWin)
                    {
                        return Input.GetAxisRaw("RightTriggerXBOX");
                    }
                }
                else
                {
                    return 0;
                }
                break;
            default:
                Debug.LogError("Invalid Axis Type");
                return 0;
        }
        return 0;
    }

}