using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIGroupWidget: SGUIWidget
    {
        protected List<SGUIWidget>             children { private set; get; }
        protected GUIStyle                     style    { private set; get; }

        public SGUIGroupWidget(GUIStyle style)
        {
            this.children = new List<SGUIWidget>();
            this.style = style;
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
