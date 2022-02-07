using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurveCreator))]
public class CurveEditor : Editor
{
    CurveCreator curveCreator;
    BezierCurve BezierCurve
    {
        get
        {
            return curveCreator.bezierCurve;
        }
    }

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
            // bezierCurve = curveCreator.bezierCurve;
        }

        bool isClosed = GUILayout.Toggle(BezierCurve.IsClosed, "Closed");
        if (isClosed != BezierCurve.IsClosed)
        {
            Undo.RecordObject(curveCreator, "Toggle close");
            BezierCurve.IsClosed = isClosed;
        }

        bool displayControlPoints = GUILayout.Toggle(BezierCurve.DisplayControlPoints, "Display Control Points");
        if (displayControlPoints != BezierCurve.DisplayControlPoints)
        {
            Undo.RecordObject(curveCreator, "Display Control Points");
            BezierCurve.DisplayControlPoints = displayControlPoints;
        }

        bool autoSetControlPoints = GUILayout.Toggle(BezierCurve.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != BezierCurve.AutoSetControlPoints)
        {
            Undo.RecordObject(curveCreator, "Toggle Auto Set Control");
            BezierCurve.AutoSetControlPoints = autoSetControlPoints;
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
        // bezierCurve = curveCreator.bezierCurve;
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
            if (selectedSegment != -1)
            {
                Undo.RecordObject(curveCreator, "Split Segment");
                BezierCurve.SplitSegment(mousePos, selectedSegment);
            }
            else if (!BezierCurve.IsClosed)
            {
                Undo.RecordObject(curveCreator, "Add Segment");
                BezierCurve.AddSegment(mousePos);
            }
        }

        // right click to delete
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistToAnchor = .5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < BezierCurve.NumberOfPoints; i += 3)
            {
                float dst = Vector2.Distance(mousePos, BezierCurve[i]);
                if (dst < minDistToAnchor)
                {
                    minDistToAnchor = dst;
                    closestAnchorIndex = i;
                }
            }
            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(curveCreator, "Delete Segment");
                BezierCurve.DeleteSegment(closestAnchorIndex);
            }
        }

        if (guiEvent.type == EventType.MouseMove)
        {
            float minDistanceToSegment = segmentSelectDistanceThreshold;
            int newSelectedSegmentIndex = -1;

            for (int i = 0; i < BezierCurve.NumberOfSegments; i++)
            {
                Vector2[] points = BezierCurve.GetPointsInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);

                if (dst < minDistanceToSegment)
                {
                    minDistanceToSegment = dst;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != selectedSegment)
            {
                selectedSegment = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }

        HandleUtility.AddDefaultControl(0);
    }

    void Draw()
    {
        for (int i = 0; i < BezierCurve.NumberOfSegments; i++)
        {
            Vector2[] points = BezierCurve.GetPointsInSegment(i);

            // draw straight line between segments
            if (i % 3 == 0 || BezierCurve.DisplayControlPoints)
            {
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }
            Color segmentColour = (i == selectedSegment && Event.current.shift) ? Color.cyan : Color.green;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColour, null, 2);
        }

        for (int i = 0; i < BezierCurve.NumberOfPoints; i++)
        {
            if (i % 3 == 0 || BezierCurve.DisplayControlPoints)
            {
                Handles.color = (i % 3 == 0) ? Color.red : Color.yellow;
                Vector2 newPosition = Handles.FreeMoveHandle(BezierCurve[i], Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);
                if (BezierCurve[i] != newPosition)
                {
                    Undo.RecordObject(curveCreator, "Move Point");
                    BezierCurve.MovePoint(i, newPosition);
                }
            }
        }
    }
}
