using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public class TestEditorWindow : EditorWindow
    {
        string text;
        Vector2 scroll_position = new Vector2(0, 0);
        Rect canvas_position = new Rect(0,0,0,0);
        Rect view_rect;

		[MenuItem("SCCD/Open Test Editor")]
        public static void createOrFocus()
        {
            TestEditorWindow window = (TestEditorWindow) EditorWindow.GetWindow(typeof(TestEditorWindow), false);
            window.wantsMouseMove = true;
            window.title = "Class Diagram Editor";
            UnityEngine.Object.DontDestroyOnLoad( window );
        }

        public void OnGUI()
        {
            Rect canvas_area = GUILayoutUtility.GetRect(0, 100000, 0, 100000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (Event.current.type == EventType.Repaint)
            {
                this.canvas_position = canvas_area;
                this.view_rect = this.canvas_position;
            }

            this.scroll_position = GUI.BeginScrollView (this.canvas_position, this.scroll_position, this.view_rect);

            GUI.Box(new Rect(100,100,200,200), "test");

            
            GUI.EndScrollView();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Toggle(false, "test");
            EditorGUILayout.TextField("bla", "ok");
            this.text = EditorGUILayout.TextArea(this.text);
            EditorGUILayout.EndVertical();
        }

    }
}