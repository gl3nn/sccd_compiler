using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIVerticalGroup : SGUIGroupWidget
    {
          
        public SGUIVerticalGroup(string style = ""): base(style)
        {
        }

        protected override void OnGUI()
        {
            if (this.style == "")
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            else
                EditorGUILayout.BeginVertical(this.style, GUILayout.ExpandHeight(true));
            base.OnGUI();
            EditorGUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
                this.position = GUILayoutUtility.GetLastRect();
        }
    }
}
