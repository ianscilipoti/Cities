using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class CitySkeleton : EdgeLoopSubdivider<CityEdge>//ISubDivScheme <SubdividableEdgeLoop>
{
    private Vector2[] cityEntrences;
    private int potentialRoadPoints;
    private System.Object[] factoryParams;

    public CitySkeleton (Vector2[] cityEntrences, int potentialRoadPoints, System.Object[] factoryParams) {
        this.cityEntrences = cityEntrences;
        this.potentialRoadPoints = potentialRoadPoints;
        this.factoryParams = factoryParams;
    }

    public override List<SubdividableEdgeLoop<CityEdge>> GetChildren (SubdividableEdgeLoop<CityEdge> parent)
    {
        Vector2[] parentPoints = parent.GetPoints();
        Polygon parentPoly = parent.GetPolygon();
        //generate points of interest
        List<RoadDestination> pointsOfInterest = new List<RoadDestination>();
        Vector2 centroid = parent.GetCenter();
        //parent.EnumerateEdges((EdgeLoopEdge edge) =>
        //{
        //    pointsOfInterest.Add(new RoadDestination(Vector2.Lerp(edge.a.pt, edge.b.pt, Random.Range(0.2f, 0.8f)), 1, false, true));
        //});
        Rect bounds = parent.GetBounds();
        bounds.width = bounds.width * 2;
        bounds.height = bounds.height * 2;
        int potentialRoadPointsRt = Mathf.CeilToInt(Mathf.Sqrt(potentialRoadPoints));

        float approxDiameter = Mathf.Sqrt(parentPoly.area);
        float minimumPerimeterDistance = approxDiameter / 4f;

        for (int x = 0; x < potentialRoadPointsRt; x++)
        {
            for (int y = 0; y < potentialRoadPointsRt; y++)
            {
                Vector2 point = new Vector2((x / (float)potentialRoadPointsRt) * bounds.width + bounds.xMin,
                                            (y / (float)potentialRoadPointsRt) * bounds.height + bounds.yMin);
                float distBtwnPts = (bounds.width + bounds.height) / (potentialRoadPoints * 2);
                point = point + new Vector2(Random.Range(-1f, 1f), Random.Range(-1, 1f)) * distBtwnPts * 3f;

                if (parentPoly.ContainsPoint(point)) // && parent.DistToPerimeter(point) > minimumPerimeterDistance)
                {
                    pointsOfInterest.Add(new RoadDestination(point, 0, false, false));  
                }
            } 
        }
        pointsOfInterest.Add(new RoadDestination(bounds.center + new Vector2(bounds.width * 100, bounds.height * 100), 0, false, false)); 
        pointsOfInterest.Add(new RoadDestination(bounds.center + new Vector2(bounds.width * 100, -bounds.height * 100), 0, false, false)); 
        pointsOfInterest.Add(new RoadDestination(bounds.center + new Vector2(-bounds.width * 100, -bounds.height * 100), 0, false, false)); 
        pointsOfInterest.Add(new RoadDestination(bounds.center + new Vector2(-bounds.width * 100, bounds.height * 100), 0, false, false)); 

        //triangulate points of interest to get potential road segments
        TriangleNet.Geometry.Polygon polygon = new TriangleNet.Geometry.Polygon();
        Dictionary<TriangleNet.Geometry.Vertex, RoadDestination> vertexDestMap = new Dictionary<TriangleNet.Geometry.Vertex, RoadDestination>();

        foreach (RoadDestination dest in pointsOfInterest)
        {
            TriangleNet.Geometry.Vertex vert = new TriangleNet.Geometry.Vertex(dest.point.x, dest.point.y);
            vertexDestMap.Add(vert, dest);
            polygon.Add(vert);
        }
        TriangleNet.Meshing.ConstraintOptions options =
            new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        TriangleNet.Meshing.GenericMesher mesher = new TriangleNet.Meshing.GenericMesher();
        TriangleNet.Meshing.IMesh mesh = mesher.Triangulate(polygon);

        TriangleNet.Voronoi.StandardVoronoi voronoi = new TriangleNet.Voronoi.StandardVoronoi((TriangleNet.Mesh)mesh);
        IEnumerable<TriangleNet.Geometry.IEdge> voronoiEdges = voronoi.Edges;
        List<TriangleNet.Topology.DCEL.Vertex> voronoiVerts = voronoi.Vertices;

        List<DividingEdge> dividingEdges = new List<DividingEdge>();
        ILinkedGraphEdgeFactory<CityEdge> factory = new CityEdgeFactory();

        foreach(TriangleNet.Geometry.IEdge edge in voronoiEdges)
        {
            Vector2 a = new Vector2((float)voronoiVerts[edge.P0].X, (float)voronoiVerts[edge.P0].Y);
            Vector2 b = new Vector2((float)voronoiVerts[edge.P1].X, (float)voronoiVerts[edge.P1].Y);

            dividingEdges.Add(new DividingEdge(a, b, factory, factoryParams));
        }

        return CollectChildren(parent, dividingEdges);
    }

    struct RoadDestination
    {
        public Vector2 point;
        public int priority;
        public bool entrence;
        public bool edge;

        public RoadDestination(Vector2 point, int priority, bool entrence, bool edge)
        {
            this.point = point;
            this.priority = priority;
            this.entrence = entrence;
            this.edge = edge;
        }
    }
}
