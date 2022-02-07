using UnityEngine;

public class CurveCreator : MonoBehaviour
{
    [HideInInspector]
    public BezierCurve bezierCurve;

    public void CreateCurve()
    {
        bezierCurve = new BezierCurve(transform.position);
    }

    void Reset() 
    {
        CreateCurve();
    }
}
