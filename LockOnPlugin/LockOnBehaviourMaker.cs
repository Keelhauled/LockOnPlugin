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
            FindObjectOfType<CameraControl>().isOutsideTargetTex = !ModPrefs.GetBool("LockOnPlugin", "HideCameraTarget", true, true);
        }

        void OnDestroy()
        {
            Console.WriteLine("Script \"{0}\" destroyed", GetType().Name);
        }
    }
}