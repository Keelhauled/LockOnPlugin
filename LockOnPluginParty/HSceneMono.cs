using System.Collections.Generic;
using UnityEngine;
using Manager;

namespace LockOnPlugin
{
    internal partial class HSceneMono : LockOnBase
    {
        private CameraControl_Ver2 camera => Singleton<CameraControl_Ver2>.Instance;
        private Character charaManager => Singleton<Character>.Instance;
        //private HSceneManager hSceneManager => Singleton<HSceneManager>.Instance;
        //private HSceneSprite sprite => Singleton<HScene>.Instance.sprite;
        //private HScene hscene => Singleton<HScene>.Instance;

        private int activeCharaCount;

        protected override void Start()
        {
            base.Start();

            currentCharaInfo = charaManager.dictFemale[0];
            targetManager.UpdateAllTargets(currentCharaInfo);
            activeCharaCount = ActiveCharaCount<CharFemale>() + ActiveCharaCount<CharMale>();
        }

        protected override void Update()
        {
            base.Update();

            int count = ActiveCharaCount<CharFemale>() + ActiveCharaCount<CharMale>();
            if(activeCharaCount != count)
            {
                currentCharaInfo = charaManager.dictFemale[0];
                targetManager.UpdateAllTargets(currentCharaInfo);
                LockOnRelease();
                activeCharaCount = count;
            }
        }

        protected override void CharaSwitch(bool scrollDown = true)
        {
            if(!lockOnTarget && LockOn()) return;

            List<CharInfo> characters = new List<CharInfo>();
            for(int i = 0; i < charaManager.dictFemale.Count; i++)
            {
                if(charaManager.dictFemale[i].animBody != null)
                {
                    characters.Add(charaManager.dictFemale[i]);
                }
            }

            if(scrollThroughMalesToo)
            {
                for(int i = 0; i < charaManager.dictMale.Count; i++)
                {
                    if(charaManager.dictMale[i].animBody != null)
                    {
                        characters.Add(charaManager.dictMale[i]);
                    }
                }
            }

            for(int i = 0; i < characters.Count; i++)
            {
                if(characters[i] == currentCharaInfo)
                {
                    int next = i + 1 > characters.Count - 1 ? 0 : i + 1;
                    currentCharaInfo = characters[next];
                    targetManager.UpdateAllTargets(currentCharaInfo);
                    if(lockOnTarget) LockOn(lockOnTarget.name, true);
                    return;
                }
            }
        }

        private int ActiveCharaCount<Sex>() where Sex : CharInfo
        {
            int count = 0;
            var characters = typeof(Sex) == typeof(CharFemale) ? charaManager.dictFemale as SortedDictionary<int, Sex> : charaManager.dictMale as SortedDictionary<int, Sex>;
            
            for(int i = 0; i < characters.Count; i++)
            {
                if(characters[i].animBody != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
