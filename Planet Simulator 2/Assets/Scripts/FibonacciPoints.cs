using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using GK;
using System.Diagnostics;

namespace FP
{
	public class FibonacciPoints3D
	{
		private static float gr = (Mathf.Sqrt(5) + 1) / 2; // golden ratio = 1.6180339887
		private static float ga = (2f - gr) * (2f * Mathf.PI); // golden angle = 2.3999632297
		public float GoldenRatio { get { return gr; } }
		public float GoldenAngle { get { return ga; } }
		
		private DelaunayCalculator Dc;
		private DelaunayTriangulation Dt;
		
		public List<Vector3> Points3D;
		public List<Vector2> Points2D;
		public List<Vector3> PointsM;
		public List<Vector3> PointsU;

		public int[] Triangles;
		public Vector3[] Vertices;
		
		public FibonacciPoints3D()
		{
			Points3D = new List<Vector3>();
			Points2D = new List<Vector2>();
			PointsM = new List<Vector3>();
			PointsU = new List<Vector3>();

			Triangles = new int[Points3D.Count * 3];
			Vertices = new Vector3[Points3D.Count];
			
			Dc = new DelaunayCalculator();
			Dt = null;

			Update_Points(50, 0.5f);
		}

		public FibonacciPoints3D(int numPoints, float radius)
		{
			Points3D = new List<Vector3>();
			Points2D = new List<Vector2>();
			PointsM = new List<Vector3>();
			PointsU = new List<Vector3>();

			Triangles = new int[Points3D.Count * 3];

			Dc = new DelaunayCalculator();
			Dt = null;

			Update_Points(numPoints, radius);
		}

		public void Update_Points(int numPoints, float radius)
        {
			//Override to avoid too small number/radius
			if (numPoints < 50)
			{
				numPoints = 50;
			}
			if (radius < 0.01f)
			{
				radius = 0.01f;
			}

			Fibonacci_Spiral_Sphere(numPoints, radius);
			Stitch_Bottom(ref Triangles, ref Vertices, radius);

			//Mercator_Project_Sphere(Vector3.right, radius, new Vector2(1, 1));
			//UV_Project_Sphere();
		}

		void Stitch_Bottom(ref int[] tris, ref Vector3[] verts, float radius)
		{
			//UnityEngine.Debug.Log("Before " + vertices.Length);

			Vector3 sPole = new Vector3(0, 0 - radius, 0);

			Vector3[] newVerts = new Vector3[verts.Length + 1];
			for (int i = 0; i < verts.Length; i++)
			{
				newVerts[i] = verts[i];
			}
			newVerts[newVerts.Length - 1] = sPole;

			//for simplicity's sake, I've just added the final point to Points3D
			Points3D.Add(sPole);

			verts = newVerts;

			//UnityEngine.Debug.Log("After " + vertices.Length);

			int[] newTris = new int[tris.Length + 15];
			for (int i = 0; i < tris.Length; i++)
			{
				newTris[i] = tris[i];
			}

			newTris[tris.Length] = verts.Length - 2;
			newTris[tris.Length + 1] = verts.Length - 4;
			newTris[tris.Length + 2] = verts.Length - 1;

			newTris[tris.Length + 3] = verts.Length - 4;
			newTris[tris.Length + 4] = verts.Length - 6;
			newTris[tris.Length + 5] = verts.Length - 1;

			newTris[tris.Length + 6] = verts.Length - 6;
			newTris[tris.Length + 7] = verts.Length - 3;
			newTris[tris.Length + 8] = verts.Length - 1;

			newTris[tris.Length + 9] = verts.Length - 3;
			newTris[tris.Length + 10] = verts.Length - 5;
			newTris[tris.Length + 11] = verts.Length - 1;

			newTris[tris.Length + 12] = verts.Length - 5;
			newTris[tris.Length + 13] = verts.Length - 2;
			newTris[tris.Length + 14] = verts.Length - 1;

			tris = newTris;
		}

		public List<Vector3> Fibonacci_Spiral_Sphere(int num_Points3D, float r)
		{
			List<Vector3> positions = new List<Vector3>();
			for (int i = 0; i < num_Points3D; i++)
			{
				float lat = Mathf.Asin((float)-1.0 + (float)2.0 * (float)i / (num_Points3D + 1));
				float lon = ga * (float)i;

				float x = Mathf.Cos(lon) * Mathf.Cos(lat);
				float y = Mathf.Sin(lon) * Mathf.Cos(lat);
				float z = Mathf.Sin(lat);
				
				Vector3 pos = (new Vector3(x, y, z) * r);

				positions.Add(Rotate_Points(pos, new Vector3(0f, 0f, 90f)));
			}
			
			Points3D = positions;
			Vertices = Points3D.ToArray();
			
			Stereograph_Project_Sphere(r);
			Dt = Dc.CalculateTriangulation(Points2D);
			Triangles = Dt.Triangles.ToArray();
			
			return positions;
		}
		
