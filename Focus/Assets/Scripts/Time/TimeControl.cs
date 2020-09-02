using Chronos;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
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

    [Range(0, 100)]
    [SerializeField] float PlayerSlowPercentage = 50;
    [Range(0, 100)]
    [SerializeField] float EnemySlowPercentage = 50;

    [SerializeField] public int MaxFocus = 100;
    public int FocusMeter { get; set; }

    public bool IsSlowMo = false;

    [SerializeField] GameObject SlowMoEffect;

    private void Start()
    {
        PlayerClock = Timekeeper.instance.Clock("Player");
        EnemyClock = Timekeeper.instance.Clock("Enemies");

        DefaultPlayerTimescale = PlayerClock.localTimeScale;
        DefaultEnemyTimescale = EnemyClock.localTimeScale;

        MaxFocus = 100;
        FocusMeter = MaxFocus;
        SlowMoEffect.SetActive(false);

    }

    public void ToggleSlowMo()
    {
        if (!IsSlowMo)
            DoSlowMo();
        else
            StopSlowMo();
    }

    public void SetSlowMo(bool slow)
    {
        if(slow)
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
        SlowMoEffect.SetActive(true);
        //stop restoring and start draining
        StopAllCoroutines();
        StartCoroutine(DrainFocusMeter());
    }



    private void StopSlowMo()
    {
        IsSlowMo = false;
        SlowMoEffect.SetActive(false);

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
        }
    }
}
