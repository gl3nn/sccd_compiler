using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUIHorizontalGroup : SGUIGroupWidget
    {

        public SGUIHorizontalGroup(): base()
        {
            this.expand_width = true;
        }

        protected override void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(this.style, GUILayout.ExpandWidth(this.expand_width));
            base.OnGUI();
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
                this.position = GUILayoutUtility.GetLastRect();
        }
    }
}
