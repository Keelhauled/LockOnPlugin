using System;

namespace LockOnPlugin
{
    internal partial class MakerMono : LockOnBase
    {
        protected override void Start()
        {
            base.Start();
            
            try { MakerPatches.Init(); }
            catch(Exception ex) { Console.WriteLine(ex); }
            currentCharaInfo = Singleton<CustomControl>.Instance.chainfo;
            targetManager.UpdateAllTargets(currentCharaInfo);
        }
    }
}
