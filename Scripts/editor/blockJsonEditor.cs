using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(blockJson)), CanEditMultipleObjects]
public class blockJsonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // If we call base the default inspector will get drawn too.
        // Remove this line if you don't want that to happen.
        base.OnInspectorGUI();

        blockJson block = target as blockJson;

        if(GUILayout.Button("Generate JSON")){
            block.generate();
        }
    }
}
