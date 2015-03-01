using UnityEngine;
using UnityEditor;
using System;

namespace SCCDEditor{

    public class SGUIModalWindow : SGUIGroupWidget
    {
        public delegate void DrawFunction(SGUIModalWindow window);

        public bool should_close { get; private set; }
        private string title;

        private EditorWindow window;

        public SGUIModalWindow(string title, float min_width = 300)
        {
            this.should_close = false;
            this.title = title;
            SGUITopLevel.current.setModalWindow(this);
            this.window = SGUITopLevel.current.window;
            Rect position = new Rect(0, 0, min_width, 75);
            position.center = new Vector2(this.window.position.width/2, this.window.position.height/2);
            this.position = position;
        }

        public void close() 
        {
            this.should_close = true;
        }

        private void windowDrawFunction(int unused_window_id)
        {
            base.OnGUI();
        }

        public void draw() 
        {
            Rect next_position = new Rect(0, 0, this.position.width, 0);
            next_position.center = new Vector2(this.window.position.width/2, this.window.position.height/2);
            next_position = GUILayout.Window(0, next_position, this.windowDrawFunction, this.title);
            GUI.FocusWindow(0);
            this.position = next_position;
        }
    }
}

