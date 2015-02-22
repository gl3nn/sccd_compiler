using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIHorizontalGroup : SGUIGroupWidget
    {

        public SGUIHorizontalGroup(GUIStyle style): base(style)
        {
        }

        public SGUIHorizontalGroup(): base(GUIStyle.none)
        {
        }

        protected override void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(this.style, GUILayout.ExpandWidth(true));
            base.OnGUI();
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
                this.position = GUILayoutUtility.GetLastRect();
        }
    }
}
