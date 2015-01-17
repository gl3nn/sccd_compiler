using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class GUIWidget
    {
        public Rect                         position    { get; protected set; }
        public int                          tag         { get; private set; }

        private static int                  tag_counter = 0;

        public GUIWidget()
        {
            this.tag = GUIWidget.tag_counter++;
        }

        protected void catchMouseDefault()
        {
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
            {
                if (this.position.Contains(Event.current.mousePosition))
                    GUIEvent.current = new GUIEvent(this.tag, Event.current.mousePosition);
            }
        }

        public void doOnGUI()
        {
            this.OnGUI();
        }

        protected virtual void OnGUI()
        {
        }
    }
}
