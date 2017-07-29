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
            var codes = new List<CodeInstruction>(instructions);
            codes[181].operand = 0f;
            return codes.AsEnumerable();
        }
    }
}
