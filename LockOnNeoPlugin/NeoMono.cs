using System;
using System.Collections.Generic;
using UnityEngine;
using Studio;

namespace LockOnPlugin
{
    internal partial class NeoMono : LockOnBase
    {
        private Studio.CameraControl camera;
        private Studio.CameraControl.CameraData cameraData;
        private OCIChar currentCharaOCI;

        protected override void Start()
        {
            camera = Singleton<Studio.Studio>.Instance.cameraCtrl;
            cameraData = GetSecureField<Studio.CameraControl, Studio.CameraControl.CameraData>("cameraData");
            Singleton<Studio.Studio>.Instance.treeNodeCtrl.onSelect += new Action<TreeNodeObject>(OnSelectWork);
            Singleton<Studio.Studio>.Instance.onDelete += new Action<ObjectCtrlInfo>(OnDeleteWork);

            base.Start();

            manageCursorVisibility = false;
        }

        protected override void Update()
        {
            base.Update();

            prevCharaHotkey.KeyDownAction(() => CharaSwitch(false, scrollThroughMalesToo));
            nextCharaHotkey.KeyDownAction(() => CharaSwitch(true, scrollThroughMalesToo));
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if(showInfoMsg && guiTimeInfo > 0.0f)
            {
                DebugGUI(1.0f, 0.0f, 200f, 50f, infoMsg);
                guiTimeInfo -= Time.deltaTime;
            }
        }

        private void OnSelectWork(TreeNodeObject _node)
        {
            ObjectCtrlInfo objectCtrlInfo = null;
            if(Singleton<Studio.Studio>.Instance.dicInfo.TryGetValue(_node, out objectCtrlInfo))
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

            showLockOnTargets = false;
            currentCharaOCI = null;
            currentCharaInfo = null;
            targetManager.UpdateAllTargets(null);
            LockOnRelease();
        }

        private void OnDeleteWork(ObjectCtrlInfo _info)
        {
            ObjectCtrlInfo objectCtrlInfo = _info;
            
            if(objectCtrlInfo.kind == 0)
            {
                OCIChar ocichar = objectCtrlInfo as OCIChar;

                if(ocichar == currentCharaOCI)
                {
                    showLockOnTargets = false;
                    currentCharaOCI = null;
                    currentCharaInfo = null;
                    targetManager.UpdateAllTargets(null);
                    LockOnRelease();
                }
            }
        }

        protected override void LockOn()
        {
            TreeNodeCtrl nodeCtrl = Singleton<Studio.Studio>.Instance.treeNodeCtrl;

            if(!nodeCtrl.selectNode)
            {
                List<TreeNodeObject> femaleNodes = GetCharaNodes<OCICharFemale>();
                if(femaleNodes.Count > 0) nodeCtrl.SelectSingle(femaleNodes[0]);
            }

            base.LockOn();
        }
        
        private void CharaSwitch(bool scrollDown, bool malesToo)
        {
            TreeNodeCtrl nodeCtrl = Singleton<Studio.Studio>.Instance.treeNodeCtrl;
            List<TreeNodeObject> charaNodes = malesToo ? GetCharaNodes<OCIChar>() : GetCharaNodes<OCICharFemale>();

            if(!nodeCtrl.selectNode && charaNodes.Count > 0)
            {
                nodeCtrl.SelectSingle(charaNodes[0]);
            }
            else
            {
                for(int i = 0; i < charaNodes.Count; i++)
                {
                    if(charaNodes[i] == nodeCtrl.selectNode)
                    {
                        int next = i + 1 > charaNodes.Count - 1 ? 0 : i + 1;
                        if(!scrollDown) next = i - 1 < 0 ? charaNodes.Count - 1 : i - 1;
                        nodeCtrl.SelectSingle(charaNodes[next]);
                        break;
                    }
                }
            }
        }

        private List<TreeNodeObject> GetCharaNodes<CharaType>()
        {
            List<TreeNodeObject> charaNodes = new List<TreeNodeObject>();

            int n = 0; TreeNodeObject nthNode;
            while(nthNode = Singleton<Studio.Studio>.Instance.treeNodeCtrl.GetNode(n))
            {
                ObjectCtrlInfo objectCtrlInfo = null;
                if(Singleton<Studio.Studio>.Instance.dicInfo.TryGetValue(nthNode, out objectCtrlInfo))
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
    }
}