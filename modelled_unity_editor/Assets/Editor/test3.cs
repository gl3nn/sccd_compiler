using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public class TestContent: PopupWindowContent
    {
        public int selected = 0;
        public string[] strings = new string[] {"Grid 1", "Grid 2", "Grid 3", "Grid 4"};


        public override void OnGUI(Rect test)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Connect to:");
            this.selected = GUILayout.SelectionGrid(this.selected, this.strings, 1, "MenuItem");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Button("Yes");
            if (GUILayout.Button("No"))
            {
                this.editorWindow.Close();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

        }
    }

    public class ExamplePopup2 : EditorWindow
    {
        static Rect pos = new Rect(0,0,100,100);

        [MenuItem("Window/Example Popup 2")]
        static void InitPopup()
        {
            PopupWindow.Show(ExamplePopup2.pos, new TestContent());
        }
    }

}
