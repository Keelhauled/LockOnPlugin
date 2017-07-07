using IllusionPlugin;
using UnityEngine;
using LockOnPluginUtilities;

namespace LockOnPlugin
{
    public class LockOnPlugin : IEnhancedPlugin
    {
        public string Name => GetType().Name;
        public string Version => "2.1.0";

        public string[] Filter => new string[]
        {
            "HoneySelect_32",
            "HoneySelect_64",
        };

        public void OnLevelWasLoaded(int level)
        {
            if(level == 15 && !GameObject.Find("HSceneMono") && FileManager.TargetSettingsExist())
                new GameObject("HSceneMono").AddComponent<HSceneMono>();

            if(level == 21 && !GameObject.Find("MakerMono") && FileManager.TargetSettingsExist())
                new GameObject("MakerMono").AddComponent<MakerMono>();
        }

        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationStart(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}