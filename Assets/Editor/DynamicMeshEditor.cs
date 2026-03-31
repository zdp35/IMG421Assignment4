using UnityEditor;
using UnityEngine;
using LanternDrift.Water;

namespace LanternDrift.EditorTools
{
    [CustomEditor(typeof(DynamicMesh))]
    public class DynamicMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DynamicMesh dynamicMesh = (DynamicMesh)target;

            if (GUILayout.Button("Create Mesh"))
            {
                Mesh mesh = dynamicMesh.GenerateGridMesh();
                dynamicMesh.ApplyToMeshFilter(mesh);
            }
        }
    }
}
