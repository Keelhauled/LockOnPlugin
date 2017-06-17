using UnityEngine;
using UnityEngine.UI;
using Manager;

namespace LockOnPlugin
{
    internal partial class MakerMono : LockOnBase
    {
        private CameraControl camera;
        private CustomControl customControl;

        protected override void Start()
        {
            currentCharaInfo = FindObjectsOfType<CharFemale>()[1];
            camera = FindObjectOfType<CameraControl>();
            customControl = FindObjectOfType<CustomControl>();

            base.Start();

            targetManager.UpdateAllTargets(currentCharaInfo);
        }

        protected override void Update()
        {
            base.Update();

            CameraSpeedHack();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if(showInfoMsg && guiTimeInfo > 0.0f && customControl.cvsMainMenu.isActiveAndEnabled)
            {
                DebugGUI(0.5f, 0.0f, 200f, 50f, infoMsg);
                guiTimeInfo -= Time.deltaTime;
            }
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