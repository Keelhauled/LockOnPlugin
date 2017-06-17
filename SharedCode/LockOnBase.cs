using System;
using System.Reflection;
using System.Collections.Generic;
using IllusionPlugin;
using UnityEngine;
using LockOnPluginUtilities;

namespace LockOnPlugin
{
    internal abstract class LockOnBase : MonoBehaviour
    {
        protected abstract float CameraMoveSpeed { get; set; }
        protected abstract Vector3 CameraTargetPos { get; set; }
        protected abstract Vector3 LockOnTargetPos { get; }
        protected abstract Vector3 CameraAngle { get; set; }
        protected abstract float CameraFov { get; set; }
        protected abstract Vector3 CameraDir { get; set; }
        protected abstract bool CameraTargetTex { set; }

        protected Hotkey lockOnHotkey;
        protected Hotkey lockOnGuiHotkey;
        protected Hotkey prevCharaHotkey;
        protected Hotkey nextCharaHotkey;
        protected Hotkey rotationHotkey;
        protected float lockedZoomSpeed;
        protected float lockedMinDistance;
        protected float trackingSpeed;
        protected bool manageCursorVisibility;
        protected bool scrollThroughMalesToo;

        protected GameObject lockOnTarget;
        protected CameraTargetManager targetManager;
        protected Vector3? lastTargetPos;
        protected List<string> targetList;
        protected CharInfo currentCharaInfo;
        
        protected float defaultCameraSpeed;
        protected float targetSize = 25.0f;
        protected float guiTimeAngle = 0.0f;
        protected float guiTimeFov = 0.0f;
        protected float guiTimeInfo = 0.0f;
        protected bool showLockOnTargets = false;
        protected bool showInfoMsg;
        protected string infoMsg = "";
        
