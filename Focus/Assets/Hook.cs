using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public Canvas hookCanvas;
    
    private void Awake()
    {
        hookCanvas.enabled = false;
    }

    public void DisplayCanvas()
    {
        hookCanvas.enabled = true;
    }

    public void HideCanvas()
    {
        hookCanvas.enabled = false;
    }
}
