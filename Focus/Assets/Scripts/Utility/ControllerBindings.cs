using System.Collections.Generic;
using UnityEngine;

public static class ControllerBindings
{
    public enum PlayerNumber
    {
        Player1 = 0,
        Player2 = 1,
        Player3 = 2,
        Player4 = 3,
        Player5 = 4,
        Player6 = 5,
        Player7 = 6,
        Player8 = 7
    };

    public enum ControlType
    {
        LeftJoystickX,
        LeftJoystickY,
        RightJoystickX,
        RightJoystickY,
        DPadX,
        DPadY,
        LeftTrigger,
        RightTrigger,
        ActionButton1,
        ActionButton2,
        ActionButton3,
        ActionButton4,
        LeftBumper,
        RightBumper,
        MenuButton,
        ShareButton,
        LStickButton,
        RStickButton
    };
    public struct Binding
    {
        public ControlType CtrlType;

        public string Ps4Name;
        public string Xbox1Name;
        public string Xbox360Name;


        public KeyCode Ps4Key;
        public KeyCode Xbox1Key;
        public KeyCode Xbox360Key;

        public bool IsAxis;

        public float XB1Neutral, XB360Neutral, PS4Neutral;

        public Binding(ControlType ctrl, string ps4, string xbox1, string xbox360, KeyCode ps4K, KeyCode xbox1K, KeyCode xbox360k)
        {
            CtrlType = ctrl;

            if (CtrlType == ControlType.LeftJoystickX ||
                CtrlType == ControlType.LeftJoystickY ||
                CtrlType == ControlType.RightJoystickX ||
                CtrlType == ControlType.RightJoystickY ||
                CtrlType == ControlType.DPadX ||
                CtrlType == ControlType.DPadY ||
                CtrlType == ControlType.LeftTrigger ||
                CtrlType == ControlType.RightTrigger)
            {
                IsAxis = true;

            }
            else
            {
                IsAxis = false;
            }


            XB1Neutral = 0;
            XB360Neutral = 0;
            PS4Neutral = 0;


            Ps4Name = ps4;
            Xbox1Name = xbox1;
            Xbox360Name = xbox360;

            Ps4Key = ps4K;
            Xbox1Key = xbox1K;
            Xbox360Key = xbox360k;
        }

        public Binding(ControlType ctrl, string ps4, string xbox1, string xbox360, KeyCode ps4K, KeyCode xbox1K, KeyCode xbox360k, float ps4N, float xb1N, float xb360N)
        {
            CtrlType = ctrl;

            if (CtrlType == ControlType.LeftJoystickX ||
                CtrlType == ControlType.LeftJoystickY ||
                CtrlType == ControlType.RightJoystickX ||
                CtrlType == ControlType.RightJoystickY ||
                CtrlType == ControlType.DPadX ||
                CtrlType == ControlType.DPadY ||
                CtrlType == ControlType.LeftTrigger ||
                CtrlType == ControlType.RightTrigger)
            {
                IsAxis = true;
            }
            else
            {
                IsAxis = false;
            }


            XB1Neutral = xb1N;
            XB360Neutral = xb360N;
            PS4Neutral = ps4N;

            Ps4Name = ps4;
            Xbox1Name = xbox1;
            Xbox360Name = xbox360;

            Ps4Key = ps4K;
            Xbox1Key = xbox1K;
            Xbox360Key = xbox360k;
        }
    }

