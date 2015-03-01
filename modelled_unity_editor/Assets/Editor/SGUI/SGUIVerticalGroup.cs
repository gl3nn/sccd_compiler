using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIVerticalGroup : SGUIGroupWidget
    {
        public SGUIVerticalGroup(): base()
        {
            this.expand_height = true;
        }

        protected override void OnGUI()
        {
            EditorGUILayout.BeginVertical(this.style, GUILayout.ExpandHeight(this.expand_height), GUILayout.MinWidth(this.min_width));
            base.OnGUI();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
                this.position = GUILayoutUtility.GetLastRect();
        }
    }
}
