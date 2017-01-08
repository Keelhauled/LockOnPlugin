using System;
using IllusionPlugin;
using UnityEngine;

namespace LockOnStudioPlugin
{
    public class LockOnStudioPlugin : IEnhancedPlugin
    {
        public string Name => GetType().Name;
        public string Version => "1.0.0";

        public string[] Filter => new string[]
        {
            "HoneyStudio_32",
            "HoneyStudio_64"
        };

        public void OnApplicationStart()
        {
            Console.Write("{0} ", Name);
        }

        public void OnLevelWasLoaded(int level)
        {
            if(level == 1 && !GameObject.Find("LockOnStudioBehaviour"))
                new GameObject("LockOnStudioBehaviour").AddComponent<LockOnStudioBehaviour>();
        }

        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}