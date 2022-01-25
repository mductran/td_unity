using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BezierCurve
{
    [SerializeField, HideInInspector]
    private List<Vector2> points;

    [SerializeField, HideInInspector]
    bool isClosed;

    [SerializeField, HideInInspector]
    bool autoSetControlPoints;

    // generate a cubic curve
    public BezierCurve(Vector2 centre)
    {
        // 1 segment 4 points, 2 segments 7 points, 3 segments 10 points, ...
        points = new List<Vector2> {
            centre + Vector2.left,
            centre + Vector2.up + Vector2.left,
            centre + Vector2.right + Vector2.down,
            centre + Vector2.right
        };
    }

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            if (isClosed != value)
            {
                isClosed = value;

                if (isClosed)
                {
                    // add 2 anchor points to open curve
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]); // add opposite of last anchor points
                    points.Add(points[0] * 2 - points[1]); // add opposite of first control points

                    if (autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoints(0);
                        AutoSetAnchorControlPoints(points.Count - 3);
                    }
                }
                else
                {
                    // remove extra anchor points when open a closed curve
                    points.RemoveRange(points.Count - 2, 2);
                    if (autoSetControlPoints)
                    {
                        AutoSetStartAndEndControls();
                    }
                }
            }
        }
    }

    public int NumberOfSegments
    {
        get
        {
            return (points.Count) / 3;
        }
    }

    public bool AutoSetControlPoints
    {
        get
        {
            return autoSetControlPoints;
        }
        set
        {
            if (autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                if (autoSetControlPoints)
                {
                    AutoSetAllControlPoints();
                }
            }
        }
    }

    public int NumberOfPoints
    {
        get
        {
            return points.Count;
        }
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    // add new curve segment at mouse position
    public void AddSegment(Vector2 newPosition)
    {
        // two anchor points
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((newPosition + points[points.Count - 1]) * .5f);
        // control points at new position
        points.Add(newPosition);

        if (autoSetControlPoints)
        {
            AutoSetAffectedControlPoints(points.Count - 1);
        }
    }


    public void SplitSegment(Vector2 anchorPosition, int segmentIndex)
    {
        points.InsertRange(segmentIndex*3 + 2, new Vector2[] {Vector2.zero, anchorPosition, Vector2.zero});  
        if (autoSetControlPoints)
        {
            AutoSetAffectedControlPoints(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
        }
    }


    public Vector2[] GetPointsInSegment(int index)
    {
        return new Vector2[]
        {
            points[index*3],
            points[index*3 + 1],
            points[index*3 + 2],
            points[LoopIndex(index*3 + 3)],  // in case last segment on closed curve
        };
    }

    public void DeleteSegment(int anchorIndex)
    {

        if (NumberOfSegments > 2 || !isClosed && NumberOfSegments > 1)
        {
            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if (anchorIndex == points.Count - 1 && !isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public void MovePoint(int index, Vector2 newPosition)
    {
        Vector2 deltaMove = newPosition - points[index];

        // move one anchorpoint also move opposite anchorpoint
        if (index % 3 == 0 || !autoSetControlPoints)
        {
            points[index] = newPosition;
            if (autoSetControlPoints)
            {
                AutoSetAffectedControlPoints(index);
            }
            else
            {
                if (index % 3 == 0)
                {
                    if (index + 1 < points.Count || isClosed)
                    {
                        points[LoopIndex(index + 1)] += deltaMove;
                    }
                    if (index - 1 >= 0 || isClosed)
                    {
                        points[LoopIndex(index - 1)] += deltaMove;
                    }
                }
                else
                {
                    bool nextPointIsAnchor = LoopIndex(index + 1) % 3 == 0;
                    int correspodingControlIndex = (nextPointIsAnchor) ? index + 2 : index - 2;
                    int anchorIndex = (nextPointIsAnchor) ? index + 1 : index - 1;

                    if (correspodingControlIndex >= 0 && correspodingControlIndex < points.Count || isClosed)
                    {
                        float distance = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspodingControlIndex)]).magnitude;
                        Vector2 direction = (points[LoopIndex(anchorIndex)] - newPosition).normalized;
                        points[LoopIndex(correspodingControlIndex)] = points[LoopIndex(anchorIndex)] + direction * distance;
                    }
                }
            }
        }
    }

    int LoopIndex(int i)
    {
        // + points.Count to handle negative i values
        return (i + points.Count) % points.Count;
    }

    // automatically calculate control point's coordinate from anchor point
    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPoint = points[anchorIndex];

        // // get the bisection line of two neighbour anchor points, flip 90 degree
        // // set 2 control points to be half the distance from anchor point to each neighbour, on the flipped bisection line
        // Vector2 bisection = points[anchorIndex - 1] + points[anchorIndex + 1];
        // bisection = Vector2.Perpendicular(bisection);

        // Vector2 controlPoint1 = bisection + (points[anchorIndex - 1] - anchorPoint);
        // Vector2 controlPoin21 = bisection + (points[anchorIndex + 1] - anchorPoint);

        Vector2 direction = Vector2.zero;
        float[] neighbourDistance = new float[2];

        if (anchorIndex - 3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex - 3)] - anchorPoint;
            direction += offset.normalized;
            neighbourDistance[0] = offset.magnitude;
        }

        if (anchorIndex + 3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPoint;
            direction -= offset.normalized;
            neighbourDistance[1] = -offset.magnitude;
        }

        direction.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
            {
                points[LoopIndex(controlIndex)] = anchorPoint + direction * neighbourDistance[i] * .5f;
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        if (!isClosed)
        {
            points[1] = (points[0] + points[2]) * 0.5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
        }
    }

    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }
        AutoSetStartAndEndControls();
    }

    void AutoSetAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex; i++)
        {
            if (i >= 0 && i < points.Count || isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }
        AutoSetStartAndEndControls();
    }
}
