using UnityEngine;
using UnityEditor;

namespace MarchingCubes
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MapGenerator mapGenerator = (MapGenerator)target;
            if (GUILayout.Button("Create Map"))
            {
                mapGenerator.GenerateMap();
                mapGenerator.transform.position += new Vector3(0, 0, 1);
                mapGenerator.transform.position -= new Vector3(0, 0, 1);
            }
        }
    }
}
