using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System;
using UnityEditor;
using System.Diagnostics;
using FP;
using WV;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

//[RequireComponent(typeof(MeshFilter))]
public class FSphere : MonoBehaviour
{
	public int Numpoints = 10;
	private int m_Numpoints = 10;
	public float Radiuspoints = 0.5f;
	private float m_Radiuspoints = 0.5f;

	private FibonacciPoints3D Fp;
	public WeatherVoxels Wv;

	//public enum Projection { Stereographic, Mercator, UV };
	//public Projection Projectiontype;
	public Transform Map;

	public int SelectedPoint = 0;

	// Start is called before the first frame update
	void Start()
	{
		Fp = new FibonacciPoints3D(Numpoints, Radiuspoints);
		Wv = new WeatherVoxels(Fp.Points3D.ToArray(), Fp.Triangles);

		//Projectiontype = Projection.Mercator;
	}

	// Update is called once per frame
	void Update()
	{
		if (Numpoints != m_Numpoints || Radiuspoints != m_Radiuspoints)
		{
			m_Numpoints = Numpoints; m_Radiuspoints = Radiuspoints;

			Fp.Update_Points(Numpoints, Radiuspoints);

			Wv = new WeatherVoxels(Fp.Vertices, Fp.Triangles);
			Wv.FillTexCoords(this.gameObject);

			//for (int i = 0; i < Wv.voxels[SelectedPoint].Get_Neighbors().Count; i++)
			//{
			//	UnityEngine.Debug.Log(Wv.voxels[SelectedPoint].Get_Neighbor(i).Get_Index());
			//}
		}

		if (SelectedPoint > Numpoints)
			SelectedPoint = Numpoints;
		else if (SelectedPoint < 0)
			SelectedPoint = 0;
	}

	public List<Vector3> GetAdjascentPositions(int index, Vector3[] verts, int[] triangles)
	{
		List<int> tris = new List<int>();
		List<Vector3> result = new List<Vector3>();

		for (int j = 0; j < triangles.Length; j++)
		{
			if (triangles[j] == Mathf.FloorToInt(index))
			{
				switch (j % 3)
				{
					case (0):
						if (!tris.Contains(triangles[j])) { tris.Add(triangles[j]); }
						if (!tris.Contains(triangles[j + 1])) { tris.Add(triangles[j + 1]); }
						if (!tris.Contains(triangles[j + 2])) { tris.Add(triangles[j + 2]); }
						break;
					case (1):
						if (!tris.Contains(triangles[j - 1])) { tris.Add(triangles[j - 1]); }
						if (!tris.Contains(triangles[j])) { tris.Add(triangles[j]); }
						if (!tris.Contains(triangles[j + 1])) { tris.Add(triangles[j + 1]); }
						break;
					case (2):
						if (!tris.Contains(triangles[j - 2])) { tris.Add(triangles[j - 2]); }
						if (!tris.Contains(triangles[j - 1])) { tris.Add(triangles[j - 1]); }
						if (!tris.Contains(triangles[j])) { tris.Add(triangles[j]); }
						break;
					default:
						UnityEngine.Debug.LogError("wierd stuff happened while looking through triangles array.");
						break;
				}
			}
		}
		foreach (int point in tris)
		{
			result.Add(transform.TransformPoint(verts[point]));
		}

		return result;
	}


	void OnDrawGizmos()
	{
		if (Fp != null)
		{

			for (int i = 0; i < Wv.voxels.Count; i++)
			{

				//Show selection
				if (i == 0)
				{
					Gizmos.color = Color.cyan;
				}
				//Highlight the final five points
				else if (i > Wv.voxels.Count - 6 && i < Fp.Vertices.Length - 1)
				{
					Gizmos.color = Color.red;
				}

				//Highlight the final point
				else if (i == Fp.Vertices.Length - 1)
				{
					Gizmos.color = Color.cyan;
				}

				//Otherwise make them lerp between two colors
				else
				{
					Gizmos.color = Color.Lerp(Color.green, Color.blue,
						((float)i / (float)Wv.voxels.Count));
				}

				if ((i == SelectedPoint))
				{
					Gizmos.color = Color.white;
					for (int j = 0; j < Wv.voxels[SelectedPoint].Get_Neighbors().Count; j++)
					{
						Gizmos.DrawWireSphere(transform.TransformPoint(Wv.voxels[SelectedPoint].Get_Neighbor(j).Get_Position())
							, 0.02f);
					}
				}

				//Draw 3D points on the sphere
				Gizmos.DrawSphere(transform.TransformPoint(Wv.voxels[i].Get_Position()), 0.01f);
				//Gizmos.DrawRay(transform.TransformPoint(Wv.voxels[i].Get_Position() * 1.1f),
				//	-transform.TransformPoint(Wv.voxels[i].Get_Position() * 1.1f - transform.position));

				Gizmos.DrawSphere(Map.transform.TransformPoint(Wv.voxels[i].Get_TexCoord3() - 
					new Vector3(0.5f, 0.5f, 0f)), 0.01f);
			}
		}
	}
}

