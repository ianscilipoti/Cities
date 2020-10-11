
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class Triangulator : MonoBehaviour {

	public class Triangulation {
		
		public List<Vector3> points;//all the points currently generated
		public List<Triangle> triangles;//all the triangles
		public List<Edge> edges;//all the edges
		private int size;

		public Triangulation (int new_size) {
			points = new List<Vector3> ();
			triangles = new List<Triangle> ();
			edges = new List<Edge> ();
			size = new_size;
			//initialize corners 
			createInitialGeometry();
		}

		public Triangulation (string name, int new_size) {
			points = new List<Vector3> ();
			triangles = new List<Triangle> ();
			edges = new List<Edge> ();
			size = new_size;

			string Path = Application.dataPath + "/" + name + ".txt";
			int parseType = 0; //0 = points, 1 = triangles, 2 = edges
			int lineNum = 0;
			try 
			{
				String line = "";
				// Create an instance of StreamReader to read from a file.
				// The using statement also closes the StreamReader.
				using (StreamReader sr = new StreamReader(Path)) 
				{
					do
					{
						line = sr.ReadLine();

						if (line != null)
						{
							// Do whatever you need to do with the text line, it's a string now
							// In this example, I split it into arguments based on comma
							// deliniators, then send that array to DoStuff()
							if(line.Equals("p")){
								parseType = 0;
								print("now reading points");
							}
							else if (line.Equals("t")){
								parseType = 1;
								print("now reading triangles");
							}
							else if (line.Equals("e")){
								parseType = 2;
								print("now reading edges");
							}
							else {
								switch (parseType) {
									case 0:
										points.Add(stringToPoint(line));
									break;
									case 1:
										triangles.Add(new Triangle(line));
									break;
									case 2:
										edges.Add(new Edge(line));
									break;
								}
							}
							lineNum++;
						}
					}
					while (line != null && line.Length > 0);
					// Done reading, close the reader and return true to broadcast success    
					sr.Close();
				}
			}
			catch (Exception e) 
			{
				// Let the user know what went wrong.
				print("The file could not be read:");
				print(e.Message);
				print ("line = " + lineNum);
			}
		}

		public void SaveToFile (string name) {
			string str = "p\n";
			for (int i = 0; i < points.Count; i++) {
				str += pointToString(points [i]) + "\n";
			}
			str += "t\n";
			for (int i = 0; i < triangles.Count; i++) {
				str += triangles [i].ToString () + "\n";
			}
			str += "e\n";
			for (int i = 0; i < edges.Count; i++) {
				str += edges [i].ToString () + "\n";
			}
			string path = Application.dataPath + "/" + name + ".txt";
			System.IO.File.WriteAllText(path, str);
		}

		public string pointToString (Vector3 point) {
			string str = (point.x + ",") + (point.y + ",") + (point.z);
			return str;
		}
		public Vector3 stringToPoint (string str) {
			char[] splitChars = {','};
			string[] properties = str.Split(splitChars);
			float x = float.Parse (properties [0]);
			float y = float.Parse (properties [1]);
			float z = float.Parse (properties [2]);
			return new Vector3(x,y,z);
		}

		public void CleanArrays () {
			int[] triangleOffsets = new int[triangles.Count];//get a list of offsets to the array indexes
			int[] edgeOffsets = new int[edges.Count];

			for (int i = triangles.Count-1; i >= 0; i --) {
				if (!triangles [i].active) {//whenever we find an inactive index, add an offset to all succeeding slots
					triangleOffsets [i] = -1;//mark this as a bad triangle for later edge removal
					for (int j = i+1; j < triangles.Count; j++) {
						if (triangleOffsets [j] != -1) {//don't overwrite bad triangle markers
							triangleOffsets [j]++;
						}
					}
					triangles.RemoveAt (i);
				}
			}

			for (int i = edges.Count - 1; i >= 0; i --) {
				bool isJunk = true;//assume true, if we have a good triangle neighbor, then set to false
				for (int j = 0; j < edges [i].triangles.Count; j++) {
					if (triangleOffsets [edges [i].triangles [j]] != -1) {//we have a good neighbor, we aren't junk
						isJunk = false;
					}
				}
				if (isJunk) {
					for (int j = i+1; j < edges.Count; j++) {
						if (edgeOffsets [j] != -1) {//don't overwrite bad edge markers
							edgeOffsets [j] ++;
						}
					}
					edges.RemoveAt (i);
				}
			}

			for (int i = edges.Count - 1; i >= 0; i --) {
				for (int j = 0; j < edges [i].triangles.Count; j++) {
					edges [i].triangles [j] -= triangleOffsets [edges [i].triangles [j]];
				}
			}

			for (int i = triangles.Count - 1; i >= 0; i --) {
				for (int j = 0; j < triangles [i].edgeList.Length; j++) {
					triangles [i].edgeList [j] -= edgeOffsets [triangles [i].edgeList [j]];
				}
			}
		}

		public Mesh GenerateMeshSimple (Color[] triangleColors, Color[] vertexColors) {
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> meshTriangles = new List<int>();
			List<Vector2> meshUVs = new List<Vector2>();
			List<Color> meshColors = new List<Color>();

			bool applyColors = false;
			if (triangleColors != null && vertexColors != null) {
				applyColors = true;
			}

			for (int i = 0; i < triangles.Count; i++) {
				Triangle thisTriangle = triangles [i];
				if (!thisTriangle.active) {
					continue;
				}

				int lastMeshTriangleIndex = meshTriangles.Count;
				for (int t = 0; t < 3; t++) {
					meshVertices.Add ( points[thisTriangle.pointList [t]] );
					meshTriangles.Add ( lastMeshTriangleIndex + (2 - t) );
					Vector2 uv = convertVector (points [thisTriangle.pointList [t]]) / size;
					meshUVs.Add (uv);

					if (applyColors) {
						Color thisColor = triangleColors [i] * vertexColors[thisTriangle.pointList[t]];
						meshColors.Add (thisColor);
					}
				}
			}

			Mesh mesh = new Mesh ();
			mesh.vertices = meshVertices.ToArray ();
			mesh.triangles = meshTriangles.ToArray ();
			mesh.uv = meshUVs.ToArray ();
			if (applyColors) {
				mesh.colors = meshColors.ToArray ();
			}
			mesh.RecalculateNormals ();
			return mesh;
		}

		public void GenerateMesh (string name, int divisions, Material meshMaterial, Color[] triangleColors, Color[] vertexColors) {
			List<Vector3>[,] meshVertices = new List<Vector3>[divisions, divisions];
			List<int>[,] meshTriangles = new List<int>[divisions, divisions];
			List<Vector2>[,] meshUVs = new List<Vector2>[divisions, divisions];
			List<Color>[,] meshColors = new List<Color>[divisions, divisions];

			bool applyColors = false;

			if (triangleColors != null && vertexColors != null) {
				applyColors = true;
			}

			//initialize all arrays
			for (int x = 0; x < divisions; x++) {
				for (int y = 0; y < divisions; y ++) {
					meshVertices [x, y] = new List<Vector3> ();
					meshTriangles [x, y] = new List<int> ();
					meshUVs [x, y] = new List<Vector2> ();
					meshColors [x, y] = new List<Color> ();
				}
			}

			//add all vertices from each division to their respective arrays
			for (int i = 0; i < triangles.Count; i++) {
				Triangle thisTriangle = triangles [i];

				if (!thisTriangle.active) {
					continue;
				}

				Vector3 triangleCenter = GetTriangleCenter (i);
				int xDiv = Mathf.RoundToInt(Mathf.Clamp01(triangleCenter.x / size) * (divisions-1));
				int yDiv = Mathf.RoundToInt(Mathf.Clamp01(triangleCenter.z / size) * (divisions-1));

				int lastMeshTriangleIndex = meshTriangles [xDiv, yDiv].Count;
				for (int t = 0; t < 3; t++) {
					meshVertices [xDiv, yDiv].Add ( points[thisTriangle.pointList [t]] );
					meshTriangles [xDiv, yDiv].Add ( lastMeshTriangleIndex + (2 - t) );
					Vector2 uv = convertVector (points [thisTriangle.pointList [t]]) / size;
					meshUVs [xDiv, yDiv].Add (uv);

					if (applyColors) {
						Color thisColor = triangleColors [i] * vertexColors[thisTriangle.pointList[t]];
						meshColors [xDiv, yDiv].Add (thisColor);
					}
				}
			}

			GameObject meshContainer = new GameObject (name + "Mesh(s)");

			for (int x = 0; x < divisions; x++) {
				for (int y = 0; y < divisions; y++) {
					GameObject meshObject = new GameObject (name + " x" + x + " y" + y);
					MeshFilter filter = meshObject.AddComponent<MeshFilter> ();
					MeshRenderer renderer = meshObject.AddComponent<MeshRenderer> ();
					MeshCollider collider = meshObject.AddComponent<MeshCollider> ();

					Mesh mesh = new Mesh ();

					mesh.vertices = meshVertices [x, y].ToArray ();
					mesh.triangles = meshTriangles [x, y].ToArray ();
					mesh.uv = meshUVs [x, y].ToArray ();
					if (applyColors) {
						mesh.colors = meshColors [x, y].ToArray ();
					}
					mesh.RecalculateNormals ();

					filter.sharedMesh = mesh;
					collider.sharedMesh = mesh;
					renderer.material = meshMaterial;

					meshObject.transform.SetParent (meshContainer.transform);
					meshObject.isStatic = true;
				}
			}
		}

		public Color[] getEmptyTriangleColors () {
			return new Color[triangles.Count];
		}
		public Color[] getEmptyVertexColors () {
			return new Color[points.Count];
		}

		//initialize a square with two triangles that all points will fit into 
		public void createInitialGeometry () { 
			points.Add(new Vector3(-size, 0, -size));
			points.Add(new Vector3(size * 2, 0, -size));
			points.Add(new Vector3(-size, 0, size * 2));
			points.Add(new Vector3(size * 2, 0, size * 2));
			edges.Add (new Edge (0, 1));
			edges.Add (new Edge (1, 2));
			edges.Add (new Edge (2, 0));
			edges.Add (new Edge (1, 3));
			edges.Add (new Edge (3, 2));

			addTriangle (2, 0, 1, 2, 0, 1);
			addTriangle (2, 1, 3, 1, 3, 4);
		}

		//add a set of points to the triangulation
		public void addPoints (Vector3[] new_points) {
			for (int i = 0; i < new_points.Length; i++) {
				addPoint (new_points [i]);
			}
			clearFoundation ();
			//print ("pointCount = " + points.Count);
			//print ("triangleCount = " + triangles.Count);
			//print ("edgeCount = " + edges.Count);
 		}

		//remove the two base triangles
		public void clearFoundation () {
			for (int i = triangles.Count - 1; i >= 0; i--) {
				bool isLegal = true;
				for (int t = 0; t < 3; t++) {
					if (triangles [i].pointList [t] < 4) {
						isLegal = false;
						break;
					}
				}
				if (isLegal == false) {
					triangles [i].active = false;
				}
			}
		}
		//get an empty array to populate with colors
		public Color[] getTriangleColorArray () {
			return new Color[triangles.Count];
		}



		//add a point to the triangulation
		public void addPoint (Vector3 vector3Point) {
			float startTime = Time.realtimeSinceStartup;

			Vector2 point = convertVector (vector3Point);
			int incasingTriangle = findTriangle(point);

			if(incasingTriangle == -1){
				return;
			}

			points.Add(vector3Point);
			int newPointIndex = points.Count-1;

			int[] neighbors = relevantNeighbors(point, incasingTriangle, incasingTriangle);
			int[] hull = findHull(neighbors);
			for(int i = 0; i < neighbors.Length; i ++){
				removeTriangle(neighbors[i]);
			}

			int firstPoint = edges[hull[0]].point1;//make sure that the first point is included in the last hull edge
			if(firstPoint != edges[hull[hull.Length-1]].point1 && firstPoint != edges[hull[hull.Length-1]].point2){
				firstPoint = edges[hull[0]].point2;
			}

			int lastEdge = 0;//initialize the first edge manually
			edges.Add(new Edge(newPointIndex, firstPoint));
			lastEdge = edges.Count - 1;

			int firstEdge = lastEdge;//keep track of the first edge to contruct the last triangle
			Edge firstHullEdge = edges [firstEdge];

			for(int i = 0; i < hull.Length-1; i ++) {
				Edge thisHullEdge = edges [hull [i]];
				Edge lastHullEdge = edges [lastEdge];

				int nextPoint = thisHullEdge.point1;

				if(nextPoint == lastHullEdge.point2){
					nextPoint = thisHullEdge.point2;
				}

				int newEdge = 0;
				edges.Add(new Edge(newPointIndex, nextPoint));
				newEdge = edges.Count - 1;
				addTriangle(newPointIndex, lastHullEdge.point2, edges[newEdge].point2, hull[i], lastEdge, newEdge);
				lastEdge = newEdge;
			}
			//problem here***
			addTriangle(newPointIndex, firstHullEdge.point2, edges[lastEdge].point2, hull[hull.Length-1], lastEdge, firstEdge);
		}

		void removeTriangle (int index){
			Triangle thisTriangle = triangles [index];
			edges[thisTriangle.edgeList[0]].triangles.Remove(index);
			edges[thisTriangle.edgeList[1]].triangles.Remove(index);
			edges[thisTriangle.edgeList[2]].triangles.Remove(index);
			thisTriangle.active = false;
			//triangles.Remove(thisTriangle);
		}

		int[] relevantNeighbors (Vector2 point, int triangleIndex, int parent) {
			Triangle thisTriangle = triangles [triangleIndex];
			if(Vector2.Distance(thisTriangle.circumCircleCenter, point) > thisTriangle.circumCircleRadius){
				return null;
			}
			int[] neighbors = new int[1];
			neighbors[0] = triangleIndex;

			int neighbor1 = getNeighbor(triangleIndex, 0);
			int neighbor2 = getNeighbor(triangleIndex, 1);
			int neighbor3 = getNeighbor(triangleIndex, 2);

			if(neighbor1 != parent && neighbor1 != -1){
				neighbors = mergeList(neighbors, relevantNeighbors(point, neighbor1, triangleIndex));
			}
			if(neighbor2 != parent && neighbor2 != -1){
				neighbors = mergeList(neighbors, relevantNeighbors(point, neighbor2, triangleIndex));
			}
			if(neighbor3 != parent && neighbor3 != -1){
				neighbors = mergeList(neighbors, relevantNeighbors(point, neighbor3, triangleIndex));
			}

			return neighbors;  
		}

		int[] mergeList (int[] a, int[] b){
			if (a == null) {
				return b;
			}
			if (b == null) {
				return a;
			}
			int newSize = a.Length + b.Length;
			int[] newList = new int[newSize];
			for(int i = 0; i < newSize; i ++){
				if(i < a.Length){
					newList[i] = a[i];
				}
				else{
					newList[i] = b[i - a.Length];
				}
			}
			return newList;
		}

		int[] findHull (int[] input) {
			List<int> edgeList = new List<int>();
			int[] occurances = new int[input.Length*3];//make sure we have enough space, we won't need this much
			//0 in this case means it found it once. 1 on this list means it found it once to initialize the edge and once again

			//find all unique edges and count uccurances
			for(int i = 0; i < input.Length; i ++){
				Triangle thisInputTriangle = triangles [input [i]];
				int[] indexes = new int[3];
				indexes[0] = edgeList.IndexOf(thisInputTriangle.edgeList[0]);//returns the index of b in a if it exists, otherwise -1
				indexes[1] = edgeList.IndexOf(thisInputTriangle.edgeList[1]);//returns the index of b in a if it exists, otherwise -1
				indexes[2] = edgeList.IndexOf(thisInputTriangle.edgeList[2]);//returns the index of b in a if it exists, otherwise -1

				for(int j = 0; j < 3; j ++) {
					if(indexes[j] == -1){
						edgeList.Add(thisInputTriangle.edgeList[j]);
					} else {
						occurances[indexes[j]] ++;
					}
				}
			}
			//once we have tallied all the edge occurances, delete all that have more than 1.. backwards iteration because we remove items in the list as we go
			for(int i = edgeList.Count-1; i >= 0; i --){
				if(occurances[i] > 0){
					edgeList.RemoveAt(i);
				}
			}
			//sort edges such that they are in order
			int[] sortedEdgeList = new int[edgeList.Count];
			sortedEdgeList[0] = edgeList[0];
			edgeList.RemoveAt(0);

			for(int i = 1; i < sortedEdgeList.Length; i ++){//go through each item in the emtpy sorted array
				for(int j = 0; j < edgeList.Count; j ++){//find the next element and pop it from the unsorted array
					//if(edges [ sortedEdgeList[i-1] ].isAdjacent(edgeList[j])){
					if(isEdgeAdjacent(sortedEdgeList[i-1], edgeList[j])){
						sortedEdgeList[i] = edgeList[j];
						edgeList.RemoveAt(j);
						break;
					}
				}
			}
			return sortedEdgeList;
		}
	//	public int totalIterations = 0;
		public int findTriangle (Vector2 target) {
			if(triangles.Count == 0){
				return -1;
			}

			int current = triangles.Count-1;//start at the end of the list becuase they are most likely to not be "removed"
			//renderTriangle(current);


			int iterator = 0;

			while(iterator < triangles.Count){
				Triangle currentTriangle = triangles[current];
				iterator ++;
				if(inTriangle(target, current)){//if we reach target than stop
					//print("findTriangle(): iterator = " + iterator + " successfully found triangle");
				//	totalIterations += iterator;
					return current;
				}
				Vector3 point1 =  (points[currentTriangle.pointList[0]]);
				Vector3 point2 =  (points[currentTriangle.pointList[1]]);
				Vector3 point3 =  (points[currentTriangle.pointList[2]]);

				Vector2[] edgeDir = new Vector2[3];
				edgeDir[0] = -new Vector2(point1.z - point2.z, point2.x - point1.x);
				edgeDir[1] = -new Vector2(point2.z - point3.z, point3.x - point2.x);
				edgeDir[2] = -new Vector2(point3.z - point1.z, point1.x - point3.x);//direction "normal" of each edge

				Vector2 currentCenter = new Vector2((point1.x + point2.x + point3.x)/3.0f, (point1.z + point2.z + point3.z)/3.0f); //center of the triangle we are on
				Vector2 targetDir = new Vector2(target.x - currentCenter.x, target.y - currentCenter.y);//the direction from this triangle to target point

				float closestAngle = -1;
				int closestDir = 0;
				for(int i = 0; i < 3; i ++){
					float angleTo = Vector2.Dot(edgeDir[i], targetDir);
					if(angleTo > closestAngle){
						closestAngle = angleTo;
						closestDir = i;
					}
				}

				current = getNeighbor(current, closestDir);
				if(current == -1){
					//print("findTriangle() next was null");
					return -1;
				}
			}
			print ("findTriangle() ran of out of iterations. target = " + target);
			return -1;
		}

		public Vector2 convertVector (Vector3 input) {
			return new Vector2 (input.x, input.z);
		}

		public Vector3 convertVector (Vector2 input) {
			return new Vector3 (input.x, 0, input.y);
		}

		public Vector3 GetTriangleCenter (int index) {
			Triangle thisTriangle = triangles [index];
			return (points [thisTriangle.pointList [0]] + points [thisTriangle.pointList [1]] + points [thisTriangle.pointList [2]]) / 3;
		}

		int getNeighbor (int thisTriangleIndex, int side) {
			Triangle thisTriangle = triangles [thisTriangleIndex];
			Edge thisEdge = edges [thisTriangle.edgeList [side]];
			if(thisEdge.triangles.Count == 2){
				
				int neighbor = thisEdge.triangles [0];
				if (neighbor == thisTriangleIndex) {
					neighbor = thisEdge.triangles [1];
				}



				return neighbor;
			}
			else{
				//  println("getNeighbor() this edges's triangle list was not 2");
				return -1;
			}
		}
		//is point inside the triangle at index (triangle index)?
		bool inTriangle (Vector2 point, int triangleIndex){
			Triangle thisTriangle = triangles [triangleIndex];
			//the coordinates of the point we want to check 
			float px = point.x;
			float py = point.y;

			//the coordinates of the triangles vertices
			// triangle thisTriangle = triangles.get(triangleIndex);
			Vector3 p0 =  (points[thisTriangle.pointList[0]]);
			Vector3 p1 =  (points[thisTriangle.pointList[1]]);
			Vector3 p2 =  (points[thisTriangle.pointList[2]]);

			float p0x = p0.x;
			float p0y = p0.z;

			float p1x = p1.x;
			float p1y = p1.z;

			float p2x = p2.x;
			float p2y = p2.z;

			float Area = 0.5f *(-p1y*p2x + p0y*(-p1x + p2x) + p0x*(p1y - p2y) + p1x*p2y);
			float s = 1/(2*Area)*(p0y*p2x - p0x*p2y + (p2y - p0y)*px + (p0x - p2x)*py);
			float t = 1/(2*Area)*(p0x*p1y - p0y*p1x + (p0y - p1y)*px + (p1x - p0x)*py);

			if(s > 0 && t > 0 && (1-s-t) > 0){
				return true;
			}
			return false;
		}

		//add a triangle to the triangulation list
		//also set edge neighbor lists

		public void addTriangle (int p1, int p2, int p3, int e1, int e2, int e3) {
			Triangle newTriangle = new Triangle (p1, p2, p3, e1, e2, e3);
			triangles.Add (newTriangle);
			int newTriangleIndex = triangles.Count - 1;
			findCircumCircle (newTriangle);

			Edge[] realEdges = new Edge[3];
			realEdges[0] = edges [e1];
			realEdges[1] = edges [e2];
			realEdges[2] = edges [e3];

			realEdges[0].triangles.Add(newTriangleIndex);
			realEdges[1].triangles.Add(newTriangleIndex);
			realEdges[2].triangles.Add(newTriangleIndex);

			//print ("what are we adding to edge list = " + newTriangleIndex + " triangleCount = " + triangles.Count);

			Vector3 point0 =  (points[newTriangle.pointList[0]]);
			Vector3 point1 =  (points[newTriangle.pointList[1]]);
			Vector3 point2 =  (points[newTriangle.pointList[2]]);

			Vector3 a = (point1 - point0);
			Vector3 b = (point2 - point1);
			Vector3 cross = Vector3.Cross(a, b);
			//print ("cross = " + cross);

			//print (cross.z);
			if(cross.y > 0){
				int backup = newTriangle.pointList[1];
				newTriangle.pointList[1] = newTriangle.pointList[2];//swap second and third point if not counter clockwise
				newTriangle.pointList[2] = backup;
				// println("Rotated Triangle " + cross);
			}

			int[] rearrangedEdges = new int[3];
			for(int i = 0; i < 3; i ++){
				if(realEdges[i].Contains(newTriangle.pointList[0], newTriangle.pointList[1])){
					rearrangedEdges[0] = newTriangle.edgeList[i];
				}
				if(realEdges[i].Contains(newTriangle.pointList[1], newTriangle.pointList[2])){
					rearrangedEdges[1] = newTriangle.edgeList[i];
				}
				if(realEdges[i].Contains(newTriangle.pointList[0], newTriangle.pointList[2])){
					rearrangedEdges[2] = newTriangle.edgeList[i];
				}
			}
			newTriangle.edgeList = rearrangedEdges;


		}
		//do the the edges at these two indexes share a vertex?
		public bool isEdgeAdjacent (int some, int other) {
			Edge edge1 = edges [some];
			Edge edge2 = edges [other];
			if(edge1.point1 == edge2.point1 || edge1.point1 == edge2.point2 || edge1.point2 == edge2.point1 || edge1.point2 == edge2.point2){
				return true;
			} 
			return false;
		}

		public void findCircumCircle (Triangle thisTriangle) {
			Vector3 point1 =  (points [thisTriangle.pointList [0]]);
			Vector3 point2 =  (points [thisTriangle.pointList [1]]);
			Vector3 point3 =  (points [thisTriangle.pointList [2]]);



			Vector3 midEdge1 = (point1 + point2) / 2f;
			float perpendicularLineSlope1 = -1/getSlope(point1, point2);
			float yIntercept1 = midEdge1.z - perpendicularLineSlope1 * midEdge1.x;


			Vector3 midEdge2 = (point2 + point3) / 2f;
			float perpendicularLineSlope2 = -1/getSlope(point2, point3);
			float yIntercept2 = midEdge2.z - perpendicularLineSlope2 * midEdge2.x;


			float xIntersect = 0;//(yIntercept2 - yIntercept1) / (perpendicularLineSlope1 - perpendicularLineSlope2);  
			float yIntersect = 0;//perpendicularLineSlope1 * xIntersect + yIntercept1;

			if(point1.x == point2.x){
				if(point2.z == point3.z){//vertical and horizontal
					xIntersect = midEdge2.x;
					yIntersect = midEdge1.z;
				} else {//vertical and other
					xIntersect = (midEdge1.z - yIntercept2) / perpendicularLineSlope2;
					yIntersect = midEdge1.z;
				}
			} else if ( point1.z == point2.z) {
				if(point2.x == point3.x){//horizontal and verticle
					xIntersect = midEdge1.x;
					yIntersect = midEdge2.z;
				} else {//horizontal and other
					xIntersect = midEdge1.x;
					yIntersect = perpendicularLineSlope2 * xIntersect + yIntercept2;
				}
			} else {//first line is "other"
				if(point2.x == point3.x){//other and vertical
					xIntersect = (midEdge2.z - yIntercept1) / perpendicularLineSlope1;
					yIntersect = midEdge2.z;
				} else if(point2.z == point3.z){//other and horizontal
					xIntersect = midEdge2.x;
					yIntersect = perpendicularLineSlope1 * midEdge2.x + yIntercept1;
				} else {//other and other
					xIntersect = (yIntercept2 - yIntercept1) / (perpendicularLineSlope1 - perpendicularLineSlope2);  
					yIntersect = perpendicularLineSlope1 * xIntersect + yIntercept1;
				}
			}

			if (point1 == point2) {
				
			}

			thisTriangle.circumCircleCenter = new Vector2 (xIntersect, yIntersect);
			thisTriangle.circumCircleRadius = Vector2.Distance(convertVector(point1), new Vector2 (xIntersect, yIntersect));
		}

		float getSlope (Vector3 p1, Vector3 p2) {
			float slope = (p1.z - p2.z)/(p1.x - p2.x);
			return slope;
		}

		float getTrianglePerimeter (Triangle someTriangle) {
			Vector3 point1 = points [someTriangle.pointList [0]];
			Vector3 point2 = points [someTriangle.pointList [1]];
			Vector3 point3 = points [someTriangle.pointList [2]];

			float left = Mathf.Min (Mathf.Min (point1.x, point2.x), point3.x);
			float right = Mathf.Max (Mathf.Max (point1.x, point2.x), point3.x);

			float down = Mathf.Min (Mathf.Min (point1.y, point2.y), point3.y);
			float up = Mathf.Max (Mathf.Max (point1.y, point2.y), point3.y);

			return (right - left) * (up - down) * 0.5f;
		}
	}

	public class Triangle {
		public bool active = true; 
		public int[] pointList;
		public int[] edgeList;

		public Vector2 circumCircleCenter;//x,y location plus radious as z
		public float circumCircleRadius;

		public Triangle (int p1, int p2, int p3, int e1, int e2, int e3) {
			pointList = new int[3];
			edgeList = new int[3];

			edgeList[0] = e1;
			edgeList[1] = e2;
			edgeList[2] = e3;

			pointList[0] = p1;
			pointList[1] = p2;
			pointList[2] = p3;
		}

		public Triangle (string str) {
			pointList = new int[3];
			edgeList = new int[3];

			char[] splitChars = {','};
			string[] properties = str.Split(splitChars);

			if(properties[0].Equals("t")){
				active = true;
			}
			else{
				active = false;
			}
			for (int i = 0; i < 3; i++) {
				pointList[i] = int.Parse(properties[i + 1]);
			}
			for (int i = 0; i < 3; i++) {
				edgeList[i] = int.Parse(properties[i + 4]);
			}
			circumCircleCenter.x = float.Parse(properties[7]);
			circumCircleCenter.y = float.Parse(properties[8]);
			circumCircleRadius = float.Parse(properties[9]);
		}

		public string ToString () {
			string str = "";
			str += active ? "t," : "f,";
			for (int i = 0; i < 3; i++) {
				str += pointList [i] + ",";
			}
			for (int i = 0; i < 3; i++) {
				str += edgeList [i] + ",";
			}
			str += circumCircleCenter.x + ",";
			str += circumCircleCenter.y + ",";
			str += circumCircleRadius;
			return str;
		}
	}

	public class Edge {
		public int point1;
		public int point2;

		public List<int> triangles;//a most 2 at least 1

		public Edge (int p1, int p2){
			point1 = p1;
			point2 = p2;
			triangles = new List<int>();
		}

		public Edge (string str){
			char[] splitChars = {','};
			string[] properties = str.Split(splitChars);
			triangles = new List<int>();

			point1 = int.Parse(properties[0]);
			point2 = int.Parse(properties[1]);

			for(int i = 2; i < properties.Length; i ++) {
				triangles.Add(int.Parse(properties[i]));
			}
		}

		public bool Contains (int p1, int p2){
			if((point1 == p1 || point2 == p1) && (point1 == p2 || point2 == p2)){
				return true;
			}
			else{
				return false;
			}
		}

		public string ToString () {
			string str = "";
			str += point1 + ",";
			str += point2;

			int triCount = this.triangles.Count;

			if (triCount > 0) {
				str += ",";
				for (int i = 0; i < triCount; i++) {
					str += this.triangles [i];
					if (i < triCount - 1) {
						str += ",";
					}
				}
			}
			return str;
		}
	}

}
