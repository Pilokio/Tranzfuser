using UnityEngine;

public class CoverData : MonoBehaviour
{
    public bool IsOccupied { get; set; }
    public Transform CoverPosition;

    private void Awake()
    {
        IsOccupied = false;
    }
}
