using System;
using IllusionPlugin;
using UnityEngine;

namespace LockOnPlugin
{
    public class LockOnPluginNeo : IEnhancedPlugin
    {
        public string Name => LockOnBase.NAME_NEO;
        public string Version => LockOnBase.VERSION;

        public string[] Filter => new string[]
        {
            "StudioNEO_32",
            "StudioNEO_64",
        };

        private NeoMono neoObject;

        public void OnApplicationStart()
        {
            try { StudioNeoPatches.Init(); }
            catch(Exception ex) { Console.WriteLine(ex); }
        }

        public void OnLevelWasLoaded(int level)
        {
            if(level == 3 && !neoObject && FileManager.TargetSettingsExist())
                neoObject = new GameObject(LockOnBase.NAME_NEO).AddComponent<NeoMono>();
        }

        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
