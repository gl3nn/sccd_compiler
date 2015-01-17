using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class GUIVerticalGroup : GUIGroupWidget
    {
        protected override void OnGUI()
        {
            GUILayout.BeginVertical();
            base.OnGUI();
            GUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
                this.position = GUILayoutUtility.GetLastRect();
        }
    }
}
