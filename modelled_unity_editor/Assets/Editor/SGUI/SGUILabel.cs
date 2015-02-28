using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUILabel: SGUIWidget
    {
        public string label { get; private set; }

        public SGUILabel(string label)
        {
            this.label = label;
        }

        public void setLabel(string label)
        {
            this.label = label;
        }
        
        protected override void OnGUI()
        {
            GUILayout.Label(this.label);
            //this.text = EditorGUILayout.TextField(this.label, this.text, GUILayout.MinWidth(400));
        }

    }
}