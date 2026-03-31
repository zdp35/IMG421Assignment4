using UnityEngine;

namespace LanternDrift.Water
{
    [RequireComponent(typeof(MeshFilter))]
    public class WaveApplier : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private Mesh workingMesh;
        private Vector3[] baseVertices;
        private Vector3[] displacedVertices;

        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            workingMesh = meshFilter.mesh;
            baseVertices = workingMesh.vertices;
            displacedVertices = new Vector3[baseVertices.Length];
        }

        private void Update()
        {
            ApplyWaves();
        }

        public void ApplyWaves()
        {
            if (workingMesh == null || baseVertices == null)
            {
                return;
            }

            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 world = transform.TransformPoint(baseVertices[i]);
                float height = WaterSurfaceUtility.GetWaterHeight(world);
                Vector3 newWorld = new Vector3(world.x, height, world.z);
                displacedVertices[i] = transform.InverseTransformPoint(newWorld);
            }

            workingMesh.vertices = displacedVertices;
            workingMesh.RecalculateNormals();
        }
    }
}
