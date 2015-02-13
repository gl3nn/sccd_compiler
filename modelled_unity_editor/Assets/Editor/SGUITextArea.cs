using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUITextArea: SGUIWidget
    {
        public string text { get; private set; }
        public string label { get; private set; }

        public SGUITextArea(string label)
        {
            this.label = label;
        }
        
        protected override void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(this.label, EditorStyles.textField);
            this.text = EditorGUILayout.TextArea(this.text, GUILayout.Height(100));
            EditorGUILayout.EndHorizontal();
        }

    }
}