        protected virtual void Start()
        {
            targetManager = new CameraTargetManager();
            targetList = FileManager.GetQuickTargetNames();
            defaultCameraSpeed = CameraMoveSpeed;

            lockOnHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "LockOnHotkey", "N", true), 0.4f);
            lockOnGuiHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "LockOnGuiHotkey", "K", true));
            prevCharaHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "PrevCharaHotkey", "false", true));
            nextCharaHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "NextCharaHotkey", "L", true));
            rotationHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "RotationHotkey", "false", true));
            lockedZoomSpeed = ModPrefs.GetFloat("LockOnPlugin", "LockedZoomSpeed", 5.0f, true);
            lockedMinDistance = Mathf.Abs(ModPrefs.GetFloat("LockOnPlugin", "LockedMinDistance", 0.2f, true));
            trackingSpeed = Mathf.Abs(ModPrefs.GetFloat("LockOnPlugin", "LockedTrackingSpeed", 0.1f, true));
            showInfoMsg = Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "ShowInfoMsg", "False", true));
            manageCursorVisibility = ModPrefs.GetString("LockOnPlugin", "ManageCursorVisibility", "True", true).ToLower() == "true" ? true : false;
            CameraTargetTex = ModPrefs.GetString("LockOnPlugin", "HideCameraTarget", "True", true).ToLower() == "true" ? false : true;
            scrollThroughMalesToo = ModPrefs.GetString("LockOnPlugin", "ScrollThroughMalesToo", "False", true).ToLower() == "true" ? true : false;
        }
        
        protected virtual void Update()
        {
            if(showLockOnTargets) targetManager.UpdateCustomTargetPositions();

            lockOnHotkey.KeyUpAction(LockOn);
            lockOnHotkey.KeyHoldAction(LockOnRelease);
            lockOnGuiHotkey.KeyDownAction(ToggleLockOnGUI);

            if(lockOnTarget)
            {
                float distance = Vector3.Distance(CameraTargetPos, lastTargetPos.Value);
                if(distance > 0.00001) CameraTargetPos = Vector3.MoveTowards(CameraTargetPos, LockOnTargetPos, distance * trackingSpeed);
                lastTargetPos = lockOnTarget.transform.position;

                if(Input.GetMouseButton(1))
                {
                    float x = Input.GetAxis("Mouse X");
                    if(Input.GetKey("left ctrl"))
                    {
                        guiTimeAngle = 0.1f;
                        if(Mathf.Abs(x) > 0)
                        {
                            //camera tilt adjustment
                            float newAngle = CameraAngle.z - x * Time.deltaTime * 50.0f;
                            newAngle = Mathf.Repeat(newAngle, 360.0f);
                            CameraAngle = new Vector3(CameraAngle.x, CameraAngle.y, newAngle);
                        }
                    }
                    else if(Input.GetKey("left shift"))
                    {
                        guiTimeFov = 0.1f;
                        if(Mathf.Abs(x) > 0)
                        {
                            //fov adjustment
                            float newFov = CameraFov + x * Time.deltaTime * 15.0f;
                            newFov = Mathf.Clamp(newFov, 10.0f, 100.0f);
                            CameraFov = newFov;
                        }
                    }
                    else if(Mathf.Abs(x) > 0)
                    {
                        //handle zooming manually when camera.movespeed = 0
                        float newDir = CameraDir.z - x * Time.deltaTime * lockedZoomSpeed;
                        if(newDir >= -lockedMinDistance) newDir = -lockedMinDistance;
                        CameraDir = new Vector3(0.0f, 0.0f, newDir);
                    }
                }
            }

            if(manageCursorVisibility)
            {
                if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.visible = true;
                }
            }
        }
        
        protected virtual void OnGUI()
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

            if(showLockOnTargets)
            {
                foreach(GameObject target in targetManager.GetAllTargets())
                {
                    Vector3 pos = Camera.main.WorldToScreenPoint(target.transform.position);
                    if(pos.z > 0.0f && GUI.Button(new Rect(pos.x - targetSize / 2, Screen.height - pos.y - targetSize / 2, targetSize, targetSize), "L"))
                    {
                        LockOn(target);
                    }
                }
            }
        }
        
        protected virtual void LockOn()
        {
            if(currentCharaInfo)
            {
                string prefix = currentCharaInfo is CharFemale ? "cf_" : "cm_";

                if(!lockOnTarget)
                {
                    LockOn(prefix + targetList[0]);
                }
                else
                {
                    bool targetChanged = false; // if locked on to something not in quicktargets.txt, lock on to first quicktarget

                    for(int i = 0; i < targetList.Count; i++)
                    {
                        if(lockOnTarget.name == prefix + targetList[i])
                        {
                            int next = i + 1 > targetList.Count - 1 ? 0 : i + 1;
                            LockOn(prefix + targetList[next]);
                            targetChanged = true;
                            break;
                        }
                    }

                    if(!targetChanged)
                    {
                        LockOn(prefix + targetList[0]);
                    }
                }
            }
        }

        protected virtual bool LockOn(GameObject target)
        {
            if(target)
            {
                lockOnTarget = target;
                if(lastTargetPos == null) lastTargetPos = lockOnTarget.transform.position;
                CameraMoveSpeed = 0.0f;

                CreateInfoMsg("Locked to \"" + lockOnTarget.name + "\"");
                return true;
            }

            return false;
        }

        protected virtual bool LockOn(string targetName, bool lockOnAnyway = false)
        {
            foreach(GameObject target in targetManager.GetAllTargets())
            {
                if(target.name.Substring(3) == targetName.Substring(3))
                {
                    LockOn(target);
                    return true;
                }
            }

            if(lockOnAnyway)
            {
                LockOn();
                return true;
            }

            return false;
        }

        protected virtual void LockOnRelease()
        {
            if(lockOnTarget)
            {
                lockOnTarget = null;
                lastTargetPos = null;
                CameraMoveSpeed = defaultCameraSpeed;

                CreateInfoMsg("Camera unlocked");
            }
        }

        protected virtual void ToggleLockOnGUI()
        {
            if(currentCharaInfo)
            {
                showLockOnTargets = !showLockOnTargets;
            }
        }

        protected void CreateInfoMsg(string msg, float time = 3.0f)
        {
            infoMsg = msg;
            guiTimeInfo = time;
        }

        protected bool DebugGUI(float screenWidthMult, float screenHeightMult, float width, float height, string msg)
        {
            float xpos = Screen.width * screenWidthMult - width / 2.0f;
            float ypos = Screen.height * screenHeightMult - height / 2.0f;
            xpos = Mathf.Clamp(xpos, 0, Screen.width - width);
            ypos = Mathf.Clamp(ypos, 0, Screen.height - height);
            return GUI.Button(new Rect(xpos, ypos, width, height), msg);
        }

        protected FieldType GetSecureField<ObjectType, FieldType>(string fieldName)
            where ObjectType : UnityEngine.Object
            where FieldType : class
        {
            try
            {
                ObjectType target = FindObjectOfType<ObjectType>();
                FieldInfo fieldinfo = typeof(Studio.CameraControl).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                FieldType field = (fieldinfo.GetValue(target)) as FieldType;
                return field;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }
    }
}