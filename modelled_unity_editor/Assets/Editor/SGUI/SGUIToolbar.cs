using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIToolbar: SGUIWidget
    {
        private int status = 0;
        private string[] labels;
        private string[] actions;

        public SGUIToolbar(string[] labels, string[] actions)
        {
            this.labels = labels;
            this.actions = actions;
            if (this.labels.Length != this.actions.Length)
                throw new SGUIException("Toolbar needs the same number of labels and actions.");
        }
        
        protected override void OnGUI()
        {
            EditorGUI.BeginChangeCheck ();
            this.status = GUILayout.Toolbar(this.status, this.labels);
            if (EditorGUI.EndChangeCheck ()) {
                this.generateEvent("toolbar_changed", "input", this.tag, this.getCurrentAction());
            }
        }

        public string getCurrentAction()
        {
            return actions [this.status];
        }

    }
}