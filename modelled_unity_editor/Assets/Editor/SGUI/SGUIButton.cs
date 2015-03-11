using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIButton: SGUIWidget
    {
        public      string      label           { private set; get; }
        public      string      action          { private set; get; }
        protected   GUIStyle    style           { private set; get; }

        public SGUIButton(string label, string action = "", GUIStyle style = null)
        {
            this.label = label;
            this.action = action;
            this.style = style == null ? EditorStyles.miniButton : style;
        }
        
        protected override void OnGUI()
        {
            //GUIStyle button_style = GUI.skin.GetStyle("button");
            if (GUILayout.Button(this.label, style))
            {
                SGUIEvent.current = null;
                this.generateEvent("button_pressed", "input", this.tag, this.action);
            }
        }

    }
}