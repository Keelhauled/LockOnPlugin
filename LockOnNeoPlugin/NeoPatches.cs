using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using System.Reflection.Emit;

namespace LockOnPlugin
{
    internal static class NeoPatches
    {
        internal static void Init()
        {
            //HarmonyInstance.DEBUG = true;
            HarmonyInstance harmony = HarmonyInstance.Create("lockonplugin.neo");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    
    /// <summary>
    /// Prevents normal camera movement with keyboard if moveSpeed is 0f
    /// </summary>
    [HarmonyPatch(typeof(Studio.CameraControl))]
    [HarmonyPatch("InputKeyProc")]
    internal static class NeoPatch1
    {
        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator ilGenerator, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label label = ilGenerator.DefineLabel();
            bool codeFound = false;

            for(int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 275)
                {
                    FieldInfo field = AccessTools.Field(typeof(Studio.CameraControl), "moveSpeed");
                    CodeInstruction thisWithLabels = new CodeInstruction(OpCodes.Ldarg_0) { labels = codes[i].labels };
                    codes[i].labels = new List<Label>();

                    List<CodeInstruction> newCodes = new List<CodeInstruction>()
                    {
                        thisWithLabels,
                        new CodeInstruction(OpCodes.Ldfld, field),
                        new CodeInstruction(OpCodes.Ldc_R4, 0f),
                        new CodeInstruction(OpCodes.Ceq),
                        new CodeInstruction(OpCodes.Brtrue, label),
                    };

                    codes.InsertRange(i, newCodes);
                    i += newCodes.Count;
                    codeFound = true;
                }

                if(codeFound && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 10f)
                {
                    codes[i].labels.Add(label);
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
}
