using IllusionUtility.GetUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Manager;
using IllusionPlugin;

namespace LockOnStudioPlugin
{
    class LockOnStudioBehaviourSlide : MonoBehaviour
    {
        string lockOnKey;
        string unlockKey;
        string rotationLockKey;
        float lockedFov;
        float lockedZoomSpeed;
        float lockedMinDistance;
        string[] boneList;

        Studio studio;
        CameraControl camera;
        float defaultCameraMoveSpeed;

        float lerpValue = 0.0f;
        List<Transform> pointlist = new List<Transform>();

        void Start()
        {
            Console.WriteLine("Script \"{0}\" started", GetType().Name);

            studio = Singleton<Studio>.Instance;
            camera = FindObjectOfType<CameraControl>();
            defaultCameraMoveSpeed = camera.moveSpeed;

            lockOnKey = ModPrefs.GetString("LockOnPlugin", "LockOnKey", "M", true).ToLower();
            unlockKey = ModPrefs.GetString("LockOnPlugin", "UnlockKey", "M", true).ToLower();
            rotationLockKey = ModPrefs.GetString("LockOnPlugin", "RotationLockKey", "L", true).ToLower();
            lockedFov = ModPrefs.GetFloat("LockOnPlugin", "LockedFOV", 50.0f, true);
            lockedZoomSpeed = ModPrefs.GetFloat("LockOnPlugin", "LockedZoomSpeed", 3.0f, true);
            lockedMinDistance = ModPrefs.GetFloat("LockOnPlugin", "LockedMinDistance", 0.2f, true);
            boneList = ModPrefs.GetString("LockOnPlugin", "BoneList", "J_Head|J_Mune00|J_Spine01|J_Kokan", true).Split('|');
            camera.isOutsideTargetTex = !ModPrefs.GetBool("LockOnPlugin", "HideCameraTarget", true, true);
        }

        void Update()
        {
            if(Input.GetKeyDown(lockOnKey))
            {
                if(studio.CurrentChara != null)
                {
                    if(pointlist.Count > 0)
                    {
                        pointlist.Clear();
                    }
                    else
                    {
                        string prefix = studio.CurrentChara is StudioFemale ? "cf_" : "cm_";
                        foreach(var item in boneList)
                            pointlist.Add(studio.CurrentChara.body.objBone.transform.FindLoop(prefix + item).transform);
                    }
                }
            }

            if(pointlist.Count > 0 && Input.GetMouseButton(1))
            {
                camera.moveSpeed = 0.0f;

                float y = Input.GetAxis("Mouse Y");
                lerpValue += y * Time.deltaTime * lockedZoomSpeed;
                lerpValue = Mathf.Clamp(lerpValue, 0.0f, 1.0f);

                float x = Input.GetAxis("Mouse X");
                float newDir = camera.CameraDir.z + camera.CameraDir.z * x * Time.deltaTime * lockedZoomSpeed;
                if(newDir >= -lockedMinDistance) newDir = -lockedMinDistance;
                camera.CameraDir = new Vector3(0.0f, 0.0f, newDir);
            }

            if(pointlist.Count > 0)
            {
                camera.TargetPos = Vector3.Lerp(pointlist[1].position, pointlist[0].position, lerpValue);
            }
        }

        void LateUpdate()
        {
            if(pointlist.Count > 0)
            {
                camera.TargetPos = Vector3.Lerp(pointlist[1].position, pointlist[0].position, lerpValue);
            }
        }

        void FixedUpdate()
        {
            if(pointlist.Count > 0)
            {
                camera.TargetPos = Vector3.Lerp(pointlist[1].position, pointlist[0].position, lerpValue);
            }
        }

        void OnDestroy()
        {
            Console.WriteLine("Script \"{0}\" destroyed", GetType().Name);
        }
    }
}