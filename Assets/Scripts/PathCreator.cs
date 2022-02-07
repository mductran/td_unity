using UnityEngine;

[RequireComponent(typeof(CurveCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PathCreator : MonoBehaviour
{
    [Range(.05f, 1.5f)]
    public float spacing = 1f;
    public float pathWidth = 1f;
    public bool autoUpdate;
    public float tiling = 1f;

    public void UpdateRoad()
    {
        BezierCurve curve = GetComponent<CurveCreator>().bezierCurve;
        Vector2[] points = curve.CalculateEvenlySpacedPoint(spacing);
        GetComponent<MeshFilter>().mesh = CreateMesh(points, curve.IsClosed);

        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * .5f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

    Mesh CreateMesh(Vector2[] points, bool isClosed)
    {
        Vector3[] vertices = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[vertices.Length];

        int numberOfTriangles = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] triangles = new int[numberOfTriangles * 3];
        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            if (i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1 + points.Length)  % points.Length] - points[i];
            }
            if (i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1)  % points.Length];
            }
            forward.Normalize(); 

            Vector2 left = new Vector2(-forward.y, forward.x);
            vertices[vertexIndex] = (points[i] + left) * .5f * pathWidth;
            vertices[vertexIndex + 1] = points[i] - left * pathWidth * .5f;

            float completionPercent = i/(float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);

            uvs[vertexIndex] = new Vector2(0, v);
            uvs[vertexIndex + 1] = new Vector2(1, v);

            if (i < points.Length - 1 || isClosed)
            {
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = (vertexIndex + 2) % vertices.Length;
                triangles[triangleIndex + 2] = vertexIndex + 1;

                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = (vertexIndex + 2) % vertices.Length;
                triangles[triangleIndex + 5] = (vertexIndex + 3) % vertices.Length;
            }
            vertexIndex += 2;
            triangleIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }

}
