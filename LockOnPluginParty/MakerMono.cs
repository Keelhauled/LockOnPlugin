namespace LockOnPlugin
{
    internal partial class MakerMono : LockOnBase
    {
        protected override void Start()
        {
            base.Start();

            currentCharaInfo = Singleton<CustomControl>.Instance.chainfo;
            targetManager.UpdateAllTargets(currentCharaInfo);
        }
    }
}
