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
        private CenterPoint centerPoint = null;

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

            if(centerPoint != null && centerPoint.GetCenterPoint()) centerPoint.UpdateCenterPointPosition();
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

                centerPoint = UpdateCenterPoint(character, prefix);
                if(centerPoint != null && centerPoint.GetCenterPoint()) allTargets.Add(centerPoint.GetCenterPoint());
            }
            else
            {
                allTargets = new List<GameObject>();
                normalTargets = new List<GameObject>();
                customTargets = new List<CustomTarget>();
                centerPoint = null;
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

        private CenterPoint UpdateCenterPoint(CharInfo character, string prefix)
        {
            Dictionary<GameObject, float> points = new Dictionary<GameObject, float>();

            foreach(List<string> data in FileManager.GetCenterTargetWeights())
            {
                GameObject point = character.chaBody.objBone.transform.FindLoop(prefix + data[0]);
                float weight = 1.0f;
                if(!float.TryParse(data[1], out weight))
                {
                    weight = 1.0f;
                }
                points.Add(point, weight);
            }

            CenterPoint centerPoint = new CenterPoint(points);
            if(centerPoint.GetCenterPoint()) return centerPoint;
            return null;
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

        public GameObject GetTarget() => target;

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
    }

    internal class CenterPoint
    {
        private Dictionary<GameObject, float> points = new Dictionary<GameObject, float>();
        private GameObject centerPoint = null;

        public CenterPoint(Dictionary<GameObject, float> newPoints)
        {
            if(newPoints.Count > 0)
            {
                centerPoint = new GameObject("CenterPoint");
                points = newPoints;
                UpdateCenterPointPosition();
            }
            else
            {
                centerPoint = null;
            }
        }

        public GameObject GetCenterPoint() => centerPoint;

        public void UpdateCenterPointPosition()
        {
            centerPoint.transform.position = GetCenterPoint(points);
        }

        private Vector3 GetCenterPoint(Dictionary<GameObject, float> points)
        {
            Vector3 center = new Vector3(0, 0, 0);
            //int count = 0;
            float totalWeight = 0.0f;
            foreach(KeyValuePair<GameObject, float> point in points)
            {
                center += point.Key.transform.position * point.Value;
                //count++;
                totalWeight += point.Value;
            }
            //return center / count;
            return center / totalWeight;
        }
    }
}
