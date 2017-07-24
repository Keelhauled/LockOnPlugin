using System.Reflection;
using Harmony;

namespace LockOnPlugin
{
    internal static class NeoPatches
    {
        internal static void Init()
        {
            var harmony = HarmonyInstance.Create("lockonplugin.neo");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Studio.CameraControl))]
    [HarmonyPatch("InputKeyProc")]
    internal class InputKeyProc_Patch
    {
        private static bool Prefix()
        {
            return false;
        }
    }
}
