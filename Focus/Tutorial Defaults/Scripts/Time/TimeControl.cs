﻿using Chronos;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimeControl : MonoBehaviour
{
    private Clock EnemyClock;
    private Clock PlayerClock;

    private float DefaultPlayerTimescale = 1.0f;
    private float DefaultEnemyTimescale = 1.0f;

    public float FocusDrainRate = 0.1f;
    public int FocusDrainAmount = 1;

    public float FocusRestoreRate = 0.1f;
    public int FocusRestoreAmount = 1;


    public int FocusMeter = 100;

    public Slider FocusBar;

    [Range(0, 100)]
    [SerializeField] float PlayerSlowPercentage = 50;
    [Range(0, 100)]
    [SerializeField] float EnemySlowPercentage = 50;

    private float MaxFocus = 100;

    public bool IsSlowMo = false;


    private void Start()
    {
        PlayerClock = Timekeeper.instance.Clock("Player");
        EnemyClock = Timekeeper.instance.Clock("Enemies");

        DefaultPlayerTimescale = PlayerClock.localTimeScale;
        DefaultEnemyTimescale = EnemyClock.localTimeScale;

        FocusBar.maxValue = MaxFocus;
        FocusBar.value = FocusMeter;
    }

    public void ToggleSlowMo()
    {
        if (!IsSlowMo)
            DoSlowMo();
        else
            StopSlowMo();
    }

    private void DoSlowMo()
    {
        IsSlowMo = true;

        //Slow the player and enemies
        PlayerClock.localTimeScale = DefaultPlayerTimescale * (PlayerSlowPercentage / 100);
        EnemyClock.localTimeScale = DefaultEnemyTimescale * (EnemySlowPercentage / 100);

        //stop restoring and start draining
        StopAllCoroutines();
        StartCoroutine(DrainFocusMeter());
    }



    private void StopSlowMo()
    {
        IsSlowMo = false;

        //Restore the player and enemies to the default time scale
        PlayerClock.localTimeScale = DefaultPlayerTimescale;
        EnemyClock.localTimeScale = DefaultEnemyTimescale;

        //Stop draining and start restoring
        StopAllCoroutines();
        StartCoroutine(RestoreFocus());
    }



    IEnumerator DrainFocusMeter()
    {
        //Drain focus value by drain amount every drain rate seconds while value is greater than 0
        while (FocusMeter > 0)
        {
            yield return new WaitForSeconds(FocusDrainRate);
            FocusMeter -= FocusDrainAmount;
            FocusBar.value = FocusMeter;
        }

        //Stop the slow mo when the focus value falls below 0
        FocusMeter = 0;
        StopSlowMo();
    }

    IEnumerator RestoreFocus()
    {
        //Restore focus value by restore amount every restore rate seconds while value is less than the max
        while (FocusMeter < MaxFocus)
        {
            yield return new WaitForSeconds(FocusRestoreRate);
            FocusMeter += FocusRestoreAmount;
            FocusBar.value = FocusMeter;
        }
    }
}