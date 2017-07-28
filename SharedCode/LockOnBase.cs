using System;
using System.Reflection;
using System.Collections.Generic;
using IllusionPlugin;
using UnityEngine;
using System.IO;

namespace LockOnPlugin
{
    internal abstract class LockOnBase : MonoBehaviour
    {
        public const string VERSION = "2.3.0";
        public const string NAME_HSCENEMAKER = "LockOnPlugin";
        public const string NAME_NEO = "LockOnPluginNeo";

        protected abstract float CameraMoveSpeed { get; set; }
        protected abstract Vector3 CameraTargetPos { get; set; }
        protected abstract Vector3 LockOnTargetPos { get; }
        protected abstract Vector3 CameraAngle { get; set; }
        protected abstract float CameraFov { get; set; }
        protected abstract Vector3 CameraDir { get; set; }
        protected abstract bool CameraTargetTex { set; }
        protected abstract float CameraZoomSpeed { get; }
        protected abstract Transform CameraTransform { get; }
        protected virtual bool AllowTracking => true;
        protected virtual bool InputFieldSelected => false;

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
        protected bool controllerInvertX;
        protected bool controllerInvertY;
        protected bool swapSticks;

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
        protected Vector2 infoMsgPosition = Vector2.zero;
        protected Vector3 targetOffsetSize = Vector3.zero;
        protected float dpadXTimeHeld = 0.0f;

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
            trackingSpeedNormal = Mathf.Clamp(ModPrefs.GetFloat("LockOnPlugin.Misc", "LockedTrackingSpeed", 0.1f, true), 0.01f, 1.0f);
            showInfoMsg = ModPrefs.GetString("LockOnPlugin.Misc", "ShowInfoMsg", "False", true).ToLower() == "true" ? true : false;
            manageCursorVisibility = ModPrefs.GetString("LockOnPlugin.Misc", "ManageCursorVisibility", "True", true).ToLower() == "true" ? true : false;
            CameraTargetTex = ModPrefs.GetString("LockOnPlugin.Misc", "HideCameraTarget", "True", true).ToLower() == "true" ? false : true;
            scrollThroughMalesToo = ModPrefs.GetString("LockOnPlugin.Misc", "ScrollThroughMalesToo", "False", true).ToLower() == "true" ? true : false;

            controllerEnabled = ModPrefs.GetString("LockOnPlugin.Gamepad", "ControllerEnabled", "True", true).ToLower() == "true" ? true : false;
            controllerMoveSpeed = ModPrefs.GetFloat("LockOnPlugin.Gamepad", "ControllerMoveSpeed", 0.3f, true);
            controllerZoomSpeed = ModPrefs.GetFloat("LockOnPlugin.Gamepad", "ControllerZoomSpeed", 0.2f, true);
            controllerRotSpeed = ModPrefs.GetFloat("LockOnPlugin.Gamepad", "ControllerRotSpeed", 0.4f, true);
            controllerInvertX = ModPrefs.GetString("LockOnPlugin.Gamepad", "ControllerInvertX", "False", true).ToLower() == "true" ? true : false;
            controllerInvertY = ModPrefs.GetString("LockOnPlugin.Gamepad", "ControllerInvertY", "False", true).ToLower() == "true" ? true : false;
            swapSticks = ModPrefs.GetString("LockOnPlugin.Gamepad", "SwapSticks", "False", true).ToLower() == "true" ? true : false;
        }

