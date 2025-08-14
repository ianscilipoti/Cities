using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry;
using Polygon = EPPZ.Geometry.Model.Polygon;

public class HelperFunctions
{
    public static float clipperScale = 10000000f;

    //gets the inner intersection point of two lines with width
    public static Vector2 GetIntersectionPoint(Vector2 a, Vector2 midPoint, Vector2 b, float w1, float w2)
    {
        Vector2 line1 = midPoint - a;
        Vector2 line2 = b - midPoint;

        float angle = 180 - Vector3.Angle(line1, line2);

        float angleRad = Mathf.Deg2Rad * angle;
        float theta1 = acot((w2 / w1 + Mathf.Cos(angleRad)) / Mathf.Sin(angleRad));

        float d1 = w1 / Mathf.Tan(theta1);

        Vector2 oppositeLine1 = -line1.normalized;
        Vector2 leftOfOL1 = new Vector2(-oppositeLine1.y, oppositeLine1.x);
        Vector2 offset = oppositeLine1 * d1 + leftOfOL1 * w1;
        Vector2 p = midPoint + offset;

        return p;
    }

    public void GetPolygonTriangles ()
    {
        
    }

    public static Rect GetOrientedBounds (List<Vector2> points, ref float relativeAngle)
    {
        float maxLength = 0f;
        Vector2 maxLengthDirection = Vector2.one;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 thisDirection = points[(i + 1) % points.Count] - points[i];
            float thisLength = Vector3.SqrMagnitude(thisDirection);
            if (thisLength > maxLength)
            {
                maxLength = thisLength;
                maxLengthDirection = thisDirection;
            }
        }

        relativeAngle = AngleBetween(Vector2.right, maxLengthDirection);
        List<Vector2> rotatedPoints = new List<Vector2>();
        Vector2 pivot = points[0];//doesn't really matter

        for (int i = 0; i < points.Count; i++)
        {
            rotatedPoints.Add(points[i].RotatedAround(pivot, -relativeAngle * Mathf.Rad2Deg));
        }
        Bounds bounds = new Bounds((Vector2)points[0], Vector3.zero);

        for (int i = 0; i < points.Count; i++)
        {
            bounds.Encapsulate(rotatedPoints[i]);
        }

