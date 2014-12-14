using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public class StateChartEditorWindow : EditorWindow
    {
		// Imported from compiled model.
        private Controller      controller;

        private double          update_time = 0;

        public  EditorCanvas    canvas { private set; get; }
		private Rect            canvas_area;
        private Vector2         canvas_scroll_position = Vector2.zero;

        private SCCDModalWindow modal_window = null;

        private int             selected_toolbar_button = 0;
        private string[]        toolbar_buttons = new string[] {"Basic", "Composite"};


		[MenuItem("SCCD/Open Editor")]
        public static void Init()
        {
            StateChartEditorWindow window = (StateChartEditorWindow) EditorWindow.GetWindow(typeof(StateChartEditorWindow), false);
            window.wantsMouseMove = true;
            window.title = "StateChart Editor";
            UnityEngine.Object.DontDestroyOnLoad( window );
        }

        public Vector2 getWindowModalPosition()
        {
            return new Vector2(this.position.width/2, this.position.height/2);
        }

		public Vector2 getCanvasModalPosition()
		{
            return new Vector2(this.canvas_area.width/2, this.canvas_area.height/2);
		}

        public StateChartEditorWindow()
        {
            this.canvas = new EditorCanvas(this);
            this.controller = new Controller(this.canvas);
            this.controller.start();
        }

		public void createModalWindow(string title, SCCDModalWindow.DrawFunction draw_function, bool over_canvas = false)
		{
            if (over_canvas)
                this.modal_window = new SCCDModalWindow(title, draw_function, this.getCanvasModalPosition);
            else
                this.modal_window = new SCCDModalWindow(title, draw_function, this.getWindowModalPosition);
		}
    
        private void handleCanvasEvents() 
        {
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
            GUILayout.BeginHorizontal("box");
            this.selected_toolbar_button = GUILayout.Toolbar(this.selected_toolbar_button, this.toolbar_buttons, "button");
            GUILayout.EndHorizontal();
        }

        private void updateController()
        {
            var stdOut = System.Console.Out;
            var consoleOut = new StringWriter();
            System.Console.SetOut(consoleOut);
            if (Event.current.type == EventType.Layout)
                this.controller.update(this.update_time);
            string output = consoleOut.ToString();
            if(output != "")
                Debug.Log(output);
            System.Console.SetOut(stdOut);
            this.update_time = 0;
        }

        private void drawGUI()
        {
            GUILayout.BeginVertical();
            this.drawToolArea();
            Rect canvas_area = GUILayoutUtility.GetRect(0, 100000, 0, 100000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            this.canvas.adjustSizeToMinimum(canvas_area);
            this.canvas_scroll_position = GUI.BeginScrollView (canvas_area, this.canvas_scroll_position, this.canvas.rect);
            if (Event.current.type == EventType.Repaint)
            {
                this.canvas_area = canvas_area;
                this.canvas.draw();
            }
            this.handleCanvasEvents();
            this.updateController();
            GUI.EndScrollView();
            GUILayout.EndVertical();
        }

        public void OnGUI()
        {
            GUI.enabled = this.modal_window == null;
            GUI.skin = (GUISkin) (Resources.LoadAssetAtPath("Assets/Editor/SCCDSkin.guiskin", typeof(GUISkin)));
            this.drawGUI();
            if (Event.current.type == EventType.Layout)
            {
                if (this.modal_window != null && this.modal_window.should_close)
                    this.modal_window = null;
            }
            if (this.modal_window != null)
            {
                GUI.enabled = true;
                this.BeginWindows();
                this.modal_window.draw();
                this.EndWindows();
            }
        }

        public void Update()
        {
			this.update_time = Time.deltaTime;
            this.Repaint ();
        }
    }
}