using UnityEngine;
using UnityEditor;
using csharp_sccd_compiler;

namespace SCCDEditor{
    public class Compile
    {
        [MenuItem("SCCD/Compile Editor")]
        public static void compile()
        {
            Compiler.generate("Assets/editor.xml", "Assets/Editor/editor.cs", CodeGenerator.Platform.GAMELOOP);
            AssetDatabase.Refresh();
        }
    }
}
