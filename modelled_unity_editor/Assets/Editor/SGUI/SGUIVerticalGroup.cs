using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIVerticalGroup : SGUIGroupWidget
    {
        protected float min_width = 0.0f;
          
        public SGUIVerticalGroup(GUIStyle style): base(style)
        {
        }

        public SGUIVerticalGroup(): base(GUIStyle.none)
        {
        }

        public void setMinWidth(float min_width)
        {
            this.min_width = min_width;
        }

        protected override void OnGUI()
        {
            EditorGUILayout.BeginVertical(this.style, GUILayout.ExpandHeight(true), GUILayout.MinWidth(this.min_width));
            base.OnGUI();
            EditorGUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
                this.position = GUILayoutUtility.GetLastRect();
        }
    }
}
