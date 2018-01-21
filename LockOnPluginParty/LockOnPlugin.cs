using System;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public void OnApplicationStart()
        {
            try
            {
                HoneySelectPatches.Init();
                StudioNeoPatches.Init();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void OnLevelWasLoaded(int level)
        {
            StartMod();
        }

        public static void StartMod()
        {
            switch(SceneManager.GetActiveScene().name)
            {
                case "Studio":
                {
                    new GameObject(LockOnBase.NAME_HSCENEMAKER).AddComponent<NeoMono>();
                    break;
                }

                case "HScene":
                {
                    new GameObject(LockOnBase.NAME_HSCENEMAKER).AddComponent<HSceneMono>();
                    break;
                }

                case "CustomScene":
                {
                    new GameObject(LockOnBase.NAME_HSCENEMAKER).AddComponent<MakerMono>();
                    break;
                }
            }
        }

        public static void Bootstrap()
        {
            var gameobject = GameObject.Find(LockOnBase.NAME_HSCENEMAKER);
            if(gameobject != null) GameObject.DestroyImmediate(gameobject);
            StartMod();
        }

        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
