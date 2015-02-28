using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIGroupWidget: SGUIWidget
    {
        protected List<SGUIWidget>              children        { private set; get; }
        protected GUIStyle                      style;
        protected float                         min_width;
        protected float                         min_height;
        protected bool                          expand_width;
        protected bool                          expand_height;
                                                

        public SGUIGroupWidget()
        {
            this.children = new List<SGUIWidget>();
            this.style = GUIStyle.none;
            this.min_width = 0.0f;
            this.min_height = 0.0f;
            this.expand_width = false;
            this.expand_height = false;
        }

        public void setStyle(GUIStyle style)
        {
            this.style = style;
        }

        public void setMinWidth(float min_width)
        {
            this.min_width = min_width;
        }
        
        public void setMinHeight(float min_height)
        {
            this.min_height = min_height;
        }

        public void setExpandWidth(bool expand_width)
        {
            this.expand_width = expand_width;
        }

        public void setExpandHeight(bool expand_height)
        {
            this.expand_height = expand_height;
        }
        
        public int children_count
        {
            get { return this.children.Count; }
        }

        protected override void OnGUI()
        {
            for (int i=0; i < this.children.Count; i++)
            {
                this.children[i].doOnGUI();
            }
        }

        public void addChild(SGUIWidget child_widget)
        {
            this.children.Add(child_widget);
            child_widget.parent = this;
        }

        public SGUIWidget getChild(int index)
        {
            return this.children[index];
        }

        public void removeChild(SGUIWidget child_widget)
        {
            this.children.Remove(child_widget);
            child_widget.parent = null;
        }

        public void removeFromParent()
        {
            this.parent.removeChild(this);
        }

        public void clearChildren()
        {
            this.children.Clear();
        }
    }
}
