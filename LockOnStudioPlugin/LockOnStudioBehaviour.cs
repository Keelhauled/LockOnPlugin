using IllusionUtility.GetUtility;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Manager;
using IllusionPlugin;
using System.IO;

namespace LockOnStudioPlugin
{
    public class LockOnStudioBehaviour : MonoBehaviour
    {
        private Hotkey lockOnHotkey;
        private Hotkey lockOnGuiHotkey;
        private Hotkey rotationHotkey;
        private Hotkey switchHotkey;
        private float lockedZoomSpeed;
        private float lockedMinDistance;
        private float trackingSpeedNormal;
        private float trackingSpeedRotate = 0.2f;
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

        private bool showLockOnTargets = false;
        private StudioChara chara = null;
        private List<GameObject> charaBones = null;
        private List<IntersectionTarget> charaIntersections = null;

        private float guiTimeAngle = 0.0f;
        private float guiTimeFov = 0.0f;
        private float guiTimeInfo = 0.0f;
        private bool showInfoMsg = false;
        private string infoMsg = "";
        private bool debugMode = false;

        private List<string> boneListGui;
        private List<string> boneListQuick;
        private List<string[]> boneListIntersections;

        private void Start()
        {
            instance = Singleton<Studio>.Instance;
            camera = FindObjectOfType<CameraControl>();
            defaultCameraMoveSpeed = camera.moveSpeed;
            canvasObjects = GameObject.Find("CanvasObjects");

            lockOnHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "LockOnHotkey", "M", true), 0.5f);
            lockOnGuiHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "LockOnGuiHotkey", "K", true));
            switchHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "CharaSwitchHotkey", "L", true));
            rotationHotkey = new Hotkey(ModPrefs.GetString("LockOnPlugin", "RotationHotkey", "N", true));
            lockedZoomSpeed = ModPrefs.GetFloat("LockOnPlugin", "LockedZoomSpeed", 5.0f, true);
            lockedMinDistance = Math.Abs(ModPrefs.GetFloat("LockOnPlugin", "LockedMinDistance", 0.2f, true));
            trackingSpeedNormal = Math.Abs(ModPrefs.GetFloat("LockOnPlugin", "LockedTrackingSpeed", 0.1f, true));
            if(trackingSpeedNormal > trackingSpeedRotate) trackingSpeedRotate = trackingSpeedNormal;
            camera.isOutsideTargetTex = !Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "HideCameraTarget", "True", true));
            manageCursorVisibility = Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "ManageCursorVisibility", "True", true));
            showInfoMsg = Convert.ToBoolean(ModPrefs.GetString("LockOnPlugin", "ShowInfoMsg", "False", true));
            
            boneListGui = ReadBoneFile("\\Plugins\\LockOnPlugin\\guibones.txt");
            boneListQuick = ReadBoneFile("\\Plugins\\LockOnPlugin\\quickbones.txt");
            boneList = boneListQuick.ToArray();
            boneListIntersections = ReadIntersectionFile("\\Plugins\\LockOnPlugin\\intersections.txt");
        }

        private void Update()
        {
            CharaHasBeenSwitched();

            lockOnHotkey.KeyUpAction(LockOn);
            lockOnGuiHotkey.KeyDownAction(ToggleLockOnTargets);
            switchHotkey.KeyDownAction(CharaSwitch);

            if(showLockOnTargets || cameraTarget)
            {
                UpdateIntersectionPositions();
            }

            if(cameraTarget)
            {
                lockOnHotkey.KeyHoldAction(LockOnRelease);
                rotationHotkey.KeyDownAction(ToggleRotationLock);
            }

            if(lockRotation && !cameraTarget)
            {
                ToggleRotationLock();
            }
            else if(lockRotation)
            {
                Vector3 targetAngle = cameraTarget.transform.eulerAngles;
                Vector3 difference = targetAngle - lastTargetAngle.Value;
                camera.CameraAngle += new Vector3(-difference.x, difference.y, -difference.z);
                lastTargetAngle = targetAngle;
            }

            if(cameraTarget)
            {
                float distance = Vector3.Distance(camera.TargetPos, lastBonePos.Value);
                if(distance > 0.00001) camera.TargetPos = Vector3.MoveTowards(camera.TargetPos, cameraTarget.transform.position, distance * (lockRotation ? trackingSpeedRotate : trackingSpeedNormal));
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
                chara = character;
                CharBody body = character.body;
                string prefix = character is StudioFemale ? "cf_" : "cm_";
                if(!cameraTarget)
                {
                    cameraTarget = body.objBone.transform.FindLoop(prefix + boneList[0]);
                }
                else
                {
                    bool targetChanged = false;

                    for(int i = 0; i < boneList.Length; i++)
                    {
                        if(cameraTarget.name == prefix + boneList[i])
                        {
                            string boneName = boneList.ElementAtOrDefault(i + 1) != null ? prefix + boneList[i + 1] : prefix + boneList[0];
                            cameraTarget = body.objBone.transform.FindLoop(boneName);
                            targetChanged = true;
                            break;
                        }
                    }

                    if(!targetChanged)
                    {
                        cameraTarget = body.objBone.transform.FindLoop(prefix + boneList[0]);
                    }
                }

                if(lastBonePos == null) lastBonePos = cameraTarget.transform.position;
                normalCameraMoveSpeed = camera.moveSpeed;
                camera.moveSpeed = 0.0f;

                CreateInfoMsg("Locked to \"" + cameraTarget.name + "\"");
            }
        }

        private void LockOn(GameObject bone)
        {
            StudioChara character = instance.CurrentChara;
            if(character != null)
            {
                chara = character;
                cameraTarget = bone;

                if(lastBonePos == null) lastBonePos = cameraTarget.transform.position;
                normalCameraMoveSpeed = camera.moveSpeed;
                camera.moveSpeed = 0.0f;

                CreateInfoMsg("Locked to \"" + cameraTarget.name + "\"");
            }
        }

        private void LockOnRelease()
        {
            if(cameraTarget)
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
        }

        private void CharaHasBeenSwitched()
        {
            StudioChara newChara = instance.CurrentChara;

            if(chara != newChara)
            {
                LockOnRelease();

                if(newChara == null)
                {
                    Console.WriteLine("Nothing important selected");
                    chara = null;
                    charaBones = null;
                    charaIntersections = null;
                    showLockOnTargets = false;
                }
                else if(newChara.sexType == 0)
                {
                    Console.WriteLine("Switched to male");
                    chara = newChara;
                    charaBones = null;
                    charaIntersections = null;
                    showLockOnTargets = false;
                }
                else if(newChara.sexType == 1)
                {
                    Console.WriteLine("Switched to female");
                    chara = newChara;
                    UpdateCharaBones(chara);
                    UpdateCharaIntersections(chara);
                }
                else if(newChara.sexType == 2)
                {
                    Console.WriteLine("Switched to item");
                    chara = null;
                    charaBones = null;
                    charaIntersections = null;
                    showLockOnTargets = false;
                }
            }
        }

        private void UpdateCharaBones(StudioChara character)
        {
            charaBones = null;

            if(character != null)
            {
                string prefix = character is StudioFemale ? "cf_" : "cm_";
                charaBones = new List<GameObject>();

                foreach(string boneName in boneListGui)
                {
                    GameObject bone = character.body.objBone.transform.FindLoop(prefix + boneName);
                    if(bone)
                        charaBones.Add(bone);
                }
            }
        }

        private void UpdateCharaIntersections(StudioChara character)
        {
            charaIntersections = null;

            if(character != null && character.sexType == 1)
            {
                string prefix = character is StudioFemale ? "cf_" : "cm_";
                charaIntersections = new List<IntersectionTarget>();

                foreach(string[] data in boneListIntersections)
                {
                    GameObject point1 = character.body.objBone.transform.FindLoop(prefix + data[1]);
                    GameObject point2 = character.body.objBone.transform.FindLoop(prefix + data[2]);
                    if(point1 && point2)
                    {
                        IntersectionTarget target = new IntersectionTarget(data[0], point1, point2);
                        charaIntersections.Add(target);
                        charaBones.Add(target.GetTarget());
                    }
                }
            }
        }

        private void UpdateIntersectionPositions()
        {
            if(charaIntersections != null)
            {
                foreach(IntersectionTarget intersection in charaIntersections)
                {
                    intersection.UpdateTargetPosition();
                }
            }
        }

        private void CharaSwitch()
        {
            if(cameraTarget)
            {
                Dictionary<uint, StudioFemale> females = instance.FemaleList;
                int charaCount = instance.FemaleList.Count;
                int currentChara = instance.CurrentCharNo;
                int nextChara = currentChara + 1 > charaCount - 1 ? 0 : currentChara + 1;
                string targetBoneName = cameraTarget.name;

                StudioChara nextGirl = females.ElementAtOrDefault(nextChara).Value;
                instance.SetCurrentCharaController(nextGirl);
                UpdateCharaBones(nextGirl);
                UpdateCharaIntersections(nextGirl);

                foreach(GameObject bone in charaBones)
                {
                    if(bone.name == targetBoneName)
                    {
                        LockOn(bone);
                        break;
                    }
                }

                CreateInfoMsg("Locked to \"" + instance.CurrentChara.GetStudioFemale().female.customInfo.name + "\"");
            }
            else
            {
                LockOn();
            }
        }

        private void ToggleRotationLock()
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

            if(showInfoMsg && guiTimeInfo > 0.0f && canvasObjects.activeInHierarchy)
            {
                 DebugGUI(1.0f, 0.0f, 200f, 50f, infoMsg);
                 guiTimeInfo -= Time.deltaTime;
            }

            if(showLockOnTargets && charaBones != null)
            {
                foreach(GameObject bone in charaBones)
                {
                    float size = 25.0f;
                    Vector3 pos = Camera.main.WorldToScreenPoint(bone.transform.position);
                    if(GUI.Button(new Rect(pos.x - size / 2, Screen.height - pos.y - size / 2, size, size), "L"))
                    {
                        LockOn(bone);
                    }
                }
            }
            
            if(debugMode)
            {
                if(DebugGUI(0.60f, 0.0f, 100f, 50f, "guibones.txt"))
                {
                    foreach(var item in boneListGui)
                        Console.WriteLine(item);
                }

                if(DebugGUI(0.65f, 0.0f, 100f, 50f, "quickbones.txt"))
                {
                    foreach(var item in boneListQuick)
                        Console.WriteLine(item);
                }

                if(DebugGUI(0.70f, 0.0f, 100f, 50f, "intersections.txt"))
                {
                    foreach(var item in boneListIntersections)
                        Console.WriteLine("{0}, {1}, {2}", item[0], item[1], item[2]);
                }

                if(DebugGUI(0.75f, 0.0f, 100f, 50f, "charaBones"))
                {
                    if(charaBones != null)
                    {
                        foreach(var item in charaBones)
                            Console.WriteLine(item.name);
                    }
                    else
                    {
                        Console.WriteLine("null");
                    }
                }

                if(DebugGUI(0.80f, 0.0f, 100f, 50f, "intersections"))
                {
                    if(charaIntersections != null)
                    {
                        foreach(var item in charaIntersections)
                            Console.WriteLine(item.name);
                    }
                    else
                    {
                        Console.WriteLine("null");
                    }
                }
            }
        }

        private void ToggleLockOnTargets()
        {
            if(instance.CurrentChara.sexType == 1)
                showLockOnTargets = !showLockOnTargets;
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

        private static string StringUntil(string text, string stopAt)
        {
            if(text != null)
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if(charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
                else if(charLocation == 0)
                {
                    return "";
                }
            }

            return text;
        }

        private static List<string> ReadBoneFile(string filePath)
        {
            List<string> list = new List<string>();
            foreach(string item in File.ReadAllLines(Environment.CurrentDirectory + filePath))
            {
                string line = StringUntil(item, "//");
                line = line.Replace(" ", "");
                if(line != "")
                    list.Add(line);
            }

            return list;
        }

        private static List<string[]> ReadIntersectionFile(string filePath)
        {
            List<string[]> list = new List<string[]>();
            foreach(string item in File.ReadAllLines(Environment.CurrentDirectory + filePath))
            {
                string line = StringUntil(item, "//");
                line = line.Replace(" ", "");
                if(line != "")
                    list.Add(line.Split('|'));
            }

            return list;
        }
    }
}