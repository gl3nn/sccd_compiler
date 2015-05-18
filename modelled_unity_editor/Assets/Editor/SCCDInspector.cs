using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SCCDScript))]
public class SCCDInspector : Editor 
{
    public override void OnInspectorGUI()
    {
        SCCDScript target_script = (SCCDScript)this.target;
        EditorGUILayout.LabelField("Model File:");
        EditorGUILayout.SelectableLabel(target_script.xml_file);
        if (GUILayout.Button("Open SCCD editor"))
            SCCDEditor.ClassDiagramEditorWindow.open(target_script);
        /*if (GUILayout.Button("compile"))
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
            EditorUtility.SetDirty(target_script);*/
    }
}