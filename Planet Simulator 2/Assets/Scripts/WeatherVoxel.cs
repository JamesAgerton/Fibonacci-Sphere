using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using UnityEngine;

namespace WV
{
	public class WeatherVoxels
	{
		public List<WeatherVoxel> voxels;

		public WeatherVoxels(Vector3[] vertices, int[] triangles)
		{
			voxels = new List<WeatherVoxel>();

			for (int i = 0; i < vertices.Length; i++)
			{
				AddVoxelFromVertex(i, vertices);
			}
			for (int i = 0; i < voxels.Count; i++)
			{
				voxels[i].Set_Neighbors(GetAdjascentVoxels(voxels[i].Get_Index(), vertices, triangles));
			}
		}

		public void AddVoxel(Vector3 position)
		{
			voxels.Add(new WeatherVoxel(voxels.Count, position));
		}

		public void AddVoxelFromVertex(int index, Vector3[] vertices)
		{
			voxels.Add(new WeatherVoxel(index, vertices[index]));
		}

		public List<Vector3> GetAdjascentPositions(int index, Vector3[] verts, int[] triangles)
		{
			List<int> tris = GetAdjascentIndexes(index, verts, triangles);
			List<Vector3> result = new List<Vector3>();

			foreach (int point in tris)
			{
				result.Add(verts[point]);
			}

			return result;
		}

		public List<int> GetAdjascentIndexes(int index, Vector3[] verts, int[] triangles)
		{
			List<int> tris = new List<int>();

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

			for(int i = 0; i < tris.Count; i++)
            {
				//UnityEngine.Debug.Log(index.ToString() + "=" + tris[i].ToString());
				if(tris[i] == index)
                {
					tris.RemoveAt(i);
                }
            }

			return tris;
		}

		public List<WeatherVoxel> GetAdjascentVoxels(int index, Vector3[] verts, int[] triangles)
		{
			List<WeatherVoxel> WVs = new List<WeatherVoxel>();
			List<int> indexes = GetAdjascentIndexes(index, verts, triangles);

			foreach (int ind in indexes)
			{
				WVs.Add(voxels[ind]);
			}

			return WVs;
		}

		public void FillTexCoords(GameObject planet)
        {
			foreach(WeatherVoxel Wv in voxels)
            {
				Wv.Set_TexCoord(planet);
            }
        }


		public class WeatherVoxel
		{
			public WeatherVoxel(int i, Vector3 pos)
			{
				index = i;
				position = pos;
				neighbors = new List<WeatherVoxel>();
				texCoord = new Vector2();
			}

			int index;
			Vector3 position;
			List<WeatherVoxel> neighbors;
			Vector2 texCoord;

			public int Get_Index() { return index; }
			public Vector3 Get_Position() { return position; }
			public List<WeatherVoxel> Get_Neighbors() { return neighbors; }
			public WeatherVoxel Get_Neighbor(int index) { return neighbors[index]; }
			public Vector2 Get_TexCoord2() { return texCoord; }
			public Vector3 Get_TexCoord3() { return new Vector3(texCoord.x, texCoord.y, 0f); }
			
			public bool Set_Index(int i) 
			{ 
				index = i;
				if (index == i)
					return true;
				return false;
			}
			public bool Set_Position( Vector3 pos)
            {
				position = pos;
				if (position == pos)
					return true;
				return false;
            }
			public void Add_Neighbor(WeatherVoxel wv)
            {
				neighbors.Add(wv);
            }
			public void Set_Neighbors(List<WeatherVoxel> WVs)
            {
				neighbors = WVs;
            }
			public void Set_TexCoord(GameObject planet)
            {
				LayerMask mask = LayerMask.GetMask("Planet");
				RaycastHit hit;
				if(Physics.Raycast(planet.transform.TransformPoint(Get_Position() * 1.1f),
					-planet.transform.TransformPoint(Get_Position() * 1.1f - planet.transform.position),
					out hit, 0.2f, mask)) 
				{
					//UnityEngine.Debug.Log("I GOT IT!");
					texCoord = hit.textureCoord;
                }
				//else
					//UnityEngine.Debug.Log("I didn't get it");
            }
        }
    }
}