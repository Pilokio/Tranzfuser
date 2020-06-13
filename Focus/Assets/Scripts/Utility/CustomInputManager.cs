using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedJoystickData
{
    public int JoystickIndex;
    public string JoystickName;

    public ConnectedJoystickData(int index, string name)
    {
        JoystickIndex = index;
        JoystickName = name;
    }

    public void SetName(string name)
    {
        JoystickName = name;
    }
}
public static class CustomInputManager
{
    public static List<ConnectedJoystickData> ConnectedJoysticks = new List<ConnectedJoystickData>();
    public static bool ControllersConnected = false;

    /// <summary>
    /// This function will create a list of joystick names, ommitting the empty entries in the default joystick names array. Simulates an editor/build restart in regards to joystick tracking
    /// </summary>
    public static void InitialiseCustomInputManager()
    {
        //ReInit the list
        ConnectedJoysticks = new List<ConnectedJoystickData>();
        ActiveControllers.Clear();
        InputManager.Clear();
        //Loop through the array provided by Unity's built in Input system
        for(int i = 0; i < Input.GetJoystickNames().Length; i++)
        {
            //If the entry is not empty, add to the list
            //This will allow only the controllers connected at the time of this function call to be tracked
            if(Input.GetJoystickNames()[i] != "")
            {
                ConnectedJoysticks.Add(new ConnectedJoystickData(i, Input.GetJoystickNames()[i]));
            }
        }

      

        GetControllers();
    }
    
    /// <summary>
    /// How frequently (in seconds realtime) will a check for controllers be carried out
    /// </summary>
    public static float CheckControllerTime = 5.0f;

    /// <summary>
    /// Coroutine to check for connected controllers every 2 seconds realtime 
    /// </summary>
    public static IEnumerator CheckForControllers()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(CheckControllerTime);

