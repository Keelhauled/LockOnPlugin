using IllusionPlugin;
using UnityEngine;
using System.IO;
using System;

namespace LockOnStudioPlugin
{
    public class LockOnStudioPlugin : IEnhancedPlugin, IPlugin
    {
        public string Name => GetType().Name;
        public string Version => "1.3.0";

        public string[] Filter => new string[]
        {
            "HoneyStudio_32",
            "HoneyStudio_64"
        };

        public void OnLevelWasLoaded(int level)
        {
            if(level == 1 && !GameObject.Find("LockOnStudioBehaviour") && BoneFilesExist())
                new GameObject("LockOnStudioBehaviour").AddComponent<LockOnStudioBehaviour>();
        }

        private bool BoneFilesExist()
        {
            if(File.Exists("Plugins\\LockOnPlugin\\guibones.txt") ||
            File.Exists("Plugins\\LockOnPlugin\\quickbones.txt") ||
            File.Exists("Plugins\\LockOnPlugin\\intersections.txt"))
            {
                return true;
            }
            else
            {
                Console.WriteLine("Bone settings are missing");
                return false;
            }
        }

        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationStart(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}