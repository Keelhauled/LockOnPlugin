using IllusionUtility.GetUtility;
using System.Linq;
using System.Collections.Generic;
using IllusionPlugin;
using UnityEngine;
using Manager;

namespace LockOnPlugin
{
    internal partial class HSceneMono : LockOnBase
    {
        private HSceneManager instance;
        private CameraControl_Ver2 camera;
        private HSceneSprite sprite;

        protected override void Start()
        {
            instance = Singleton<HSceneManager>.Instance;
            currentCharaInfo = instance.females[0];
            camera = Singleton<CameraControl_Ver2>.Instance;
            sprite = Singleton<HScene>.Instance.sprite;
            camera.isOutsideTargetTex = ModPrefs.GetString("LockOnPlugin", "HideCameraTarget", "True", true).ToLower() == "true" ? false : true;

            base.Start();
        }

        protected override void Update()
        {
            base.Update();

            nextCharaHotkey.KeyDownAction(CharaSwitch);
        }

        protected override void OnGUI()
        {
            if(guiTimeAngle > 0.0f)
            {
                DebugGUI(0.5f, 0.5f, 100f, 50f, "Camera tilt\n" + CameraAngle.z.ToString("0.0"));
                guiTimeAngle -= Time.deltaTime;
            }

            if(guiTimeFov > 0.0f)
            {
                DebugGUI(0.5f, 0.5f, 100f, 50f, "Field of view\n" + CameraFov.ToString("0.0"));
                guiTimeFov -= Time.deltaTime;
            }

            if(showInfoMsg && guiTimeInfo > 0.0f && sprite.categoryToggleOption.isActiveAndEnabled)
            {
                DebugGUI(0.5f, 0.0f, 200f, 50f, infoMsg);
                guiTimeInfo -= Time.deltaTime;
            }

            if(showLockOnTargets)
            {
                foreach(GameObject target in targetManager.GetAllTargetsMultiple())
                {
                    if(target)
                    {
                        Vector3 pos = Camera.main.WorldToScreenPoint(target.transform.position);
                        if(pos.z >= 0.0f && GUI.Button(new Rect(pos.x - targetSize / 2, Screen.height - pos.y - targetSize / 2, targetSize, targetSize), "L"))
                        {
                            LockOn(target);
                        }
                    }
                    else
                    {
                        showLockOnTargets = false;
                        targetManager.UpdateAllTargetsMultiple(null);
                    }
                }
            }
        }

        protected override void ToggleLockOnGUI()
        {
            if(showLockOnTargets)
            {
                showLockOnTargets = false;
            }
            else
            {
                List<CharInfo> females = new List<CharInfo>();
                foreach(CharInfo character in FindObjectsOfType<CharInfo>().ToList())
                {
                    if(character is CharFemale)
                    {
                        females.Add(character);
                    }
                }

                targetManager.UpdateAllTargetsMultiple(females);
                showLockOnTargets = true;
            }
        }

        private void CharaSwitch()
        {
            if(lockOnTarget && currentCharaInfo)
            {
                string targetName = lockOnTarget.name;
                CharFemale[] females = FindObjectsOfType<CharFemale>();
                int activeFemCount = 0;

                foreach(CharFemale female in females)
                {
                    if(female.chaBody.objBone)
                    {
                        activeFemCount++;
                    }
                }

                if(activeFemCount == 2)
                {
                    if(currentCharaInfo == females[0])
                    {
                        currentCharaInfo = females[1];
                        LockOn(currentCharaInfo.chaBody.objBone.transform.FindLoop(targetName));
                    }
                    else if(currentCharaInfo == females[1])
                    {
                        currentCharaInfo = females[0];
                        LockOn(currentCharaInfo.chaBody.objBone.transform.FindLoop(targetName));
                    }
                }
                else
                {
                    currentCharaInfo = instance.females[0];
                    LockOn(currentCharaInfo.chaBody.objBone.transform.FindLoop(targetName));
                }
            }
            else
            {
                currentCharaInfo = instance.females[0];
                LockOn();
            }
        }
    }
}