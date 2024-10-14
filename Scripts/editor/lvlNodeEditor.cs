using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(lvlNode)), CanEditMultipleObjects]
public class MyEditorClass : Editor
{
    public override void OnInspectorGUI()
    {
        // If we call base the default inspector will get drawn too.
        // Remove this line if you don't want that to happen.
        base.OnInspectorGUI();

        lvlNode node = target as lvlNode;

        //node.secretExit = EditorGUILayout.Toggle("myBool", node.secretExit);

        if (node.secretExit)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Secret Level:");
            node.secretLvl = (Transform)EditorGUILayout.ObjectField (node.secretLvl, typeof(Transform));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Secret Input:");
            node.secretLvl = (Transform)EditorGUILayout.ObjectField (node.secretLvl, typeof(Transform));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        if(GUILayout.Button("Reset Line")){
            node.resetLine();
        }
        if(GUILayout.Button("Delete Node")){
            node.destroy();
        }
    }
}