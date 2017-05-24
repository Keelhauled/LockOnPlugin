using UnityEngine;

class IntersectionTarget
{
    public string name;
    private GameObject target;
    private GameObject point1;
    private GameObject point2;

    public IntersectionTarget(string newName, GameObject newPoint1, GameObject newPoint2)
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