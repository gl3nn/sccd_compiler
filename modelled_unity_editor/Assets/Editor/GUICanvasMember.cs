using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
    public class GUICanvasMember : GUIWidgetGroup
    {
        public static float DEFAULT_WIDTH = 100;
        public static float DEFAULT_HEIGHT = 100;
        public static float MARGIN = 5;

        public string                       label { get; set; }

        //private Dictionary<int, Vector2>    connection_points = new Dictionary<int, Vector2>();

        private Color?                      color       = null;

        public GUICanvasMember (GUIWidgetGroup parent, Vector2 center) : base(parent)
        {
            Rect rect = new Rect(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT); 
            rect.center = center;
            this.position = rect;

            this.label = string.Format("state {0}", this.tag);
            //this.setDefaultConnectionPoints();
        }

        /*private void setDefaultConnectionPoints()
        {
            this.connection_points[0] = new Vector2(this.rect.center.x, this.rect.yMin);
            this.connection_points[1] = new Vector2(this.rect.xMax, this.rect.center.y);
            this.connection_points[2] = new Vector2(this.rect.center.x, this.rect.yMax);
            this.connection_points[3] = new Vector2(this.rect.xMin, this.rect.center.y);
        }*/

       /* public Vector2 getPoint(int id)
        {
            return this.connection_points [id];
        }*/

        public void setColor(Color color)
        {
            this.color = color;
        }

        public void resetColor()
        {
            this.color = null;
        }
        
        public void move(Vector2 delta)
		{
            this.position = new Rect(this.position.x + delta [0], this.position.y + delta [1], this.position.width, this.position.height);
			/*for (int i=0; i < this.children.Count; i++)
				this.children[i].move(delta);*/
		}

        public override void OnGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                /*if (this.position.Contains(Event.current.mousePosition))
                    GUIWidget.hoover_tag = this.tag;*/
                if (this.color != null)
                {
                    Color old_color = GUI.backgroundColor;
                    GUI.backgroundColor = Color.Lerp(GUI.backgroundColor, Color.green, 0.5f);
                    GUI.Box(this.position, this.label);
                    GUI.backgroundColor = old_color;
                } else
                {
                    GUI.Box(this.position, this.label);
                }
            }
            base.OnGUI();
        }
        
        /*public int catchClick(Vector2 mouse_position) {
            for (int i = 0; i < this.children.Count; i++)
            {
                if (this.children[i].containsPosition(mouse_position))
                    return this.children[i].catchClick(mouse_position);
            }
            return this.tag;
        }*/

        /*
        public List<CanvasItem> getOverlappings() 
        {
            return this.canvas.getOverlappingsOf(this);
        }

        public void getOverlappingsOf(List<CanvasItem> overlappings, CanvasItem item) {
            if (this != item)
            {
                if (this.rect.Overlaps(item.rect))
                {
                    overlappings.Add(this);
                }
                for (int i =0; i < this.children.Count; i++)
                {
                    this.children[i].getOverlappingsOf(overlappings, item);
                }
            }
        }

        public bool completelyContains(CanvasItem item) 
        {
            return this.rect.Contains( new Vector2(item.rect.xMin, item.rect.yMin))
				&& this.rect.Contains (new Vector2 (item.rect.xMax, item.rect.yMax));
        }*/
    }
}
