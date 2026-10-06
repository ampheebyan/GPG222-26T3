using System;
using UnityEngine;

    public class FrameLimiter : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
    }
