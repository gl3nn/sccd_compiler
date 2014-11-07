using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public class StateChartEditorWindow : EditorWindow
    {
		// Imported from compiled model.
        Controller controller;

        CanvasItem canvas;
		double update_time = 0;
		sccdlib.Event draw_event = new sccdlib.Event("draw", "engine");


		[MenuItem("SCCD/Open Editor")]
        public static void Init()
        {
            StateChartEditorWindow window = (StateChartEditorWindow) EditorWindow.GetWindow(typeof(StateChartEditorWindow), false);
            window.wantsMouseMove = true;
            window.title = "StateChart Editor";
            UnityEngine.Object.DontDestroyOnLoad( window );
        }
    
        public StateChartEditorWindow()
        {
            this.canvas = new CanvasItem();
            this.controller = new Controller(this.canvas);
            this.controller.start();
        }
    
        private void handleEvents() {
            if (Event.current.type == EventType.MouseDown) {    
				if (Event.current.button == 2) {
					sccdlib.Event create_event = new sccdlib.Event ("create", "input", new object[] {Event.current.mousePosition});
					this.controller.addInput (create_event);
				} else if (Event.current.button == 0) {
					int tag = this.catchClick (Event.current.mousePosition);
					if (tag >= 0) {
						sccdlib.Event select_event = new sccdlib.Event ("select", "input", new object[] {tag});
						this.controller.addInput (select_event);
					}
				}
			} else if (Event.current.type == EventType.MouseDrag) {
				if (Event.current.button == 0) {
					int tag = this.catchClick (Event.current.mousePosition - Event.current.delta);
					if (tag >= 0) {
						sccdlib.Event drag_event = new sccdlib.Event ("drag", "input", new object[] {tag, Event.current.delta});
						this.controller.addInput (drag_event);
					}
				}
			}
			this.controller.addInput(this.draw_event);
        }

        private int catchClick(Vector2 mouse_position) {
            if (this.canvas.containsPosition(mouse_position))
                return this.canvas.catchClick(mouse_position);
            return -1;
        }

        public void OnGUI()
        {
            this.canvas.setSize(this.position.width, this.position.height);
            var stdOut = System.Console.Out;
            var consoleOut = new StringWriter();
            System.Console.SetOut(consoleOut);
            this.handleEvents();
            this.controller.update (this.update_time);
            string output = consoleOut.ToString();
            if(output != "")
                Debug.Log(output);
            System.Console.SetOut(stdOut);
            this.update_time = 0;
        }

        public void Update()
        {
			this.update_time = Time.deltaTime;
            this.Repaint ();
        }
    }
}