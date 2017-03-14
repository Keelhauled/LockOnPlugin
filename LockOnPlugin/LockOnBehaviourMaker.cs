using IllusionPlugin;
using System;
using UnityEngine;

namespace LockOnPlugin
{
    class LockOnBehaviourMaker : MonoBehaviour
    {
        void Start()
        {
            FindObjectOfType<CameraControl>().isOutsideTargetTex = !Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "HideCameraTarget", "True", true));
        }
    }
}