using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurveCreator))]
public class CurveEditor : Editor
{
    CurveCreator curveCreator;
    BezierCurve bezierCurve;

    const float segmentSelectDistanceThreshold = .1f;
    int selectedSegment = -1;

    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create new")) 
        {
            Undo.RecordObject(curveCreator, "Create new");
            curveCreator.CreateCurve();
            bezierCurve = curveCreator.bezierCurve;
        }

        bool isClosed = GUILayout.Toggle(bezierCurve.IsClosed, "Toggle Close");
        if (isClosed != bezierCurve.IsClosed) 
        {
            Undo.RecordObject(curveCreator, "Toggle close");
            bezierCurve.IsClosed = isClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(bezierCurve.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != bezierCurve.AutoSetControlPoints) 
        {
            Undo.RecordObject(curveCreator , "Toggle Auto Set Control");
            bezierCurve.AutoSetControlPoints = autoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

    }

    void OnEnable()
    {
        // curveCreator = target as CurveCreator;
        curveCreator = (CurveCreator)target;
        if (curveCreator.bezierCurve == null)
        {
            curveCreator.CreateCurve();
        }
        bezierCurve = curveCreator.bezierCurve;
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        // shift + left-click to add new point to curve
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(curveCreator, "Add Segment");
            bezierCurve.AddSegment(mousePos); 
        }

        // right click to delete
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistToAnchor = .5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < bezierCurve.NumberOfPoints; i+=3)
            {
                float dst = Vector2.Distance(mousePos, bezierCurve[i]);
                if (dst < minDistToAnchor)
                {
                    minDistToAnchor = dst;
                    closestAnchorIndex = i;
                }
            }
            if (closestAnchorIndex != -1) 
            {
                Undo.RecordObject(curveCreator, "Delete Segment");
                bezierCurve.DeleteSegment(closestAnchorIndex);
            }
        }
        float minDistanceToSegment = segmentSelectDistanceThreshold;
        int newSelectedSegmentIndex = -1;

        for (int i = 0; i < bezierCurve.NumberOfSegments; i++)
        {
            Vector2[] points = bezierCurve.GetPointsInSegment(i);
            float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);

            if (dst < minDistanceToSegment)
            {
                minDistanceToSegment = dst;
                newSelectedSegmentIndex = i;
            }
        }
    }

    void Draw()
    {
        for (int i = 0; i < bezierCurve.NumberOfSegments; i++)
        {
            Vector2[] points = bezierCurve.GetPointsInSegment(i);

            // draw straight line between segments
            Handles.color = Color.black;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);

            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
        }

        for (int i = 0; i < bezierCurve.NumberOfPoints; i++)
        {
            Handles.color = (i % 3 == 0) ? Color.red : Color.yellow;
            Vector2 newPosition = Handles.FreeMoveHandle(bezierCurve[i], Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);
            if (bezierCurve[i] != newPosition)
            {
                Undo.RecordObject(curveCreator, "Move Point");
                bezierCurve.MovePoint(i, newPosition);
            }
        }
    }
}
