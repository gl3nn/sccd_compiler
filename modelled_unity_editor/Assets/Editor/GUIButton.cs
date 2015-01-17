using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class GUIButton: GUIWidget
    {
        public GUIButtonInformation properties { get; private set; }
        public bool is_on = false;

        public GUIButton(GUIButtonInformation button_information)
        {
            this.properties = button_information;
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

    public class GUIButtonInformation
    {
        public string label         { private set; get; }
        public string action        { private set; get; }

        public GUIButtonInformation(string label, string action)
        {
            this.label = label;
            this.action = action;
        }
    }
}