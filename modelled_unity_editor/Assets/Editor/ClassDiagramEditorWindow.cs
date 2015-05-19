using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public class ClassDiagramEditorWindow : SGUIEditorWindow
    {
        SCCDScript sccd_script;

        public static void open(SCCDScript sccd_script)
        {
            ClassDiagramEditorWindow window = (ClassDiagramEditorWindow) EditorWindow.GetWindow(typeof(ClassDiagramEditorWindow), false);
            window.wantsMouseMove = true;
            window.title = "Class Diagram Editor";
            UnityEngine.Object.DontDestroyOnLoad( window );
            window.sccd_script = sccd_script;
            window.start();
        }

        public override void performRestart()
        {
            StateChartEditorWindow.clear();
            this.start();
        }

        public void start()
        {
            this.top_level_widget = new SGUIVerticalGroup();
            this.controller = new ClassDiagramEditor.Controller(this, this.sccd_script);
            this.controller.start();
        }

        public void OnDestroy()
        {
            StateChartEditorWindow.clear();
        }
    }
}