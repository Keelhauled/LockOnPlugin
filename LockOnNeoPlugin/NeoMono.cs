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
        private Studio.Studio studio = Singleton<Studio.Studio>.Instance;
        private Studio.CameraControl camera = Singleton<Studio.Studio>.Instance.cameraCtrl;
        private TreeNodeCtrl treeNodeCtrl = Singleton<Studio.Studio>.Instance.treeNodeCtrl;
        private GuideObjectManager guideObjectManager = Singleton<GuideObjectManager>.Instance;

        private Studio.CameraControl.CameraData cameraData;
        private OCIChar currentCharaOCI;
        private List<DynamicBone_Ver02> boobs;
        private AnimatorOverrideController overrideController;

        protected override void Start()
        {
            base.Start();

            cameraData = Utils.GetSecureField<Studio.CameraControl.CameraData, Studio.CameraControl>("cameraData", camera);
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
            Guitime.pos = new Vector2(1f, 1f);
            animMoveSetCurrent = Mathf.Clamp(ModPrefs.GetInt("LockOnPlugin.Misc", "MovementAnimSet", 1, true), 0, animMoveSets.Count - 1);
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
                        boobs = new List<DynamicBone_Ver02>
                        {
                            ((CharFemaleBody)ocichar.charBody).getDynamicBone(CharFemaleBody.DynamicBoneKind.BreastL),
                            ((CharFemaleBody)ocichar.charBody).getDynamicBone(CharFemaleBody.DynamicBoneKind.BreastR),
                        };

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
                Button buttonClose = Utils.GetSecureField<Button, SceneLoadScene>("buttonClose", scene);
                Button buttonLoad = Utils.GetSecureField<Button, SceneLoadScene>("buttonLoad", scene);

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

        protected override void GamepadMovement(Vector3 stick)
        {
            if(stick.magnitude > 0.1f)
            {
                if(currentCharaOCI != null)
                {
                    bool rotatingInPov = !CameraEnabled && Mathf.Abs(stick.y) < 0.2f;

                    if((!moving || animSwitched) && !rotatingInPov)
                    {
                        moving = true;
                        currentCharaOCI.charInfo.animBody.runtimeAnimatorController = overrideController;
                        currentCharaOCI.charInfo.animBody.CrossFadeInFixedTime(animMoveSets[animMoveSetCurrent].move, 0.2f);
                        currentCharaOCI.optionItemCtrl.LoadAnimeItem(null, "", 0f, 0f);
                        //currentCharaOCI.ActiveKinematicMode(OICharInfo.KinematicMode.FK, false, false);
                        //currentCharaOCI.ActiveKinematicMode(OICharInfo.KinematicMode.IK, false, false);
                        //GameObject toggle = GameObject.Find("Toggle Function");
                        //if(toggle) toggle.GetComponent<Toggle>().isOn = false;
                    }

                    float animSpeed = animMoveSets[animMoveSetCurrent].animSpeed * currentCharaOCI.guideObject.changeAmount.scale.z;
                    if(!rotatingInPov) currentCharaOCI.animeSpeed = stick.magnitude * animSpeed / currentCharaOCI.guideObject.changeAmount.scale.z;
                    stick = stick * 0.04f;

                    if(CameraEnabled)
                    {
                        Vector3 forward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)).normalized;
                        Vector3 right = Vector3.Scale(Camera.main.transform.right, new Vector3(1f, 0f, 1f)).normalized;
                        Vector3 lookDirection = right * stick.x + forward * -stick.y;
                        lookDirection = new Vector3(lookDirection.x, 0f, lookDirection.z);
                        currentCharaOCI.guideObject.changeAmount.pos += lookDirection * Time.deltaTime * (animSpeed * animMoveSets[animMoveSetCurrent].speedMult);
                        Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                        Quaternion finalRotation = Quaternion.RotateTowards(Quaternion.Euler(currentCharaOCI.guideObject.changeAmount.rot), lookRotation, Time.deltaTime * 60f * 10f);
                        currentCharaOCI.guideObject.changeAmount.rot = finalRotation.eulerAngles;
                    }
                    else
                    {
                        Vector3 forward = Vector3.Scale(currentCharaInfo.transform.forward, new Vector3(1f, 0f, 1f)).normalized;
                        Vector3 lookDirection = forward * -stick.y;
                        lookDirection = new Vector3(lookDirection.x, 0f, lookDirection.z);
                        currentCharaOCI.guideObject.changeAmount.pos += lookDirection * Time.deltaTime * (animSpeed * animMoveSets[animMoveSetCurrent].speedMult);
                        currentCharaOCI.guideObject.changeAmount.rot += new Vector3(0f, stick.x * Time.deltaTime * 60f * 100f, 0f);
                    }

                    Vector3 charaforward = Vector3.Scale(currentCharaInfo.transform.forward, new Vector3(1f, 0f, 1f)).normalized;
                    for(int i = 0; i < boobs.Count; i++)
                    {
                        boobs[i].Force = charaforward * stick.magnitude * 0.08f * (animMoveSets[animMoveSetCurrent].animSpeed / 2.5f);
                        //boobs[i].HeavyLoopMaxCount = 10;
                        //Utils.GetSecureField<List<DynamicBone_Ver02.Particle>, DynamicBone_Ver02>("Particles", boobs[i]).ForEach(x => x.Inert = 0.5f);
                    }
                }
            }
            else if(moving || animSwitched)
            {
                moving = false;
                currentCharaOCI.charInfo.animBody.runtimeAnimatorController = overrideController;
                currentCharaOCI.charInfo.animBody.CrossFadeInFixedTime(animMoveSets[animMoveSetCurrent].idle, 0.4f);
                currentCharaOCI.animeSpeed = 1f;
                for(int i = 0; i < boobs.Count; i++) boobs[i].Force = new Vector3();
            }
        }

        private void OverrideControllerCreate()
        {
            Studio.Info.AnimeLoadInfo infoBase = GetAnimeInfo(0, 0, 1);
            Studio.Info.AnimeLoadInfo infoWalk = GetAnimeInfo(1, 6, 0);
            Studio.Info.AnimeLoadInfo infoGentle = GetAnimeInfo(2, 10, 0);
            Studio.Info.AnimeLoadInfo infoActive = GetAnimeInfo(2, 17, 7);
            Studio.Info.AnimeLoadInfo infoEnergetic = GetAnimeInfo(2, 16, 3);
            RuntimeAnimatorController controllerBase = CommonLib.LoadAsset<RuntimeAnimatorController>(infoBase.bundlePath, infoBase.fileName);
            RuntimeAnimatorController controllerWalk = CommonLib.LoadAsset<RuntimeAnimatorController>(infoWalk.bundlePath, infoWalk.fileName);
            RuntimeAnimatorController controllerGentle = CommonLib.LoadAsset<RuntimeAnimatorController>(infoGentle.bundlePath, infoGentle.fileName);
            RuntimeAnimatorController controllerActive = CommonLib.LoadAsset<RuntimeAnimatorController>(infoActive.bundlePath, infoActive.fileName);
            RuntimeAnimatorController controllerEnergetic = CommonLib.LoadAsset<RuntimeAnimatorController>(infoEnergetic.bundlePath, infoEnergetic.fileName);

            overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = controllerBase;
            overrideController[animMoveSets[0].move] = controllerWalk.animationClips[0];
            overrideController[animMoveSets[1].idle] = controllerGentle.animationClips[0];
            overrideController[animMoveSets[1].move] = controllerWalk.animationClips[1];
            overrideController[animMoveSets[2].idle] = controllerActive.animationClips[7];
            overrideController[animMoveSets[2].move] = controllerWalk.animationClips[3];
            overrideController[animMoveSets[3].idle] = controllerEnergetic.animationClips[5];
        }

        private Studio.Info.AnimeLoadInfo GetAnimeInfo(int group, int category, int no)
        {
            Dictionary<int, Dictionary<int, Studio.Info.AnimeLoadInfo>> dictionary = null;
            if(!Singleton<Studio.Info>.Instance.dicFemaleAnimeLoadInfo.TryGetValue(group, out dictionary)) return null;

            Dictionary<int, Studio.Info.AnimeLoadInfo> dictionary2 = null;
            if(!dictionary.TryGetValue(category, out dictionary2)) return null;

            Studio.Info.AnimeLoadInfo animeLoadInfo = null;
            if(!dictionary2.TryGetValue(no, out animeLoadInfo)) return null;

            return animeLoadInfo;
        }
    }
}
