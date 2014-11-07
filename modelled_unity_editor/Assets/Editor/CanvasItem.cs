using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
    public class CanvasItem {

        public static float DEFAULT_WIDTH = 100;
        public static float DEFAULT_HEIGHT = 100;
        public static int tag_counter = 0;


        private List<CanvasItem> children = new List<CanvasItem>();
        //private CanvasItem parent;
        public int tag { get; private set; }
        public Rect rect { get; private set; }

        public CanvasItem (): this(0, 0, null)
        {
        }

        public CanvasItem (Vector2 position, CanvasItem parent): this(DEFAULT_WIDTH, DEFAULT_HEIGHT, parent)
        {
            Rect temp_rect = this.rect;
            temp_rect.center = position;
            this.rect = temp_rect;
        }

        public CanvasItem(float width, float height, CanvasItem parent)
        {
            this.rect = new Rect(0, 0, width, height); 
            this.tag = tag_counter;
            tag_counter++;
            //this.parent = parent;
        }

        public bool containsPosition(Vector2 position)
        {
            return rect.Contains(position);
        }

        public void addChild(CanvasItem item) 
        {
            this.children.Add(item);
        }

        public void setSize(float width, float height)
        {
            this.rect = new Rect(this.rect.x, this.rect.y, width, height);
        }

		public void move(Vector2 delta)
		{
			this.rect = new Rect(this.rect.x + delta [0], this.rect.y + delta [1], this.rect.width, this.rect.height);
		}

        public int catchClick(Vector2 mouse_position) {
            int i = this.children.Count;
            while (i > 0)
            {
                i--;
                if (this.children[i].containsPosition(mouse_position))
                    return this.children[i].catchClick(mouse_position);
            }
            return this.tag;
        }
    }
}