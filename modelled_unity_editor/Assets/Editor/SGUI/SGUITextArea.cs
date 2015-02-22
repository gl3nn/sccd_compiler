using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUITextArea: SGUIWidget
    {
        public string text { get; private set; }
        public string label { get; private set; }

        public SGUITextArea(string label, string text = "")
        {
            this.label = label;
            this.text = text;
        }
        
        protected override void OnGUI()
        {
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(this.label, EditorStyles.textField);
            this.text = EditorGUILayout.TextArea(this.text, GUILayout.Height(100));
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck ()) {
                this.generateEvent("changed", "input", this.tag);
            }
        }

    }
}