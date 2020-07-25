//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using EPPZ.Geometry.Model;


//namespace TriangleNet
//{
//    using ClipperLib;
//    using EPPZ.Geometry.AddOns;
//    using Path = List<ClipperLib.IntPoint>;
//    using Paths = List<List<ClipperLib.IntPoint>>;




//    public class TestPlayground : MonoBehaviour
//    {
//        TriangleNet.Meshing.IMesh mesh;
//        Polygon boundary;
//        Paths solution;
//        Paths triClipPaths;
//        Path boundaryPath;
//        float scale = 500f;
//        Paths offsetTris;

//        // Start is called before the first frame update
//        void Start()
//        {
//            print( HelperFunctions.AngleBetween(Vector2.up, Vector2.left) * Mathf.Rad2Deg);
//            return;

//            //Random.seed = 0;
//            int numPoints = 10;
//            List<Vector2> bPoly = new List<Vector2>();
//            for (int i = 0; i < numPoints; i++)
//            {
//                float angle = (i / ((float)numPoints)) * Mathf.PI * 2;
//                float cos = Mathf.Cos(angle);
//                float sin = Mathf.Sin(angle);
//                float rnd = 1.4f;//Random.value;
//                bPoly.Add(new Vector2((int)(10f + cos * 5f*rnd), (int)(10f + sin * 5f*rnd)));
//            }

//            boundary = Polygon.PolygonWithPointList(bPoly);

//            boundaryPath = ClipperAddOns.ClipperPath(boundary, scale);

//            TriangleNet.Geometry.Polygon polygon = new TriangleNet.Geometry.Polygon();
//            for (int i = 0; i < 120; i++)
//            {
//                polygon.Add(new TriangleNet.Geometry.Vertex(Random.Range(0.0f, 20f), Random.Range(0.0f, 20f)));
//            }

//            TriangleNet.Meshing.ConstraintOptions options =
//                new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };

//            TriangleNet.Meshing.GenericMesher mesher = new TriangleNet.Meshing.GenericMesher();

//            mesh = mesher.Triangulate(polygon);

//            ICollection<TriangleNet.Topology.Triangle> triangles = mesh.Triangles;

//            triClipPaths = new Paths();

//            //int counter = 0;
//            foreach (TriangleNet.Topology.Triangle tri in triangles)
//            {
                
//                Path path = new Path();
//                for (int j = 0; j < 3; j++)
//                {
//                    TriangleNet.Geometry.Vertex vert = tri.GetVertex(j);
//                    path.Add(new IntPoint((int)(vert[0] * scale), (int)(vert[1] * scale)));
//                }
//                path.Reverse();
//                triClipPaths.Add(path);
//            }
//            solution = new Paths();
//            offsetTris = new Paths();
//            for (int i = 0; i < triClipPaths.Count; i ++){
//                Paths solutionI = new Paths();
//                Clipper clipper = new Clipper();
//                clipper.AddPath(triClipPaths[i], PolyType.ptSubject, true);
//                clipper.AddPath(boundaryPath, PolyType.ptClip, true);
//                clipper.Execute(ClipType.ctIntersection, solutionI);

//                if (solutionI.Count > 0){
//                    solution.Add(solutionI[0]);

//                    ClipperOffset co = new ClipperOffset();
//                    co.AddPath(solutionI[0], JoinType.jtSquare, EndType.etClosedPolygon);
//                    Paths tempSltn = new Paths();
//                    co.Execute(ref tempSltn, -50f);
//                    if (tempSltn.Count > 0) {
//                        offsetTris.Add(tempSltn[0]);
//                    }
//                }

//            }
//            //Clipper clipper = new Clipper();

//            //clipper.AddPaths(triClipPaths, PolyType.ptSubject, true);
//            //clipper.AddPath(boundaryPath, PolyType.ptClip, true);



//        }

//        public void OnDrawGizmos()
//        {
//            if (mesh == null)
//            {
//                // We're probably in the editor
//                return;
//            }

//            Gizmos.color = Color.red;
//            //ICollection<TriangleNet.Geometry.Vertex> vertices = mesh.Vertices;
//            //int numV = vertices.Count;
//            //TriangleNet.Geometry.Vertex[] vList = new TriangleNet.Geometry.Vertex[numV];
//            //vertices.CopyTo(vList, 0);

//            //foreach (TriangleNet.Geometry.Edge edge in mesh.Edges)
//            //{
//            //    TriangleNet.Geometry.Vertex v0 = vList[edge.P0];
//            //    TriangleNet.Geometry.Vertex v1 = vList[edge.P1];
//            //    Vector3 p0 = new Vector3((float)v0[0], 0.0f, (float)v0[1]);
//            //    Vector3 p1 = new Vector3((float)v1[0], 0.0f, (float)v1[1]);
//            //    Gizmos.DrawLine(p0, p1);
//            //}

//            boundary.EnumerateEdges((Edge edge) =>
//            {
//                Vector3 p0 = new Vector3(edge.vertexA.x, 0f, edge.vertexA.y);
//                Vector3 p1 = new Vector3(edge.vertexB.x, 0f, edge.vertexB.y);
//                Gizmos.DrawLine(p0, p1);
//            });



//            Gizmos.color = Color.green;

//            for (int i = 0; i < solution.Count; i++)
//            {
//                Polygon poly = ClipperAddOns.PolygonFromClipperPath(solution[i], scale);
//                poly.EnumerateEdges((Edge edge) =>
//                {
//                    Vector3 p0 = new Vector3(edge.vertexA.x, 0f, edge.vertexA.y);
//                    Vector3 p1 = new Vector3(edge.vertexB.x, 0f, edge.vertexB.y);
//                    Gizmos.DrawLine(p0, p1);
//                });
//            }

//            //Gizmos.color = new Color(0f, 0f, 1f, 1f);
//            //for (int i = 0; i < triClipPaths.Count; i++)
//            //{
//            //    Polygon poly = ClipperAddOns.PolygonFromClipperPath(triClipPaths[i], scale);
//            //    poly.EnumerateEdges((Edge edge) =>
//            //    {
//            //        Vector3 p0 = new Vector3(edge.vertexA.x + 0.05f, 0f, edge.vertexA.y + 0.05f);
//            //        Vector3 p1 = new Vector3(edge.vertexB.x + 0.05f, 0f, edge.vertexB.y + 0.05f);
//            //        Gizmos.DrawLine(p0, p1);
//            //    });
//            //}

//            Gizmos.color = Color.white;

//            for (int i = 0; i < offsetTris.Count; i++)
//            {
//                Polygon poly = ClipperAddOns.PolygonFromClipperPath(offsetTris[i], scale);
//                poly.EnumerateEdges((Edge edge) =>
//                {
//                    Vector3 p0 = new Vector3(edge.vertexA.x, 0f, edge.vertexA.y);
//                    Vector3 p1 = new Vector3(edge.vertexB.x, 0f, edge.vertexB.y);
//                    Gizmos.DrawLine(p0, p1);
//                });
//            }


//        }
//    }
//}