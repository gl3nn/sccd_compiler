using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using csharp_sccd_compiler;

[CustomEditor(typeof(SCCDScript))]
public class LevelScriptEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        SCCDScript target_script = (SCCDScript)this.target;
        target_script.xml_file = EditorGUILayout.TextField("Model File", target_script.xml_file);
        if (GUILayout.Button("Open SCCD editor"))
            SCCDEditor.ClassDiagramEditorWindow.open(target_script.xml_file);
        if (GUILayout.Button("compile"))
        {
            Logger.verbose = 2;
            var stdOut = System.Console.Out;
            StringWriter consoleOut = new StringWriter();
            System.Console.SetOut(consoleOut);
            try {
                Compiler.generate(Path.Combine(Application.dataPath, "player_tank.xml"), Path.Combine(Application.dataPath, "player_tank.cs"), CodeGenerator.Platform.GAMELOOP);
            } catch (Exception e) {
                Debug.Log(e);
            }
            Debug.Log( consoleOut.ToString());
            System.Console.SetOut(stdOut);
            AssetDatabase.Refresh();
        }
        if (GUI.changed)
            EditorUtility.SetDirty(target_script);
    }
}