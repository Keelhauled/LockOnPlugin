using UnityEngine;
using UnityEngine.UI;

namespace LockOnPlugin
{
    internal partial class MakerMono : LockOnBase
    {
        protected override float CameraMoveSpeed
        {
            get { return camera.moveSpeed; }
            set { camera.moveSpeed = value; }
        }

        protected override Vector3 CameraTargetPos
        {
            get { return camera.TargetPos; }
            set { camera.TargetPos = value; }
        }

        protected override Vector3 LockOnTargetPos
        {
            get { return lockOnTarget.transform.position; }
        }

        protected override Vector3 CameraAngle
        {
            get { return camera.CameraAngle; }
            set { camera.CameraAngle = value; }
        }

        protected override float CameraFov
        {
            get { return camera.CameraFov; }
            set { camera.CameraFov = value; }
        }

        protected override Vector3 CameraDir
        {
            get { return camera.CameraDir; }
            set { camera.CameraDir = value; }
        }

        protected override bool CameraTargetTex
        {
            set { camera.isOutsideTargetTex = value; }
        }

        protected override float CameraZoomSpeed
        {
            get { return defaultCameraSpeed; }
        }

        protected override Transform CameraTransform
        {
            get { return camera.transform; }
        }

        protected override bool InputFieldSelected
        {
            get
            {
                InputField[] inputFields = customControl.inputText;
                for(int i = 0; i < inputFields.Length; i++)
                {
                    if(inputFields[i].isFocused)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // Singleton<CustomControl>.Instance.cvsMainMenu.isActiveAndEnabled
    }
}
