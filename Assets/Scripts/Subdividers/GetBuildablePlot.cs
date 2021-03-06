﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

//this subdiv behavior creates one child that exactly uses the space the roads leave behind

public class GetBuildablePlot : EdgeLoopSubdivider<CityEdge>
{
    private City city;
    public GetBuildablePlot (City city)
    {
        this.city = city;
    }

    public override List<SubdividableEdgeLoop<CityEdge>> GetChildren (SubdividableEdgeLoop<CityEdge> parent) 
    {
        Polygon parentPoly = parent.GetPolygon();

        Path polygonAsClip = parentPoly.ClipperPath(HelperFunctions.clipperScale);


        CityEdge[] edges = parent.GetEdges();

        //------------------------------------------ OLD BAD STUFF
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


        //--------------------------------------------------------------------------- OLD BAD STUFF END


        bool shapeRemains = true;

        //int uniqueEdgeStartEdge = -1;
        //for (int i = 0; i < edges.Length; i ++)
        //{
        //    if (!edges[(i+1)%edges.Length].GetID().Equals(edges[i].GetID()))
        //    {
        //        uniqueEdgeStartEdge = (i + 1) % edges.Length;
        //        break;
        //    }
        //}

        //LinkedGraphVertex anchorVert = edges[uniqueEdgeStartEdge].GetOppositeVertex(edges[uniqueEdgeStartEdge].GetSharedVertex(edges[(uniqueEdgeStartEdge+1)%edges.Length]));
        //LinkedGraphVertex previusVert = null;

        for (int j = 0; j < edges.Length; j ++)
        {
            CityEdge edge = edges[j];

            //int nextIndex = (j + uniqueEdgeStartEdge + 1) % edges.Length;
            //LinkedGraphVertex thisVert = edge.GetOppositeVertex()

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
        if (shapeRemains)
        {
            for (int i = polygonAsClip.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (polygonAsClip[i].X == polygonAsClip[j].X && polygonAsClip[i].Y == polygonAsClip[j].Y)
                    {
                        polygonAsClip.RemoveAt(i);
                        Debug.Log("removed dup of interior plot");
                    }
                }
            }
        }

        Vector2[] parentPoints = parent.GetPoints();
        ILinkedGraphEdgeFactory<CityEdge> factory = new CityEdgeFactory();
        System.Object[] roadCapBoundarySettings = new System.Object[] { CityEdgeType.EdgeCap, 0f };
        System.Object[] plotBoundarySettings = new System.Object[] { CityEdgeType.PlotBoundary, 0f };

        if (shapeRemains && polygonAsClip.Count > 0)
        {
            List<DividingEdge> dividingEdges = new List<DividingEdge>();
            Vector2[] subPlotVerts = new Vector2[polygonAsClip.Count];
            for (int i = 0; i < polygonAsClip.Count; i++)
            {
                subPlotVerts[i] = HelperFunctions.GetPoint(polygonAsClip[i]);
            }

            List<CityEdge> knownEdges = new List<CityEdge>(parent.GetEdgesEnumerable());


            for (int i = 0; i < parentPoints.Length; i++)
            {
                float closestVertDistance = float.MaxValue;
                int closestVertIndex = -1;
                for (int j = 0; j < subPlotVerts.Length; j++)
                {
                    float thisDist = (parentPoints[i] - subPlotVerts[j]).sqrMagnitude;
                    if (thisDist < closestVertDistance)
                    {
                        closestVertDistance = thisDist;
                        closestVertIndex = j;
                    }
                }
                dividingEdges.Add(new DividingEdge(parentPoints[i], subPlotVerts[closestVertIndex], factory, roadCapBoundarySettings));
            }

            for (int i = 0; i < subPlotVerts.Length; i++)
            {
                dividingEdges.Add(new DividingEdge(subPlotVerts[i], subPlotVerts[(i + 1) % subPlotVerts.Length], factory, plotBoundarySettings));
            }

            List<CityEdge[]> formedChildRegions = CollectChildLoops(parent, dividingEdges);

            List<SubdividableEdgeLoop<CityEdge>> children = new List<SubdividableEdgeLoop<CityEdge>>();

            for (int i = 0; i < formedChildRegions.Count; i ++)
            {
                CityEdge[] loop = formedChildRegions[i];
                bool allPlotBoundaries = true;
                for (int j = 0; j < loop.Length; j ++)
                {
                    if (loop[j].GetRoadType() != CityEdgeType.PlotBoundary)
                    {
                        allPlotBoundaries = false;
                    }
                }
                if (allPlotBoundaries)
                {
                    children.Add(parent.GetNextChild(loop));
                }
                else
                {
                    children.Add(new Road(loop, city));
                }
            }

            return children;
            //return new List<SubdividableEdgeLoop<CityEdge>>();
        }


        return new List<SubdividableEdgeLoop<CityEdge>>();
    }

}
