using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class GUIButton: GUIWidget
    {
        public GUIButtonInformation properties { get; private set; }
        public bool is_on = false;

        public GUIButton(GUIWidgetGroup parent, GUIButtonInformation button_information): base(parent)
        {
            this.properties = button_information;
        }
        
        public override void OnGUI()
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
                    this.doLeftMouseDown();
                    //Event.current.Use();
                }
            } else if (event_type == EventType.MouseUp)
            {
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    //Event.current.Use();
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
        }

    }

    public class GUIButtonInformation
    {
        public string label         { private set; get; }
        public string event_name    { private set; get; }

        public GUIButtonInformation(string label, string event_name)
        {
            this.label = label;
            this.event_name = event_name;
        }
    }
}