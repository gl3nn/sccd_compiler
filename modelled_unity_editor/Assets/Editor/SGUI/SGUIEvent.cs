using UnityEngine;

namespace SCCDEditor
{
    public sealed class SGUIEvent
    {
        public static SGUIEvent current = null;

        public int focus_tag { private set; get; }
        public Vector2 mouse_position { private set; get; }

        public SGUIEvent(int focus_tag, Vector2 mouse_position)
        {
            this.focus_tag = focus_tag;
            this.mouse_position = mouse_position;
        }
    }
}
