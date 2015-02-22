using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIButton: SGUIWidget
    {
        public      bool        is_on = false;
        public      bool        is_toggle       { get; private set; }
        public      string      label           { private set; get; }
        public      string      action          { private set; get; }
        protected   GUIStyle    style           { private set; get; }

        public SGUIButton(string label, string action, bool is_toggle_button, GUIStyle style = null)
        {
            this.label = label;
            this.action = action;
            this.is_toggle = is_toggle_button;
            this.style = style == null ? EditorStyles.miniButton : style;
        }
        
        protected override void OnGUI()
        {
            //GUIStyle button_style = GUI.skin.GetStyle("button");
            Rect position = GUILayoutUtility.GetRect(new GUIContent(this.label), this.style);
            int id = GUIUtility.GetControlID (FocusType.Keyboard);

            EventType event_type = Event.current.GetTypeForControl(id);
            if (event_type == EventType.MouseDown)
            {
                if (this.position.Contains(Event.current.mousePosition))
                {
                    GUIUtility.hotControl = id;
                }
            } else if (event_type == EventType.MouseUp)
            {
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                }
            } else if (event_type == EventType.Repaint)
            {
                this.position = position;
                this.style.Draw(
                    this.position,
                    this.label,
                    this.position.Contains(Event.current.mousePosition), //isHover
                    GUIUtility.hotControl == id, //isActive
                    this.is_on, //on
                    false //hasKeyboardFocus
                );
            }
            this.catchMouseDefault();
        }

    }
}