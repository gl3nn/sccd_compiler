using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public class ClassDiagramEditorWindow : SGUIEditorWindow
    {

		[MenuItem("SCCD/Open Editor")]
        public static void open(string file_name = "")
        {
            ClassDiagramEditorWindow window = (ClassDiagramEditorWindow) EditorWindow.GetWindow(typeof(ClassDiagramEditorWindow), false);
            window.wantsMouseMove = true;
            window.title = "Class Diagram Editor";
            UnityEngine.Object.DontDestroyOnLoad( window );
            window.start(file_name);
        }

        public ClassDiagramEditorWindow()
        {
        }

        public void start(string file_name)
        {
            this.top_level_widget = new SGUITopLevel(this);
            this.controller = new ClassDiagramEditor.Controller(this.top_level_widget, file_name);
            this.controller.start();
        }

        /*public void load_file(string file_name)
        {
            this.controller.addInput(
                new sccdlib.Event("load_file", "input", new object[] {
                file_name
            }));
        }*/
    }
}