using UnityEngine;
using Chronos;

public class BaseBehaviour : MonoBehaviour
{
    public Timeline time
    {
        get
        {
            return GetComponent<Timeline>();
        }
    }
}
