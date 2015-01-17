using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
   public class GUICanvas : GUICanvasBase {

        public Rect                 view_rect { get; private set; }

        private Vector2             scroll_position = Vector2.zero;

        public GUICanvas()
        {
            this.view_rect = new Rect(0, 0, 0, 0);
            this.canvas = this;
        }

        protected override void OnGUI()
        {
            Rect canvas_area = GUILayoutUtility.GetRect(0, 100000, 0, 100000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (Event.current.type == EventType.Repaint)
            {
                this.position = canvas_area;
                this.view_rect = this.calculateContainer(this.position, 2.0f);
            }
            bool get_mouse = false;
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
            {
                get_mouse = this.position.Contains(Event.current.mousePosition);
            }
            this.scroll_position = GUI.BeginScrollView (this.position, this.scroll_position, this.view_rect);
            if (get_mouse)
                GUIEvent.current = new GUIEvent(this.tag, Event.current.mousePosition);
            base.OnGUI();
            GUI.EndScrollView();
        }
        
        public List<GUICanvasElement> getOverlappingsOf(GUICanvasElement item)
        {
            List<GUICanvasElement> overlappings = new List<GUICanvasElement>(); 
            for (int i =0; i < this.elements.Count; i++)
            {
                this.elements[i].getOverlappingsOf(overlappings, item);
            }
            return overlappings;
        }
    }
}