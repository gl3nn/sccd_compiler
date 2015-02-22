using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIWidget
    {
        public Rect                                 position    { get; protected set; }
        public int                                  tag         { get; private set; }
        public SGUIGroupWidget                      parent      { get; set; }

        private static int                          tag_counter = 0;

        public SGUIWidget()
        {
            this.tag = SGUIWidget.tag_counter++;
        }

        protected void catchMouseDefault()
        {
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
            {
                if (this.position.Contains(Event.current.mousePosition))
                    SGUIEvent.current = new SGUIEvent(this.tag, Event.current.mousePosition);
            }
        }

        public void doOnGUI()
        {
            this.OnGUI();
        }

        protected virtual void OnGUI()
        {
        }

        protected void generateEvent(string event_name, string port, params object[] parameters)
        {
            SGUITopLevel.current.window.generateEvent(event_name, port, parameters);
        }
    }
}
