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

        private Studio.CameraControl.CameraData cameraData;
        private Studio.CameraControl.CameraData cameraReset;
        private OCIChar currentCharaOCI;

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
            NeoPatches.Init();
        }

        protected override void LoadSettings()
        {
            base.LoadSettings();

            manageCursorVisibility = false;
            infoMsgPosition = new Vector2(1.0f, 1.0f);
            float nearClipPlane = ModPrefs.GetFloat("LockOnPlugin.Misc", "NearClipPlane", Camera.main.nearClipPlane, true);
            Camera.main.nearClipPlane = nearClipPlane;
            GameObject nearClipSlider = GameObject.Find("Slider NearClipPlane");
            if(nearClipSlider) nearClipSlider.GetComponent<Slider>().value = nearClipPlane;
        }

        protected override void Update()
        {
            base.Update();

            InputKeyProc();
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
            Transform systemMenuContent = studio.transform.Find("Canvas Main Menu/04_System/Viewport/Content");
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
            Transform sliderParentObject = studio.transform.Find("Canvas Main Menu/04_System/02_Option/Slider Camera Speed");
            if(sliderParentObject && !GameObject.Find("Slider NearClipPlane"))
            {
                GameObject nearClipSlider = Instantiate(sliderParentObject.gameObject);
                nearClipSlider.name = "Slider NearClipPlane";
                nearClipSlider.transform.SetParent(sliderParentObject.parent);
                nearClipSlider.transform.localPosition = new Vector3(114.0f, -16.0f, 0.0f);
                nearClipSlider.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);

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

            Transform textParentObject = studio.transform.Find("Canvas Main Menu/04_System/02_Option/Text Camera Speed");
            if(sliderParentObject && textParentObject && !GameObject.Find("Text NearClipPlane"))
            {
                GameObject nearClipText = Instantiate(textParentObject.gameObject);
                nearClipText.name = "Text NearClipPlane";
                nearClipText.transform.SetParent(sliderParentObject.parent);
                nearClipText.transform.localPosition = new Vector3(83.0f, -16.0f, 0.0f);
                nearClipText.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
                Text nearClipTextComponent = nearClipText.GetComponent<Text>();
                nearClipTextComponent.text = "NearClip";
                Console.WriteLine("NearClipPlane text installed");
            }
        }

        protected virtual bool InputKeyProc()
        {
            bool flag = false;
            if(Input.GetKeyDown(KeyCode.A))
            {
                camera.Reset(0);
            }
            else if(Input.GetKeyDown(KeyCode.Keypad5))
            {
                cameraData.rotate.x = cameraReset.rotate.x;
                cameraData.rotate.y = cameraReset.rotate.y;
            }
            else if(Input.GetKeyDown(KeyCode.Slash))
            {
                cameraData.rotate.z = 0f;
            }
            else if(Input.GetKeyDown(KeyCode.Semicolon))
            {
                camera.fieldOfView = cameraReset.parse;
            }
            float deltaTime = Time.deltaTime;
            if(Input.GetKey(KeyCode.Home))
            {
                flag = true;
                cameraData.distance.z = cameraData.distance.z + deltaTime;
                cameraData.distance.z = Mathf.Min(0f, cameraData.distance.z);
            }
            else if(Input.GetKey(KeyCode.End))
            {
                flag = true;
                cameraData.distance.z = cameraData.distance.z - deltaTime;
            }

            if(!lockOnTarget)
            {
                if(Input.GetKey(KeyCode.RightArrow))
                {
                    flag = true;
                    if(camera.transBase != null)
                    {
                        cameraData.pos += camera.transBase.InverseTransformDirection(Camera.main.transform.TransformDirection(new Vector3(deltaTime, 0f, 0f)));
                    }
                    else
                    {
                        cameraData.pos += Camera.main.transform.TransformDirection(new Vector3(deltaTime, 0f, 0f));
                    }
                }
                else if(Input.GetKey(KeyCode.LeftArrow))
                {
                    flag = true;
                    if(camera.transBase != null)
                    {
                        cameraData.pos += camera.transBase.InverseTransformDirection(Camera.main.transform.TransformDirection(new Vector3(-deltaTime, 0f, 0f)));
                    }
                    else
                    {
                        cameraData.pos += Camera.main.transform.TransformDirection(new Vector3(-deltaTime, 0f, 0f));
                    }
                }
                if(Input.GetKey(KeyCode.UpArrow))
                {
                    flag = true;
                    if(camera.transBase != null)
                    {
                        cameraData.pos += camera.transBase.InverseTransformDirection(Camera.main.transform.TransformDirection(new Vector3(0f, 0f, deltaTime)));
                    }
                    else
                    {
                        cameraData.pos += Camera.main.transform.TransformDirection(new Vector3(0f, 0f, deltaTime));
                    }
                }
                else if(Input.GetKey(KeyCode.DownArrow))
                {
                    flag = true;
                    if(camera.transBase != null)
                    {
                        cameraData.pos += camera.transBase.InverseTransformDirection(Camera.main.transform.TransformDirection(new Vector3(0f, 0f, -deltaTime)));
                    }
                    else
                    {
                        cameraData.pos += Camera.main.transform.TransformDirection(new Vector3(0f, 0f, -deltaTime));
                    }
                }
                if(Input.GetKey(KeyCode.PageUp))
                {
                    flag = true;
                    cameraData.pos.y = cameraData.pos.y + deltaTime;
                }
                else if(Input.GetKey(KeyCode.PageDown))
                {
                    flag = true;
                    cameraData.pos.y = cameraData.pos.y - deltaTime;
                }
            }

            float num = 10f * Time.deltaTime;
            Vector3 zero = Vector3.zero;
            if(Input.GetKey(KeyCode.Period))
            {
                flag = true;
                zero.z += num;
            }
            else if(Input.GetKey(KeyCode.Backslash))
            {
                flag = true;
                zero.z -= num;
            }
            if(Input.GetKey(KeyCode.Keypad2))
            {
                flag = true;
                zero.x -= num * camera.yRotSpeed;
            }
            else if(Input.GetKey(KeyCode.Keypad8))
            {
                flag = true;
                zero.x += num * camera.yRotSpeed;
            }
            if(Input.GetKey(KeyCode.Keypad4))
            {
                flag = true;
                zero.y += num * camera.xRotSpeed;
            }
            else if(Input.GetKey(KeyCode.Keypad6))
            {
                flag = true;
                zero.y -= num * camera.xRotSpeed;
            }
            if(flag)
            {
                cameraData.rotate.y = (cameraData.rotate.y + zero.y) % 360f;
                cameraData.rotate.x = (cameraData.rotate.x + zero.x) % 360f;
                cameraData.rotate.z = (cameraData.rotate.z + zero.z) % 360f;
            }
            float deltaTime2 = Time.deltaTime;
            if(Input.GetKey(KeyCode.Equals))
            {
                flag = true;
                camera.fieldOfView = Mathf.Max(cameraData.parse - deltaTime2 * 15f, 1f);
            }
            else if(Input.GetKey(KeyCode.RightBracket))
            {
                flag = true;
                camera.fieldOfView = Mathf.Min(cameraData.parse + deltaTime2 * 15f, camera.limitFov);
            }
            return flag;
        }

        //public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        //{
        //    //var codes = new List<CodeInstruction>();
        //    var ilinstructions = MethodBodyReader.GetInstructions(typeof(LockOnBase).GetMethod("TestProper", BindingFlags.NonPublic | BindingFlags.Instance));
        //    //var ilinstructions = MethodBodyReader.GetInstructions(typeof(LockOnBase).GetMethod("TestReplacer"));
        //    //for(int i = 0; i < ilinstructions.Count; i++)
        //    //{
        //    //    codes.Add(ilinstructions[i].GetCodeInstruction());
        //    //}
        //    var codetrans = new CodeTranspiler(ilinstructions);
        //    var results = codetrans.GetResult(generator, typeof(LockOnBase).GetMethod("TestProper", BindingFlags.NonPublic | BindingFlags.Instance));
        //    return results;
        //    //return codes.AsEnumerable();
        //}
    }
}
