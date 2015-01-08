using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class GUIWidgetGroup: GUIWidget
    {
        private List<GUIWidget>             children = new List<GUIWidget>();

        public GUIWidgetGroup(GUIWidgetGroup parent): base(parent)
        {
        }

        public override void OnGUI()
        {
            for (int i=0; i < this.children.Count; i++)
            {
                //Debug.Log(children [i].GetType().Name);
                this.children [i].OnGUI();
            }
        }

        public void addChildWidget(GUIWidget child_widget)
        {
            this.children.Add(child_widget);
            child_widget.setParent(this);
        }

        public void removeChildWidget(GUIWidget child_widget)
        {
            this.children.Remove(child_widget);
            child_widget.setParent(null);
        }

        public void pushChildToFront(GUIWidget child_widget) 
        {
            if (this.children.Remove(child_widget))
                this.children.Add(child_widget);
        }

        public Rect calculateContainer(Rect minimum = default(Rect), float margin = 0.0f)
        {
            float x_min = 0;
            float y_min = 0;
            float x_max = minimum.width;
            float y_max = minimum.height;
            foreach(GUIWidget child in this.children)
            {
                if (child.position.xMin < x_min) x_min = child.position.xMin;
                if (child.position.yMin < y_min) y_min = child.position.yMin;
                if (child.position.xMax > x_max) x_max = child.position.xMax;
                if (child.position.yMax > y_max) y_max = child.position.yMax;
            }
            return new Rect (x_min - margin, y_min - margin, x_max-x_min + 2 * margin, y_max-y_min + 2 * margin);
        }

        /*public override int catchMouse(Vector2 mouse_position)
        {
            if (this.position.Contains(mouse_position))
            {
                for (int i = 0; i < this.children.Count; i++)
                {
                    int catched_tag = this.children [i].catchMouse(mouse_position);
                    if (catched_tag >= 0)
                        return catched_tag;
                }
                return this.tag;
            }
            return -1;
        }*/
    }
}
