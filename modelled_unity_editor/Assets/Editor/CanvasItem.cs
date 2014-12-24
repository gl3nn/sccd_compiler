using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
    public class CanvasItem {

        public static float DEFAULT_WIDTH = 100;
        public static float DEFAULT_HEIGHT = 100;
        public static float MARGIN = 5;
        
        public Rect                         rect { get; private set; }
        public int                          tag { get; private set; }
        public string                       label { get; set; }
        public CanvasItem                   parent { get; /*private*/ set; }
		public EditorCanvas                 canvas { get; set; }

        private List<CanvasItem>            children    = new List<CanvasItem>();
        private Dictionary<int, Vector2>    connection_points = new Dictionary<int, Vector2>();

        private Color?                      color       = null;

        public CanvasItem (Vector2 center, EditorCanvas canvas)
        {
            Rect rect = new Rect(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT); 
            rect.center = center;
            this.rect = rect;
            this.canvas = canvas;
            this.tag = canvas.getUniqueTag();
            //this.canvas.addChild(this);
            this.label = string.Format("state {0}", this.tag);
            this.parent = null;
            this.setDefaultConnectionPoints();
        }

        private void setDefaultConnectionPoints()
        {
            this.connection_points[0] = new Vector2(this.rect.center.x, this.rect.yMin);
            this.connection_points[1] = new Vector2(this.rect.xMax, this.rect.center.y);
            this.connection_points[2] = new Vector2(this.rect.center.x, this.rect.yMax);
            this.connection_points[3] = new Vector2(this.rect.xMin, this.rect.center.y);
        }

        public Vector2 getPoint(int id)
        {
            return this.connection_points [id];
        }

        public void addChild(CanvasItem child) 
        {
            this.children.Add(child);
            child.parent = this;
        }

        public void removeChild(CanvasItem child)
		{
            this.children.Remove (child);
            child.parent = null;
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
			for (int i=0; i < this.children.Count; i++)
				this.children[i].move(delta);
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
        }
        
        public bool isAncestorOf(CanvasItem item)
        {
            while (item.parent != null)
            {
                if (item.parent == this)
                    return true;
                item = item.parent;
            }
            return false;
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
                if (child.rect.xMin - MARGIN < xMin) xMin = child.rect.xMin - MARGIN;
                if (child.rect.yMin - MARGIN < yMin) yMin = child.rect.yMin - MARGIN;
                if (child.rect.xMax + MARGIN > xMax) xMax = child.rect.xMax + MARGIN;
                if (child.rect.yMax + MARGIN > yMax) yMax = child.rect.yMax + MARGIN;
            }
           this.rect = new Rect (xMin, yMin, xMax-xMin, yMax-yMin);
           if (this.parent != null)
                this.parent.adjustSize();
        }
    }
}
