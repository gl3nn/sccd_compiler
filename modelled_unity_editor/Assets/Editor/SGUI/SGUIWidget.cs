using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIWidget
    {
        public Rect                                 position    { get; protected set; }
        public int                                  tag         { get; private set; }
        public SGUIGroupWidget                      parent      { get; set; }
        public bool                                 is_enabled  { get; private set; }

        private static int                          tag_counter = 0;

        public SGUIWidget()
        {
            this.tag = SGUIWidget.tag_counter++;
            this.is_enabled = true;
        }
        
        public void setCenter(Vector2 center)
        {
            Rect rect = this.position;
            rect.center = center;
            this.position = rect;
        }

        protected void catchMouseDefault()
        {
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
            {
                if (this.position.Contains(Event.current.mousePosition))
                    SGUIEvent.current = new SGUIEvent(this.tag, Event.current.mousePosition);
            }
        }

        public void setEnabled(bool is_enabled = true)
        {
            this.is_enabled = is_enabled;
        }

        public void doOnGUI()
        {
            bool previous_enabled_state = GUI.enabled;
            if (!this.is_enabled)
                GUI.enabled = false;
            this.OnGUI();
            GUI.enabled = previous_enabled_state;
        }

        protected virtual void OnGUI()
        {
        }

        public void setPosition(Rect rect)
        {
            this.position = position;
        }

        public void setPosition(float x, float y, float w, float h)
        {
            this.position = new Rect(x, y, w, h);
        }

        protected void generateEvent(string event_name, string port, params object[] parameters)
        {
            SGUIEditorWindow.current.generateEvent(event_name, port, parameters);
        }
    }
}
