using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider HealthBarObject;
    [SerializeField] private Text StateTxt;

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
