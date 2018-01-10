using System;
using IllusionPlugin;
using UnityEngine;

namespace LockOnPlugin
{
    public class LockOnPlugin : IEnhancedPlugin
    {
        public string Name => LockOnBase.NAME_HSCENEMAKER;
        public string Version => LockOnBase.VERSION;

        public string[] Filter => new string[]
        {
            "HoneySelect_32",
            "HoneySelect_64",
            "StudioNEO_32",
            "StudioNEO_64",
        };

        private HSceneMono hsceneObject;
        private MakerMono makerObject;
        private NeoMono neoObject;

        public void OnApplicationStart()
        {
            try { HoneySelectPatches.Init(); }
            catch(Exception ex) { Console.WriteLine(ex); }

            try { StudioNeoPatches.Init(); }
            catch(Exception ex) { Console.WriteLine(ex); }
        }

        public void OnLevelWasLoaded(int level)
        {
            if(level == 15 && !hsceneObject && FileManager.TargetSettingsExist())
                hsceneObject = new GameObject(LockOnBase.NAME_HSCENEMAKER).AddComponent<HSceneMono>();

            else if(level == 21 && !makerObject && FileManager.TargetSettingsExist())
                makerObject = new GameObject(LockOnBase.NAME_HSCENEMAKER).AddComponent<MakerMono>();

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
