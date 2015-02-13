using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUITextField: SGUIWidget
    {
        public string text { get; private set; }
        public string label { get; private set; }

        public SGUITextField(string label)
        {
            this.label = label;
        }
        
        protected override void OnGUI()
        {
            this.text = EditorGUILayout.TextField(this.label, this.text, GUILayout.MinWidth(400));
        }

    }
}