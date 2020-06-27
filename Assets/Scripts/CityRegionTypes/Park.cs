//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//using Polygon = EPPZ.Geometry.Model.Polygon;

//public class Park : CityRegion
//{

//    public Vector2 entrence;

//    public Park (Vector2[] boundary, City cityRoot) : base(boundary, cityRoot, false) {
//        entrence = (edges[0].a + edges[0].b) / 2;
//    }

//	public override Color getDebugColor()
//	{
//        return new Color(0, 1f, 0.1f, 0.6f);
//	}

//	public override ISubdividable GetNextChild (Vector2[] boundary) 
//    {
//        return null;//unsubdividable
//    }

//    //Cities always subdivide with citySkeleton
//    //this function could randomize what subdivscheme is returned easily
//    public override ISubDivScheme GetDivScheme () {
//        return null;//unsubdividable
//    }
//}
