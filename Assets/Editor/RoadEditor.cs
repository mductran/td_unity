using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathCreator))]
public class RoadEditor : Editor
{
    PathCreator pathCreator;

    void OnSceneGUI()
    {
        if (pathCreator.autoUpdate && Event.current.type == EventType.Repaint)
        {
            pathCreator.UpdateRoad();
        }
    }

    void OnEnable()
    {
        pathCreator = (PathCreator)target;
    }
}
