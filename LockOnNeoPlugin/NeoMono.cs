using IllusionUtility.GetUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.UI;
using Studio;
using Manager;
using LockOnPluginUtilities;

namespace LockOnPlugin
{
    internal partial class NeoMono : LockOnBase
    {
        private Studio.Studio studio => Singleton<Studio.Studio>.Instance;
        private Studio.CameraControl camera => studio.cameraCtrl;
        private TreeNodeCtrl treeNodeCtrl => studio.treeNodeCtrl;

        private Studio.CameraControl.CameraData cameraData;
        private OCIChar currentCharaOCI;

        protected override void Start()
        {
            base.Start();
            
            cameraData = GetSecureField<Studio.CameraControl.CameraData, Studio.CameraControl>("cameraData", camera);
            treeNodeCtrl.onSelect += new Action<TreeNodeObject>(OnSelectWork);
            studio.onDelete += new Action<ObjectCtrlInfo>(OnDeleteWork);
            Transform systemMenuContent = studio.gameObject.transform.Find("Canvas Main Menu/04_System/Viewport/Content");
            systemMenuContent.Find("Load").GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OnSceneMenuOpen()));
            systemMenuContent.Find("End").GetComponent<Button>().onClick.AddListener(() => showLockOnTargets = false);
            systemMenuContent.Find("Option").GetComponent<Button>().onClick.AddListener(InstallNearClipPlaneSlider);
            StartCoroutine(InstallSettingsReloadButton());
        }

        protected override void LoadSettings()
        {
            base.LoadSettings();

            manageCursorVisibility = false;
            infoMsgPosition = new Vector2(1.0f, 1.0f);
            Camera.main.nearClipPlane = ModPrefs.GetFloat("LockOnPlugin.Misc", "NearClipPlane", Camera.main.nearClipPlane, true);
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
                            if(!LockOn(lockOnTarget.name, true))
                            {
                                LockOnRelease();
                            }
                        }

                        //foreach(var item in targetManager.GetAllTargets())
                        //{
                        //    Console.WriteLine(item.name);
                        //}
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
            if(PluginInstalled("HoneyStudioNEO Adddon"))
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
                if(studio.dicInfo.TryGetValue(nthNode, out objectCtrlInfo))
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
            Transform systemMenuContent = studio.gameObject.transform.Find("Canvas Main Menu/04_System/Viewport/Content");
            if(systemMenuContent && !systemMenuContent.Find("LockOnPluginReload"))
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
                    LockOn(lockOnTarget);
                    targetOffsetSize = Vector3.zero;
                });

                Console.WriteLine("LockOnPlugin reload button installed");
            }

            yield break;
        }

        private void InstallNearClipPlaneSlider()
        {
            GameObject sliderParentObject = GameObject.Find("Slider Camera Speed");
            if(sliderParentObject && !GameObject.Find("Slider NearClipPlane"))
            {
                GameObject nearClipSlider = Instantiate(sliderParentObject);
                nearClipSlider.name = "Slider NearClipPlane";
                nearClipSlider.transform.SetParent(sliderParentObject.transform);
                nearClipSlider.transform.position = sliderParentObject.transform.position + new Vector3(45.0f, 82.0f, 0.0f);

                Slider nearClipSliderComponent = nearClipSlider.GetComponent<Slider>();
                nearClipSliderComponent.maxValue = 0.060f;
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

            GameObject textParentObject = GameObject.Find("Text Camera Speed");
            if(sliderParentObject && textParentObject && !GameObject.Find("Text NearClipPlane"))
            {
                GameObject nearClipText = Instantiate(textParentObject);
                nearClipText.name = "Text NearClipPlane";
                nearClipText.transform.SetParent(sliderParentObject.transform);
                nearClipText.transform.position = sliderParentObject.transform.position + new Vector3(0.0f, 82.0f, 0.0f);
                Text nearClipTextComponent = nearClipText.GetComponent<Text>();
                nearClipTextComponent.text = "NearClip";
                Console.WriteLine("NearClipPlane text installed");
            }
        }
    }
}
