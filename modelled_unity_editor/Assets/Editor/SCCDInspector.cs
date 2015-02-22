using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SCCDScript))]
public class LevelScriptEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        SCCDScript target_script = (SCCDScript)this.target;
        target_script.xml_file = EditorGUILayout.TextField("Model File", target_script.xml_file);
        if (GUILayout.Button("Open SCCD editor"))
            SCCDEditor.ClassDiagramEditorWindow.open(target_script.xml_file);
        if (GUI.changed)
            EditorUtility.SetDirty(target_script);
    }
}