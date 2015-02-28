using UnityEngine;

namespace SCCDEditor
{
    public class SGUITopLevel : SGUIVerticalGroup
    {
        private SGUIModalWindow modal_window = null;

        public SGUIEditorWindow window { get; private set; }

        public static SGUITopLevel current { get; private set; }
        
        public SGUITopLevel(SGUIEditorWindow window)
        {
            this.window = window;
        }

        public void setModalWindow(SGUIModalWindow modal_window)
        {
            this.modal_window = modal_window;
        }

        protected override void OnGUI()
        {
            SGUITopLevel.current = this;

            if (Event.current.type != EventType.Layout)
            {
                this.position = this.window.position;
                if (Event.current.type != EventType.Repaint)
                    SGUIEvent.current = null;
            }

            GUI.enabled = this.modal_window == null;

            base.OnGUI();

            this.window.processEvent();

            if (this.modal_window != null && this.modal_window.should_close)
                this.modal_window = null;

            if (this.modal_window != null)
            {
                GUI.enabled = true;
                SGUIEvent.current = null;
                this.window.BeginWindows();
                this.modal_window.draw();
                this.window.EndWindows();
                this.window.processEvent();
            }

            SGUITopLevel.current = null;
        }
    }
}
