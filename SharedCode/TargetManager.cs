using IllusionUtility.GetUtility;
using System.Collections.Generic;
using UnityEngine;

namespace LockOnPluginUtilities
{
    public class CameraTargetManager
    {
        private List<GameObject> allTargets = new List<GameObject>();
        private List<GameObject> allTargetsMultiple = new List<GameObject>();
        private List<GameObject> normalTargets = new List<GameObject>();
        private List<CustomTarget> customTargets = new List<CustomTarget>();

        private List<string> normalTargetNames;
        private List<string[]> customTargetNames;

        public CameraTargetManager()
        {
            normalTargetNames = FileManager.GetNormalTargetNames();
            customTargetNames = FileManager.GetCustomTargetNames();
        }

        public List<GameObject> GetAllTargets()
        {
            return allTargets;
        }

        public List<GameObject> GetAllTargetsMultiple()
        {
            return allTargetsMultiple;
        }

        public void UpdateCustomTargetPositions()
        {
            foreach(CustomTarget target in customTargets)
            {
                target.UpdateTargetPosition();
            }
        }

        public void UpdateAllTargets(CharInfo character)
        {
            if(character)
            {
                string prefix = character is CharFemale ? "cf_" : "cm_";

                normalTargets = UpdateNormalTargets(character, prefix);
                customTargets = UpdateCustomTargets(character, prefix);
                allTargets = normalTargets;

                foreach(CustomTarget target in customTargets)
                {
                    allTargets.Add(target.GetTarget());
                }
            }
            else
            {
                allTargets = new List<GameObject>();
                normalTargets = new List<GameObject>();
                customTargets = new List<CustomTarget>();
            }
        }

        public void UpdateAllTargetsMultiple(List<CharInfo> characters)
        {
            if(characters == null)
            {
                this.allTargetsMultiple = null;
                return;
            }

            List<List<GameObject>> allTargetsMultiple = new List<List<GameObject>>();

            foreach(CharInfo character in characters)
            {
                string prefix = character is CharFemale ? "cf_" : "cm_";

                List<GameObject> normalTargets = UpdateNormalTargets(character, prefix);
                List<CustomTarget> customTargets = UpdateCustomTargets(character, prefix);
                List<GameObject> allTargets = normalTargets;

                foreach(CustomTarget target in customTargets)
                {
                    allTargets.Add(target.GetTarget());
                }

                allTargetsMultiple.Add(allTargets);
            }

            List<GameObject> allTargetsNew = new List<GameObject>();
            for(int i = 0; i < allTargetsMultiple.Count; i++)
            {
                allTargetsNew.AddRange(allTargetsMultiple[i]);
            }

            this.allTargetsMultiple = allTargetsNew;
        }

        private List<GameObject> UpdateNormalTargets(CharInfo character, string prefix)
        {
            List<GameObject> normalTargets = new List<GameObject>();

            foreach(string targetName in normalTargetNames)
            {
                GameObject bone = character.chaBody.objBone.transform.FindLoop(prefix + targetName);
                if(bone) normalTargets.Add(bone);
            }

            return normalTargets;
        }

        private List<CustomTarget> UpdateCustomTargets(CharInfo character, string prefix)
        {
            List<CustomTarget> customTargets = new List<CustomTarget>();

            foreach(string[] data in customTargetNames)
            {
                GameObject point1 = character.chaBody.objBone.transform.FindLoop(prefix + data[1]);
                GameObject point2 = character.chaBody.objBone.transform.FindLoop(prefix + data[2]);
                if(point1 && point2)
                {
                    CustomTarget target = new CustomTarget(data[0], point1, point2);
                    customTargets.Add(target);
                }
            }

            return customTargets;
        }
    }

    internal class CustomTarget
    {
        private string name;
        private GameObject target;
        private GameObject point1;
        private GameObject point2;

        public CustomTarget(string newName, GameObject newPoint1, GameObject newPoint2)
        {
            name = newName;
            target = new GameObject(name);

            point1 = newPoint1;
            point2 = newPoint2;

            UpdateTargetPosition();
        }

        public void UpdateTargetPosition()
        {
            Vector3 pos1 = point1.transform.position;
            Vector3 pos2 = point2.transform.position;
            target.transform.position = Vector3.Lerp(pos1, pos2, 0.5f);
        }

        public GameObject GetTarget() => target;
    }
}