    public static Dictionary<ControlType, Binding>[] ControllerSupport =
    {
        new Dictionary<ControlType, Binding>()
        {
        {ControlType.LeftJoystickX,     new Binding(ControlType.LeftJoystickX,  "joy_0_axis_0", "joy_0_axis_0", "joy_0_axis_0", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftJoystickY,     new Binding(ControlType.LeftJoystickY,  "joy_0_axis_1", "joy_0_axis_1", "joy_0_axis_1", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickX,    new Binding(ControlType.RightJoystickX, "joy_0_axis_2", "joy_0_axis_3", "joy_0_axis_3", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickY,    new Binding(ControlType.RightJoystickY, "joy_0_axis_5", "joy_0_axis_4", "joy_0_axis_4", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftTrigger,       new Binding(ControlType.LeftTrigger,    "joy_0_axis_3", "joy_0_axis_8", "joy_0_axis_8", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.RightTrigger,      new Binding(ControlType.RightTrigger,   "joy_0_axis_4", "joy_0_axis_9", "joy_0_axis_9", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.DPadX,             new Binding(ControlType.DPadX,          "joy_0_axis_6", "joy_0_axis_5", "joy_0_axis_5", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.DPadY,             new Binding(ControlType.DPadY,          "joy_0_axis_7", "joy_0_axis_6", "joy_0_axis_6", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.ActionButton1,     new Binding(ControlType.ActionButton1,  "", "", "", KeyCode.Joystick1Button1 , KeyCode.Joystick1Button0, KeyCode.Joystick1Button0)},
        {ControlType.ActionButton2,     new Binding(ControlType.ActionButton2,  "", "", "", KeyCode.Joystick1Button2 , KeyCode.Joystick1Button1, KeyCode.Joystick1Button1)},
        {ControlType.ActionButton3,     new Binding(ControlType.ActionButton3,  "", "", "", KeyCode.Joystick1Button3 , KeyCode.Joystick1Button3, KeyCode.Joystick1Button2)},
        {ControlType.ActionButton4,     new Binding(ControlType.ActionButton4,  "", "", "", KeyCode.Joystick1Button0 , KeyCode.Joystick1Button2, KeyCode.Joystick1Button3)},
        {ControlType.LeftBumper,        new Binding(ControlType.LeftBumper,     "", "", "", KeyCode.Joystick1Button4 , KeyCode.Joystick1Button4, KeyCode.Joystick1Button4)},
        {ControlType.RightBumper,       new Binding(ControlType.RightBumper,    "", "", "", KeyCode.Joystick1Button5 , KeyCode.Joystick1Button5, KeyCode.Joystick1Button5)},
        {ControlType.ShareButton,       new Binding(ControlType.ShareButton,    "", "", "", KeyCode.Joystick1Button8 , KeyCode.Joystick1Button6, KeyCode.Joystick1Button6)},
        {ControlType.MenuButton,        new Binding(ControlType.MenuButton,     "", "", "", KeyCode.Joystick1Button9 , KeyCode.Joystick1Button7, KeyCode.Joystick1Button7)},
        {ControlType.LStickButton,      new Binding(ControlType.LStickButton,   "", "", "", KeyCode.Joystick1Button10, KeyCode.Joystick1Button8, KeyCode.Joystick1Button8)},
        {ControlType.RStickButton,      new Binding(ControlType.RStickButton,   "", "", "", KeyCode.Joystick1Button11, KeyCode.Joystick1Button9, KeyCode.Joystick1Button9)}

        },
        new Dictionary<ControlType, Binding>()
    {
        {ControlType.LeftJoystickX,    new Binding(ControlType.LeftJoystickX,  "joy_1_axis_0", "joy_1_axis_0", "joy_1_axis_0", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftJoystickY,    new Binding(ControlType.LeftJoystickY,  "joy_1_axis_1", "joy_1_axis_1", "joy_1_axis_1", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickX,   new Binding(ControlType.RightJoystickX, "joy_1_axis_2", "joy_1_axis_3", "joy_1_axis_3", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickY,   new Binding(ControlType.RightJoystickY, "joy_1_axis_5", "joy_1_axis_4", "joy_1_axis_4", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftTrigger,      new Binding(ControlType.LeftTrigger,    "joy_1_axis_3", "joy_1_axis_8", "joy_1_axis_8", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.RightTrigger,     new Binding(ControlType.RightTrigger,   "joy_1_axis_4", "joy_1_axis_9", "joy_1_axis_9", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.DPadX,            new Binding(ControlType.DPadX,          "joy_1_axis_6", "joy_1_axis_5", "joy_1_axis_5", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.DPadY,            new Binding(ControlType.DPadY,          "joy_1_axis_7", "joy_1_axis_6", "joy_1_axis_6", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.ActionButton1,    new Binding(ControlType.ActionButton1,  "", "", "", KeyCode.Joystick2Button1 , KeyCode.Joystick2Button0, KeyCode.Joystick2Button0)},
        {ControlType.ActionButton2,    new Binding(ControlType.ActionButton2,  "", "", "", KeyCode.Joystick2Button2 , KeyCode.Joystick2Button1, KeyCode.Joystick2Button1)},
        {ControlType.ActionButton3,    new Binding(ControlType.ActionButton3,  "", "", "", KeyCode.Joystick2Button3 , KeyCode.Joystick2Button3, KeyCode.Joystick2Button2)},
        {ControlType.ActionButton4,    new Binding(ControlType.ActionButton4,  "", "", "", KeyCode.Joystick2Button0 , KeyCode.Joystick2Button2, KeyCode.Joystick2Button3)},
        {ControlType.LeftBumper,       new Binding(ControlType.LeftBumper,     "", "", "", KeyCode.Joystick2Button4 , KeyCode.Joystick2Button4, KeyCode.Joystick2Button4)},
        {ControlType.RightBumper,      new Binding(ControlType.RightBumper,    "", "", "", KeyCode.Joystick2Button5 , KeyCode.Joystick2Button5, KeyCode.Joystick2Button5)},
        {ControlType.ShareButton,      new Binding(ControlType.ShareButton,    "", "", "", KeyCode.Joystick2Button8 , KeyCode.Joystick2Button6, KeyCode.Joystick2Button6)},
        {ControlType.MenuButton,       new Binding(ControlType.MenuButton,     "", "", "", KeyCode.Joystick2Button9 , KeyCode.Joystick2Button7, KeyCode.Joystick2Button7)},
        {ControlType.LStickButton,     new Binding(ControlType.LStickButton,   "", "", "", KeyCode.Joystick2Button10, KeyCode.Joystick2Button8, KeyCode.Joystick2Button8)},
        {ControlType.RStickButton,    new Binding(ControlType.RStickButton,   "", "", "",  KeyCode.Joystick2Button11, KeyCode.Joystick2Button9, KeyCode.Joystick2Button9)}
    },
        new Dictionary<ControlType, Binding>()
    {
        {ControlType.LeftJoystickX,    new Binding(ControlType.LeftJoystickX,  "joy_2_axis_0", "joy_2_axis_0", "joy_2_axis_0", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftJoystickY,    new Binding(ControlType.LeftJoystickY,  "joy_2_axis_1", "joy_2_axis_1", "joy_2_axis_1", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickX,   new Binding(ControlType.RightJoystickX, "joy_2_axis_2", "joy_2_axis_3", "joy_2_axis_3", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickY,   new Binding(ControlType.RightJoystickY, "joy_2_axis_5", "joy_2_axis_4", "joy_2_axis_4", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftTrigger,      new Binding(ControlType.LeftTrigger,    "joy_2_axis_3", "joy_2_axis_8", "joy_2_axis_8", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.RightTrigger,     new Binding(ControlType.RightTrigger,   "joy_2_axis_4", "joy_2_axis_9", "joy_2_axis_9", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.DPadX,            new Binding(ControlType.DPadX,          "joy_2_axis_6", "joy_2_axis_5", "joy_2_axis_5", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.DPadY,            new Binding(ControlType.DPadY,          "joy_2_axis_7", "joy_2_axis_6", "joy_2_axis_6", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.ActionButton1,    new Binding(ControlType.ActionButton1,  "", "", "", KeyCode.Joystick3Button1 , KeyCode.Joystick3Button0, KeyCode.Joystick3Button0)},
        {ControlType.ActionButton2,    new Binding(ControlType.ActionButton2,  "", "", "", KeyCode.Joystick3Button2 , KeyCode.Joystick3Button1, KeyCode.Joystick3Button1)},
        {ControlType.ActionButton3,    new Binding(ControlType.ActionButton3,  "", "", "", KeyCode.Joystick3Button3 , KeyCode.Joystick3Button3, KeyCode.Joystick3Button2)},
        {ControlType.ActionButton4,    new Binding(ControlType.ActionButton4,  "", "", "", KeyCode.Joystick3Button0 , KeyCode.Joystick3Button2, KeyCode.Joystick3Button3)},
        {ControlType.LeftBumper,       new Binding(ControlType.LeftBumper,     "", "", "", KeyCode.Joystick3Button4 , KeyCode.Joystick3Button4, KeyCode.Joystick3Button4)},
        {ControlType.RightBumper,      new Binding(ControlType.RightBumper,    "", "", "", KeyCode.Joystick3Button5 , KeyCode.Joystick3Button5, KeyCode.Joystick3Button5)},
        {ControlType.ShareButton,      new Binding(ControlType.ShareButton,    "", "", "", KeyCode.Joystick3Button8 , KeyCode.Joystick3Button6, KeyCode.Joystick3Button6)},
        {ControlType.MenuButton,       new Binding(ControlType.MenuButton,     "", "", "", KeyCode.Joystick3Button9 , KeyCode.Joystick3Button7, KeyCode.Joystick3Button7)},
        {ControlType.LStickButton,     new Binding(ControlType.LStickButton,   "", "", "", KeyCode.Joystick3Button10, KeyCode.Joystick3Button8, KeyCode.Joystick3Button8)},
        {ControlType.RStickButton,     new Binding(ControlType.RStickButton,   "", "", "", KeyCode.Joystick3Button11, KeyCode.Joystick3Button9, KeyCode.Joystick3Button9)}
    },
        new Dictionary<ControlType, Binding>()
    {
        {ControlType.LeftJoystickX,    new Binding(ControlType.LeftJoystickX,  "joy_3_axis_0", "joy_3_axis_0", "joy_3_axis_0", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftJoystickY,    new Binding(ControlType.LeftJoystickY,  "joy_3_axis_1", "joy_3_axis_1", "joy_3_axis_1", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickX,   new Binding(ControlType.RightJoystickX, "joy_3_axis_2", "joy_3_axis_3", "joy_3_axis_3", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickY,   new Binding(ControlType.RightJoystickY, "joy_3_axis_5", "joy_3_axis_4", "joy_3_axis_4", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftTrigger,      new Binding(ControlType.LeftTrigger,    "joy_3_axis_3", "joy_3_axis_8", "joy_3_axis_8", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.RightTrigger,     new Binding(ControlType.RightTrigger,   "joy_3_axis_4", "joy_3_axis_9", "joy_3_axis_9", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.DPadX,            new Binding(ControlType.DPadX,          "joy_3_axis_6", "joy_3_axis_5", "joy_3_axis_5", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.DPadY,            new Binding(ControlType.DPadY,          "joy_3_axis_7", "joy_3_axis_6", "joy_3_axis_6", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.ActionButton1,    new Binding(ControlType.ActionButton1,  "", "", "", KeyCode.Joystick4Button1 , KeyCode.Joystick4Button0, KeyCode.Joystick4Button0)},
        {ControlType.ActionButton2,    new Binding(ControlType.ActionButton2,  "", "", "", KeyCode.Joystick4Button2 , KeyCode.Joystick4Button1, KeyCode.Joystick4Button1)},
        {ControlType.ActionButton3,    new Binding(ControlType.ActionButton3,  "", "", "", KeyCode.Joystick4Button3 , KeyCode.Joystick4Button3, KeyCode.Joystick4Button2)},
        {ControlType.ActionButton4,    new Binding(ControlType.ActionButton4,  "", "", "", KeyCode.Joystick4Button0 , KeyCode.Joystick4Button2, KeyCode.Joystick4Button3)},
        {ControlType.LeftBumper,       new Binding(ControlType.LeftBumper,     "", "", "", KeyCode.Joystick4Button4 , KeyCode.Joystick4Button4, KeyCode.Joystick4Button4)},
        {ControlType.RightBumper,      new Binding(ControlType.RightBumper,    "", "", "", KeyCode.Joystick4Button5 , KeyCode.Joystick4Button5, KeyCode.Joystick4Button5)},
        {ControlType.ShareButton,      new Binding(ControlType.ShareButton,    "", "", "", KeyCode.Joystick4Button8 , KeyCode.Joystick4Button6, KeyCode.Joystick4Button6)},
        {ControlType.MenuButton,       new Binding(ControlType.MenuButton,     "", "", "", KeyCode.Joystick4Button9 , KeyCode.Joystick4Button7, KeyCode.Joystick4Button7)},
        {ControlType.LStickButton,     new Binding(ControlType.LStickButton,   "", "", "", KeyCode.Joystick4Button10, KeyCode.Joystick4Button8, KeyCode.Joystick4Button8)},
        {ControlType.RStickButton,     new Binding(ControlType.RStickButton,   "", "", "", KeyCode.Joystick4Button11, KeyCode.Joystick4Button9, KeyCode.Joystick4Button9)}
    },
        new Dictionary<ControlType, Binding>()
    {
        {ControlType.LeftJoystickX,    new Binding(ControlType.LeftJoystickX,  "joy_4_axis_0", "joy_4_axis_0", "joy_4_axis_0", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftJoystickY,    new Binding(ControlType.LeftJoystickY,  "joy_4_axis_1", "joy_4_axis_1", "joy_4_axis_1", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickX,   new Binding(ControlType.RightJoystickX, "joy_4_axis_2", "joy_4_axis_3", "joy_4_axis_3", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickY,   new Binding(ControlType.RightJoystickY, "joy_4_axis_5", "joy_4_axis_4", "joy_4_axis_4", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftTrigger,      new Binding(ControlType.LeftTrigger,    "joy_4_axis_3", "joy_4_axis_8", "joy_4_axis_8", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.RightTrigger,     new Binding(ControlType.RightTrigger,   "joy_4_axis_4", "joy_4_axis_9", "joy_4_axis_9", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.DPadX,            new Binding(ControlType.DPadX,          "joy_4_axis_6", "joy_4_axis_5", "joy_4_axis_5", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.DPadY,            new Binding(ControlType.DPadY,          "joy_4_axis_7", "joy_4_axis_6", "joy_4_axis_6", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.ActionButton1,    new Binding(ControlType.ActionButton1,  "", "", "", KeyCode.Joystick5Button1 , KeyCode.Joystick5Button0, KeyCode.Joystick5Button0)},
        {ControlType.ActionButton2,    new Binding(ControlType.ActionButton2,  "", "", "", KeyCode.Joystick5Button2 , KeyCode.Joystick5Button1, KeyCode.Joystick5Button1)},
        {ControlType.ActionButton3,    new Binding(ControlType.ActionButton3,  "", "", "", KeyCode.Joystick5Button3 , KeyCode.Joystick5Button3, KeyCode.Joystick5Button2)},
        {ControlType.ActionButton4,    new Binding(ControlType.ActionButton4,  "", "", "", KeyCode.Joystick5Button0 , KeyCode.Joystick5Button2, KeyCode.Joystick5Button3)},
        {ControlType.LeftBumper,       new Binding(ControlType.LeftBumper,     "", "", "", KeyCode.Joystick5Button4 , KeyCode.Joystick5Button4, KeyCode.Joystick5Button4)},
        {ControlType.RightBumper,      new Binding(ControlType.RightBumper,    "", "", "", KeyCode.Joystick5Button5 , KeyCode.Joystick5Button5, KeyCode.Joystick5Button5)},
        {ControlType.ShareButton,      new Binding(ControlType.ShareButton,    "", "", "", KeyCode.Joystick5Button8 , KeyCode.Joystick5Button6, KeyCode.Joystick5Button6)},
        {ControlType.MenuButton,       new Binding(ControlType.MenuButton,     "", "", "", KeyCode.Joystick5Button9 , KeyCode.Joystick5Button7, KeyCode.Joystick5Button7)},
        {ControlType.LStickButton,     new Binding(ControlType.LStickButton,   "", "", "", KeyCode.Joystick5Button10, KeyCode.Joystick5Button8, KeyCode.Joystick5Button8)},
        {ControlType.RStickButton,     new Binding(ControlType.RStickButton,   "", "", "", KeyCode.Joystick5Button11, KeyCode.Joystick5Button9, KeyCode.Joystick5Button9)}
    },
        new Dictionary<ControlType, Binding>()
    {
        {ControlType.LeftJoystickX,    new Binding(ControlType.LeftJoystickX,  "joy_5_axis_0", "joy_5_axis_0", "joy_5_axis_0", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftJoystickY,    new Binding(ControlType.LeftJoystickY,  "joy_5_axis_1", "joy_5_axis_1", "joy_5_axis_1", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickX,   new Binding(ControlType.RightJoystickX, "joy_5_axis_2", "joy_5_axis_3", "joy_5_axis_3", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickY,   new Binding(ControlType.RightJoystickY, "joy_5_axis_5", "joy_5_axis_4", "joy_5_axis_4", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftTrigger,      new Binding(ControlType.LeftTrigger,    "joy_5_axis_3", "joy_5_axis_8", "joy_5_axis_8", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.RightTrigger,     new Binding(ControlType.RightTrigger,   "joy_5_axis_4", "joy_5_axis_9", "joy_5_axis_9", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.DPadX,            new Binding(ControlType.DPadX,          "joy_5_axis_6", "joy_5_axis_5", "joy_5_axis_5", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.DPadY,            new Binding(ControlType.DPadY,          "joy_5_axis_7", "joy_5_axis_6", "joy_5_axis_6", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.ActionButton1,    new Binding(ControlType.ActionButton1,  "", "", "", KeyCode.Joystick6Button1 , KeyCode.Joystick6Button0, KeyCode.Joystick6Button0)},
        {ControlType.ActionButton2,    new Binding(ControlType.ActionButton2,  "", "", "", KeyCode.Joystick6Button2 , KeyCode.Joystick6Button1, KeyCode.Joystick6Button1)},
        {ControlType.ActionButton3,    new Binding(ControlType.ActionButton3,  "", "", "", KeyCode.Joystick6Button3 , KeyCode.Joystick6Button3, KeyCode.Joystick6Button2)},
        {ControlType.ActionButton4,    new Binding(ControlType.ActionButton4,  "", "", "", KeyCode.Joystick6Button0 , KeyCode.Joystick6Button2, KeyCode.Joystick6Button3)},
        {ControlType.LeftBumper,       new Binding(ControlType.LeftBumper,     "", "", "", KeyCode.Joystick6Button4 , KeyCode.Joystick6Button4, KeyCode.Joystick6Button4)},
        {ControlType.RightBumper,      new Binding(ControlType.RightBumper,    "", "", "", KeyCode.Joystick6Button5 , KeyCode.Joystick6Button5, KeyCode.Joystick6Button5)},
        {ControlType.ShareButton,      new Binding(ControlType.ShareButton,    "", "", "", KeyCode.Joystick6Button8 , KeyCode.Joystick6Button6, KeyCode.Joystick6Button6)},
        {ControlType.MenuButton,       new Binding(ControlType.MenuButton,     "", "", "", KeyCode.Joystick6Button9 , KeyCode.Joystick6Button7, KeyCode.Joystick6Button7)},
        {ControlType.LStickButton,     new Binding(ControlType.LStickButton,   "", "", "", KeyCode.Joystick6Button10, KeyCode.Joystick6Button8, KeyCode.Joystick6Button8)},
        {ControlType.RStickButton,     new Binding(ControlType.RStickButton,   "", "", "", KeyCode.Joystick6Button11, KeyCode.Joystick6Button9, KeyCode.Joystick6Button9)}
    },
        new Dictionary<ControlType, Binding>()
    {
        {ControlType.LeftJoystickX,    new Binding(ControlType.LeftJoystickX,  "joy_6_axis_0", "joy_6_axis_0", "joy_6_axis_0", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftJoystickY,    new Binding(ControlType.LeftJoystickY,  "joy_6_axis_1", "joy_6_axis_1", "joy_6_axis_1", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickX,   new Binding(ControlType.RightJoystickX, "joy_6_axis_2", "joy_6_axis_3", "joy_6_axis_3", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickY,   new Binding(ControlType.RightJoystickY, "joy_6_axis_5", "joy_6_axis_4", "joy_6_axis_4", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftTrigger,      new Binding(ControlType.LeftTrigger,    "joy_6_axis_3", "joy_6_axis_8", "joy_6_axis_8", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.RightTrigger,     new Binding(ControlType.RightTrigger,   "joy_6_axis_4", "joy_6_axis_9", "joy_6_axis_9", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.DPadX,            new Binding(ControlType.DPadX,          "joy_6_axis_6", "joy_6_axis_5", "joy_6_axis_5", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.DPadY,            new Binding(ControlType.DPadY,          "joy_6_axis_7", "joy_6_axis_6", "joy_6_axis_6", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.ActionButton1,    new Binding(ControlType.ActionButton1,  "", "", "", KeyCode.Joystick7Button1 , KeyCode.Joystick7Button0, KeyCode.Joystick7Button0)},
        {ControlType.ActionButton2,    new Binding(ControlType.ActionButton2,  "", "", "", KeyCode.Joystick7Button2 , KeyCode.Joystick7Button1, KeyCode.Joystick7Button1)},
        {ControlType.ActionButton3,    new Binding(ControlType.ActionButton3,  "", "", "", KeyCode.Joystick7Button3 , KeyCode.Joystick7Button3, KeyCode.Joystick7Button2)},
        {ControlType.ActionButton4,    new Binding(ControlType.ActionButton4,  "", "", "", KeyCode.Joystick7Button0 , KeyCode.Joystick7Button2, KeyCode.Joystick7Button3)},
        {ControlType.LeftBumper,       new Binding(ControlType.LeftBumper,     "", "", "", KeyCode.Joystick7Button4 , KeyCode.Joystick7Button4, KeyCode.Joystick7Button4)},
        {ControlType.RightBumper,      new Binding(ControlType.RightBumper,    "", "", "", KeyCode.Joystick7Button5 , KeyCode.Joystick7Button5, KeyCode.Joystick7Button5)},
        {ControlType.ShareButton,      new Binding(ControlType.ShareButton,    "", "", "", KeyCode.Joystick7Button8 , KeyCode.Joystick7Button6, KeyCode.Joystick7Button6)},
        {ControlType.MenuButton,       new Binding(ControlType.MenuButton,     "", "", "", KeyCode.Joystick7Button9 , KeyCode.Joystick7Button7, KeyCode.Joystick7Button7)},
        {ControlType.LStickButton,     new Binding(ControlType.LStickButton,   "", "", "", KeyCode.Joystick7Button10, KeyCode.Joystick7Button8, KeyCode.Joystick7Button8)},
        {ControlType.RStickButton,     new Binding(ControlType.RStickButton,   "", "", "", KeyCode.Joystick7Button11, KeyCode.Joystick7Button9, KeyCode.Joystick7Button9)}
    },
        new Dictionary<ControlType, Binding>()
    {
        {ControlType.LeftJoystickX,    new Binding(ControlType.LeftJoystickX,  "joy_7_axis_0", "joy_7_axis_0", "joy_7_axis_0", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftJoystickY,    new Binding(ControlType.LeftJoystickY,  "joy_7_axis_1", "joy_7_axis_1", "joy_7_axis_1", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickX,   new Binding(ControlType.RightJoystickX, "joy_7_axis_2", "joy_7_axis_3", "joy_7_axis_3", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.RightJoystickY,   new Binding(ControlType.RightJoystickY, "joy_7_axis_5", "joy_7_axis_4", "joy_7_axis_4", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.LeftTrigger,      new Binding(ControlType.LeftTrigger,    "joy_7_axis_3", "joy_7_axis_8", "joy_7_axis_8", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.RightTrigger,     new Binding(ControlType.RightTrigger,   "joy_7_axis_4", "joy_7_axis_9", "joy_7_axis_9", KeyCode.None, KeyCode.None, KeyCode.None, -1.0f, 0.0f, 0.0f)},
        {ControlType.DPadX,            new Binding(ControlType.DPadX,          "joy_7_axis_6", "joy_7_axis_5", "joy_7_axis_5", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.DPadY,            new Binding(ControlType.DPadY,          "joy_7_axis_7", "joy_7_axis_6", "joy_7_axis_6", KeyCode.None, KeyCode.None, KeyCode.None)},
        {ControlType.ActionButton1,    new Binding(ControlType.ActionButton1,  "", "", "", KeyCode.Joystick8Button1 , KeyCode.Joystick8Button0, KeyCode.Joystick8Button0)},
        {ControlType.ActionButton2,    new Binding(ControlType.ActionButton2,  "", "", "", KeyCode.Joystick8Button2 , KeyCode.Joystick8Button1, KeyCode.Joystick8Button1)},
        {ControlType.ActionButton3,    new Binding(ControlType.ActionButton3,  "", "", "", KeyCode.Joystick8Button3 , KeyCode.Joystick8Button3, KeyCode.Joystick8Button2)},
        {ControlType.ActionButton4,    new Binding(ControlType.ActionButton4,  "", "", "", KeyCode.Joystick8Button0 , KeyCode.Joystick8Button2, KeyCode.Joystick8Button3)},
        {ControlType.LeftBumper,       new Binding(ControlType.LeftBumper,     "", "", "", KeyCode.Joystick8Button4 , KeyCode.Joystick8Button4, KeyCode.Joystick8Button4)},
        {ControlType.RightBumper,      new Binding(ControlType.RightBumper,    "", "", "", KeyCode.Joystick8Button5 , KeyCode.Joystick8Button5, KeyCode.Joystick8Button5)},
        {ControlType.ShareButton,      new Binding(ControlType.ShareButton,    "", "", "", KeyCode.Joystick8Button8 , KeyCode.Joystick8Button6, KeyCode.Joystick8Button6)},
        {ControlType.MenuButton,       new Binding(ControlType.MenuButton,     "", "", "", KeyCode.Joystick8Button9 , KeyCode.Joystick8Button7, KeyCode.Joystick8Button7)},
        {ControlType.LStickButton,     new Binding(ControlType.LStickButton,   "", "", "", KeyCode.Joystick8Button10, KeyCode.Joystick8Button8, KeyCode.Joystick8Button8)},
        {ControlType.RStickButton,     new Binding(ControlType.RStickButton,   "", "", "", KeyCode.Joystick8Button11, KeyCode.Joystick8Button9, KeyCode.Joystick8Button9)}
    }
    };




    public static Binding GetBinding(PlayerNumber numb, ControlType type)
    {
        return ControllerSupport[(int)numb][type];
    }
}
