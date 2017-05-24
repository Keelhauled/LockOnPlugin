using IllusionPlugin;
using UnityEngine;
using System.IO;
using System;

namespace LockOnPlugin
{
    public class LockOnPlugin : IEnhancedPlugin
    {
        public string Name => GetType().Name;
        public string Version => "1.3.0";

        public string[] Filter => new string[]
        {
            "HoneySelect_32",
            "HoneySelect_64"
        };

        public void OnLevelWasLoaded(int level)
        {
            if(level == 14 && !GameObject.Find("LockOnBehaviour") && BoneFilesExist())
                new GameObject("LockOnBehaviour").AddComponent<LockOnBehaviour>();

            else if(level == 20 && !GameObject.Find("LockOnBehaviourMaker") && BoneFilesExist())
                new GameObject("LockOnBehaviourMaker").AddComponent<LockOnBehaviourMakerV2>();
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