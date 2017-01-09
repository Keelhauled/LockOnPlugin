using IllusionUtility.GetUtility;
using System;
using System.Linq;
using UnityEngine;
using Manager;
using IllusionPlugin;

namespace LockOnStudioPlugin
{
    class LockOnStudioBehaviour : MonoBehaviour
    {
        Hotkey lockOnHotkey;
        Hotkey rotationHotkey;
        float lockedFov;
        float lockedZoomSpeed;
        float lockedMinDistance;
        float lockedTrackingSpeed;
        string[] boneList;

        Studio instance;
        CameraControl camera;
        GameObject cameraTarget;
        float defaultCameraMoveSpeed;
        Vector3? lastBonePos = null;
        bool lockRotation = false;
        Vector3? lastTargetAngle = null;

        void Start()
        {
            Console.WriteLine("Script \"{0}\" started", GetType().Name);

            instance = Singleton<Studio>.Instance;
            camera = FindObjectOfType<CameraControl>();
            
            defaultCameraMoveSpeed = camera.moveSpeed;
            
            lockOnHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "LockOnHotkey", "M", true).ToLower(), 0.5f);
            rotationHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "RotationHotkey", "N", true).ToLower(), 0.5f);
            lockedFov = ModPrefs.GetFloat("LockOnPlugin", "LockedFOV", 50.0f, true);
            lockedZoomSpeed = ModPrefs.GetFloat("LockOnPlugin", "LockedZoomSpeed", 3.0f, true);
            lockedMinDistance = ModPrefs.GetFloat("LockOnPlugin", "LockedMinDistance", 0.2f, true);
            lockedTrackingSpeed = ModPrefs.GetFloat("LockOnPlugin", "LockedTrackingSpeed", 0.1f, true);
            boneList = ModPrefs.GetString("LockOnPlugin", "BoneList", "J_Head|J_Mune00|J_Spine01|J_Kokan", true).Split('|');
            camera.isOutsideTargetTex = !Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "HideCameraTarget", "True", true));
        }

        void Update()
        {
            if(Input.GetKey(lockOnHotkey.keyString))
            {
                lockOnHotkey.timeHeld += Time.deltaTime;
                if(lockOnHotkey.timeHeld >= lockOnHotkey.procTime && cameraTarget && lockOnHotkey.released)
                {
                    lockOnHotkey.released = false;
                    lockRotation = false;
                    lastTargetAngle = null;
                    cameraTarget = null;
                    lastBonePos = null;
                    if(camera.moveSpeed <= 0.0f)
                        camera.moveSpeed = defaultCameraMoveSpeed;
                    Console.WriteLine("Camera unlocked");
                }
            }

            if(Input.GetKeyUp(lockOnHotkey.keyString))
            {
                if(lockOnHotkey.released)
                {
                    StudioChara character = instance.CurrentChara;
                    if(character != null)
                    {
                        CharBody body = character.body;
                        string prefix = character is StudioFemale ? "cf_" : "cm_";
                        if(!cameraTarget)
                        {
                            cameraTarget = body.objBone.transform.FindLoop(prefix + boneList[0]);
                        }
                        else
                        {
                            for(int i = 0; i < boneList.Length; i++)
                            {
                                if(cameraTarget.name == prefix + boneList[i])
                                {
                                    string boneName = boneList.ElementAtOrDefault(i + 1) != null ? prefix + boneList[i + 1] : prefix + boneList[0];
                                    cameraTarget = body.objBone.transform.FindLoop(boneName);
                                    break;
                                }
                            }
                        }

                        if(lastBonePos == null) lastBonePos = cameraTarget.transform.position;
                        camera.moveSpeed = 0.0f;

                        Console.WriteLine("Camera locked to \"{0}\"", cameraTarget.name);
                    }
                }

                lockOnHotkey.timeHeld = 0.0f;
                lockOnHotkey.released = true;
            }

            if(Input.GetKeyDown(rotationHotkey.keyString) && cameraTarget)
            {
                if(lockRotation)
                {
                    lockRotation = false;
                    lastTargetAngle = null;
                    Console.WriteLine("Camera rotation released");
                }
                else
                {
                    lockRotation = true;
                    lastTargetAngle = cameraTarget.transform.eulerAngles;
                    Console.WriteLine("Camera rotation locked");
                }
            }

            if(cameraTarget)
            {
                camera.CameraFov = lockedFov;
                
                float distance = Vector3.Distance(camera.TargetPos, (Vector3)lastBonePos);
                camera.TargetPos = Vector3.MoveTowards(camera.TargetPos, cameraTarget.transform.position, distance * lockedTrackingSpeed);
                lastBonePos = cameraTarget.transform.position;

                if(Input.GetMouseButton(1))
                {
                    // prevent default camera movement and handle zooming manually
                    float x = Input.GetAxis("Mouse X");
                    float newDir = camera.CameraDir.z + camera.CameraDir.z * x * Time.deltaTime * lockedZoomSpeed;
                    if(newDir >= -lockedMinDistance) newDir = -lockedMinDistance;
                    camera.CameraDir = new Vector3(0.0f, 0.0f, newDir);
                }
            }

            if(lockRotation)
            {
                Vector3 targetAngle = cameraTarget.transform.eulerAngles;
                camera.CameraAngle += new Vector3(-(targetAngle.x - lastTargetAngle.Value.x), (targetAngle.y - lastTargetAngle.Value.y), -(targetAngle.z - lastTargetAngle.Value.z));
                lastTargetAngle = cameraTarget.transform.eulerAngles;
            }

            if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
                Cursor.visible = false;
            else
                Cursor.visible = true;
        }

        void OnDestroy()
        {
            Console.WriteLine("Script \"{0}\" destroyed", GetType().Name);
        }
    }

    public class Hotkey
    {
        public string keyString;
        public float procTime;
        public float timeHeld;
        public bool released;

        public Hotkey(string keyString, float procTime)
        {
            this.keyString = keyString;
            this.procTime = procTime;
            this.timeHeld = 0.0f;
            this.released = true;
        }
    }
}