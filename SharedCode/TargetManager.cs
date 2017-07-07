using IllusionUtility.GetUtility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LockOnPluginUtilities
{
    public class CameraTargetManager
    {
        private List<GameObject> allTargets = new List<GameObject>();
        private List<GameObject> allTargetsMultiple = new List<GameObject>();
        private List<GameObject> normalTargets = new List<GameObject>();
        private List<CustomTarget> customTargets = new List<CustomTarget>();

        public List<GameObject> GetAllTargets()
        {
            return allTargets;
        }

        public List<GameObject> GetAllTargetsMultiple()
        {
            return allTargetsMultiple;
        }

        public void UpdateCustomTargetTransforms()
        {
            foreach(CustomTarget target in customTargets)
            {
                target.UpdateTargetTransform();
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

            foreach(string targetName in FileManager.GetNormalTargetNames())
            {
                GameObject bone = character.chaBody.objBone.transform.FindLoop(prefix + targetName);
                if(bone) normalTargets.Add(bone);
            }

            return normalTargets;
        }

        private List<CustomTarget> UpdateCustomTargets(CharInfo character, string prefix)
        {
            List<CustomTarget> customTargets = new List<CustomTarget>();

            foreach(List<string> data in FileManager.GetCustomTargetNames())
            {
                GameObject point1 = character.chaBody.objBone.transform.FindLoop(prefix + data[1]);
                GameObject point2 = character.chaBody.objBone.transform.FindLoop(prefix + data[2]);
                if(point1 && point2)
                {
                    float midpoint = 0.5f;
                    if(data.ElementAtOrDefault(3) != null)
                    {
                        if(!float.TryParse(data[3], out midpoint))
                        {
                            midpoint = 0.5f;
                        }
                    }

                    CustomTarget target = new CustomTarget(data[0], point1, point2, midpoint);
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
        private float midpoint;

        public CustomTarget(string newName, GameObject newPoint1, GameObject newPoint2, float newMidpoint = 0.5f)
        {
            name = newName;
            target = new GameObject(name);

            point1 = newPoint1;
            point2 = newPoint2;
            midpoint = newMidpoint;

            UpdateTargetTransform();
        }

        public void UpdateTargetTransform()
        {
            UpdateTargetPosition();
            UpdateTargetAngle();
        }

        private void UpdateTargetPosition()
        {
            Vector3 pos1 = point1.transform.position;
            Vector3 pos2 = point2.transform.position;
            target.transform.position = Vector3.Lerp(pos1, pos2, midpoint);
        }

        private void UpdateTargetAngle()
        {
            Quaternion rot1 = point1.transform.rotation;
            Quaternion rot2 = point2.transform.rotation;
            target.transform.rotation = Quaternion.Slerp(rot1, rot2, 0.5f);
        }

        public GameObject GetTarget() => target;
    }
}