            GetControllers();
        }
    }

    /// <summary>
    /// This function creates a default input manager for single player inputs.
    /// </summary>
    public static void CreateDefaultSinglePlayerInputManager()
    {
        ControllerBindings.PlayerNumber PlayerNumber;

        if (ConnectedJoysticks.Count > 0)
            PlayerNumber = (ControllerBindings.PlayerNumber)ConnectedJoysticks[0].JoystickIndex;
        else
            PlayerNumber = ControllerBindings.PlayerNumber.Player1;


        InputManager.Add("ActionButton1", new CustomInput( PlayerNumber, KeyCode.Space,       ControllerBindings.ControlType.ActionButton1));
        InputManager.Add("ActionButton2", new CustomInput(PlayerNumber, KeyCode.LeftControl, ControllerBindings.ControlType.ActionButton2)); 
        InputManager.Add("ActionButton3", new CustomInput(PlayerNumber, KeyCode.Tab,         ControllerBindings.ControlType.ActionButton3)); 
        InputManager.Add("ActionButton4", new CustomInput(PlayerNumber, KeyCode.R,           ControllerBindings.ControlType.ActionButton4)); 
        InputManager.Add("LeftBumper",    new CustomInput(PlayerNumber, KeyCode.Q,           ControllerBindings.ControlType.LeftBumper));
        InputManager.Add("RightBumper",   new CustomInput(PlayerNumber, KeyCode.E,           ControllerBindings.ControlType.RightBumper)); 
        InputManager.Add("Menu",          new CustomInput(PlayerNumber, KeyCode.P,           ControllerBindings.ControlType.MenuButton));  
        InputManager.Add("Share",         new CustomInput(PlayerNumber, KeyCode.L,           ControllerBindings.ControlType.ShareButton)); 
        InputManager.Add("LeftStick",     new CustomInput(PlayerNumber, KeyCode.LeftShift,   ControllerBindings.ControlType.LStickButton));
        InputManager.Add("RightStick",    new CustomInput(PlayerNumber, KeyCode.CapsLock,    ControllerBindings.ControlType.RStickButton));

        InputManager.Add("LeftStickHorizontal", new CustomInput(PlayerNumber, CustomInput.KeyboardAndMouseAxis.DefaultHorizontal, ControllerBindings.ControlType.LeftJoystickX)); 
        InputManager.Add("LeftStickVertical", new CustomInput(PlayerNumber, CustomInput.KeyboardAndMouseAxis.DefaultVertical, ControllerBindings.ControlType.LeftJoystickY, true));       
        InputManager.Add("RightStickHorizontal", new CustomInput(PlayerNumber, CustomInput.KeyboardAndMouseAxis.MouseX, ControllerBindings.ControlType.RightJoystickX));            
        InputManager.Add("RightStickVertical", new CustomInput(PlayerNumber, CustomInput.KeyboardAndMouseAxis.MouseY, ControllerBindings.ControlType.RightJoystickY, true));              
        InputManager.Add("DPadHorizontal", new CustomInput(PlayerNumber, CustomInput.KeyboardAndMouseAxis.SecondaryHorizontal, ControllerBindings.ControlType.DPadX));              
        InputManager.Add("DPadVertical", new CustomInput(PlayerNumber, CustomInput.KeyboardAndMouseAxis.SecondaryVertical, ControllerBindings.ControlType.DPadY));                  
        InputManager.Add("LeftTrigger", new CustomInput(PlayerNumber, KeyCode.Mouse1, ControllerBindings.ControlType.LeftTrigger));                                                 
        InputManager.Add("RightTrigger", new CustomInput(PlayerNumber, KeyCode.Mouse0, ControllerBindings.ControlType.RightTrigger));                                               
    }

    public static void SetKeyBinding(string InputName, KeyCode key)
    {
        if (InputManager.ContainsKey(InputName))
            InputManager[InputName].Key = key;
        else
            Debug.LogError(InputName + " is not present in the input manager. Unable to bind key");
    }
    /// <summary>
    /// This function creates a default input manager for multiplayer inputs. The button names are preceeded by "Player'X'", where 'X' is replaced with the player number you wish to recieve inputs from.
    /// </summary>
    public static void CreateDefaultMultiPlayerInputManager(int PlayerCount)
    {
        string playerName = "";
        for(int i = 0; i < Mathf.Min(ConnectedJoysticks.Count, PlayerCount); i++)
        {
            playerName = "Player" + (i + 1).ToString();

            InputManager.Add(playerName + "ActionButton1", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.Space, ControllerBindings.ControlType.ActionButton1));
            InputManager.Add(playerName + "ActionButton2", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.LeftControl, ControllerBindings.ControlType.ActionButton2));
            InputManager.Add(playerName + "ActionButton3", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.Tab, ControllerBindings.ControlType.ActionButton3));
            InputManager.Add(playerName + "ActionButton4", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.R, ControllerBindings.ControlType.ActionButton4));
            InputManager.Add(playerName + "LeftBumper", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.Q, ControllerBindings.ControlType.LeftBumper));
            InputManager.Add(playerName + "RightBumper", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.E, ControllerBindings.ControlType.RightBumper));
            InputManager.Add(playerName + "Menu", new CustomInput( (ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.P, ControllerBindings.ControlType.MenuButton));
            InputManager.Add(playerName + "Share", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.L, ControllerBindings.ControlType.ShareButton));
            InputManager.Add(playerName + "LeftStick", new CustomInput( (ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.LeftShift, ControllerBindings.ControlType.LStickButton));
            InputManager.Add(playerName + "RightStick", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.CapsLock, ControllerBindings.ControlType.RStickButton));
          
            InputManager.Add(playerName + "LeftStickHorizontal", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, CustomInput.KeyboardAndMouseAxis.DefaultHorizontal, ControllerBindings.ControlType.LeftJoystickX));
            InputManager.Add(playerName + "LeftStickVertical", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, CustomInput.KeyboardAndMouseAxis.DefaultVertical, ControllerBindings.ControlType.LeftJoystickY));
            InputManager.Add(playerName + "RightStickHorizontal", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, CustomInput.KeyboardAndMouseAxis.MouseX, ControllerBindings.ControlType.RightJoystickX));
            InputManager.Add(playerName + "RightStickVertical", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, CustomInput.KeyboardAndMouseAxis.MouseY, ControllerBindings.ControlType.RightJoystickY));
            InputManager.Add(playerName + "DPadHorizontal", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, CustomInput.KeyboardAndMouseAxis.SecondaryHorizontal, ControllerBindings.ControlType.DPadX));
            InputManager.Add(playerName + "DPadVertical", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, CustomInput.KeyboardAndMouseAxis.SecondaryVertical, ControllerBindings.ControlType.DPadY));
            InputManager.Add(playerName + "LeftTrigger", new CustomInput( (ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.Mouse1, ControllerBindings.ControlType.LeftTrigger));
            InputManager.Add(playerName + "RightTrigger", new CustomInput((ControllerBindings.PlayerNumber)ConnectedJoysticks[i].JoystickIndex, KeyCode.Mouse0, ControllerBindings.ControlType.RightTrigger));
        }
    }

    /// <summary>
    /// The list of inputs in the custom input manager, accessible via string name as with Unity's default system
    /// </summary>
    public static Dictionary<string, CustomInput> InputManager = new Dictionary<string, CustomInput>();

    /// <summary>
    /// A list of all supported controller types
    /// </summary>
    static List<SupportedController> SupportedControllers = new List<SupportedController>()
    {
        new SupportedController("PS4 for Windows", SupportedController.ControllerType.PS4, new string[]{"Wireless Controller"}),
        new SupportedController("XBOX One For Windows", SupportedController.ControllerType.XBOX1, new string[]{"Controller (Xbox One For Windows)"}),
        new SupportedController("XBOX360 for Windows", SupportedController.ControllerType.XBOX360, new string[]{
            "Controller (Afterglow Gamepad for Xbox 360)",
            "Controller (Batarang wired controller (XBOX))",
            "Controller (Gamepad for Xbox 360)",
            "Controller (Infinity Controller 360)",
            "Controller (Mad Catz FPS Pro GamePad)",
            "Controller (MadCatz Call of Duty GamePad)",
            "Controller (MadCatz GamePad)",
            "Controller (MLG GamePad for Xbox 360)",
            "Controller (Razer Sabertooth Elite)",
            "Controller (Rock Candy Gamepad for Xbox 360)",
            "Controller (Xbox 360 For Windows)",
            "Controller (Xbox 360 Wireless Receiver for Windows)",
            "XBOX 360 For Windows (Controller)",
            "Controller (XEOX Gamepad)"})

    };

    /// <summary>
    /// The list of currently active controllers. The result of the detect Controllers function.
    /// </summary>
    static Dictionary<ControllerBindings.PlayerNumber, ActiveController> ActiveControllers = new Dictionary<ControllerBindings.PlayerNumber, ActiveController>();

    /// <summary>
    /// This function is used when detecting controllers to handle the specifics of determining the type being used
    /// </summary>
    static void GetControllers()
    {

        //Handle if no controllers are connected
        if (ConnectedJoysticks.Count == 0 && !ActiveControllers.ContainsKey(ControllerBindings.PlayerNumber.Player1))
        {
            Debug.Log("No Controllers Connected"); 
            ControllersConnected = false;
             ActiveControllers.Add(ControllerBindings.PlayerNumber.Player1, new ActiveController(ControllerBindings.PlayerNumber.Player1, SupportedController.ControllerType.KEYANDMOUSE));
            return;
        }

        if (ConnectedJoysticks.Count == 0)
            ControllersConnected = false;
        else
            ControllersConnected = true;


        //Debug warning if too many controllers are connected
        if (ConnectedJoysticks.Count > 8)
        {
            Debug.LogWarning("Too many controllers detected. Only the first 8 will be accepted.");
        }

     
        string[] joysticksConnected = Input.GetJoystickNames();


        //Loop through the connected joystick list
        for (int ControllerNumber = 0; ControllerNumber < Mathf.Min(ConnectedJoysticks.Count, 8); ControllerNumber++)
        {
            //If the connected joystick name from the list does not match the one in the Input array
            if (ConnectedJoysticks[ControllerNumber].JoystickName != joysticksConnected[ConnectedJoysticks[ControllerNumber].JoystickIndex])
            {
                //Check if it is not null
                if (joysticksConnected[ConnectedJoysticks[ControllerNumber].JoystickIndex] != "")
                {
                    ConnectedJoysticks[ControllerNumber].SetName(joysticksConnected[ConnectedJoysticks[ControllerNumber].JoystickIndex]);
                }
                else
                {
                    ConnectedJoysticks[ControllerNumber].SetName("");
                    Debug.Log("Controller Disconnected. Defaulting to keyboard and mouse for Player " + ControllerNumber);
                    ActiveControllers[(ControllerBindings.PlayerNumber)(ConnectedJoysticks[ControllerNumber].JoystickIndex)].SetControllerType(SupportedController.ControllerType.KEYANDMOUSE);
                }
            }
        }

        //Loop through the connected joystick list
        for (int ControllerNumber = 0; ControllerNumber < Mathf.Min(ConnectedJoysticks.Count, 8); ControllerNumber++)
        {

            //Loop through all the supported controllers
            for (int i = 0; i < SupportedControllers.Count; i++)
            {
                //Loop through all the possible joystick names for this controller type
                foreach (string suppName in SupportedControllers[i].JoystickNames)
                {
                    //If the connected joystick name for this player is one of the supported joystick names 

                    if (suppName == ConnectedJoysticks[ControllerNumber].JoystickName)
                    {
                        //Add to the active controllers list if it isnt already
                        //otherwise update that player's controller type

                        if (!ActiveControllers.ContainsKey((ControllerBindings.PlayerNumber)(ConnectedJoysticks[ControllerNumber].JoystickIndex)) && ConnectedJoysticks[ControllerNumber].JoystickIndex < 8)
                        {
                            //Debug.Log("Adding " + SupportedControllers[i].Type + " controller for Player " + (ConnectedJoysticks[ControllerNumber].JoystickIndex).ToString());
                            ActiveControllers.Add((ControllerBindings.PlayerNumber)(ConnectedJoysticks[ControllerNumber].JoystickIndex), new ActiveController((ControllerBindings.PlayerNumber)(ConnectedJoysticks[ControllerNumber].JoystickIndex), SupportedControllers[i].Type));
                        }
                        else
                        {
                            if (ActiveControllers[(ControllerBindings.PlayerNumber)ConnectedJoysticks[ControllerNumber].JoystickIndex].ControllerType_ != SupportedControllers[i].Type)
                            {
                                //Debug.Log("Updating to " + SupportedControllers[i].Type + " controller for Player " + (ConnectedJoysticks[ControllerNumber].JoystickIndex).ToString());
                                ActiveControllers[(ControllerBindings.PlayerNumber)(ConnectedJoysticks[ControllerNumber].JoystickIndex)].SetControllerType(SupportedControllers[i].Type);
                            }
                        }
                    }
                }
            }
        }

        return;
    }

    /// <summary>
    /// Deprecated. Use GetControllers() instead.
    /// This function detects the type of controller type (if any) is being used by each connected player
    /// </summary>
    static void DetectControllers()
    { 
        //Handle if no controllers are connected
        if (ConnectedJoysticks.Count ==0 && !ActiveControllers.ContainsKey(ControllerBindings.PlayerNumber.Player1))
        {
            Debug.Log("No Controllers Connected");
            ActiveControllers.Add(ControllerBindings.PlayerNumber.Player1, new ActiveController(ControllerBindings.PlayerNumber.Player1, SupportedController.ControllerType.KEYANDMOUSE));
            return;
        }
        foreach(ConnectedJoystickData data in ConnectedJoysticks)
        {
            //Debug.Log("Index: " + data.JoystickIndex + ", Name: " + data.JoystickName);
        }
     

            //Debug warning if too many controllers are connected
            if (ConnectedJoysticks.Count > 8)
        {
            Debug.LogWarning("Too many controllers detected. Only the first 8 will be accepted.");
        }

        //Update the active controller list
        for (int i = 0; i < Mathf.Min(ConnectedJoysticks.Count, 8); i++)
        {
           // GetController(i);
        }
        return; 
    }

    /// <summary>
    /// This function performs similar to Unity's default Input.GetButtonDown() function, however supports multiple controller types as well as mouse & keyboard.
    /// </summary>
    public static bool GetButtonDown(string ButtonName)
    {
        if (!InputManager.ContainsKey(ButtonName))
        {
            Debug.LogError(ButtonName + " is not present in the custom input manager");
            return false;
        }

        CustomInput input = InputManager[ButtonName];

        if (input != null)
        {
            if (!input.ControllerBinding.IsAxis)
            {
                if (ActiveControllers.ContainsKey(input._PlayerNumber) && ControllersConnected)
                {
                    switch (ActiveControllers[input._PlayerNumber].ControllerType_)
                    {
                        case SupportedController.ControllerType.PS4:
                            return Input.GetKeyDown(input.ControllerBinding.Ps4Key);
                        case SupportedController.ControllerType.XBOX1:
                            return Input.GetKeyDown(input.ControllerBinding.Xbox1Key);
                        case SupportedController.ControllerType.XBOX360:
                            return Input.GetKeyDown(input.ControllerBinding.Xbox360Key);
                    }
                }
                return Input.GetKeyDown(input.Key);
            }
            else
            {
                Debug.LogError(ButtonName + " is an axis, not a button. Use GetAxis() instead");
            }
        }
        else
        {
            Debug.LogError(ButtonName + " does not exist in the CustomInputManager");
        }
        return false;
    }

    /// <summary>
    /// This function performs similar to Unity's default Input.GetButton() function, however supports multiple controller types as well as mouse & keyboard.
    /// </summary>
    public static bool GetButton(string ButtonName)
    {
        if (!InputManager.ContainsKey(ButtonName))
        {
            Debug.LogError(ButtonName + " is not present in the custom input manager");
            return false;
        }
        CustomInput input = InputManager[ButtonName];

        if (input != null)
        {
            if (!input.ControllerBinding.IsAxis)
            {
                if (ActiveControllers.ContainsKey(input._PlayerNumber) && ControllersConnected)
                {
                    switch (ActiveControllers[input._PlayerNumber].ControllerType_)
                    {
                        case SupportedController.ControllerType.PS4:
                            return Input.GetKey(input.ControllerBinding.Ps4Key);
                        case SupportedController.ControllerType.XBOX1:
                            return Input.GetKey(input.ControllerBinding.Xbox1Key);
                        case SupportedController.ControllerType.XBOX360:
                            return Input.GetKey(input.ControllerBinding.Xbox360Key);
                    }
                }
                return Input.GetKey(input.Key);
            }
            else
            {
                Debug.LogError(ButtonName + " is an axis, not a button. Use GetAxis() instead");
            }
        }
        else
        {
            Debug.LogError(ButtonName + " does not exist in the CustomInputManager");
        }
        return false;
    }

    /// <summary>
    /// This function performs similar to Unity's default Input.GetButtonUp() function, however supports multiple controller types as well as mouse & keyboard.
    /// </summary>
    public static bool GetButtonUp(string ButtonName)
    {
        if (!InputManager.ContainsKey(ButtonName))
        {
            Debug.LogError(ButtonName + " is not present in the custom input manager");
            return false;
        }

        CustomInput input = InputManager[ButtonName];

        if (input != null)
        {
            if (!input.ControllerBinding.IsAxis)
            {
                if (ActiveControllers.ContainsKey(input._PlayerNumber) && ControllersConnected)
                {
                    switch (ActiveControllers[input._PlayerNumber].ControllerType_)
                    {
                        case SupportedController.ControllerType.PS4:
                            return Input.GetKeyUp(input.ControllerBinding.Ps4Key);
                        case SupportedController.ControllerType.XBOX1:
                            return Input.GetKeyUp(input.ControllerBinding.Xbox1Key);
                        case SupportedController.ControllerType.XBOX360:
                            return Input.GetKeyUp(input.ControllerBinding.Xbox360Key);
                    }
                }
                return Input.GetKeyUp(input.Key);
            }
            else
            {
                Debug.LogError(ButtonName + " is an axis, not a button. Use GetAxis() instead");
            }
        }
        else
        {
            Debug.LogError(ButtonName + " does not exist in the CustomInputManager");
        }
        return false;
    }

    /// <summary>
    /// This function performs similar to Unity's default Input.GetAxis() function, however supports multiple controller types as well as mouse and keyboard.
    /// </summary>
    public static float GetAxis(string AxisName)
    {
        if (!InputManager.ContainsKey(AxisName))
        {
            Debug.LogError(AxisName + " is not present in the custom input manager");
            return 0.0f;
        }

        CustomInput input = InputManager[AxisName];

        if (input != null)
        {
            if (input.ControllerBinding.IsAxis)
            {
                if (ActiveControllers.ContainsKey(input._PlayerNumber) )
                {
                    switch (ActiveControllers[input._PlayerNumber].ControllerType_)
                    {
                        case SupportedController.ControllerType.PS4:
                            return Input.GetAxis(input.ControllerBinding.Ps4Name);
                        case SupportedController.ControllerType.XBOX1:
                            return Input.GetAxis(input.ControllerBinding.Xbox1Name);
                        case SupportedController.ControllerType.XBOX360:
                            return Input.GetAxis(input.ControllerBinding.Xbox360Name);
                        case SupportedController.ControllerType.KEYANDMOUSE:
                            if (input.AxisName != "")
                                return Input.GetAxis(input.AxisName);
                            else
                                Debug.LogError("Axis Name is null");
                            break;
                    }
                }
                if (input.AxisName != "")
                    return Input.GetAxis(input.AxisName);
                else
                    Debug.LogError("Axis Name is null");
            }
            else
            {
                Debug.LogError(AxisName + " is a button, not an axis. Use GetButton() instead");
            }
        }
        else
        {
            Debug.LogError(AxisName + " does not exist in the CustomInputManager");
        }
        return 0.0f;
    }

    /// <summary>
    /// This function performs similar to Unity's default Input.GetAxisRaw() function, however supports multiple controller types as well as mouse and keyboard.
    /// </summary>
    public static float GetAxisRaw(string AxisName)
    {
        if (!InputManager.ContainsKey(AxisName))
        {
            Debug.LogError(AxisName + " is not present in the custom input manager");
            return 0.0f;
        }

        CustomInput input = InputManager[AxisName];

        if (input != null)
        {
            if (input.ControllerBinding.IsAxis)
            {
                if (ActiveControllers.ContainsKey(input._PlayerNumber))
                {
                    switch (ActiveControllers[input._PlayerNumber].ControllerType_)
                    {
                        case SupportedController.ControllerType.PS4:
                            if (input.ControllerBinding.Ps4Name != "")
                            {
                                if (input.IsInverted)
                                    return Input.GetAxisRaw(input.ControllerBinding.Ps4Name) * -1;
                                else
                                    return Input.GetAxisRaw(input.ControllerBinding.Ps4Name);
                            }
                            else if (input.ControllerBinding.Ps4Key != KeyCode.None)
                            {
                                if (Input.GetKeyDown(input.ControllerBinding.Ps4Key))
                                    return 1.0f;
                                else
                                    return 0.0f;
                            }
                            else
                            {
                                Debug.LogError("Axis Name is null");
                            }
                            break;
                        case SupportedController.ControllerType.XBOX1:
                            if (input.ControllerBinding.Xbox1Name != "")
                            {
                                if (input.IsInverted)
                                    return Input.GetAxisRaw(input.ControllerBinding.Xbox1Name) * -1;
                                else
                                    return Input.GetAxisRaw(input.ControllerBinding.Xbox1Name);
                            }
                            else if (input.ControllerBinding.Xbox1Key != KeyCode.None)
                            {
                                if (Input.GetKeyDown(input.ControllerBinding.Xbox1Key))
                                    return 1.0f;
                                else
                                    return 0.0f;
                            }
                            else
                            {
                                Debug.LogError("Axis Name is null");
                            }
                            break;
                        case SupportedController.ControllerType.XBOX360:
                            if (input.ControllerBinding.Xbox360Name != "")
                            {
                                if (input.IsInverted)
                                    return Input.GetAxisRaw(input.ControllerBinding.Xbox360Name) * -1;
                                else
                                    return Input.GetAxisRaw(input.ControllerBinding.Xbox360Name);
                            }
                            else if (input.ControllerBinding.Xbox360Key != KeyCode.None)
                            {
                                if (Input.GetKeyDown(input.ControllerBinding.Xbox360Key))
                                    return 1.0f;
                                else
                                    return 0.0f;
                            }
                            else
                            {
                                Debug.LogError("Axis Name is null");
                            }
                            break;
                        case SupportedController.ControllerType.KEYANDMOUSE:
                            if (input.AxisName != "")
                            {
                                return Input.GetAxisRaw(input.AxisName);
                            }
                            else if (input.Key != KeyCode.None)
                            {
                                if (Input.GetKeyDown(input.Key))
                                    return 1.0f;
                                else
                                    return 0.0f;
                            }
                            else
                            {
                                Debug.LogError("Axis Name is null");
                            }
                            break;
                    }
                }

            }
            else
            {
                Debug.LogError(AxisName + " is a button, not an axis. Use GetButton() instead");
            }
        }
        else
        {
            Debug.LogError(AxisName + " does not exist in the CustomInputManager");
        }
        return 0.0f;
    }


    public static bool GetAxisAsButton(string AxisName)
    {
        if (!InputManager.ContainsKey(AxisName))
        {
            Debug.LogError(AxisName + " is not present in the custom input manager");
            return false;
        }

        CustomInput input = InputManager[AxisName];

        if (input != null)
        {
            if (input.ControllerBinding.IsAxis)
            {
                if (ActiveControllers.ContainsKey(input._PlayerNumber))
                {
                    switch (ActiveControllers[input._PlayerNumber].ControllerType_)
                    {
                        case SupportedController.ControllerType.PS4:
                            if (input.ControllerBinding.Ps4Name != "")
                            {
                                if(Input.GetAxisRaw(input.ControllerBinding.Ps4Name) != input.ControllerBinding.PS4Neutral)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (input.ControllerBinding.Ps4Key != KeyCode.None)
                            {
                                return Input.GetKeyDown(input.ControllerBinding.Ps4Key);
                            }
                            else
                            {
                                Debug.LogError("Axis Name is null");
                            }
                            break;
                        case SupportedController.ControllerType.XBOX1:
                            if (input.ControllerBinding.Xbox1Name != "")
                            {
                                if (Input.GetAxisRaw(input.ControllerBinding.Xbox1Name) != input.ControllerBinding.XB1Neutral)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (input.ControllerBinding.Xbox1Key != KeyCode.None)
                            {
                                return Input.GetKeyDown(input.ControllerBinding.Xbox1Key);
                            }
                            else
                            {
                                Debug.LogError("Axis Name is null");
                            }
                            break;
                        case SupportedController.ControllerType.XBOX360:
                            if (input.ControllerBinding.Xbox360Name != "")
                            {
                                if (Input.GetAxisRaw(input.ControllerBinding.Xbox360Name) != input.ControllerBinding.XB360Neutral)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (input.ControllerBinding.Xbox360Key != KeyCode.None)
                            {
                                return Input.GetKeyDown(input.ControllerBinding.Xbox360Key);
                            }
                            else
                            {
                                Debug.LogError("Axis Name is null");
                            }
                            break;
                        case SupportedController.ControllerType.KEYANDMOUSE:
                            if (input.AxisName != "")
                            {

                                if (Input.GetAxisRaw(input.AxisName) != 0.0f)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (input.Key != KeyCode.None)
                            {
                                return Input.GetKeyDown(input.Key);
                            }
                            else
                            {
                                Debug.LogError("Axis Name is null");
                            }
                            break;
                    }
                }
            }
            else
            {
                Debug.LogError(AxisName + " is a button, not an axis. Use GetButton() instead");
            }
        }
        else
        {
            Debug.LogError(AxisName + " does not exist in the CustomInputManager");
        }
        return false;
    }
}

/// <summary>
/// This class represents an active controller, containing the keybinding information, the controller type, and the player number
/// </summary>
public class ActiveController
{
    /// <summary>
    /// Which player is controlling this active controller
    /// </summary>
    ControllerBindings.PlayerNumber PlayerNumber;
    /// <summary>
    /// What type of controller is currently active
    /// </summary>
    public SupportedController.ControllerType ControllerType_ { get; private set; }

    public void SetControllerType(SupportedController.ControllerType type)
    {
        ControllerType_ = type;
    }

    /// <summary>
    /// Standard constructor
    /// </summary>
    public ActiveController(ControllerBindings.PlayerNumber player, SupportedController.ControllerType type)
    {
        PlayerNumber = player;
        ControllerType_ = type;
    }
}

/// <summary>
/// This class is used to represent the supported controller types and their associated data
/// </summary>
public class SupportedController
{
    /// <summary>
    /// The types of supported controllers
    /// </summary>
    public enum ControllerType
    {
        PS4,
        XBOX1,
        XBOX360,
        KEYANDMOUSE
    };

    /// <summary>
    /// The name assigned to this controller type
    /// </summary>
    public string Handle;
    /// <summary>
    /// The type of controller this represents
    /// </summary>
    public ControllerType Type { get; set; }
    /// <summary>
    /// The different possible names used to identify this controller with Unity's built-in Input manager
    /// </summary>
    public string[] JoystickNames;

    /// <summary>
    /// Standard constructor
    /// </summary>
    public SupportedController(string handle, ControllerType type, string[] joyNames)
    {
        Handle = handle;
        Type = type;
        JoystickNames = joyNames;
    }
}

/// <summary>
/// This class represents the custom inputs present in the custom input manager.
/// </summary>
public class CustomInput
{
    /// <summary>
    /// The controller this input is bound to
    /// </summary>
    public ControllerBindings.PlayerNumber _PlayerNumber;
    /// <summary>
    /// The keyboard and mouse Key binding
    /// </summary>
    public KeyCode Key;
    /// <summary>
    /// The keyboard and mouse axis name.
    /// </summary>
    public string AxisName;
    /// <summary>
    /// The keyboard and mouse axis type. Used for a predefined axis type
    /// </summary>
    public KeyboardAndMouseAxis AxisType;

    public bool IsInverted = false;

    /// <summary>
    /// The controller input type
    /// </summary>
    ControllerBindings.ControlType ControllerInput;

    /// <summary>
    /// The controller bindings for the different controller types
    /// </summary>
    public ControllerBindings.Binding ControllerBinding;

    /// <summary>
    /// Constructor for creating a custom input axis using an existing axis type
    /// </summary>
    public CustomInput(ControllerBindings.PlayerNumber playerNumber, KeyboardAndMouseAxis kmAxisType, ControllerBindings.ControlType controllerInputType)
    {
        _PlayerNumber = playerNumber;
        AxisType = kmAxisType;

        switch(AxisType)
        {
            case KeyboardAndMouseAxis.DefaultHorizontal:
                AxisName = "Horizontal";
                break;
            case KeyboardAndMouseAxis.DefaultVertical:
                AxisName = "Vertical";
                break;
            case KeyboardAndMouseAxis.MouseX:
                AxisName = "mouse_axis_0";
                break;
            case KeyboardAndMouseAxis.MouseY:
                AxisName = "mouse_axis_1";
                break;
            case KeyboardAndMouseAxis.SecondaryHorizontal:
                AxisName = "HorizontalSecondary";
                break;
            case KeyboardAndMouseAxis.SecondaryVertical:
                AxisName = "VerticalSecondary";
                break;
            default:
                AxisName = "";
                break;
        }
        Key = KeyCode.None;
        ControllerInput = controllerInputType;
        ControllerBinding = ControllerBindings.GetBinding(_PlayerNumber, ControllerInput);
    }

    public CustomInput(ControllerBindings.PlayerNumber playerNumber, KeyboardAndMouseAxis kmAxisType, ControllerBindings.ControlType controllerInputType, bool Inverted)
    {
        _PlayerNumber = playerNumber;
        AxisType = kmAxisType;

        switch (AxisType)
        {
            case KeyboardAndMouseAxis.DefaultHorizontal:
                AxisName = "Horizontal";
                break;
            case KeyboardAndMouseAxis.DefaultVertical:
                AxisName = "Vertical";
                break;
            case KeyboardAndMouseAxis.MouseX:
                AxisName = "mouse_axis_0";
                break;
            case KeyboardAndMouseAxis.MouseY:
                AxisName = "mouse_axis_1";
                break;
            case KeyboardAndMouseAxis.SecondaryHorizontal:
                AxisName = "HorizontalSecondary";
                break;
            case KeyboardAndMouseAxis.SecondaryVertical:
                AxisName = "VerticalSecondary";
                break;
            default:
                AxisName = "";
                break;
        }
        Key = KeyCode.None;
        ControllerInput = controllerInputType;
        ControllerBinding = ControllerBindings.GetBinding(_PlayerNumber, ControllerInput);
        IsInverted = Inverted;
    }

    /// <summary>
    /// Constructor for creating a custom input axis using a defined axis name. (ie one not included with the default input manager provided with this script)
    /// </summary>
    public CustomInput(ControllerBindings.PlayerNumber playerNumber, string kmAxisName, ControllerBindings.ControlType controllerInputType)
    {
        _PlayerNumber = playerNumber;
        AxisName = kmAxisName;
        AxisType = KeyboardAndMouseAxis.NULL;
        Key = KeyCode.None;
        ControllerInput = controllerInputType;
        ControllerBinding = ControllerBindings.GetBinding(_PlayerNumber, ControllerInput);
    }
    public CustomInput(ControllerBindings.PlayerNumber playerNumber, string kmAxisName, ControllerBindings.ControlType controllerInputType, bool Inverted)
    {
        _PlayerNumber = playerNumber;
        AxisName = kmAxisName;
        AxisType = KeyboardAndMouseAxis.NULL;
        Key = KeyCode.None;
        ControllerInput = controllerInputType;
        ControllerBinding = ControllerBindings.GetBinding(_PlayerNumber, ControllerInput);
        IsInverted = Inverted;
    }


    /// <summary>
    /// Constructor for creating a custom input button with a keyboard and mouse keybinding 
    /// </summary>
    public CustomInput(ControllerBindings.PlayerNumber playerNumber, KeyCode key, ControllerBindings.ControlType controllerInputType)
    {
        _PlayerNumber = playerNumber;
        AxisType = KeyboardAndMouseAxis.NULL;
        AxisName = "";
        Key = key;
        ControllerInput = controllerInputType;
        ControllerBinding = ControllerBindings.GetBinding(_PlayerNumber, ControllerInput);
    }

    /// <summary>
    /// The default keyboard and mouse axis types
    /// </summary>
    public enum KeyboardAndMouseAxis
    {
        NULL,
        MouseX,
        MouseY,
        MouseWheel,
        DefaultHorizontal,
        DefaultVertical,
        SecondaryHorizontal,
        SecondaryVertical
    }

}

