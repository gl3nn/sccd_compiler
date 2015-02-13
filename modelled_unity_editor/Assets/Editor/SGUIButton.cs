using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIButton: SGUIWidget
    {
        public SGUIButtonInformation properties { get; private set; }
        public bool is_on = false;
        public bool is_toggle { get; private set; }

        public SGUIButton(SGUIButtonInformation button_information, bool is_toggle_button)
        {
            this.properties = button_information;
            this.is_toggle = is_toggle_button;
        }
        
        protected override void OnGUI()
        {
            GUIStyle button_style = GUI.skin.GetStyle("button");
            Rect position = GUILayoutUtility.GetRect(new GUIContent(this.properties.label), button_style);
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
                button_style.Draw(
                    this.position,
                    this.properties.label,
                    this.position.Contains(Event.current.mousePosition), //isHover
                    GUIUtility.hotControl == id, //isActive
                    this.is_on, //on
                    false //hasKeyboardFocus
                );
            }
            this.catchMouseDefault();
        }

    }

    public class SGUIButtonInformation
    {
        public string label         { private set; get; }
        public string action        { private set; get; }

        public SGUIButtonInformation(string label, string action)
        {
            this.label = label;
            this.action = action;
        }
    }
}