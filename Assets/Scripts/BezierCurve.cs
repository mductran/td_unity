using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BezierCurve
{ 
    [SerializeField, HideInInspector]   
    private List<Vector2> anchorPoints;
    
    // generate a cubic curve
    public BezierCurve(Vector2 centre)
    {
        anchorPoints = new List<Vector2> {
            centre + Vector2.left,
            centre + Vector2.up + Vector2.left,
            centre + Vector2.right + Vector2.down,
            centre + Vector2.right
        };
    }

    public void AddSegment(Vector2 newAnchorPoint)
    {
        anchorPoints.Add(anchorPoints[anchorPoints.Count - 1] * 2 - anchorPoints[anchorPoints.Count - 2]);
        anchorPoints.Add((newAnchorPoint + anchorPoints[anchorPoints.Count - 1]) * .5f);
        anchorPoints.Add(newAnchorPoint);
    }

    public int NumberOfSegments
    {
        get
        {
            return (anchorPoints.Count - 4) / 3 + 1;
        }
    }

    public int NumberOfPoints
    {
        get{
            return anchorPoints.Count;
        }
    }

    public Vector2 this[int i]
    {
        get
        {
            return anchorPoints[i];
        }
    }

    public Vector2[] GetPointsInSegment(int index)
    {
        return new Vector2[]
        {
            anchorPoints[index*3],
            anchorPoints[index*3 + 1],
            anchorPoints[index*3 + 2],
            anchorPoints[index*3 + 3],
        };
    }

    public void MovePoint(int index, Vector2 newPosition)
    {
        // anchorPoints[i] = newPosition;

        Vector2 deltaMove = newPosition - anchorPoints[index];
        anchorPoints[index] = newPosition;

        // move one anchorpoint also move opposite anchorpoint
        if (index % 3 == 0)
        {
            if (index + 1 < anchorPoints.Count)
            {
                anchorPoints[index + 1] += deltaMove;
            }
            if (index - 1 >= 0)
            {
                anchorPoints[index - 1] += deltaMove;
            }
        }
        else 
        {
            bool nextPointIsAnchor = (index + 1) % 3 == 0;
            int correspodingControlIndex = (nextPointIsAnchor) ? index + 2 : index - 2;
            int anchorIndex = (nextPointIsAnchor) ? index + 1 : index - 1;

            if (correspodingControlIndex >= 0 && correspodingControlIndex < anchorPoints.Count) 
            {
                float distance = (anchorPoints[anchorIndex] - anchorPoints[correspodingControlIndex]).magnitude;
                Vector2 direction = (anchorPoints[anchorIndex] - newPosition).normalized;
                anchorPoints[correspodingControlIndex] = anchorPoints[anchorIndex] + direction * distance;
            }
        }
    }
}
