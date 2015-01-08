using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
   public class GUIScrollCanvas : GUIWidgetGroup {

        public Rect                 view_rect { get; private set; }

        private Vector2             scroll_position = Vector2.zero;

		public GUIScrollCanvas(GUIWidgetGroup parent): base(parent)
        {
            this.view_rect = new Rect(0, 0, 0, 0);
        }
        
        /*public int catchClick(Vector2 mouse_position) {
            for (int i = 0; i < this.children.Count; i++)
            {
                if (this.children[i].containsPosition(mouse_position))
                    return this.children[i].catchClick(mouse_position);
            }
            return -1;
        }*/

        public override void OnGUI()
        {
            Rect canvas_area = GUILayoutUtility.GetRect(0, 100000, 0, 100000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (Event.current.type == EventType.Repaint)
            {
                this.position = canvas_area;
                this.view_rect = this.calculateContainer(this.position);
                /*if (this.position.Contains(Event.current.mousePosition))
                    GUIWidget.hoover_tag = this.tag;*/
            }
            this.scroll_position = GUI.BeginScrollView (this.position, this.scroll_position, this.view_rect);
            base.OnGUI();
            GUI.EndScrollView();
        }

        /*public List<CanvasItem> getOverlappingsOf(CanvasItem item) {
            List<CanvasItem> overlappings = new List<CanvasItem>(); 
            for (int i =0; i < this.children.Count; i++)
            {
                this.children[i].getOverlappingsOf(overlappings, item);
            }
            return overlappings;
        }*/



		/*public void createModalWindow(string title, SCCDModalWindow.DrawFunction draw_function)
		{
			this.window.createModalWindow(title, draw_function, true);
		}*/
    }
}