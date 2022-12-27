using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MaterialColumn))]
public class ColumnEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MaterialColumn myTarget = (MaterialColumn)target;
        DrawDefaultInspector();
        if (GUILayout.Button("RebuildMesh"))
        {
            myTarget.RebuildMesh();
        }
    }
}
