using UnityEngine;

namespace SCCDEditor
{
    public class GUITopLevel : GUIVerticalGroup
    {
        private GUIModalWindow modal_window = null;

        private StateChartEditorWindow window;

        public static GUITopLevel current { get; private set; }
        
        public GUITopLevel(StateChartEditorWindow window)
        {
            this.window = window;
        }

        public void setModalWindow(GUIModalWindow modal_window)
        {
            this.modal_window = modal_window;
        }

        protected override void OnGUI()
        {
            GUITopLevel.current = this;

            if (Event.current.type != EventType.Layout)
            {
                this.position = this.window.position;
                if (Event.current.type != EventType.Repaint)
                    GUIEvent.current = null;
            }

            GUI.enabled = this.modal_window == null;

            base.OnGUI();

            this.window.processEvent();

            if (this.modal_window != null && this.modal_window.should_close)
                this.modal_window = null;

            if (this.modal_window != null)
            {
                GUI.enabled = true;
                this.window.BeginWindows();
                this.modal_window.draw();
                this.window.EndWindows();
            }

            GUITopLevel.current = null;
        }
    }
}
