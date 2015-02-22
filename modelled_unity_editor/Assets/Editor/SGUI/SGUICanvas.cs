using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
   public class SGUICanvas : SGUICanvasBase {

        public Rect                     view_rect { get; private set; }

        private Vector2                 scroll_position = Vector2.zero;

        protected List<SGUICanvasEdge>  edges    { get; private set; }

        public SGUICanvas()
        {
            this.edges = new List<SGUICanvasEdge>();
            this.view_rect = new Rect(0, 0, 0, 0);
            this.canvas = this;
        }

        public void addEdge(SGUICanvasEdge edge)
        {
            this.edges.Add(edge);
        }
        
        public void removeEdge(SGUICanvasEdge edge)
        {
            this.edges.Remove(edge);
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
                SGUIEvent.current = new SGUIEvent(this.tag, Event.current.mousePosition);
            base.OnGUI();
            for (int i=0; i < this.edges.Count; i++)
            {
                this.edges[i].doOnGUI();
            }
            GUI.EndScrollView();
        }
        
        public List<SGUICanvasElement> getOverlappingsOf(SGUICanvasElement item)
        {
            List<SGUICanvasElement> overlappings = new List<SGUICanvasElement>(); 
            for (int i =0; i < this.elements.Count; i++)
            {
                this.elements[i].getOverlappingsOf(overlappings, item);
            }
            return overlappings;
        }
    }
}