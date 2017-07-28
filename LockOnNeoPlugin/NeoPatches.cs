using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
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
    internal class StudioCameraControl_InputKeyProc_Patch
    {
        private static bool Prefix()
        {
            return false;
        }

        //private static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        //{
        //    var codes = new List<CodeInstruction>(instructions);

        //    for(int i = 0; i < codes.Count; i++)
        //    {
        //        LockOnBase.Log("InputKeyProc_Transpiler.txt", i + " = " + codes[i].ToString());
        //    }

        //    return codes.AsEnumerable();
        //}
    }
}
