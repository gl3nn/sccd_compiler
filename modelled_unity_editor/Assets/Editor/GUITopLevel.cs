using UnityEngine;

namespace SCCDEditor
{
    public class GUITopLevel : GUIVerticalGroup
    {
        public GUITopLevel() : base(null)
        {
        }

        public override void OnGUI()
        {
            if (Event.current.type != EventType.Layout)
            {
                GUIWidget.current_event = null;
            }
            base.OnGUI();
            if (Event.current.type != EventType.Layout)
            {
                if (GUIWidget.current_event != null)
                    GUIWidget.controller.addInput(GUIWidget.current_event);
            }

        }
    }
}
