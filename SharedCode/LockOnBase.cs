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
        public static string version = "2.1.1";

        protected abstract float CameraMoveSpeed { get; set; }
        protected abstract Vector3 CameraTargetPos { get; set; }
        protected abstract Vector3 LockOnTargetPos { get; }
        protected abstract Vector3 CameraAngle { get; set; }
        protected abstract float CameraFov { get; set; }
        protected abstract Vector3 CameraDir { get; set; }
        protected abstract bool CameraTargetTex { set; }
        protected abstract float CameraZoomSpeed { get; }
        protected abstract Transform CameraTransform { get; }

        protected Vector3 targetOffsetAmount = new Vector3();
        protected Vector3 TargetOffsetAdjusted => (CameraTransform.right * targetOffsetAmount.x) + (Vector3.up * targetOffsetAmount.y); // best version yet
        //protected Vector3 TargetOffsetAdjusted => (CameraTransform.right * targetOffsetAmount.x) + (CameraTransform.up * targetOffsetAmount.y); // bad
        //protected Vector3 TargetOffsetAdjusted => (CameraTransform.right * targetOffsetAmount.x) + (lockOnTarget.transform.up * targetOffsetAmount.y); // confusing

        protected Hotkey lockOnHotkey;
        protected Hotkey lockOnGuiHotkey;
        protected Hotkey prevCharaHotkey;
        protected Hotkey nextCharaHotkey;
        protected Hotkey rotationHotkey;
        protected float lockedMinDistance;
        protected float trackingSpeedNormal;
        protected float trackingSpeedRotation = 0.2f;
        protected bool manageCursorVisibility;
        protected bool scrollThroughMalesToo;
        protected bool showInfoMsg;

        protected bool controllerEnabled;
        protected float controllerMoveSpeed;
        protected float controllerZoomSpeed;
        protected float controllerRotSpeed;

        protected CameraTargetManager targetManager;
        protected CharInfo currentCharaInfo;
        protected GameObject lockOnTarget;
        protected Vector3? lastTargetPos;
        protected Vector3 lastTargetAngle;
        protected bool lockRotation = false;
        protected bool showLockOnTargets = false;
        protected float defaultCameraSpeed;
        protected float targetSize = 25.0f;
        protected float guiTimeAngle = 0.0f;
        protected float guiTimeFov = 0.0f;
        protected float guiTimeInfo = 0.0f;
        protected string infoMsg = "";
        protected Vector2 infoMsgPosition = new Vector2();

        protected virtual void Start()
        {
            targetManager = new CameraTargetManager();
            defaultCameraSpeed = CameraMoveSpeed;
            LoadSettings();
        }

        protected virtual void LoadSettings()
        {
            lockOnHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin.Hotkeys", "LockOnHotkey", "N", true), 0.4f);
            lockOnGuiHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin.Hotkeys", "LockOnGuiHotkey", "K", true));
            prevCharaHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin.Hotkeys", "PrevCharaHotkey", "false", true));
            nextCharaHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin.Hotkeys", "NextCharaHotkey", "L", true));
            rotationHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin.Hotkeys", "RotationHotkey", "false", true));
            lockedMinDistance = Mathf.Abs(ModPrefs.GetFloat("LockOnPlugin.Misc", "LockedMinDistance", 0.0f, true));
            trackingSpeedNormal = Mathf.Abs(ModPrefs.GetFloat("LockOnPlugin.Misc", "LockedTrackingSpeed", 0.1f, true));
            showInfoMsg = ModPrefs.GetString("LockOnPlugin.Misc", "ShowInfoMsg", "False", true).ToLower() == "true" ? true : false;
            manageCursorVisibility = ModPrefs.GetString("LockOnPlugin.Misc", "ManageCursorVisibility", "True", true).ToLower() == "true" ? true : false;
            CameraTargetTex = ModPrefs.GetString("LockOnPlugin.Misc", "HideCameraTarget", "True", true).ToLower() == "true" ? false : true;
            scrollThroughMalesToo = ModPrefs.GetString("LockOnPlugin.Misc", "ScrollThroughMalesToo", "False", true).ToLower() == "true" ? true : false;

            controllerEnabled = ModPrefs.GetString("LockOnPlugin.Gamepad", "ControllerEnabled", "True", true).ToLower() == "true" ? true : false;
            controllerMoveSpeed = ModPrefs.GetFloat("LockOnPlugin.Gamepad", "ControllerMoveSpeed", 0.3f, true);
            controllerZoomSpeed = ModPrefs.GetFloat("LockOnPlugin.Gamepad", "ControllerZoomSpeed", 0.2f, true);
            controllerRotSpeed = ModPrefs.GetFloat("LockOnPlugin.Gamepad", "ControllerRotSpeed", 0.4f, true);
        }

        protected virtual void Update()
        {
            if(controllerEnabled) GamepadControls();
            targetManager.UpdateCustomTargetTransforms();

            lockOnHotkey.KeyHoldAction(LockOnRelease);
            lockOnHotkey.KeyUpAction(() => LockOn());
            lockOnGuiHotkey.KeyDownAction(ToggleLockOnGUI);
            prevCharaHotkey.KeyDownAction(() => CharaSwitch(false));
            nextCharaHotkey.KeyDownAction(() => CharaSwitch(true));
            //rotationHotkey.KeyDownAction(RotationLockToggle);

            if(lockOnTarget)
            {
                float trackingSpeed = (lockRotation && trackingSpeedNormal < trackingSpeedRotation) ? trackingSpeedRotation : trackingSpeedNormal;

                if(Input.GetMouseButton(0) && Input.GetMouseButton(1))
                {
                    float x = Input.GetAxis("Mouse X");
                    float y = Input.GetAxis("Mouse Y");
                    if(Mathf.Abs(x) > 0 || Mathf.Abs(y) > 0)
                    {
                        targetOffsetAmount.x += x * defaultCameraSpeed;
                        targetOffsetAmount.y += y * defaultCameraSpeed;
                        trackingSpeed = 1.0f;
                    }
                }
                else if(Input.GetMouseButton(1))
                {
                    float x = Input.GetAxis("Mouse X");
                    if(Input.GetKey("left ctrl"))
                    {
                        guiTimeAngle = 0.1f;
                        if(Mathf.Abs(x) > 0)
                        {
                            //camera tilt adjustment
                            float newAngle = CameraAngle.z - x;
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
                            float newFov = CameraFov + x;
                            newFov = Mathf.Clamp(newFov, 1.0f, 300.0f);
                            CameraFov = newFov;
                        }
                    }
                    else if(Mathf.Abs(x) > 0)
                    {
                        //handle zooming manually when camera.movespeed = 0
                        float newDir = CameraDir.z - x * CameraZoomSpeed;
                        if(newDir >= -lockedMinDistance) newDir = -lockedMinDistance;
                        CameraDir = new Vector3(0.0f, 0.0f, newDir);
                    }
                }

                float distance = Vector3.Distance(CameraTargetPos, lastTargetPos.Value);
                if(distance > 0.00001) CameraTargetPos = Vector3.MoveTowards(CameraTargetPos, LockOnTargetPos + TargetOffsetAdjusted, distance * trackingSpeed);
                lastTargetPos = LockOnTargetPos + TargetOffsetAdjusted;
            }

            if(lockRotation)
            {
                Vector3 targetAngle = CameraAdjustedEulerAngles(lockOnTarget, CameraTransform);
                Vector3 difference = targetAngle - lastTargetAngle;
                CameraAngle += new Vector3(-difference.x, -difference.y, -difference.z);
                lastTargetAngle = targetAngle;
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                showLockOnTargets = false;
            }

            if(Hotkey.allowHotkeys && manageCursorVisibility)
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
            if(showInfoMsg && guiTimeInfo > 0.0f)
            {
                DebugGUI(infoMsgPosition.x, infoMsgPosition.y, 200f, 45f, infoMsg);
                guiTimeInfo -= Time.deltaTime;
            }

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
        
        protected virtual bool LockOn()
        {
            if(currentCharaInfo)
            {
                List<string> targetList = currentCharaInfo is CharFemale ? FileManager.GetQuickFemaleTargetNames() : FileManager.GetQuickMaleTargetNames();
                
                if(!lockOnTarget)
                {
                    return LockOn(targetList[0]);
                }
                else
                {
                    for(int i = 0; i < targetList.Count; i++)
                    {
                        if(lockOnTarget.name == targetList[i])
                        {
                            int next = i + 1 > targetList.Count - 1 ? 0 : i + 1;
                            return LockOn(targetList[next]);
                        }
                    }
                    
                    return LockOn(targetList[0]);
                }
            }

            return false;
        }

        protected virtual bool LockOn(string targetName, bool lockOnAnyway = false)
        {
            foreach(GameObject target in targetManager.GetAllTargets())
            {
                if(target.name.Substring(3) == targetName.Substring(3))
                {
                    if(LockOn(target))
                    {
                        targetOffsetAmount = new Vector3();
                        return true;
                    }
                }
            }

            if(lockOnAnyway)
            {
                if(LockOn())
                {
                    targetOffsetAmount = new Vector3();
                    return true;
                }
            }

            return false;
        }

        protected virtual bool LockOn(GameObject target)
        {
            if(target)
            {
                lockOnTarget = target;
                if(lastTargetPos == null) lastTargetPos = LockOnTargetPos + TargetOffsetAdjusted;
                CameraMoveSpeed = 0.0f;
                CreateInfoMsg("Locked to \"" + lockOnTarget.name + "\"");
                return true;
            }

            return false;
        }

        protected virtual void LockOnRelease()
        {
            if(lockOnTarget)
            {
                targetOffsetAmount = new Vector3();
                lockOnTarget = null;
                lastTargetPos = null;
                lockRotation = false;
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

        protected virtual void CharaSwitch(bool scrollDown = true){}

        protected virtual void RotationLockToggle()
        {
            if(lockRotation)
            {
                lockRotation = false;
                CreateInfoMsg("Rotation released");
            }
            else
            {
                lockRotation = true;
                lastTargetAngle = CameraAdjustedEulerAngles(lockOnTarget, CameraTransform);
                CreateInfoMsg("Rotation locked");
            }
        }

        protected void CreateInfoMsg(string msg, float time = 3.0f)
        {
            infoMsg = msg;
            guiTimeInfo = time;
        }

        protected static bool DebugGUI(float screenWidthMult, float screenHeightMult, float width, float height, string msg)
        {
            float xpos = Screen.width * screenWidthMult - width / 2.0f;
            float ypos = Screen.height * screenHeightMult - height / 2.0f;
            xpos = Mathf.Clamp(xpos, 0, Screen.width - width);
            ypos = Mathf.Clamp(ypos, 0, Screen.height - height);
            return GUI.Button(new Rect(xpos, ypos, width, height), msg);
        }

        protected static FieldType GetSecureField<ObjectType, FieldType>(string fieldName)
            where ObjectType : UnityEngine.Object
            where FieldType : class
        {
            try
            {
                ObjectType target = FindObjectOfType<ObjectType>();
                FieldInfo fieldinfo = typeof(ObjectType).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                FieldType field = fieldinfo.GetValue(target) as FieldType;
                return field;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        protected static Vector3 CameraAdjustedEulerAngles(GameObject target, Transform cameraTransform)
        {
            float x = AngleSigned(target.transform.forward, Vector3.forward, cameraTransform.right);
            float y = AngleSigned(target.transform.right, Vector3.right, cameraTransform.up);
            float z = AngleSigned(target.transform.up, Vector3.up, cameraTransform.forward);
            return new Vector3(x, y, z);
        }

        protected static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }
        
        protected static bool PluginInstalled(string name, string version = "")
        {
            foreach(var item in IllusionInjector.PluginManager.Plugins)
            {
                if(item.Name == name)
                {
                    if(version != "" && item.Version != version)
                    {
                        return false;
                    }
                    
                    return true;
                }
            }

            return false;
        }

        protected void GamepadControls()
        {
            if(Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                LockOn();
            }

            if(Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                LockOnRelease();
            }

            //if(Input.GetKeyDown(KeyCode.JoystickButton2))
            //{
                
            //}

            if(Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                CharaSwitch(true);
            }

            if(Input.GetKeyDown(KeyCode.JoystickButton9))
            {
                targetOffsetAmount = new Vector3();
            }

            float ly = Input.GetAxis("Oculus_GearVR_LThumbstickX");
            float lx = Input.GetAxis("Oculus_GearVR_LThumbstickY");
            Vector2 cameraInput = new Vector2(ly, lx);
            if(cameraInput.magnitude > 0.2f)
            {
                if(Input.GetKey(KeyCode.JoystickButton4))
                {
                    float newDir = CameraDir.z + lx * Mathf.Lerp(0.01f, 0.4f, controllerZoomSpeed);
                    if(newDir >= -lockedMinDistance) newDir = -lockedMinDistance;
                    CameraDir = new Vector3(0.0f, 0.0f, newDir);
                }
                else
                {
                    float power = Mathf.Lerp(1.0f, 4.0f, controllerRotSpeed);
                    CameraAngle += new Vector3(lx * power, -ly * power, 0.0f);
                }
            }
            
            float ry = Input.GetAxis("Oculus_GearVR_RThumbstickY");
            float rx = Input.GetAxis("Oculus_GearVR_DpadX");
            Vector2 stickInput = new Vector2(rx, ry);
            if(stickInput.magnitude > 0.2f)
            {
                if(lockOnTarget)
                {
                    float power = Mathf.Lerp(0.001f, 0.04f, controllerMoveSpeed);
                    targetOffsetAmount.x += -ry * power;
                    targetOffsetAmount.y += -rx * power;
                }
                else
                {
                    float power = Mathf.Lerp(0.001f, 0.04f, controllerMoveSpeed);
                    CameraTargetPos += (CameraTransform.right * -ry * power) + (CameraTransform.up * -rx * power);
                }
            }
        }
    }
}