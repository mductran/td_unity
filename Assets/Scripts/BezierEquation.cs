using UnityEngine;

public static class BezierEquation
{
    public static Vector2 EvalQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);

        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 EvalCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 p0 = EvalQuadratic(a, b, c, t);
        Vector2 p1 = EvalQuadratic(b, c, d, t);

        return Vector2.Lerp(p0, p1, t);
    }
}
