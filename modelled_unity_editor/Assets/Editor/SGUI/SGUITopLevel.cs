using UnityEngine;
using UnityEditor;

namespace SCCDEditor
{
    public class SGUITopLevell : SGUIVerticalGroup
    {
        private SGUIModalWindow modal_window = null;

        public SGUIEditorWindow window { get; private set; }

        public static SGUITopLevell current { get; private set; }

        private bool open_save_dialog = false;
        private bool do_restart = false;
        
        public SGUITopLevell(SGUIEditorWindow window)
        {
            this.window = window;
        }

        public void setModalWindow(SGUIModalWindow modal_window)
        {
            this.modal_window = modal_window;
        }

        public void openSaveDialog()
        {
            this.open_save_dialog = true;
        }

        public void restart()
        {
            this.do_restart = true;
        }

        protected override void OnGUI()
        {
            
            SGUITopLevell.current = this;

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
                this.modal_window.doOnGUI();
                this.window.EndWindows();
                this.window.processEvent();
            }

            SGUITopLevell.current = null;

            if (this.do_restart)
            {
                this.window.restart();
            }
        }

        public void Update()
        {
            
            if (this.open_save_dialog)
            {
                this.open_save_dialog = false;
                string save_path = EditorUtility.SaveFilePanelInProject("Save Model", "mycoolmodel", "xml", "Save model");
                this.window.generateEvent("save_dialog_closed", "input", save_path);
            }
        }
    }
}
