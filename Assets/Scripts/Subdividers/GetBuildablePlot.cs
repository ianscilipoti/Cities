using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

//this subdiv behavior creates one child that exactly uses the space the roads leave behind

public class GetBuildablePlot : ISubDivScheme<SubdividableEdgeLoop<CityEdge>>
{
    private City city;
    public GetBuildablePlot (City city)
    {
        this.city = city;
    }

    public List<SubdividableEdgeLoop<CityEdge>> GetChildren (SubdividableEdgeLoop<CityEdge> parent) 
    {
        Polygon parentPoly = parent.GetPolygon();

        Path polygonAsClip = parentPoly.ClipperPath(HelperFunctions.clipperScale);

        CityEdge[] edges = parent.GetEdges();

        //Paths edgePaths = new Paths();

        //Paths expandedLine = new Paths();

        //float width = 2f;
        //ClipperOffset clipperOffset = new ClipperOffset();
        //clipperOffset.AddPath(polygonAsClip, JoinType.jtSquare, EndType.etClosedPolygon);
        //clipperOffset.Execute(ref expandedLine, HelperFunctions.clipperScale * (-width / 2));

        //if (expandedLine.Count > 0)
        //{
        //    Path shrunkPoly = expandedLine[0];
        //    LinkedGraphVertex[] subPlotVerts = new LinkedGraphVertex[shrunkPoly.Count];
        //    for (int i = 0; i < shrunkPoly.Count; i++)
        //    {
        //        subPlotVerts[i] = new LinkedGraphVertex(HelperFunctions.GetPoint(shrunkPoly[i]));
        //    }
        //    CityEdge[] subPlotEdges = new CityEdge[shrunkPoly.Count];
        //    for (int i = 0; i < shrunkPoly.Count; i++)
        //    {
        //        subPlotEdges[i] = new CityEdge(subPlotVerts[i], subPlotVerts[(i + 1) % shrunkPoly.Count], CityEdgeType.PlotBoundary, 0f);
        //    }

        //    SubdividableEdgeLoop<CityEdge> plot = parent.GetNextChild(subPlotEdges);
        //    Polygon plotPoly = parent.GetPolygon();


        //    return new List<SubdividableEdgeLoop<CityEdge>> { plot };
        //}

        //return new List<SubdividableEdgeLoop<CityEdge>>();

        bool shapeRemains = true;

        foreach (CityEdge edge in edges)
        {
            Path edgeLine = new Path();
            edgeLine.Add(HelperFunctions.GetIntPoint(edge.a.pt));
            edgeLine.Add(HelperFunctions.GetIntPoint(edge.b.pt));

            Paths expandedLine = new Paths();

            float width = edge.GetWidth() * HelperFunctions.clipperScale;
            ClipperOffset clipperOffset = new ClipperOffset();
            clipperOffset.AddPath(edgeLine, JoinType.jtMiter, EndType.etOpenSquare);
            clipperOffset.Execute(ref expandedLine, width/2);

            //since we only expand a single line, we should only have one path left

            Paths differenceSolution = new Paths();
            Clipper clipper = new Clipper();
            clipper.AddPath(polygonAsClip, PolyType.ptSubject, true);
            clipper.AddPath(expandedLine[0], PolyType.ptClip, true);
            clipper.Execute(ClipType.ctDifference, differenceSolution);
            //Debug.Log("diff sol count: " + differenceSolution.Count);
            if (differenceSolution.Count == 0)
            {
                shapeRemains = false;
                break;
            }
            else
            {
                Path maxAreaPath = null;
                float maxArea = 0f;

                foreach (Path path in differenceSolution)
                {
                    Vector2[] points = new Vector2[path.Count];
                    int i = 0;
                    foreach (IntPoint p in path)
                    {
                        points[i] = HelperFunctions.GetPoint(p);
                        i ++;
                    }
                    Polygon testPoly = new Polygon(points);
                    if (testPoly.area > maxArea)
                    {
                        maxArea = testPoly.area;
                        maxAreaPath = path;
                    }
                }
                polygonAsClip = maxAreaPath;

                if (maxAreaPath == null)
                {
                    shapeRemains = false;
                    break;
                }
            }
        }

        if (shapeRemains && polygonAsClip.Count > 0)
        {
            LinkedGraphVertex[] subPlotVerts = new LinkedGraphVertex[polygonAsClip.Count];
            for (int i = 0; i < polygonAsClip.Count; i++)
            {
                subPlotVerts[i] = new LinkedGraphVertex(HelperFunctions.GetPoint(polygonAsClip[i]));
            }
            CityEdge[] subPlotEdges = new CityEdge[polygonAsClip.Count];
            for (int i = 0; i < polygonAsClip.Count; i++)
            {
                subPlotEdges[i] = new CityEdge(subPlotVerts[i], subPlotVerts[(i + 1) % polygonAsClip.Count], CityEdgeType.PlotBoundary, 0f);
            }
            SubdividableEdgeLoop<CityEdge> plot = parent.GetNextChild(subPlotEdges);
            Polygon plotPoly = parent.GetPolygon();


            return new List<SubdividableEdgeLoop<CityEdge>> { plot };
        }

        return new List<SubdividableEdgeLoop<CityEdge>>();
    }

}
