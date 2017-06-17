using System;
using System.Collections.Generic;
using System.IO;

namespace LockOnPluginUtilities
{
    public static class FileManager
    {
        private static string normalTargetNamesPath = Environment.CurrentDirectory + "\\Plugins\\LockOnPlugin\\normaltargets.txt";
        private static string customTargetNamesPath = Environment.CurrentDirectory + "\\Plugins\\LockOnPlugin\\customtargets.txt";
        private static string quickTargetNamesPath = Environment.CurrentDirectory + "\\Plugins\\LockOnPlugin\\quicktargets.txt";

        public static bool TargetSettingsExist()
        {
            if(File.Exists(normalTargetNamesPath) &&
            File.Exists(customTargetNamesPath) &&
            File.Exists(quickTargetNamesPath))
            {
                return true;
            }
            else
            {
                Console.WriteLine("Target settings are missing");
                return false;
            }
        }

        public static List<string> GetNormalTargetNames()
        {
            return GetTargetNames(normalTargetNamesPath);
        }

        public static List<string> GetQuickTargetNames()
        {
            return GetTargetNames(quickTargetNamesPath);
        }

        public static List<string[]> GetCustomTargetNames()
        {
            List<string[]> list = new List<string[]>();
            foreach(string item in GetTargetNames(customTargetNamesPath))
            {
                list.Add(item.Split('|'));
            }
            return list;
        }

        private static List<string> GetTargetNames(string filePath)
        {
            List<string> list = new List<string>();
            foreach(string item in File.ReadAllLines(filePath))
            {
                string line = StringUntil(item, "//");
                line = line.Replace(" ", "");
                if(line != "")
                {
                    list.Add(line);
                }
            }

            return list;
        }

        private static string StringUntil(string text, string stopAt)
        {
            if(text != null && stopAt != null)
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if(charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
                else if(charLocation == 0)
                {
                    return "";
                }
            }

            return text;
        }
    }
}