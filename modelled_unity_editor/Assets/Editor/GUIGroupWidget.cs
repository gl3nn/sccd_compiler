using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class GUIGroupWidget: GUIWidget
    {
        protected List<GUIWidget>             children { private set; get; }

        public GUIGroupWidget(): base()
        {
            this.children = new List<GUIWidget>();
        }

        protected override void OnGUI()
        {
            for (int i=0; i < this.children.Count; i++)
            {
                this.children[i].doOnGUI();
            }
        }

        public void addChild(GUIWidget child_widget)
        {
            this.children.Add(child_widget);
            //child_widget.parent = this;
        }

        public void removeChildWidget(GUIWidget child_widget)
        {
            this.children.Remove(child_widget);
            //child_widget.parent = null;
        }
    }
}
