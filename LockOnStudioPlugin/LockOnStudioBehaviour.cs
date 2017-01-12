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
        float lockedZoomSpeed;
        float lockedMinDistance;
        float lockedTrackingSpeed1;
        float lockedTrackingSpeed2;
        string[] boneList;

        Studio instance;
        CameraControl camera;
        GameObject cameraTarget;
        float defaultCameraMoveSpeed;
        float normalCameraMoveSpeed;
        Vector3? lastBonePos = null;
        bool lockRotation = false;
        Vector3? lastTargetAngle = null;
        float guiTimerFov = 0.0f;
        float guiTimerAngle = 0.0f;

        void Start()
        {
            Console.WriteLine("Script \"{0}\" started", GetType().Name);

            instance = Singleton<Studio>.Instance;
            camera = FindObjectOfType<CameraControl>();
            
            defaultCameraMoveSpeed = camera.moveSpeed;

            lockOnHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "LockOnHotkey", "M", true).ToLower(), 0.5f);
            rotationHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "RotationHotkey", "N", true).ToLower(), 0.5f);
            lockedZoomSpeed = ModPrefs.GetFloat("LockOnPlugin", "LockedZoomSpeed", 5.0f, true);
            lockedMinDistance = ModPrefs.GetFloat("LockOnPlugin", "LockedMinDistance", 0.2f, true);
            lockedTrackingSpeed1 = lockedTrackingSpeed2 = ModPrefs.GetFloat("LockOnPlugin", "LockedTrackingSpeed", 0.1f, true);
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

                    if(camera.moveSpeed <= 0.0f && normalCameraMoveSpeed > 0.0f)
                        camera.moveSpeed = normalCameraMoveSpeed;
                    else if(camera.moveSpeed <= 0.0f)
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
                                    string boneName = boneList.ElementAtOrDefault(i+1) != null ? prefix + boneList[i+1] : prefix + boneList[0];
                                    cameraTarget = body.objBone.transform.FindLoop(boneName);
                                    break;
                                }
                            }
                        }

                        if(lastBonePos == null) lastBonePos = cameraTarget.transform.position;
                        normalCameraMoveSpeed = camera.moveSpeed;
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

            if(lockRotation && !cameraTarget)
            {
                lockRotation = false;
                lastTargetAngle = null;
            }

            if(lockRotation)
            {
                if(lockedTrackingSpeed1 < 0.2f)
                    lockedTrackingSpeed1 = 0.2f;

                Vector3 targetAngle = cameraTarget.transform.eulerAngles;
                Vector3 difference = targetAngle - lastTargetAngle.Value;
                camera.CameraAngle += new Vector3(-difference.x, difference.y, -difference.z);
                lastTargetAngle = targetAngle;
            }
            else
            {
                lockedTrackingSpeed1 = lockedTrackingSpeed2;
            }

            if(cameraTarget)
            {
                float distance = Vector3.Distance(camera.TargetPos, lastBonePos.Value);
                camera.TargetPos = Vector3.MoveTowards(camera.TargetPos, cameraTarget.transform.position, distance * lockedTrackingSpeed1);
                lastBonePos = cameraTarget.transform.position;

                if(Input.GetMouseButton(1))
                {
                    if(Input.GetKey("left ctrl"))
                    {
                        //camera tilt adjustment
                        float x = Input.GetAxis("Mouse X");
                        float newAngle = camera.CameraAngle.z - x * Time.deltaTime * 50.0f;
                        newAngle = Mathf.Repeat(newAngle, 360.0f);
                        camera.CameraAngle = new Vector3(camera.CameraAngle.x, camera.CameraAngle.y, newAngle);
                        guiTimerAngle = 0.1f;
                    }
                    else if(Input.GetKey("left shift"))
                    {
                        //fov adjustment
                        float x = Input.GetAxis("Mouse X");
                        float newFov = camera.CameraFov + x * Time.deltaTime * 15.0f;
                        newFov = Mathf.Clamp(newFov, 10.0f, 100.0f);
                        camera.CameraFov = newFov;
                        guiTimerFov = 0.1f;
                    }
                    else
                    {
                        // prevent default camera movement and handle zooming manually
                        float x = Input.GetAxis("Mouse X");
                        float newDir = camera.CameraDir.z - x * Time.deltaTime * lockedZoomSpeed;
                        if(newDir >= -lockedMinDistance) newDir = -lockedMinDistance;
                        camera.CameraDir = new Vector3(0.0f, 0.0f, newDir);
                    }
                }
            }

            if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
                Cursor.visible = false;
            else
                Cursor.visible = true;
        }

        void OnGUI()
        {
            if(guiTimerAngle > 0.0f)
            {
                DebugGUI(0.5f, 0.5f, 100f, 50f, "Camera tilt\n" + camera.CameraAngle.z.ToString("0.0"));
                guiTimerAngle -= Time.deltaTime;
            }

            if(guiTimerFov > 0.0f)
            {
                DebugGUI(0.5f, 0.5f, 100f, 50f, "Field of view\n" + camera.CameraFov.ToString("0.0"));
                guiTimerFov -= Time.deltaTime;
            }

            DebugGUI(0.95f, 0.90f, 100f, 50f, "lockRotation\n" + lockRotation.ToString());
            DebugGUI(0.95f, 0.95f, 100f, 50f, "trackingspeed\n" + lockedTrackingSpeed1.ToString());
        }
        
        bool DebugGUI(float screenWidthMult, float screenHeightMult, float width, float height, string msg)
        {
            float xpos = Screen.width * screenWidthMult - width / 2.0f;
            float ypos = Screen.height * screenHeightMult - height / 2.0f;
            xpos = Mathf.Clamp(xpos, width / 2.0f, Screen.width - width);
            ypos = Mathf.Clamp(ypos, height / 2.0f, Screen.height - height);
            return GUI.Button(new Rect(xpos, ypos, width, height), msg);
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