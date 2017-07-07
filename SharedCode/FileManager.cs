using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LockOnPluginUtilities
{
    public static class FileManager
    {
        private static string quickFemaleTargetNamesPath = Environment.CurrentDirectory + "\\Plugins\\LockOnPlugin\\quicktargetsfemale.txt";
        private static string quickMaleTargetNamesPath = Environment.CurrentDirectory + "\\Plugins\\LockOnPlugin\\quicktargetsmale.txt";
        private static string normalTargetNamesPath = Environment.CurrentDirectory + "\\Plugins\\LockOnPlugin\\normaltargets.txt";
        private static string customTargetNamesPath = Environment.CurrentDirectory + "\\Plugins\\LockOnPlugin\\customtargets.txt";

        public static bool TargetSettingsExist()
        {
            if(File.Exists(quickFemaleTargetNamesPath) &&
            File.Exists(quickMaleTargetNamesPath) &&
            File.Exists(normalTargetNamesPath) &&
            File.Exists(customTargetNamesPath))
            {
                return true;
            }
            else
            {
                Console.WriteLine("Target settings are missing");
                return false;
            }
        }

        public static List<string> GetQuickFemaleTargetNames()
        {
            return GetTargetNames(quickFemaleTargetNamesPath);
        }

        public static List<string> GetQuickMaleTargetNames()
        {
            return GetTargetNames(quickMaleTargetNamesPath);
        }

        public static List<string> GetNormalTargetNames()
        {
            return GetTargetNames(normalTargetNamesPath);
        }

        public static List<List<string>> GetCustomTargetNames()
        {
            List<List<string>> list = new List<List<string>>();
            foreach(string item in GetTargetNames(customTargetNamesPath))
            {
                list.Add(item.Split('|').ToList());
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