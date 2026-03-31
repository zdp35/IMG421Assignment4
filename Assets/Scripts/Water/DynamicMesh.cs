using UnityEngine;

namespace LanternDrift.Water
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class DynamicMesh : MonoBehaviour
    {
        [Min(1)] public int width = 10;
        [Min(1)] public int height = 10;

        public Mesh GenerateGridMesh(Mesh target = null)
        {
            int vertCountX = width + 1;
            int vertCountZ = height + 1;

            Vector3[] vertices = new Vector3[vertCountX * vertCountZ];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[width * height * 6];

            int vi = 0;
            float offsetX = -width * 0.5f;
            float offsetZ = -height * 0.5f;

            for (int z = 0; z < vertCountZ; z++)
            {
                for (int x = 0; x < vertCountX; x++)
                {
                    vertices[vi] = new Vector3(x + offsetX, 0f, z + offsetZ);
                    uvs[vi] = new Vector2((float)x / width, (float)z / height);
                    vi++;
                }
            }

            int ti = 0;
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int v00 = z * vertCountX + x;
                    int v10 = v00 + 1;
                    int v01 = v00 + vertCountX;
                    int v11 = v01 + 1;

                    triangles[ti++] = v00;
                    triangles[ti++] = v01;
                    triangles[ti++] = v10;

                    triangles[ti++] = v10;
                    triangles[ti++] = v01;
                    triangles[ti++] = v11;
                }
            }

            Mesh mesh = target ?? new Mesh();
            mesh.name = "DynamicWaterMesh";
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public void ApplyToMeshFilter(Mesh toAssign)
        {
            if (toAssign == null)
            {
                return;
            }

            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = gameObject.AddComponent<MeshFilter>();
            }

            mf.sharedMesh = toAssign;
        }
    }
}
