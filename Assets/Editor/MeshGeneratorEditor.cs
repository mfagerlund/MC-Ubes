using UnityEditor;
using UnityEngine;

namespace MarchingCubes
{
    [CustomEditor(typeof(MeshGenerator))]
    public class MeshGeneratorEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MeshGenerator meshGenerator = (MeshGenerator)target;
            if (GUILayout.Button("Create Map"))
            {
                meshGenerator.GenerateMesh();
                meshGenerator.transform.position += new Vector3(0, 0, 1);
                meshGenerator.transform.position -= new Vector3(0, 0, 1);
            }
        }
    }
}