using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
    public class GUICanvasElement : GUICanvasBase
    {
        public static float DEFAULT_WIDTH = 100;
        public static float DEFAULT_HEIGHT = 100;
        public static float MARGIN = 5;

        public string                       label       { get; set; }
        public GUICanvasBase                parent      { get; private set; }
            
        //private Dictionary<int, Vector2>    connection_points = new Dictionary<int, Vector2>();

        private Color?                      color       = null;

        public GUICanvasElement (GUICanvasBase parent, Vector2 center)
        {
            Rect rect = new Rect(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT); 
            rect.center = center;
            this.position = rect;

            this.label = string.Format("state {0}", this.tag);
            parent.addElement(this);
            this.canvas = this.parent.canvas;
            //this.setDefaultConnectionPoints();
        }

        public void setParent(GUICanvasBase parent)
        {
            this.parent = parent;
        }

        public void setColor(Color color)
        {
            this.color = color;
        }
        
        public void resetColor()
        {
            this.color = null;
        }

        public void pushToFront() 
        {
            if (this.parent != null)
                this.parent.pushChildToFront(this);
        }

        public bool isAncestorOf(GUICanvasElement element)
        {
            while (element.parent != this.canvas)
            {
                if (element.parent == this)
                    return true;
                element = (GUICanvasElement) element.parent;
            }
            return false;
        }

        public override void move(Vector2 delta)
		{
            this.position = new Rect(this.position.x + delta [0], this.position.y + delta [1], this.position.width, this.position.height);
            base.move(delta);
		}

        protected override void OnGUI()
        {
            this.catchMouseDefault();
            if (Event.current.type == EventType.Repaint)
            {
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

        public List<GUICanvasElement> getOverlappings() 
        {
            return this.canvas.getOverlappingsOf(this);
        }
        
        public void getOverlappingsOf(List<GUICanvasElement> overlappings, GUICanvasElement element)
        {
            if (this != element)
            {
                if (this.position.Overlaps(element.position))
                {
                    overlappings.Add(this);
                }
                for (int i =0; i < this.elements.Count; i++)
                {
                    this.elements[i].getOverlappingsOf(overlappings, element);
                }
            }
        }

        public bool completelyContains(GUICanvasElement item)
        {
            return this.position.Contains( new Vector2(item.position.xMin, item.position.yMin))
                && this.position.Contains (new Vector2 (item.position.xMax, item.position.yMax));
        }

        public void adjustSize()
        {
            this.position = this.calculateContainer(this.position, 5.0f);
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
    }
}
