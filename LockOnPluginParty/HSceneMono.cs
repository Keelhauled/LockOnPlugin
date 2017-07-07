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

        protected override void Start()
        {
            base.Start();

            currentCharaInfo = charaManager.dictFemale[0];
            targetManager.UpdateAllTargets(currentCharaInfo);
        }

        protected override void LoadSettings()
        {
            base.LoadSettings();

            infoMsgPosition = new Vector2(0.5f, 0.0f);
        }

        protected override void CharaSwitch(bool scrollDown = true)
        {
            SortedDictionary<int, CharFemale> charaList = charaManager.dictFemale;

            for(int i = 0; i < charaList.Count; i++)
            {
                if(charaList[i] == currentCharaInfo)
                {
                    int next = i + 1 > charaList.Count - 1 ? 0 : i + 1;
                    currentCharaInfo = charaList[next];
                    targetManager.UpdateAllTargets(currentCharaInfo);
                    if(lockOnTarget) LockOn(lockOnTarget.name, true);
                    return;
                }
            }
        }
    }
}
