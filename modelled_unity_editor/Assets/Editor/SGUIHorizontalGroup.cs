using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIHorizontalGroup : SGUIGroupWidget
    {

        public SGUIHorizontalGroup(string style = ""): base(style)
        {
        }

        protected override void OnGUI()
        {
            if (this.style == "")
                EditorGUILayout.BeginHorizontal();
            else
                EditorGUILayout.BeginHorizontal(this.style);
            base.OnGUI();
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
                this.position = GUILayoutUtility.GetLastRect();
        }
    }
}
