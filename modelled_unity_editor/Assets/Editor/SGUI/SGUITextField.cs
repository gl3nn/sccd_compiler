using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUITextField: SGUIWidget
    {
        public string text { get; private set; }
        public string label { get; private set; }

        public SGUITextField(string label, string text = "")
        {
            this.label = label;
            this.text = text;
        }
        
        protected override void OnGUI()
        {
            EditorGUI.BeginChangeCheck ();
            this.text = EditorGUILayout.TextField(this.label, this.text);
            if (EditorGUI.EndChangeCheck ()) {
                this.generateEvent("changed", "input", this.tag);
            }
        }

    }
}