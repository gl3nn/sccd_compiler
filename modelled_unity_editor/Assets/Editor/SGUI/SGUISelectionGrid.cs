using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUISelectionGrid: SGUIWidget
    {
        public string text { get; private set; }
        public string label { get; private set; }

        public int selected_option { get; private set; }
        string[] connection_options;

        public SGUISelectionGrid(string[] connection_options)
        {
            this.connection_options = connection_options;
            this.selected_option = 0;
        }
        
        protected override void OnGUI()
        {
            this.selected_option = GUILayout.SelectionGrid(this.selected_option, this.connection_options, 1, "MenuItem");
        }

    }
}