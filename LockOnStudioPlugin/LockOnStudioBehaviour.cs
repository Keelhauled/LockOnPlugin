using IllusionUtility.GetUtility;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Manager;
using IllusionPlugin;

namespace LockOnStudioPlugin
{
    public class LockOnStudioBehaviour : MonoBehaviour
    {
        private Hotkey lockOnHotkey;
        private Hotkey rotationHotkey;
        private Hotkey switchHotkey;
        private float lockedZoomSpeed;
        private float lockedMinDistance;
        private float lockedTrackingSpeed1;
        private float lockedTrackingSpeed2;
        private string[] boneList;
        private bool manageCursorVisibility;

        private Studio instance;
        private GameObject canvasObjects;
        private CameraControl camera;
        private GameObject cameraTarget;
        private float defaultCameraMoveSpeed;
        private float normalCameraMoveSpeed;
        private Vector3? lastBonePos = null;
        private bool lockRotation = false;
        private Vector3? lastTargetAngle = null;
        private int characterHash;

        private float guiTimeAngle = 0.0f;
        private float guiTimeFov = 0.0f;
        private float guiTimeInfo = 0.0f;
        private bool showInfoMsg = false;
        private string infoMsg = "";

        private void Start()
        {
            instance = Singleton<Studio>.Instance;
            canvasObjects = GameObject.Find("CanvasObjects");
            camera = FindObjectOfType<CameraControl>();
            defaultCameraMoveSpeed = camera.moveSpeed;

            lockOnHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "LockOnHotkey", "M", true).ToLower()[0].ToString(), 0.5f);
            switchHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "CharaSwitchHotkey", "L", true).ToLower()[0].ToString(), 0.5f);
            rotationHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "RotationHotkey", "N", true).ToLower()[0].ToString(), 0.5f);
            lockedZoomSpeed = ModPrefs.GetFloat("LockOnPlugin", "LockedZoomSpeed", 5.0f, true);
            lockedMinDistance = Math.Abs(ModPrefs.GetFloat("LockOnPlugin", "LockedMinDistance", 0.2f, true));
            lockedTrackingSpeed1 = lockedTrackingSpeed2 = Math.Abs(ModPrefs.GetFloat("LockOnPlugin", "LockedTrackingSpeed", 0.1f, true));
            boneList = ModPrefs.GetString("LockOnPlugin", "BoneList", "J_Head|J_Mune00|J_Spine01|J_Kokan", true).Split('|');
            camera.isOutsideTargetTex = !Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "HideCameraTarget", "True", true));
            manageCursorVisibility = Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "ManageCursorVisibility", "True", true));
        }

        private void Update()
        {
            lockOnHotkey.KeyUpAction(LockOn);
            switchHotkey.KeyDownAction(CharaSwitch);

            if(cameraTarget)
            {
                lockOnHotkey.KeyHoldAction(LockOnRelease);
                rotationHotkey.KeyDownAction(LockRotation);
            }

            if(lockRotation && !cameraTarget)
            {
                lockRotation = false;
                lastTargetAngle = null;
            }
            else if(lockRotation)
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
                    float x = Input.GetAxis("Mouse X");
                    if(Input.GetKey("left ctrl"))
                    {
                        //camera tilt adjustment
                        float newAngle = camera.CameraAngle.z - x * Time.deltaTime * 50.0f;
                        newAngle = Mathf.Repeat(newAngle, 360.0f);
                        camera.CameraAngle = new Vector3(camera.CameraAngle.x, camera.CameraAngle.y, newAngle);
                        guiTimeAngle = 0.1f;
                    }
                    else if(Input.GetKey("left shift"))
                    {
                        //fov adjustment
                        float newFov = camera.CameraFov + x * Time.deltaTime * 15.0f;
                        newFov = Mathf.Clamp(newFov, 10.0f, 100.0f);
                        camera.CameraFov = newFov;
                        guiTimeFov = 0.1f;
                    }
                    else
                    {
                        //prevent default camera movement and handle zooming manually
                        float newDir = camera.CameraDir.z - x * Time.deltaTime * lockedZoomSpeed;
                        if(newDir >= -lockedMinDistance) newDir = -lockedMinDistance;
                        camera.CameraDir = new Vector3(0.0f, 0.0f, newDir);
                    }
                }
            }

            if(manageCursorVisibility)
            {
                if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
                    Cursor.visible = false;
                else
                    Cursor.visible = true;
            }
        }

        private void LockOn()
        {
            StudioChara character = instance.CurrentChara;
            if(character != null)
            {
                int newHash = character.GetHashCode();
                if(characterHash != newHash) cameraTarget = null;
                characterHash = newHash;

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
                normalCameraMoveSpeed = camera.moveSpeed;
                camera.moveSpeed = 0.0f;

                CreateInfoMsg("Locked to \"" + cameraTarget.name + "\"");
            }
        }

        private void LockOn(int listItem)
        {
            StudioChara character = instance.CurrentChara;
            if(character != null)
            {
                characterHash = character.GetHashCode();

                CharBody body = character.body;
                string prefix = character is StudioFemale ? "cf_" : "cm_";

                cameraTarget = body.objBone.transform.FindLoop(prefix + boneList[listItem]);

                if(lastBonePos == null) lastBonePos = cameraTarget.transform.position;
                normalCameraMoveSpeed = camera.moveSpeed;
                camera.moveSpeed = 0.0f;

                CreateInfoMsg("Locked to \"" + cameraTarget.name + "\"");
            }
        }

        private int GetLockOnTarget()
        {
            StudioChara character = instance.CurrentChara;
            if(character != null)
            {
                CharBody body = character.body;
                string prefix = character is StudioFemale ? "cf_" : "cm_";

                for(int i = 0; i < boneList.Length; i++)
                {
                    if(cameraTarget.name == prefix + boneList[i])
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void CharaSwitch()
        {
            if(cameraTarget && cameraTarget)
            {
                var females = instance.FemaleList;
                int charaCount = instance.FemaleList.Count;
                int currentChara = instance.CurrentCharNo;
                int nextChara = currentChara + 1 > charaCount - 1 ? 0 : currentChara + 1;
                int targetBone = GetLockOnTarget();

                if(targetBone != -1)
                {
                    instance.SetCurrentCharaController(females.ElementAtOrDefault(nextChara).Value);
                    LockOn(targetBone);
                    CreateInfoMsg("Locked to \"" + instance.CurrentChara.GetStudioFemale().female.customInfo.name + "\"");
                }
            }
            else
            {
                LockOn();
            }
        }

        private void LockOnRelease()
        {
            lockRotation = false;
            lastTargetAngle = null;
            cameraTarget = null;
            lastBonePos = null;

            if(camera.moveSpeed <= 0.0f && normalCameraMoveSpeed > 0.0f)
                camera.moveSpeed = normalCameraMoveSpeed;
            else if(camera.moveSpeed <= 0.0f)
                camera.moveSpeed = defaultCameraMoveSpeed;
            
            CreateInfoMsg("Camera unlocked");
        }

        private void LockRotation()
        {
            if(lockRotation)
            {
                lockRotation = false;
                lastTargetAngle = null;
                CreateInfoMsg("Camera rotation released");
            }
            else
            {
                lockRotation = true;
                lastTargetAngle = cameraTarget.transform.eulerAngles;
                CreateInfoMsg("Camera rotation locked");
            }
        }

        private void OnGUI()
        {
            if(guiTimeAngle > 0.0f)
            {
                DebugGUI(0.5f, 0.5f, 100f, 50f, "Camera tilt\n" + camera.CameraAngle.z.ToString("0.0"));
                guiTimeAngle -= Time.deltaTime;
            }

            if(guiTimeFov > 0.0f)
            {
                DebugGUI(0.5f, 0.5f, 100f, 50f, "Field of view\n" + camera.CameraFov.ToString("0.0"));
                guiTimeFov -= Time.deltaTime;
            }

            if(showInfoMsg && canvasObjects.activeInHierarchy)
            {
                if(guiTimeInfo > 0.0f)
                {
                    DebugGUI(1.0f, 0.0f, 200f, 50f, infoMsg);
                    guiTimeInfo -= Time.deltaTime;
                }
            }
        }

        private void CreateInfoMsg(string msg, float time = 3.0f)
        {
            infoMsg = msg;
            guiTimeInfo = time;
        }

        private static bool DebugGUI(float screenWidthMult, float screenHeightMult, float width, float height, string msg)
        {
            float xpos = Screen.width * screenWidthMult - width / 2.0f;
            float ypos = Screen.height * screenHeightMult - height / 2.0f;
            xpos = Mathf.Clamp(xpos, 0, Screen.width - width);
            ypos = Mathf.Clamp(ypos, 0, Screen.height - height);
            return GUI.Button(new Rect(xpos, ypos, width, height), msg);
        }
    }

    internal class Hotkey
    {
        private string key;
        private float procTime;
        private float timeHeld = 0.0f;
        private bool released = true;

        public Hotkey(string key, float procTime)
        {
            this.key = key;
            this.procTime = procTime;
        }

        private bool GetModifiers() => Input.GetKey("left shift") || Input.GetKey("left alt");
        private bool GetKey() => Input.GetKey(key);
        private bool GetKeyUp() => Input.GetKeyUp(key);
        private bool GetKeyDown() => Input.GetKeyDown(key);
        
        private bool ShouldProc() => timeHeld >= procTime;
        private void AddTime() => timeHeld += Time.deltaTime;
        private void ResetTime() => timeHeld = 0.0f;

        private bool Released() => released;
        private void Released(bool val) => released = val;

        public void KeyHoldAction(UnityAction action)
        {
            if(GetKey() && !GetModifiers())
            {
                AddTime();
                if(ShouldProc() && Released())
                {
                    Released(false);
                    action();
                }
            }
        }

        public void KeyUpAction(UnityAction action)
        {
            if(GetKeyUp() && !GetModifiers())
            {
                if(Released())
                {
                    action();
                }

                ResetTime();
                Released(true);
            }
        }

        public void KeyDownAction(UnityAction action)
        {
            if(GetKeyDown() && !GetModifiers())
            {
                action();
                Released(false);
            }
        }
    }
}