using IllusionPlugin;
using UnityEngine;
using LockOnPluginUtilities;
using System;

namespace LockOnPlugin
{
    public class LockOnPluginNeo : IEnhancedPlugin
    {
        public string Name => GetType().Name;
        public string Version => "2.1.0";

        public string[] Filter => new string[]
        {
            "StudioNEO_32",
            "StudioNEO_64",
        };

        public void OnLevelWasLoaded(int level)
        {
            if(level == 3 && !GameObject.Find("NeoMono") && FileManager.TargetSettingsExist())
                new GameObject("NeoMono").AddComponent<NeoMono>();
        }

        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationStart(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}