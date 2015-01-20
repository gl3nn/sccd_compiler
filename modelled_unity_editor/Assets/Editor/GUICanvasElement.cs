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
        
        public Vector2 getConnectionPointPosition(int id)
        {
            if (id == 0)
                return new Vector2(this.position.center.x, this.position.yMin);
            else if (id == 1)
                return new Vector2(this.position.xMax, this.position.center.y);
            else if (id == 2)
                return new Vector2(this.position.center.x, this.position.yMax);
            else if (id == 3)
                return new Vector2(this.position.xMin, this.position.center.y);
            else
                throw new GUIException("Invalid connection point position.");
        }

        public int getClosestConnectionPoint(Vector2 position)
        {
            int smallest_id = 0;
            float smallest_distance = Vector2.Distance(this.getConnectionPointPosition(0), position);
            for (int i = 1; i < 4; i++)
            {
                float distance = Vector2.Distance(this.getConnectionPointPosition(i), position);
                if (distance < smallest_distance)
                {
                    smallest_id = i;
                    smallest_distance = distance;
                }
            }
            return smallest_id;
        }
    }
}
