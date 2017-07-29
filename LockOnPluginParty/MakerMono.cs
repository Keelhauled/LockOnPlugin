using UnityEngine;
using UnityEngine.UI;
using Manager;

namespace LockOnPlugin
{
    internal partial class MakerMono : LockOnBase
    {
        private CameraControl camera => Singleton<CameraControl>.Instance;
        private CustomControl customControl => Singleton<CustomControl>.Instance;

        protected override void Start()
        {
            base.Start();

            MakerPatches.Init();
            currentCharaInfo = customControl.chainfo;
            targetManager.UpdateAllTargets(currentCharaInfo);
        }
    }
}
