using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(nodeDaddy)), CanEditMultipleObjects]
public class nodeDaddyInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // If we call base the default inspector will get drawn too.
        // Remove this line if you don't want that to happen.
        base.OnInspectorGUI();

        nodeDaddy node = target as nodeDaddy;

        //node.secretExit = EditorGUILayout.Toggle("myBool", node.secretExit);

        if(GUILayout.Button("Add Node")){
            node.addNode();
        };
    }
}