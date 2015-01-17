using UnityEngine;
using UnityEditor;
using System;

namespace SCCDEditor{

    public class GUIModalWindow : GUIWidget
    {
        public delegate void DrawFunction(GUIModalWindow window);

        public bool should_close { get; private set; }
        private DrawFunction draw_function;
        private string title;

        private GUIWidget container;

        public GUIModalWindow(string title, GUIWidget container, DrawFunction draw_function)
        {
            this.should_close = false;
            this.title = title;
            this.draw_function = draw_function;
            this.container = container;
            GUITopLevel.current.setModalWindow(this);
            Rect position = new Rect(0, 0, 200, 75);
            position.center = new Vector2(this.container.position.width/2, this.container.position.height/2);
            this.position = position;
        }

        public void close() 
        {
            this.should_close = true;
        }

        private void windowDrawFunction(int unused_window_id)
        {
            if (draw_function != null)
                draw_function(this);
        }

        public void draw() 
        {
            Rect next_position = new Rect(0, 0, this.position.width, this.position.height);
            next_position.center = new Vector2(this.container.position.width/2, this.container.position.height/2);
            next_position = GUILayout.Window(0, next_position, this.windowDrawFunction, this.title);
            GUI.FocusWindow(0);
            this.position = next_position;
        }
    }
}

