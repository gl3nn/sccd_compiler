using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUICheckBox: SGUIWidget
    {
        public string label { get; private set; }
        public bool is_checked { get; private set; }

        public SGUICheckBox(string label, bool is_checked = false)
        {
            this.label = label;
            this.is_checked = is_checked;
        }

        public void setChecked(bool is_checked = true)
        {
            this.is_checked = is_checked;
        }
        
        protected override void OnGUI()
        {

            EditorGUI.BeginChangeCheck ();
            this.is_checked = EditorGUILayout.Toggle(this.label, this.is_checked);
            if (EditorGUI.EndChangeCheck ()) {
                this.generateEvent("changed", "input", this.tag);
            }
        }

    }
}