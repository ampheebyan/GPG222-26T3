using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<NetworkedCarHandler> networkedCars = new ();

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(this);
    }
}