        return new Rect((Vector2)(bounds.center - bounds.size/2), (Vector2)bounds.size);
    }

    public static float TriangleInterp (Vector2 p, Vector2 v1, Vector2 v2, Vector2 v3, float val1, float val2, float val3)
    {
        float denom = ((v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y));

        float w1 = ((v2.y - v3.y) * (p.x - v3.x) + (v3.x - v2.x) * (p.y - v3.y)) / denom;
        float w2 = ((v3.y - v1.y) * (p.x - v3.x) + (v1.x - v3.x) * (p.y - v3.y)) / denom;
        float w3 = 1 - w1 - w2;

        return w1 * val1 + w2 * val2 + w3 * val3;
    }

    public static Mesh GetTriangulationMesh (Vector3[] verts, HashSet<int> borderVerts, Polygon boundary)
    {
        TriangleNet.Geometry.Polygon polygon = new TriangleNet.Geometry.Polygon();
        //Dictionary<TriangleNet.Geometry.Vertex, float> vertexElevMap = new Dictionary<TriangleNet.Geometry.Vertex, float>();
        foreach (Vector3 vert in verts)
        {
            TriangleNet.Geometry.Vertex triangulationVert = new TriangleNet.Geometry.Vertex(vert.x, vert.z);
            //vertexElevMap.Add(triangulationVert, vert.y);
            polygon.Add(triangulationVert);
        }

        TriangleNet.Meshing.ConstraintOptions options =
            new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        TriangleNet.Meshing.GenericMesher mesher = new TriangleNet.Meshing.GenericMesher();
        TriangleNet.Meshing.IMesh mesh = mesher.Triangulate(polygon);

        Vector3[] meshVerts = new Vector3[mesh.Vertices.Count];
        List<int> meshTriangles = new List<int>();

        Dictionary<TriangleNet.Geometry.Vertex, int> vertexIndexMap = new Dictionary<TriangleNet.Geometry.Vertex, int>();

        int v = 0;
        foreach (TriangleNet.Geometry.Vertex triangulatorVert in mesh.Vertices)
        {
            meshVerts[v] = new Vector3((float)triangulatorVert.X, verts[v].y, (float)triangulatorVert.Y);
            vertexIndexMap[triangulatorVert] = v;
            v++;
        }
        //for (int i = 0; i < vertexList.Length; i ++)
        //{
        //    TriangleNet.Geometry.Vertex triangulatorVert = vertexList[i];
        //    meshVerts[i] = new Vector3((float)triangulatorVert.X, vertexElevMap[triangulatorVert], (float)triangulatorVert.Y);
        //    vertexIndexMap[triangulatorVert] = i;
        //}

        foreach (TriangleNet.Topology.Triangle tri in mesh.Triangles)
        {
            int ind1 = vertexIndexMap[tri.GetVertex(0)];
            int ind2 = vertexIndexMap[tri.GetVertex(2)];
            int ind3 = vertexIndexMap[tri.GetVertex(1)];
            Vector2 center = new Vector2((float)(tri.GetVertex(0).X + tri.GetVertex(1).X + tri.GetVertex(2).X) / 3f, (float)(tri.GetVertex(0).Y + tri.GetVertex(1).Y + tri.GetVertex(2).Y) / 3f);

            if (!(borderVerts.Contains(ind1) && borderVerts.Contains(ind2) && borderVerts.Contains(ind3)) || (boundary.ContainsPoint(center) && !boundary.PermiterContainsPoint(center)))
            {
                meshTriangles.Add(ind1);
                meshTriangles.Add(ind2);
                meshTriangles.Add(ind3);
            }
        }

        Mesh unityMesh = new Mesh();
        unityMesh.vertices = meshVerts;
        unityMesh.triangles = meshTriangles.ToArray();
        unityMesh.RecalculateBounds();
        unityMesh.RecalculateNormals();
        return unityMesh;
    }

    //get the CCW angle in radians between a and b
    public static float AngleBetween(Vector2 a, Vector2 b)
    {
        float dot = Vector2.Dot(a, b);      // dot product between [x1, y1] and [x2, y2]
        float det = a.x * b.y - a.y * b.x;      // determinant
        float angle = Mathf.Atan2(det, dot);
        if (angle < 0)
        {
            angle += Mathf.PI * 2;
        }
        return angle;
    }

    public static Vector2 ScaleFrom(Vector2 vec, Vector2 from, float factor)
    {
        Vector2 fromTo = (vec - from).normalized;
        return vec + fromTo * factor;
    }

    //https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
    public static float SegmentPointDist(Vector2 v, Vector2 w, Vector2 p)
    {
        // Return minimum distance between line segment vw and point p
        float l2 = Vector2.SqrMagnitude(v - w);  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2 == 0.0) 
        {
            return Vector3.Distance(p, v);   // v == w case
        }
                                                // Consider the line extending the segment, parameterized as v + t (w - v).
                                                // We find projection of point p onto the line. 
                                                // It falls where t = [(p-v) . (w-v)] / |w-v|^2
                                                // We clamp t from [0,1] to handle points outside the segment vw.
        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(p - v, w - v) / l2));
        Vector2 projection = v + t * (w - v);  // Projection falls on the segment
        return Vector2.Distance(p, projection);
    }

    public static ClipperLib.IntPoint GetIntPoint (Vector2 pt)
    {
        return new ClipperLib.IntPoint(pt.x * clipperScale, pt.y * clipperScale);
    }

    public static Vector2 GetPoint (ClipperLib.IntPoint pt)
    {
        return new Vector2((float)pt.X / clipperScale, (float)pt.Y / clipperScale);
    }

    // Start is called before the first frame update
    public static Vector2 projVec3 (Vector3 inp)
    {
        return new Vector2(inp.x, inp.z);
    }

    public static Vector3 projVec2 (Vector2 inp) 
    {
        return new Vector3 (inp.x, 0, inp.y);
    }

    public static Vector3 projVec2(Vector2 inp, float y)
    {
        return new Vector3(inp.x, y, inp.y);
    }

    public static float acot(float x)
    {
        return Mathf.PI / 2 - Mathf.Atan(x);
    }

    public static Vector2 getRightPerpendicularDirection (Vector2 vec)
    {
        return new Vector2(vec.y, -vec.x).normalized;
    }

    public static Vector2 getLeftPerpendicularDirection(Vector2 vec)
    {
        return -getRightPerpendicularDirection(vec);
    }

    public static Vector2 getReverseDirection(Vector2 vec)
    {
        return -vec.normalized;
    }

    public static float aAlongB (Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b) / b.magnitude;
    }



}
