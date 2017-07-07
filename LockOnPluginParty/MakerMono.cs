using UnityEngine;
using UnityEngine.UI;
using Manager;

namespace LockOnPlugin
{
    internal partial class MakerMono : LockOnBase
    {
        private CameraControl camera => Singleton<CameraControl>.Instance;
        private CustomControl customControl => Singleton<CustomControl>.Instance;

        protected override void Start()
        {
            base.Start();

            currentCharaInfo = customControl.chainfo;
            targetManager.UpdateAllTargets(currentCharaInfo);
        }

        protected override void LoadSettings()
        {
            base.LoadSettings();
            
            infoMsgPosition = new Vector2(0.5f, 0.0f);
        }

        protected override void Update()
        {
            base.Update();

            CameraSpeedHack();
        }

        private void CameraSpeedHack()
        {
            if(lockOnTarget)
            {
                customControl.modeOverScene = true;
                bool enableCamKey = true;

                foreach(InputField inputField in customControl.inputText)
                {
                    if(inputField.isFocused)
                    {
                        enableCamKey = false;
                    }
                }

                if(Singleton<Scene>.Instance.AddSceneName == "" && !customControl.checkMode)
                {
                    if(Input.GetKeyDown(KeyCode.F1))
                    {
                        Singleton<Scene>.Instance.LoadReserv("Config", false, false, false, false, false);
                    }
                    else if(enableCamKey && Input.GetKeyDown(KeyCode.R))
                    {
                        if(customControl.modePhoto)
                        {
                            customControl.photoCtrlPanel.OnResetCamera();
                        }
                        else
                        {
                            customControl.ResetCamera(false);
                        }
                    }
                    else if(Input.GetKeyDown(KeyCode.Escape))
                    {
                        customControl.EndGame();
                    }
                }

                if(camera != null)
                {
                    camera.KeyCondition = (() => enableCamKey);
                    float cameraSpeed = Manager.Config.EtcData.CameraSpeed;
                    camera.xRotSpeed = Mathf.Lerp(0.5f, 4f, cameraSpeed);
                    camera.yRotSpeed = Mathf.Lerp(0.5f, 4f, cameraSpeed);
                    camera.moveSpeed = lockOnTarget ? 0.0f : Mathf.Lerp(0.01f, 0.1f, cameraSpeed);
                }
            }
            else
            {
                customControl.modeOverScene = false;
            }
        }
    }
}
