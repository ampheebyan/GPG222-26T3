using System;
using System.Collections.Generic;
using UnityEngine;

public class GearShiftVisual : MonoBehaviour
{
    [Header("Ensure your positions align to the gear shift enum in CarModel.cs")]
    public List<Vector3> positions = new ();

    public CarState state;
    public Transform head;

    private void Update()
    {
        if (state && head && positions.Count == 4)
        {
            head.localPosition = positions[(int)state.car.Gear];
        }
    }
}
