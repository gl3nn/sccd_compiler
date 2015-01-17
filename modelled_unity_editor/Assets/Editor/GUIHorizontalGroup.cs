using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class GUIHorizontalGroup : GUIGroupWidget
    {
        protected override void OnGUI()
        {
            GUILayout.BeginHorizontal("box");
            base.OnGUI();
            GUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
                this.position = GUILayoutUtility.GetLastRect();
        }
    }
}