        protected virtual void Update()
        {
            Hotkey.inputFieldSelected = InputFieldSelected;
            targetManager.UpdateCustomTargetTransforms();
            GamepadControls();

            lockOnHotkey.KeyHoldAction(LockOnRelease);
            lockOnHotkey.KeyUpAction(() => LockOn());
            lockOnGuiHotkey.KeyDownAction(ToggleLockOnGUI);
            prevCharaHotkey.KeyDownAction(() => CharaSwitch(false));
            nextCharaHotkey.KeyDownAction(() => CharaSwitch(true));
            //rotationHotkey.KeyDownAction(RotationLockToggle);

            if(lockOnTarget)
            {
                if(Input.GetMouseButton(0) && Input.GetMouseButton(1))
                {
                    float x = Input.GetAxis("Mouse X");
                    float y = Input.GetAxis("Mouse Y");
                    if(Mathf.Abs(x) > 0 || Mathf.Abs(y) > 0)
                    {
                        targetOffsetSize += (CameraTransform.right * x * defaultCameraSpeed) + (CameraTransform.forward * y * defaultCameraSpeed);
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
                            CameraFov = Mathf.Clamp(newFov, 1.0f, 160.0f);
                        }
                    }
                    else
                    {
                        if(Mathf.Abs(x) > 0)
                        {
                            //handle zooming manually when camera.movespeed = 0
                            float newDir = CameraDir.z - x * CameraZoomSpeed;
                            newDir = Mathf.Clamp(newDir, -float.MaxValue, 0.0f);
                            CameraDir = new Vector3(0.0f, 0.0f, newDir);
                        }

                        float y = Input.GetAxis("Mouse Y");
                        if(Mathf.Abs(y) > 0)
                        {
                            targetOffsetSize += (Vector3.up * y * defaultCameraSpeed);
                        }
                    }
                }

                float speed = Time.deltaTime * 0.33f;
                if(Input.GetKey(KeyCode.RightArrow))
                {
                    targetOffsetSize += Camera.main.transform.TransformDirection(new Vector3(speed, 0f, 0f));
                }
                else if(Input.GetKey(KeyCode.LeftArrow))
                {
                    targetOffsetSize += Camera.main.transform.TransformDirection(new Vector3(-speed, 0f, 0f));
                }
                if(Input.GetKey(KeyCode.UpArrow))
                {
                    targetOffsetSize += Camera.main.transform.TransformDirection(new Vector3(0f, 0f, speed));
                }
                else if(Input.GetKey(KeyCode.DownArrow))
                {
                    targetOffsetSize += Camera.main.transform.TransformDirection(new Vector3(0f, 0f, -speed));
                }
                if(Input.GetKey(KeyCode.PageUp))
                {
                    targetOffsetSize += Camera.main.transform.TransformDirection(new Vector3(0f, speed, 0f));
                }
                else if(Input.GetKey(KeyCode.PageDown))
                {
                    targetOffsetSize += Camera.main.transform.TransformDirection(new Vector3(0f, -speed, 0f));
                }

                if(AllowTracking)
                {
                    float trackingSpeed = (lockRotation && trackingSpeedNormal < trackingSpeedRotation) ? trackingSpeedRotation : trackingSpeedNormal;
                    float distance = Vector3.Distance(CameraTargetPos, lastTargetPos.Value);
                    if(distance > 0.00001) CameraTargetPos = Vector3.MoveTowards(CameraTargetPos, LockOnTargetPosOffset(), distance * trackingSpeed);
                    lastTargetPos = LockOnTargetPosOffset(); 
                }
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
                List<GameObject> targets = targetManager.GetAllTargets();
                for(int i = 0; i < targets.Count; i++)
                {
                    Vector3 pos = Camera.main.WorldToScreenPoint(targets[i].transform.position);
                    if(pos.z > 0.0f && GUI.Button(new Rect(pos.x - targetSize / 2, Screen.height - pos.y - targetSize / 2, targetSize, targetSize), "L"))
                    {
                        targetOffsetSize = Vector3.zero;
                        LockOn(targets[i]);
                    }
                }
            }
        }

        protected virtual bool LockOn()
        {
            if(currentCharaInfo)
            {
                if(targetOffsetSize.magnitude > 0.0f)
                {
                    targetOffsetSize = Vector3.zero;
                    return true;
                }

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
                        targetOffsetSize = Vector3.zero;
                        return true;
                    }
                }
            }

            if(lockOnAnyway)
            {
                if(LockOn())
                {
                    targetOffsetSize = Vector3.zero;
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
                if(lastTargetPos == null) lastTargetPos = LockOnTargetPosOffset();
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
                targetOffsetSize = Vector3.zero;
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

        protected Vector3 LockOnTargetPosOffset()
        {
            return LockOnTargetPos + (Vector3.right * targetOffsetSize.x) + (Vector3.up * targetOffsetSize.y) + (Vector3.forward * targetOffsetSize.z);
        }

        protected static bool DebugGUI(float screenWidthMult, float screenHeightMult, float width, float height, string msg)
        {
            float xpos = Screen.width * screenWidthMult - width / 2.0f;
            float ypos = Screen.height * screenHeightMult - height / 2.0f;
            xpos = Mathf.Clamp(xpos, 0, Screen.width - width);
            ypos = Mathf.Clamp(ypos, 0, Screen.height - height);
            return GUI.Button(new Rect(xpos, ypos, width, height), msg);
        }

        protected static FieldType GetSecureField<FieldType, ObjectType>(string fieldName, ObjectType target = null)
            where ObjectType : UnityEngine.Object
            where FieldType : class
        {
            if(target.Equals(null))
            {
                target = FindObjectOfType<ObjectType>();
            }

            if(!target.Equals(null))
            {
                FieldInfo fieldinfo = typeof(ObjectType).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if(!fieldinfo.Equals(null))
                {
                    return fieldinfo.GetValue(target) as FieldType;
                }
            }

            return null;
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

        public static void Log(string filename, string msg)
        {
            string path = Environment.CurrentDirectory + "\\Plugins\\LOPLog\\";
            StreamWriter sw = File.AppendText(path + filename);
            try
            {
                sw.WriteLine(msg);
            }
            finally
            {
                sw.Close();
            }
        }

        private void GamepadControls()
        {
            if(!controllerEnabled) return;
            if(Input.GetJoystickNames().Length == 0) return;
            
            if(Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                LockOn();
            }

            if(Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                LockOnRelease();
            }

            if(Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                CharaSwitch(true);
            }

            Vector2 leftStick = new Vector2(Input.GetAxis("Oculus_GearVR_LThumbstickX"), -Input.GetAxis("Oculus_GearVR_LThumbstickY"));
            Vector2 rightStick = new Vector2(-Input.GetAxis("Oculus_GearVR_RThumbstickY"), Input.GetAxis("Oculus_GearVR_DpadX"));
            KeyCode L1 = KeyCode.JoystickButton4;
            KeyCode R1 = KeyCode.JoystickButton5;

            if(swapSticks)
            {
                Vector3 temp = rightStick;
                rightStick = leftStick;
                leftStick = temp;
                L1 = KeyCode.JoystickButton5;
                R1 = KeyCode.JoystickButton4;
            }

            if(leftStick.magnitude > 0.2f)
            {
                if(Input.GetKey(R1))
                {
                    guiTimeFov = 1.0f;
                    float newFov = CameraFov + leftStick.y;
                    CameraFov = Mathf.Clamp(newFov, 1.0f, 160.0f);
                }
                else if(Input.GetKey(L1))
                {
                    float newDir = CameraDir.z - leftStick.y * Mathf.Lerp(0.01f, 0.4f, controllerZoomSpeed);
                    newDir = Mathf.Clamp(newDir, float.MinValue, 0.0f);
                    CameraDir = new Vector3(0.0f, 0.0f, newDir);
                }
                else
                {
                    float power = Mathf.Lerp(1.0f, 4.0f, controllerRotSpeed);
                    float newX = Mathf.Repeat(controllerInvertX || CameraDir.z == 0.0f ? leftStick.y : -leftStick.y * power, 360.0f);
                    float newY = Mathf.Repeat(controllerInvertY || CameraDir.z == 0.0f ? leftStick.x : -leftStick.x * power, 360.0f);
                    CameraAngle += new Vector3(newX, newY, 0.0f);
                }
            }

            if(rightStick.magnitude > 0.2f)
            {
                float power = Input.GetKey(R1) ? Mathf.Lerp(0.01f, 0.4f, controllerZoomSpeed) : Mathf.Lerp(0.001f, 0.04f, controllerMoveSpeed);
                if(lockOnTarget)
                {
                    if(Input.GetKey(R1))
                    {
                        targetOffsetSize += (CameraTransform.forward * -rightStick.y * power);
                    }
                    else
                    {
                        targetOffsetSize += (CameraTransform.right * rightStick.x * power) + (Vector3.up * -rightStick.y * power);
                    }
                }
                else
                {
                    if(Input.GetKey(R1))
                    {
                        CameraTargetPos += (CameraTransform.forward * -rightStick.y * power);
                    }
                    else
                    {
                        CameraTargetPos += (CameraTransform.right * rightStick.x * power) + (Vector3.up * -rightStick.y * power);
                    }
                }
            }

            float dpadX = -Input.GetAxis("Oculus_GearVR_DpadY");
            if(Math.Abs(dpadX) > 0)
            {
                if(dpadXTimeHeld == 0.0f || dpadXTimeHeld > 0.15f)
                {
                    guiTimeAngle = 1.0f;
                    float newAngle = CameraAngle.z - dpadX;
                    newAngle = Mathf.Repeat(newAngle, 360.0f);
                    CameraAngle = new Vector3(CameraAngle.x, CameraAngle.y, newAngle);
                }

                dpadXTimeHeld += Time.deltaTime;
            }
            else
            {
                dpadXTimeHeld = 0.0f;
            }
        }
    }
}
