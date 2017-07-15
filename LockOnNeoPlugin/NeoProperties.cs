﻿using UnityEngine;
using Studio;

namespace LockOnPlugin
{
    internal partial class NeoMono : LockOnBase
    {
        protected override float CameraMoveSpeed
        {
            get { return camera.moveSpeed; }
            set { camera.moveSpeed = value; }
        }

        protected override Vector3 CameraTargetPos
        {
            get { return camera.targetPos; }
            set { camera.targetPos = value; }
        }

        protected override Vector3 LockOnTargetPos
        {
            get { return lockOnTarget.transform.position; }
        }

        protected override Vector3 CameraAngle
        {
            get { return camera.cameraAngle; }
            set { camera.cameraAngle = value; }
        }

        protected override float CameraFov
        {
            get { return camera.fieldOfView; }
            set { camera.fieldOfView = value; }
        }

        protected override Vector3 CameraDir
        {
            get { return cameraData.distance; }
            set { cameraData.distance = value; }
        }

        protected override bool CameraTargetTex
        {
            set { camera.isConfigTargetTex = value; }
        }

        protected override float CameraZoomSpeed
        {
            get { return defaultCameraSpeed * Studio.Studio.optionSystem.cameraSpeed; }
        }

        protected override Transform CameraTransform
        {
            get { return camera.transform; }
        }

        protected override bool CameraMovementCheck
        {
            get { return !Singleton<GuideObjectManager>.Instance.isOperationTarget; }
        }
        
        protected override bool OnInputField
        {
            get { return studio.isInputNow; }
        }
    }
}
