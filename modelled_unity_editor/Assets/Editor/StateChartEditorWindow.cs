using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public class StateChartEditorWindow : EditorWindow
    {
		// Imported from compiled model.
        Controller controller;

        EditorCanvas canvas;
        Vector2 canvas_scroll_position = Vector2.zero;

		double update_time = 0;

        int selected_toolbar_button = 0;
        string[] toolbar_buttons = new string[] {"Basic", "Composite"};


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
            this.canvas = new EditorCanvas();
            this.controller = new Controller(this.canvas);
            this.controller.start();
        }
    
        private void handleCanvasEvents() {
            //if (Event.current.type != EventType.Ignore && Event.current.type != EventType.Used){
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 2)
                {
                    this.controller.addInput(new sccdlib.Event("select", "input", new object[] {-1}));
                    this.controller.addInput(new sccdlib.Event("create", "input", new object[] {Event.current.mousePosition}));
                    Event.current.Use();
                } else if (Event.current.button == 0)
                {
                    int tag = this.catchClick(Event.current.mousePosition);
                    this.controller.addInput(new sccdlib.Event("select", "input", new object[] {tag}));
                    Event.current.Use();
                }
            } else if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button == 0)
                {
                    this.controller.addInput(new sccdlib.Event("mouse_up", "input", new object[] {}));
                    Event.current.Use();
                }
            } else if (Event.current.type == EventType.MouseDrag)
            {
                if (Event.current.button == 0)
                {
                    int tag = this.catchClick(Event.current.mousePosition - Event.current.delta);
                    sccdlib.Event drag_event = new sccdlib.Event("drag", "input", new object[]
                    {
                        tag,
                        Event.current.delta
                    });
                    this.controller.addInput(drag_event);
                    Event.current.Use();
                }
            }
            //}
			
        }

        private int catchClick(Vector2 mouse_position) {
            return this.canvas.catchClick(mouse_position);
        }

        private void drawToolArea()
        {
            GUILayout.BeginArea (new Rect(0,0, this.position.width, 40) , "", "box");
            GUILayout.BeginHorizontal();
            this.selected_toolbar_button = GUILayout.Toolbar(this.selected_toolbar_button, this.toolbar_buttons);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        
        public void OnGUI()
        {
            this.drawToolArea();
            Rect draw_area = new Rect(0, 40, this.position.width, this.position.height - 40);
            this.canvas.adjustSizeToMinimum(draw_area);
            this.canvas_scroll_position = GUI.BeginScrollView (draw_area, this.canvas_scroll_position, this.canvas.rect);    
            var stdOut = System.Console.Out;
            var consoleOut = new StringWriter();
            System.Console.SetOut(consoleOut);
            this.handleCanvasEvents();
            if (Event.current.type == EventType.Repaint)
                this.canvas.draw();
            this.controller.update (this.update_time);
            string output = consoleOut.ToString();
            if(output != "")
                Debug.Log(output);
            System.Console.SetOut(stdOut);
            this.update_time = 0;
            GUI.EndScrollView();
        }

        public void Update()
        {
			this.update_time = Time.deltaTime;
            this.Repaint ();
        }
    }
}