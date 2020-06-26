using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public class TimeControl : MonoBehaviour
{
    void Update()
    {
        // Get the Enemies global clock
        Clock clock = Timekeeper.instance.Clock("Player");

        // Change its time scale on key press
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            clock.localTimeScale = -1; // Rewind
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            clock.localTimeScale = 0; // Pause
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            clock.localTimeScale = 0.1f; // Slow
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            clock.localTimeScale = 1; // Normal
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            clock.localTimeScale = 2; // Accelerate
        }
    }
}
