using System.Collections;
using System.Collections.Generic;
using UnityEngine;using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using ClipperLib;


using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public abstract class EdgeLoopSubdivider <EdgeType>: ISubDivScheme<SubdividableEdgeLoop<EdgeType>> where EdgeType : EdgeLoopEdge
{
    public abstract List<SubdividableEdgeLoop<EdgeType>> GetChildren(SubdividableEdgeLoop<EdgeType> parent); 

    protected List<SubdividableEdgeLoop<EdgeType>> CollectChildren (SubdividableEdgeLoop<EdgeType> parent, List<DividingEdge> dividingEdges)
    {
        List<EdgeType> knownEdges = new List<EdgeType>(parent.GetEdgesEnumerable());
        Polygon parentPoly = parent.GetPolygon();
        Path polygonAsClip = parentPoly.ClipperPath(HelperFunctions.clipperScale);

        //kinda ugly, these two variables are implicitly paired together
        Paths edgePaths = new Paths();
        List<ILinkedGraphEdgeFactory<EdgeType>> edgePathFactories = new List<ILinkedGraphEdgeFactory<EdgeType>>();

        foreach (DividingEdge edge in dividingEdges)
        {
            Path edgePath = new Path();
            edgePath.Add(HelperFunctions.GetIntPoint(edge.p1));
            edgePath.Add(HelperFunctions.GetIntPoint(edge.p2));

            PolyTree clippedResults = new PolyTree();
            Clipper clipper = new Clipper();

            clipper.AddPath(edgePath, PolyType.ptSubject, false);
            clipper.AddPath(polygonAsClip, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, clippedResults);

            Paths subPaths = Clipper.OpenPathsFromPolyTree(clippedResults);
            edgePaths.AddRange(subPaths);
            //if this edge was split into multiple paths when intersecting with parent poly, note that each subpath has the same factory
            foreach (Path path in subPaths)
            {
                edgePathFactories.Add(edge.factory);
            }
        }

        //foreach (Path edgePath in edgePaths)
        for (int j = 0; j < edgePaths.Count; j ++)
        {
            Path edgePath = edgePaths[j];
            ILinkedGraphEdgeFactory<EdgeType> edgeFactory = edgePathFactories[j];
            //this is almost always just 2 elements in which case it runs once
            for (int i = 0; i < edgePath.Count - 1; i++)
            {
                //convert path back into regular coordinates. Watch out that there is high enough resolution
                //that when this conversion happens, the linkedGraph still thinks points/edges are adjacent and connects them
                Vector2 p1 = HelperFunctions.GetPoint(edgePath[i]);
                Vector2 p2 = HelperFunctions.GetPoint(edgePath[i + 1]);
                LinkedGraph<EdgeType>.ConnectNewEdge(p1, p2, edgeFactory, knownEdges);
            }
        }

        List<EdgeType[]> formedChildLoops = parent.GetInteriorEdgeLoops();
        Debug.Log("Found " + formedChildLoops.Count + " child loops while subdividing.");

        List<SubdividableEdgeLoop<EdgeType>> children = new List<SubdividableEdgeLoop<EdgeType>>();
        foreach (EdgeType[] childLoop in formedChildLoops)
        {
            children.Add(parent.GetNextChild(childLoop));
        }
        return children;
    }

    //could add things here that are shared among edgeloop subdividers
    protected struct DividingEdge
    {
        public Vector2 p1;
        public Vector2 p2;
        public ILinkedGraphEdgeFactory<EdgeType> factory;
        public System.Object factoryParams;

        public DividingEdge (Vector2 p1, Vector2 p2, ILinkedGraphEdgeFactory<EdgeType> factory, System.Object factoryParams)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.factory = factory;
            this.factoryParams = factoryParams;
        }
    }
}

