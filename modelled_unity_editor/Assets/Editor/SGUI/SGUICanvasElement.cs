using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor{
    public class SGUICanvasElement : SGUICanvasBase
    {
        public static float DEFAULT_WIDTH = 100;
        public static float DEFAULT_HEIGHT = 100;
        public static float RESIZE_MARGIN = 10;
        public static float BORDER_MARGIN = 5;

        public string                       label       { get; set; }
        new public SGUICanvasBase           parent      { get; private set; }

        private Color?                      color       = null;
        private bool                        resize_enabled = false;

        public SGUICanvasElement(SGUICanvasBase parent, Vector2 center) : this(parent)
        {
            this.setCenter(center);
        }

        public SGUICanvasElement(SGUICanvasBase parent) : this()
        {
            this.position = new Rect(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT); 
            parent.addElement(this);
            //this.setDefaultConnectionPoints();
        }

        public SGUICanvasElement()
        {
            this.label = "";
        }

        public void setCenter(Vector2 center)
        {
            Rect rect = this.position;
            rect.center = center;
            this.position = rect;
        }

        public void setParent(SGUICanvasBase parent)
        {
            this.parent = parent;
            if(this.parent != null)
                this.canvas = this.parent.canvas;
        }

        public void setColor(Color color)
        {
            this.color = color;
        }
        
        public void resetColor()
        {
            this.color = null;
        }

        public void enableResize(bool enable = true)
        {
            this.resize_enabled = enable;
        }

        public void pushToFront() 
        {
            if (this.parent != null)
                this.parent.pushChildToFront(this);
        }

        public bool isAncestorOf(SGUICanvasElement element)
        {
            while (element.parent != this.canvas)
            {
                if (element.parent == this)
                    return true;
                element = (SGUICanvasElement) element.parent;
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
            if (this.color != null)
            {
                Color old_color = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp(GUI.backgroundColor, Color.green, 0.5f);
                GUILayout.BeginArea(this.position, "", "button");
                EditorGUILayout.LabelField(this.label);
                GUILayout.EndArea();
                GUI.backgroundColor = old_color;
            } else
            {
                GUILayout.BeginArea(this.position, "", "button");
                EditorGUILayout.LabelField(this.label);
                GUILayout.EndArea();
            }
            base.OnGUI();

            if(this.resize_enabled)
                this.setResizeRects();
        }

        private void setResizeRects()
        {
            EditorGUIUtility.AddCursorRect(this.getResizeRect(0), MouseCursor.ResizeUpLeft);
            EditorGUIUtility.AddCursorRect(this.getResizeRect(1), MouseCursor.ResizeUpRight);
            EditorGUIUtility.AddCursorRect(this.getResizeRect(2), MouseCursor.ResizeUpRight);
            EditorGUIUtility.AddCursorRect(this.getResizeRect(3), MouseCursor.ResizeUpLeft);
        }


        public int getContainingResizeRect(Vector2 pos)
        {
            for (int i = 0; i < 4; i++)
            {
                Rect rect = this.getResizeRect(i);
                if (rect.Contains(pos))
                    return i;
            }
            return -1;
        }

        public Rect getResizeRect(int resize_id)
        {
            EditorGUIUtility.AddCursorRect(new Rect(this.position.xMin, this.position.yMin, RESIZE_MARGIN, RESIZE_MARGIN), MouseCursor.ResizeUpLeft);
            EditorGUIUtility.AddCursorRect(new Rect(this.position.xMax - RESIZE_MARGIN, this.position.yMin, RESIZE_MARGIN, RESIZE_MARGIN), MouseCursor.ResizeUpRight);
            EditorGUIUtility.AddCursorRect(new Rect(this.position.xMin, this.position.yMax - RESIZE_MARGIN, RESIZE_MARGIN, RESIZE_MARGIN), MouseCursor.ResizeUpRight);
            EditorGUIUtility.AddCursorRect(new Rect(this.position.xMax - RESIZE_MARGIN,  this.position.yMax - RESIZE_MARGIN, RESIZE_MARGIN, RESIZE_MARGIN), MouseCursor.ResizeUpLeft);
            if (resize_id == 0) //Left top corner
                return new Rect(this.position.xMin, this.position.yMin, RESIZE_MARGIN, RESIZE_MARGIN);
            else if (resize_id == 1) //Right top corner
                return new Rect(this.position.xMax - RESIZE_MARGIN, this.position.yMin, RESIZE_MARGIN, RESIZE_MARGIN);
            else if (resize_id == 2) //Left bottom corner
                return new Rect(this.position.xMin, this.position.yMax - RESIZE_MARGIN, RESIZE_MARGIN, RESIZE_MARGIN);
            else if (resize_id == 3) //Right bottom corner
                return new Rect(this.position.xMax - RESIZE_MARGIN, this.position.yMax - RESIZE_MARGIN, RESIZE_MARGIN, RESIZE_MARGIN);
            else
                throw new SGUIException("Invalid resize rect.");
        }

        public void resize(int resize_id, Vector2 delta)
        {
            if (resize_id == 0) //Left top corner
                this.position = new Rect(this.position.xMin + delta.x, this.position.yMin + delta.y, this.position.width - delta.x, this.position.height - delta.y);
            else if (resize_id == 1) //Right top corner
                this.position = new Rect(this.position.xMin, this.position.yMin + delta.y, this.position.width + delta.x, this.position.height - delta.y);
            else if (resize_id == 2) //Left bottom corner
                this.position = new Rect(this.position.xMin + delta.x, this.position.yMin, this.position.width - delta.x, this.position.height + delta.y);
            else if (resize_id == 3) //Right bottom corner
                this.position = new Rect(this.position.xMin, this.position.yMin, this.position.width + delta.x, this.position.height + delta.y);
            else
                throw new SGUIException("Invalid resize rect.");
        }

        public List<SGUICanvasElement> getOverlappings() 
        {
            return this.canvas.getOverlappingsOf(this);
        }
        
        public void getOverlappingsOf(List<SGUICanvasElement> overlappings, SGUICanvasElement element)
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

        public bool completelyContains(SGUICanvasElement item)
        {
            return this.position.Contains( new Vector2(item.position.xMin, item.position.yMin))
                && this.position.Contains (new Vector2 (item.position.xMax, item.position.yMax));
        }

        public void adjustSize()
        {
            this.position = this.calculateContainer(this.position, BORDER_MARGIN);
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
                throw new SGUIException("Invalid connection point position.");
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
