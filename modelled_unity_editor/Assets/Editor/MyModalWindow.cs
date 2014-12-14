using UnityEngine;
using UnityEditor;
using System;

namespace SCCDEditor{

    public class SCCDModalWindow
    {
        public delegate void DrawFunction(SCCDModalWindow window);
		public delegate Vector2 PositionFunction();

        public bool should_close { get; private set; }
        private DrawFunction draw_function;
		private PositionFunction position_function;
        private string title;
        private Vector2 size;

		public SCCDModalWindow(string title, DrawFunction draw_function, PositionFunction position_function = null)
        {
            this.should_close = false;
            this.title = title;
            this.draw_function = draw_function;
            this.position_function = position_function;
            this.size = new Vector2(0, 0);
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
            Rect position = new Rect(0, 0, size.x, size.y);
			position.center = this.position_function();
            position = GUILayout.Window(0, position, this.windowDrawFunction, this.title);
            size.x = position.width;
            size.y = position.height;
        }
    }
}

