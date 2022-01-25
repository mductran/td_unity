using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurveCreator))]
public class CurveEditor : Editor
{
    CurveCreator curveCreator;
    BezierCurve bezierCurve;

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

        if (GUILayout.Button("Toggle closed")) 
        {
            Undo.RecordObject(curveCreator, "Toggle close");
            bezierCurve.ToggleClose();
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

        Handles.color = Color.red;
        for (int i = 0; i < bezierCurve.NumberOfPoints; i++)
        {
            Vector2 newPosition = Handles.FreeMoveHandle(bezierCurve[i], Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);
            if (bezierCurve[i] != newPosition)
            {
                Undo.RecordObject(curveCreator, "Move Point");
                bezierCurve.MovePoint(i, newPosition);
            }
        }
    }
}
