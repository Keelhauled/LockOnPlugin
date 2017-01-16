using IllusionPlugin;
using System;
using UnityEngine;

namespace LockOnPlugin
{
    class LockOnBehaviourMaker : MonoBehaviour
    {
        void Start()
        {
            Console.WriteLine("Script \"{0}\" started", GetType().Name);
            FindObjectOfType<CameraControl>().isOutsideTargetTex = !Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "HideCameraTarget", "True", true));
        }

        void OnDestroy()
        {
            Console.WriteLine("Script \"{0}\" destroyed", GetType().Name);
        }
    }
}