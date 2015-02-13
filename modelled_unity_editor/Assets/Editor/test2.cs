using UnityEngine;
using UnityEditor;

namespace SCCDEditor{

    public class Test2Window : EditorWindow
    {
        Rect window_position = new Rect(0, 0, 100, 100);
        public int selected = 0;
        public string[] strings = new string[] {"Grid 1", "Grid 2", "Grid 3", "Grid 4"};

        //[MenuItem("SCCD/Open Test")]
        public static void Init()
        {
            Test2Window window = (Test2Window) EditorWindow.GetWindow(typeof(Test2Window), false);
            window.position = new Rect(10, 10, 10, 10);
            window.title = "Editor Window";
        }

        private void WindowFunction(int id)
        {

            //selected = GUILayout.SelectionGrid(this.selected, this.strings, 1, "GridList");
            GUILayout.Label("Connect to:");
            selected = GUILayout.SelectionGrid(this.selected, this.strings, 1, "MenuItem");
            GUILayout.BeginHorizontal();
            GUILayout.Button("Yes");
            GUILayout.Button("No");
            GUILayout.EndHorizontal();
        }

        public void OnGUI() {
            //GUI.skin = (GUISkin) (Resources.LoadAssetAtPath("Assets/Editor/SCCDSkin.guiskin", typeof(GUISkin)));
            this.BeginWindows();
            GUILayout.Window(0, this.window_position, WindowFunction, "State Drop");
            this.EndWindows();
        }
    }
}