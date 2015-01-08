using UnityEngine;

namespace SCCDEditor
{
    public class GUIWidget
    {
        public Rect                         position    { get; protected set; }
        public GUIWidgetGroup               parent      { get; private set; }
        public int                          tag         { get; private set; }

        public static int                   tag_counter = 0;

        public static sccdlib.Event         current_event = null;

        public static Controller            controller;

        public GUIWidget(GUIWidgetGroup parent)
        {
            this.tag = GUIWidget.tag_counter++;
            if (parent != null)
                parent.addChildWidget(this);
        }

        public void setParent(GUIWidgetGroup parent)
        {
            this.parent = parent;
        }

        public void pushToFront() 
        {
            if (this.parent != null)
                this.parent.pushChildToFront(this);
        }

        public bool isAncestorOf(GUIWidget widget)
        {
            while (widget.parent != null)
            {
                if (widget.parent == this)
                    return true;
                widget = widget.parent;
            }
            return false;
        }

        public virtual void OnGUI()
        {
        }

        protected void doLeftMouseDown()
        {
            GUIWidget.current_event = new sccdlib.Event("left-mouse-down", "input", new object[] {this.tag, Event.current.mousePosition});
        }

        protected void doMiddleMouseDown()
        {
            GUIWidget.controller.addInput(new sccdlib.Event("middle-mouse-down", "input", new object[] {this.tag, Event.current.mousePosition}));
        }
    }
}
