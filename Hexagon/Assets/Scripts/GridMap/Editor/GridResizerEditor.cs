using UnityEditor;
using UnityEngine;

namespace HexagonMusapKahraman.GridMap
{
    [CustomEditor(typeof(GridResizer))]
    public class LookAtPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var gridResizer = (GridResizer) target;
            if (GUILayout.Button("Resize Grid Map")) gridResizer.ResizeGridMap();
        }
    }
}