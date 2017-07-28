using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using System.IO;

namespace LockOnPlugin
{
    internal static class NeoPatches
    {
        internal static void Init()
        {
            var harmony = HarmonyInstance.Create("lockonplugin.neo");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void Log(string msg)
        {
            string path = Environment.CurrentDirectory + "\\Plugins\\";
            StreamWriter sw = File.AppendText(path + "LOP.txt");
            try
            {
                sw.WriteLine(msg);
            }
            finally
            {
                sw.Close();
            }
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

        //private static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        //{
        //    var codes = new List<CodeInstruction>(instructions);

        //    for(int i = 0; i < codes.Count; i++)
        //    {
        //        NeoPatches.Log(i + " = " + codes[i].ToString());
        //    }

        //    return codes.AsEnumerable();
        //}
    }
}
