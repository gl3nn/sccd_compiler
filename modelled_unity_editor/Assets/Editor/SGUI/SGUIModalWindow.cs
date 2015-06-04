using UnityEngine;
using UnityEditor;
using System;

namespace SCCDEditor{

    public class SGUIModalWindow : SGUIGroupWidget
    {
        public delegate void DrawFunction(SGUIModalWindow window);

        public bool                 should_close { get; private set; }
        private string              title;

        private SGUIEditorWindow    window;

        public SGUIModalWindow(string title, float min_width = 300)
        {
            this.should_close = false;
            this.title = title;
            this.window = SGUIEditorWindow.current;
            this.window.setModalWindow(this);
            Rect position = new Rect(0, 0, min_width, 0);
            position.center = new Vector2(this.window.position.width/2, this.window.position.height/2);
            this.position = position;
            this.window.setRepaints(5);
        }

        public void close() 
        {
            this.should_close = true;
        }

        private void windowDrawFunction(int unused_window_id)
        {
            base.OnGUI();
        }

        protected override void OnGUI()
        {
            Rect next_position = new Rect(0, 0, this.position.width, this.position.height);
            next_position.center = new Vector2(this.window.position.width/2, this.window.position.height/2);
            next_position = GUILayout.Window(0, next_position, this.windowDrawFunction, this.title);
            GUI.FocusWindow(0);
            this.position = next_position;
        }

        /*public override void addChild(SGUIWidget child_widget)
        {
            base.addChild(child_widget);
            this.window.setRepaints(3);
        }

        public override void removeChild(SGUIWidget child_widget)
        {
            base.removeChild(child_widget);
            this.window.setRepaints(3);
        }*/
    }
}

