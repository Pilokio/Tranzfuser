using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton

    public static PlayerManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public GameObject Player;
}
