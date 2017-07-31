using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Harmony;

namespace LockOnPlugin
{
    internal static class MakerPatches
    {
        internal static void Init()
        {
            HarmonyInstance.DEBUG = true;
            var harmony = HarmonyInstance.Create("lockonplugin.maker");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(CustomControl))]
    [HarmonyPatch("Update")]
    internal class CustomControl_Update_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            //change this into yield return loop
            var codes = new List<CodeInstruction>(instructions);
            if((float)codes[181].operand == 0.01f) codes[181].operand = 0f;
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(BaseCameraControl))]
    [HarmonyPatch("InputKeyProc")]
    internal class BaseCameraControl_InputKeyProc_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            return codes.AsEnumerable();
        }
    }
}
