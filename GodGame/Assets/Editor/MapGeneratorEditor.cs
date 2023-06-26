using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var target = base.target as MapGenerator;

        if (DrawDefaultInspector() && target.AutoUpdate)
        {
            target.DrawMapInEditor();
        }

        GUILayout.Space(15);
        if (GUILayout.Button("Generate Map"))
        {
            target.DrawMapInEditor();
        }
        GUILayout.Space(15);
    }
}
