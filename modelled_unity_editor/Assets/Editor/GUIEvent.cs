using UnityEngine;

namespace SCCDEditor
{
    public sealed class GUIEvent
    {
        public static GUIEvent current = null;

        public int focus_tag { private set; get; }
        public Vector2 mouse_position { private set; get; }

        public GUIEvent(int focus_tag, Vector2 mouse_position)
        {
            this.focus_tag = focus_tag;
            this.mouse_position = mouse_position;
        }
    }
}
