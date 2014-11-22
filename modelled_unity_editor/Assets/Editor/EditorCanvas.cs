using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
    public class EditorCanvas {

        private List<CanvasItem>    children = new List<CanvasItem>();
        private int                 tag_counter = 0;

        public Rect                 rect { get; private set; }

        public EditorCanvas()
        {
            this.rect = new Rect(0, 0, 0, 0);
        }
        
        public void addChild(CanvasItem child) 
        {
            this.children.Add(child);
        }

        public void removeChild(CanvasItem child)
        {
            this.children.Remove (child);
        }
        
        public int getUniqueTag()
        {
            return this.tag_counter++;
        }
        
        public void setSize(float width, float height)
        {
            this.rect = new Rect(this.rect.x, this.rect.y, width, height);
        }

        public bool containsPosition(Vector2 position)
        {
            return this.rect.Contains(position);
        }
        
        public int catchClick(Vector2 mouse_position) {
            for (int i = 0; i < this.children.Count; i++)
            {
                if (this.children[i].containsPosition(mouse_position))
                    return this.children[i].catchClick(mouse_position);
            }
            return -1;
        }

        public void draw() {
            for (int i = 0; i < this.children.Count; i++)
            {
                this.children[i].draw();
            }
        }
        
        public void adjustSizeToMinimum(Rect minimum) {
            this.rect = this.calculateContainer(minimum);
        }
        
        public Rect calculateContainer(Rect minimum)
        {
            float xMin = 0;
            float yMin = 0;
            float xMax = minimum.width;
            float yMax = minimum.height;
            foreach(CanvasItem child in this.children)
            {
                if (child.rect.xMin < xMin) xMin = child.rect.xMin;
                if (child.rect.yMin < yMin) yMin = child.rect.yMin;
                if (child.rect.xMax > xMax) xMax = child.rect.xMax;
                if (child.rect.yMax > yMax) yMax = child.rect.yMax;
            }
            return new Rect (xMin,yMin,xMax-xMin, yMax-yMin);
        }

        public List<CanvasItem> getOverlappings(CanvasItem item) {
            List<CanvasItem> overlappings = new List<CanvasItem>(); 
            for (int i =0; i < this.children.Count; i++)
            {
                this.children[i].getOverlappings(overlappings, item);
            }
            return overlappings;
        }

        public void pushChildToFront(CanvasItem item) 
        {
            this.children.Remove(item);
            this.children.Add(item);
        }
    }
}