using System;
using UnityEngine;

    public class FrameLimiter : MonoBehaviour
    {
        private void Start()
        {
            // just to make sure we don't use literally every resource available to do things
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
    }