		private Vector3 Rotate_Points(Vector3 vec, Vector3 R)
		{
			Vector3 rot = new Vector3(R.x * (Mathf.PI / 180f), R.y * (Mathf.PI / 180f), R.z * (Mathf.PI / 180f));

			float x = Mathf.Cos(rot.x) * Mathf.Cos(rot.y) * vec.x + 
				(Mathf.Cos(rot.x) * Mathf.Sin(rot.y) * Mathf.Sin(rot.z) - Mathf.Sin(rot.x) * Mathf.Cos(rot.z)) * vec.y + 
				(Mathf.Cos(rot.x) * Mathf.Sin(rot.y) * Mathf.Cos(rot.z) + Mathf.Sin(rot.x) * Mathf.Sin(rot.z)) * vec.z;
			float y = Mathf.Sin(rot.x) * Mathf.Cos(rot.y) * vec.x +
				(Mathf.Sin(rot.x) * Mathf.Sin(rot.y) * Mathf.Sin(rot.z) + Mathf.Cos(rot.x) * Mathf.Cos(rot.z)) * vec.y +
				(Mathf.Sin(rot.x) * Mathf.Sin(rot.y) * Mathf.Cos(rot.z) - Mathf.Cos(rot.x) * Mathf.Sin(rot.z)) * vec.z; ;
			float z = (-1f * Mathf.Sin(rot.y)) * vec.x +
				(Mathf.Cos(rot.y) * Mathf.Sin(rot.z)) * vec.y +
				(Mathf.Cos(rot.y) * Mathf.Cos(rot.z)) * vec.z;
			
			return new Vector3(x,y,z);
		}
		
		public List<Vector2> Stereograph_Project_Sphere(float radius)
		{
			List<Vector2> positions = new List<Vector2>();
			for (int i = 0; i < Points3D.Count; i++)
			{
				positions.Add(Stereograph_Projection(Points3D[i], radius));
			}
			
			Points2D = positions;
			return positions;
		}

		public Vector2 Stereograph_Projection(Vector3 point3, float radius)
		{
			float x = point3.x / (point3.y + radius);
			float y = point3.z / (point3.y + radius);

			return new Vector2(y, x);
		}
		
		//public List<Vector3> Mercator_Project_Sphere(Vector3 PrimeMeridian, float radius, Vector2 scale)
		//{
		//	List<Vector3> positions = new List<Vector3>();
		//	float yMin = 0;
		//	float yMax = 0;
		//	float xMin = 0;
		//	float xMax = 0;

		//	for(int i = 0; i < Points3D.Count; i++)
		//	{
		//		positions.Add(Mercator_Projection(Points3D[i], PrimeMeridian, radius));

		//		if(positions[i].y < yMin) yMin = positions[i].y;
		//		if(positions[i].y > yMax) yMax = positions[i].y;
		//		if(positions[i].x < xMin) xMin = positions[i].x;
		//		if(positions[i].x > xMax) xMax = positions[i].x;
		//	}

		//	//UnityEngine.Debug.Log("y " + yMin + ":" + yMax + " | " + xMin + ":" + xMax);

		//	positions[0] = new Vector3(0f * scale.x, 1f * scale.y, 0);
		//	for(int i = 1; i < Points3D.Count; i++)
  //          {
		//		float x = ((positions[i].x - xMin) / (xMax - xMin)) * scale.x;
		//		float y = ((positions[i].y - yMin) / (yMax - yMin)) * scale.y;

		//		positions[i] = new Vector3(x, y, 0);
  //          }
			
		//	PointsM = positions;
		//	return positions;
		//}
		
		//public Vector3 Mercator_Projection(Vector3 point3, Vector3 PrimeMeridian, float radius)
		//{
		//	///This Mercator Projection uses the provided Prime Meridian (i.e. the 
		//	///transform.right of the parent object.
			
		//	float PM = Mathf.Atan2(PrimeMeridian.z, PrimeMeridian.x);
			
		//	float lat = Mathf.Asin(point3.y / radius);
		//	float lon = Mathf.Atan2(point3.z, point3.x);
			
		//	//Debug.Log(point3 + " | " + lat + " " + lon);
			
		//	float x = lon - PM;
		//	float y = Mathf.Log(Mathf.Tan(lat) + 1/Mathf.Cos(lat));
			
		//	return new Vector3(x, y, 0);
		//}

		//public List<Vector3> UV_Project_Sphere()
  //      {
		//	List<Vector3> positions = new List<Vector3>();

		//	for(int i = 0; i < Points3D.Count; i++)
  //          {
		//		positions.Add(UV_Projection(Points3D[i]));
  //          }

		//	PointsU = positions;

		//	return positions;
  //      }

