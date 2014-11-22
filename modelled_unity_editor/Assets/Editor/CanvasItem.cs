using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
    public class CanvasItem {

        public static float DEFAULT_WIDTH = 100;
        public static float DEFAULT_HEIGHT = 100;

        public Rect                 rect { get; private set; }
        public int                  tag { get; private set; }
        public string               label { get; set; }
        private CanvasItem          parent { get; set; }

        private List<CanvasItem>    children    = new List<CanvasItem>();
        private string              name;
        private EditorCanvas        canvas;

        private Color?              color       = null;

        public CanvasItem (Vector2 center, EditorCanvas canvas)
        {
            Rect rect = new Rect(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT); 
            rect.center = center;
            this.rect = rect;
            this.canvas = canvas;
            this.tag = canvas.getUniqueTag();
            this.canvas.addChild(this);
            this.label = string.Format("state {0}", this.tag);
            this.parent = null;
        }

        public void addChild(CanvasItem child) 
        {
            this.children.Add(child);
        }

        public void removeChild(CanvasItem child)
		{
            this.children.Remove (child);
		}
        
        public void setSize(float width, float height)
        {
            this.rect = new Rect(this.rect.x, this.rect.y, width, height);
        }

        public void setColor(Color color)
        {
            this.color = color;
        }

        public void resetColor()
        {
            this.color = null;
        }
        
        public bool containsPosition(Vector2 position)
        {
            return this.rect.Contains(position);
        }
        
        public void move(Vector2 delta)
		{
			this.rect = new Rect(this.rect.x + delta [0], this.rect.y + delta [1], this.rect.width, this.rect.height);
		}

        public void draw()
        {
            if (this.color != null)
            {
                Color old_color = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp(GUI.backgroundColor, Color.green, 0.5f);
                GUI.Box(this.rect, this.label);
                GUI.backgroundColor = old_color;
            } else
            {
                GUI.Box(this.rect, this.label);
            }
            for (int i = 0; i < this.children.Count; i++)
            {
                this.children[i].draw();
            }
        }
        
        public int catchClick(Vector2 mouse_position) {
            for (int i = 0; i < this.children.Count; i++)
            {
                if (this.children[i].containsPosition(mouse_position))
                    return this.children[i].catchClick(mouse_position);
            }
            return this.tag;
        }

        public List<CanvasItem> getOverlappings() 
        {
            return this.canvas.getOverlappings(this);
        }

        public void getOverlappings(List<CanvasItem> overlappings, CanvasItem item) {
            if (this != item)
            {
                if (this.rect.Overlaps(item.rect))
                {
                    overlappings.Add(this);
                }
                for (int i =0; i < this.children.Count; i++)
                {
                    this.children[i].getOverlappings(overlappings, item);
                }
            }
        }

        public void pushToFront() 
        {
            if (this.parent != null)
                this.parent.pushChildToFront(this);
            else
                this.canvas.pushChildToFront(this);
        }

        public void pushChildToFront(CanvasItem item) 
        {
            this.children.Remove(item);
            this.children.Add(item);
            this.pushToFront();
        }

        public void adjustSize()
        {
            float xMin = this.rect.xMin;
            float yMin = this.rect.yMin;
            float xMax = this.rect.xMax;
            float yMax = this.rect.yMax;
            foreach(CanvasItem child in this.children)
            {
                if (child.rect.xMin < xMin) xMin = child.rect.xMin;
                if (child.rect.yMin < yMin) yMin = child.rect.yMin;
                if (child.rect.xMax > xMax) xMax = child.rect.xMax;
                if (child.rect.yMax > yMax) yMax = child.rect.yMax;
            }
           this.rect = new Rect (xMin - 5,yMin -5 ,xMax-xMin +10, yMax-yMin +10);
        }
    }
}
