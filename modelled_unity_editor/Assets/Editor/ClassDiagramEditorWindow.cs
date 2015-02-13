using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public class ClassDiagramEditorWindow : SGUIEditorWindow
    {

		[MenuItem("SCCD/Open Editor")]
        public static void createOrFocus()
        {
            ClassDiagramEditorWindow window = (ClassDiagramEditorWindow) EditorWindow.GetWindow(typeof(ClassDiagramEditorWindow), false);
            window.wantsMouseMove = true;
            window.title = "Class Diagram Editor";
            UnityEngine.Object.DontDestroyOnLoad( window );
        }

        public ClassDiagramEditorWindow()
        {
            this.window_widget = new SGUITopLevel(this);
            this.controller = new ClassDiagramEditor.Controller(this.window_widget);
            this.controller.start();
        }

    }
}