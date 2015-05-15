using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using csharp_sccd_compiler;

namespace SCCDEditor{
    public class Compile
    {
        [MenuItem("SCCD/Compile Editor")]
        public static void compile()
        {
            Logger.verbose = 2;
            var stdOut = System.Console.Out;
            StringWriter consoleOut = new StringWriter();
            System.Console.SetOut(consoleOut);
            try {
                Compiler.generate(Path.Combine(Application.dataPath, "classdiagram_editor.xml"), Path.Combine(Application.dataPath, "Editor/classdiagram_editor.cs"), CodeGenerator.Platform.GAMELOOP);
                Compiler.generate(Path.Combine(Application.dataPath, "xml/statechart/editor.xml"), Path.Combine(Application.dataPath, "Editor/statechart_editor.cs"), CodeGenerator.Platform.GAMELOOP);
            } catch (Exception e) {
                Debug.Log(e);
            }
            Debug.Log( consoleOut.ToString());
            System.Console.SetOut(stdOut);
            AssetDatabase.Refresh();
        }
    }
}