		//public Vector3 UV_Projection(Vector3 point3)
  //      {
		//	float u = 0.5f + ((Mathf.Atan2(point3.x, point3.z)) / (2 * Mathf.PI));
		//	float v = 0.5f + (Mathf.Atan2(point3.y, Mathf.Sqrt(point3.x * point3.x + point3.z * point3.z)) / Mathf.PI);

		//	return new Vector3(u, v, 0);
  //      }

		//public void Make_Mesh(ref Mesh mesh, int[] tris, Vector3[] verts)
		//{
		//	mesh.Clear();
		//	mesh.vertices = verts;
		//	mesh.triangles = tris;
		//	mesh.RecalculateNormals();

		//	Vector2[] M = new Vector2[mesh.vertices.Length];

		//	//Mercator Projection
		//	for (int i = 0; i < PointsM.Count - 1; i++)
		//	{
		//		M[i] = new Vector2(PointsM[i].x, PointsM[i].y);
		//	}
		//	M[PointsM.Count - 1] = new Vector2(1f, 0);

		//	/*
		//	// UV Projection
		//	List<Vector2> Mm = Fp.UV_Project_Sphere();
		//	for(int i = 0; i < Mm.Count - 1; i++)
		//	{
		//		M[i] = Mm[i];
		//	}
		//	M[vertices.Length - 1] = new Vector2(
		//		0.5f + ((Mathf.Atan2(vertices[vertices.Length - 1].z, vertices[vertices.Length - 1].x)) / (2 * Mathf.PI)), 
		//		0.5f - ((Mathf.Asin(vertices[vertices.Length - 1].y))/ (Mathf.PI))
		//		);
		//	*/

		//	mesh.uv = M;
		//}
		//public int[] Detect_Wrapped_UV_Coords(Mesh mesh)
  //      {
		//	List<int> indices = new List<int>();
		//	for (int i = 0; i < mesh.triangles.Length / 3; i++)
  //          {
		//		int a = mesh.triangles[(i * 3)];
		//		int b = mesh.triangles[(i * 3) + 1];
		//		int c = mesh.triangles[(i * 3) + 2];
		//		Vector3 texA = (Vector3)mesh.uv[a];
		//		Vector3 texB = (Vector3)mesh.uv[b];
		//		Vector3 texC = (Vector3)mesh.uv[c];

		//		Vector3 texNormal = Vector3.Cross(texB - texA, texC - texA);
		//		if (texNormal.z > 0)
		//			indices.Add(i);
  //          }
			
		//	//UnityEngine.Debug.Log(indices.Count);
		//	return indices.ToArray();
  //      }


		//public void Fix_Wrapped_UV(int[] wrapped, ref Mesh mesh)
  //      {
		//	List<Vector3> vertices = new List<Vector3>(mesh.vertices);
		//	int verticeIndex = (int)vertices.Count - 1;
		//	Dictionary<int, int> visited = new Dictionary<int, int>();

		//	foreach(int i in wrapped)
  //          {
		//		int a = mesh.triangles[(i * 3)];
		//		int b = mesh.triangles[(i * 3) + 1];
		//		int c = mesh.triangles[(i * 3) + 2];

		//		Vector2 A = mesh.uv[a];
		//		Vector2 B = mesh.uv[b];
		//		Vector2 C = mesh.uv[c];

		//		if(A.x < 0.2f)
  //              {
		//			int tempA = a;
		//			if(!visited.TryGetValue(a, out tempA))
  //                  {
		//				A.x += 1;
		//				vertices.Add(mesh.vertices[a]);
		//				verticeIndex++;
		//				visited[a] = verticeIndex;
		//				tempA = verticeIndex;
  //                  }
		//			a = tempA;
  //              }
		//		if(B.x < 0.2f)
  //              {
		//			int tempB = b;
		//			if (!visited.TryGetValue(b, out tempB))
		//			{
		//				B.x += 1;
		//				vertices.Add(mesh.vertices[b]);
		//				verticeIndex++;
		//				visited[b] = verticeIndex;
		//				tempB = verticeIndex;
		//			}
		//			b = tempB;
		//		}
		//		if (C.x < 0.2f)
		//		{
		//			int tempC = c;
		//			if (!visited.TryGetValue(c, out tempC))
		//			{
		//				C.x += 1;
		//				vertices.Add(mesh.vertices[c]);
		//				verticeIndex++;
		//				visited[c] = verticeIndex;
		//				tempC = verticeIndex;
		//			}
		//			c = tempC;
		//		}
				
		//		mesh.triangles[(i * 3)] = a;
		//		mesh.triangles[(i * 3) + 1] = b;
		//		mesh.triangles[(i * 3) + 2] = c;

		//		mesh.uv[mesh.triangles[(i * 3)]] = A;
		//		mesh.uv[mesh.triangles[(i * 3) + 1]] = B;
		//		mesh.uv[mesh.triangles[(i * 3) + 2]] = C;
		//	}
		//	mesh.vertices = vertices.ToArray();
  //      }
	}
}
