using IllusionUtility.GetUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.UI;
using Studio;
using Manager;

namespace LockOnPlugin
{
    internal partial class NeoMono : LockOnBase
    {
        private Studio.Studio studio => Singleton<Studio.Studio>.Instance;
        private Studio.CameraControl camera => studio.cameraCtrl;
        private TreeNodeCtrl treeNodeCtrl => studio.treeNodeCtrl;
        private GuideObjectManager guideObjectManager => Singleton<GuideObjectManager>.Instance;

        private Studio.CameraControl.CameraData cameraData;
        private Studio.CameraControl.CameraData cameraReset;
        private OCIChar currentCharaOCI;

        private AnimatorOverrideController overrideController;
        private float rotateSpeed;
        private float animationSpeed;
        private bool moving = false;
        private int animMoveSetCurrent;
        private List<List<int>> animMoveSets = new List<List<int>>
        {
            new List<int> { 0, 0, 1, 1, 6, 0 }, // hands on the side
            new List<int> { 1, 3, 21, 1, 6, 1 }, // hands in front
            new List<int> { 1, 3, 17, 1, 6, 3 }, // catwalk
        };

        protected override void Start()
        {
            base.Start();

            cameraData = GetSecureField<Studio.CameraControl.CameraData, Studio.CameraControl>("cameraData", camera);
            cameraReset = GetSecureField<Studio.CameraControl.CameraData, Studio.CameraControl>("cameraReset", camera);
            treeNodeCtrl.onSelect += new Action<TreeNodeObject>(OnSelectWork);
            studio.onDelete += new Action<ObjectCtrlInfo>(OnDeleteWork);
            Transform systemMenuContent = studio.transform.Find("Canvas Main Menu/04_System/Viewport/Content");
            systemMenuContent.Find("Load").GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OnSceneMenuOpen()));
            systemMenuContent.Find("End").GetComponent<Button>().onClick.AddListener(() => showLockOnTargets = false);
            InstallNearClipPlaneSlider();
            StartCoroutine(InstallSettingsReloadButton());
            OverrideControllerCreate();
        }

        protected override void LoadSettings()
        {
            base.LoadSettings();

            manageCursorVisibility = false;
            infoMsgPosition = new Vector2(1f, 1f);
            animMoveSetCurrent = Mathf.Clamp(ModPrefs.GetInt("LockOnPlugin.Misc", "MovementAnimSet", 0, true), 0, animMoveSets.Count - 1);
            animationSpeed = Mathf.Clamp(ModPrefs.GetFloat("LockOnPlugin.Misc", "AnimationSpeed", 9f, true), 0f, 100f);
            rotateSpeed = Mathf.Clamp(ModPrefs.GetFloat("LockOnPlugin.Misc", "RotateSpeed", 1f, true), 0f, 1000f);
            float nearClipPlane = Mathf.Clamp(ModPrefs.GetFloat("LockOnPlugin.Misc", "NearClipPlane", Camera.main.nearClipPlane, true), 0.001f, 0.06f);
            Camera.main.nearClipPlane = nearClipPlane;
            GameObject nearClipSlider = GameObject.Find("Slider NearClipPlane");
            if(nearClipSlider) nearClipSlider.GetComponent<Slider>().value = nearClipPlane;
        }

        private void OnSelectWork(TreeNodeObject node)
        {
            ObjectCtrlInfo objectCtrlInfo = null;
            if(studio.dicInfo.TryGetValue(node, out objectCtrlInfo))
            {
                if(objectCtrlInfo.kind == 0)
                {
                    OCIChar ocichar = objectCtrlInfo as OCIChar;

                    if(ocichar != currentCharaOCI)
                    {
                        currentCharaOCI = ocichar;
                        currentCharaInfo = ocichar.charInfo;
                        targetManager.UpdateAllTargets(ocichar.charInfo);

                        if(lockOnTarget)
                        {
                            if(!LockOn(lockOnTarget.name, true, false))
                            {
                                LockOnRelease();
                            }
                        }
                    }
                    else
                    {
                        currentCharaOCI = ocichar;
                        currentCharaInfo = ocichar.charInfo;
                    }
                    
                    return;
                }
            }

            LockOnRelease();
            showLockOnTargets = false;

            currentCharaOCI = null;
            currentCharaInfo = null;
            targetManager.UpdateAllTargets(null);
        }

        private void OnDeleteWork(ObjectCtrlInfo info)
        {
            if(info.kind == 0)
            {
                LockOnRelease();
                showLockOnTargets = false;

                currentCharaOCI = null;
                currentCharaInfo = null;
                targetManager.UpdateAllTargets(null);
            }
        }

        private IEnumerator OnSceneMenuOpen()
        {
            SceneLoadScene scene;
            while((scene = FindObjectOfType<SceneLoadScene>()) == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            LockOnRelease();
            showLockOnTargets = false;

            // if HSStudioNEOAddon is installed everything has to be cleared here already
            if(FileManager.PluginInstalled("HoneyStudioNEO Adddon"))
            {
                currentCharaOCI = null;
                currentCharaInfo = null;
                targetManager.UpdateAllTargets(null);
                treeNodeCtrl.SelectSingle(null);
            }
            else
            {
                Button buttonClose = GetSecureField<Button, SceneLoadScene>("buttonClose", scene);
                Button buttonLoad = GetSecureField<Button, SceneLoadScene>("buttonLoad", scene);

                if(buttonClose != null && buttonLoad != null)
                {
                    buttonClose.onClick.AddListener(() =>
                    {
                        Hotkey.allowHotkeys = true;
                    });

                    buttonLoad.onClick.AddListener(() =>
                    {
                        Hotkey.allowHotkeys = true;
                        currentCharaOCI = null;
                        currentCharaInfo = null;
                        targetManager.UpdateAllTargets(null);
                        StartCoroutine(scene.NotificationLoadCoroutine());
                        Singleton<Scene>.Instance.UnLoad();
                    });

                    Hotkey.allowHotkeys = false; 
                }
                else
                {
                    Hotkey.allowHotkeys = true;
                    currentCharaOCI = null;
                    currentCharaInfo = null;
                    targetManager.UpdateAllTargets(null);
                    treeNodeCtrl.SelectSingle(null);
                    Console.WriteLine("SceneLoadScene buttons not found");
                }
            }
            
            yield break;
        }

        protected override bool LockOn()
        {
            if(base.LockOn()) return true;

            List<TreeNodeObject> charaNodes = scrollThroughMalesToo ? GetCharaNodes<OCIChar>() : GetCharaNodes<OCICharFemale>();
            if(charaNodes.Count > 0)
            {
                studio.treeNodeCtrl.SelectSingle(charaNodes[0]);
                if(base.LockOn()) return true;
            }

            return false;
        }

        protected override void CharaSwitch(bool scrollDown = true)
        {
            List<TreeNodeObject> charaNodes = scrollThroughMalesToo ? GetCharaNodes<OCIChar>() : GetCharaNodes<OCICharFemale>();

            for(int i = 0; i < charaNodes.Count; i++)
            {
                if(charaNodes[i] == treeNodeCtrl.selectNode)
                {
                    int next = i + 1 > charaNodes.Count - 1 ? 0 : i + 1;
                    if(!scrollDown) next = i - 1 < 0 ? charaNodes.Count - 1 : i - 1;
                    treeNodeCtrl.SelectSingle(charaNodes[next]);
                    return;
                }
            }

            if(charaNodes.Count > 0)
            {
                treeNodeCtrl.SelectSingle(charaNodes[0]);
            }
        }

        private List<TreeNodeObject> GetCharaNodes<CharaType>()
        {
            List<TreeNodeObject> charaNodes = new List<TreeNodeObject>();

            int n = 0; TreeNodeObject nthNode;
            while(nthNode = treeNodeCtrl.GetNode(n))
            {
                ObjectCtrlInfo objectCtrlInfo = null;
                if(nthNode.visible && studio.dicInfo.TryGetValue(nthNode, out objectCtrlInfo))
                {
                    if(objectCtrlInfo is CharaType)
                    {
                        charaNodes.Add(nthNode);
                    }
                }
                n++;
            }

            return charaNodes;
        }

        private IEnumerator InstallSettingsReloadButton()
        {
            Transform systemMenuContent = studio.transform.Find("Canvas Main Menu/04_System/Viewport/Content");
            if(systemMenuContent && !GameObject.Find("LockOnPluginReload"))
            {
                // wait for HSStudioNEOAddon specifically
                yield return new WaitForSeconds(0.1f);

                List<Transform> buttonlist = new List<Transform>();
                List<GameObject> menuContents = new List<GameObject>();
                systemMenuContent.FindLoopAll(menuContents);
                foreach(GameObject item in menuContents)
                {
                    if(item.GetComponent<Button>())
                    {
                        buttonlist.Add(item.transform);
                    }
                }
                Transform parentButton = buttonlist[buttonlist.Count - 1];

                GameObject newButton = Instantiate(parentButton.gameObject);
                newButton.name = "LockOnPluginReload";
                newButton.transform.SetParent(parentButton.parent);
                newButton.transform.Find("Text").gameObject.GetComponent<Text>().text = "LockOnPlugin rld";
                newButton.transform.localPosition = parentButton.localPosition - new Vector3(0f, 30f, 0f);
                newButton.transform.localScale = Vector3.one;

                Button buttonComponent = newButton.GetComponent<Button>();
                buttonComponent.onClick = new Button.ButtonClickedEvent();
                buttonComponent.onClick.AddListener(() =>
                {
                    LoadSettings();
                    targetManager.UpdateAllTargets(currentCharaInfo);
                    LockOn((currentCharaInfo is CharFemale ? FileManager.GetQuickFemaleTargetNames() : FileManager.GetQuickMaleTargetNames())[0]);
                    reduceOffset = true;
                });

                Console.WriteLine("LockOnPlugin reload button installed");
            }

            yield break;
        }

        private void InstallNearClipPlaneSlider()
        {
            Transform sliderParentObject = studio.transform.Find("Canvas Main Menu/04_System/02_Option/Slider Camera Speed");
            if(sliderParentObject && !GameObject.Find("Slider NearClipPlane"))
            {
                GameObject nearClipSlider = Instantiate(sliderParentObject.gameObject);
                nearClipSlider.name = "Slider NearClipPlane";
                nearClipSlider.transform.SetParent(sliderParentObject.parent);
                nearClipSlider.transform.localPosition = new Vector3(114f, -16f, 0f);
                nearClipSlider.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);

                Slider nearClipSliderComponent = nearClipSlider.GetComponent<Slider>();
                nearClipSliderComponent.maxValue = 0.06f;
                nearClipSliderComponent.minValue = 0.001f;
                nearClipSliderComponent.value = ModPrefs.GetFloat("LockOnPlugin.Misc", "NearClipPlane", Camera.main.nearClipPlane, true);
                nearClipSliderComponent.onValueChanged = new Slider.SliderEvent();
                nearClipSliderComponent.onValueChanged.AddListener((value) =>
                {
                    Camera.main.nearClipPlane = value;
                    ModPrefs.SetFloat("LockOnPlugin.Misc", "NearClipPlane", value);
                });

                Console.WriteLine("NearClipPlane slider installed");
            }

            Transform textParentObject = studio.transform.Find("Canvas Main Menu/04_System/02_Option/Text Camera Speed");
            if(sliderParentObject && textParentObject && !GameObject.Find("Text NearClipPlane"))
            {
                GameObject nearClipText = Instantiate(textParentObject.gameObject);
                nearClipText.name = "Text NearClipPlane";
                nearClipText.transform.SetParent(sliderParentObject.parent);
                nearClipText.transform.localPosition = new Vector3(83f, -16f, 0f);
                nearClipText.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
                Text nearClipTextComponent = nearClipText.GetComponent<Text>();
                nearClipTextComponent.text = "NearClip";
                Console.WriteLine("NearClipPlane text installed");
            }
        }

        protected override void GamepadControls()
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
                Vector2 temp = rightStick;
                rightStick = leftStick;
                leftStick = temp;
                L1 = KeyCode.JoystickButton5;
                R1 = KeyCode.JoystickButton4;
            }

            if(leftStick.magnitude > 0.2f)
            {
                if(Input.GetKey(R1))
                {
                    guiTimeFov = 1f;
                    float newFov = CameraFov + leftStick.y * Time.deltaTime * 60f;
                    CameraFov = Mathf.Clamp(newFov, 1f, 160f);
                }
                else if(Input.GetKey(L1))
                {
                    float newDir = CameraDir.z - leftStick.y * Mathf.Lerp(0.01f, 0.4f, controllerZoomSpeed) * Time.deltaTime * 60f;
                    newDir = Mathf.Clamp(newDir, float.MinValue, 0f);
                    CameraDir = new Vector3(0f, 0f, newDir);
                }
                else
                {
                    float speed = Mathf.Lerp(1f, 4f, controllerRotSpeed) * Time.deltaTime * 60f;
                    float newX = Mathf.Repeat((controllerInvertX || CameraDir.z == 0f ? leftStick.y : -leftStick.y) * speed, 360f);
                    float newY = Mathf.Repeat((controllerInvertY || CameraDir.z == 0f ? leftStick.x : -leftStick.x) * speed, 360f);
                    CameraAngle += new Vector3(newX, newY, 0f);
                }
            }

            if(rightStick.magnitude > 0.2f)
            {
                if(currentCharaOCI != null)
                {
                    if(!moving)
                    {
                        moving = true;
                        currentCharaOCI.charInfo.animBody.runtimeAnimatorController = overrideController;
                        currentCharaOCI.charInfo.animBody.CrossFadeInFixedTime("tachi_pose_02", 0.2f);
                    }

                    currentCharaOCI.animeSpeed = rightStick.magnitude * animationSpeed;
                    rightStick = rightStick * 0.04f;

                    Vector3 forward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)).normalized;
                    Vector3 lookDirection = Camera.main.transform.right * rightStick.x + forward * -rightStick.y;
                    lookDirection = new Vector3(lookDirection.x, 0f, lookDirection.z);
                    currentCharaOCI.guideObject.changeAmount.pos += lookDirection * Time.deltaTime * (animationSpeed * 9.6f);
                    Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                    Quaternion finalRotation = Quaternion.RotateTowards(Quaternion.Euler(currentCharaOCI.guideObject.changeAmount.rot), lookRotation, Time.deltaTime * 60f * rotateSpeed);
                    currentCharaOCI.guideObject.changeAmount.rot = finalRotation.eulerAngles;
                }
            }
            else if(moving)
            {
                moving = false;
                currentCharaOCI.charInfo.animBody.runtimeAnimatorController = overrideController;
                currentCharaOCI.charInfo.animBody.CrossFadeInFixedTime("tachi_pose_01", 0.2f);
                currentCharaOCI.animeSpeed = 1f;
            }

            float dpadX = -Input.GetAxis("Oculus_GearVR_DpadY");
            if(Math.Abs(dpadX) > 0f)
            {
                if(dpadXTimeHeld == 0f || dpadXTimeHeld > 0.15f)
                {
                    guiTimeAngle = 1f;
                    float newAngle = CameraAngle.z - dpadX * Time.deltaTime * 60f;
                    newAngle = Mathf.Repeat(newAngle, 360f);
                    CameraAngle = new Vector3(CameraAngle.x, CameraAngle.y, newAngle);
                }

                dpadXTimeHeld += Time.deltaTime;
            }
            else
            {
                dpadXTimeHeld = 0f;
            }
        }

        private void OverrideControllerCreate()
        {
            Studio.Info.AnimeLoadInfo animeLoadInfoIdle = GetAnimeInfo(0, 0, 1);
            RuntimeAnimatorController animeLoadInfoIdleCtrl = CommonLib.LoadAsset<RuntimeAnimatorController>(animeLoadInfoIdle.bundlePath, animeLoadInfoIdle.fileName);
            Studio.Info.AnimeLoadInfo animeLoadInfoMove = GetAnimeInfo(1, 6, 0);
            RuntimeAnimatorController animeLoadInfoMoveCtrl = CommonLib.LoadAsset<RuntimeAnimatorController>(animeLoadInfoMove.bundlePath, animeLoadInfoMove.fileName);
            overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = animeLoadInfoIdleCtrl;
            overrideController["tachi_pose_02"] = animeLoadInfoMoveCtrl.animationClips[0];
        }

        private Studio.Info.AnimeLoadInfo GetAnimeInfo(int group, int category, int no)
        {
            Dictionary<int, Dictionary<int, Studio.Info.AnimeLoadInfo>> dictionary = null;
            if(!Singleton<Studio.Info>.Instance.dicFemaleAnimeLoadInfo.TryGetValue(group, out dictionary))
            {
                return null;
            }

            Dictionary<int, Studio.Info.AnimeLoadInfo> dictionary2 = null;
            if(!dictionary.TryGetValue(category, out dictionary2))
            {
                return null;
            }

            Studio.Info.AnimeLoadInfo animeLoadInfo = null;
            if(!dictionary2.TryGetValue(no, out animeLoadInfo))
            {
                return null;
            }

            return animeLoadInfo;
        }
    }
}
