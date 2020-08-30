using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HealthBar : MonoBehaviour
{
#pragma warning disable 0649

    [SerializeField] private Slider HealthBarObject;
    [SerializeField] private Text StateTxt;
#pragma warning restore 0649

    public void UpdateStateText(string state)
    {
        StateTxt.text = state;
    }
    public void SetMaxHP(float maxHP)
    {
        HealthBarObject.maxValue = maxHP;
    }

    public void UpdateHealthbar(float currentHP)
    {
        HealthBarObject.value = currentHP;
    }
}
