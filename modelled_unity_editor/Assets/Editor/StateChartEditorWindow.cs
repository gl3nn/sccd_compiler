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

        //public  EditorCanvas    canvas { private set; get; }

        private GUITopLevel  window_widget;


		[MenuItem("SCCD/Open Editor")]
        public static void Init()
        {
            StateChartEditorWindow window = (StateChartEditorWindow) EditorWindow.GetWindow(typeof(StateChartEditorWindow), false);
            //window.wantsMouseMove = true;
            window.title = "StateChart Editor";
            //UnityEngine.Object.DontDestroyOnLoad( window );
        }

        public StateChartEditorWindow()
        {
            this.window_widget = new GUITopLevel(this);
            this.controller = new Controller(this.window_widget);
            this.controller.start();
        }

        public void processEvent()
        {
            if (Event.current.type == EventType.Layout)
                return;
            
            if (Event.current.type == EventType.Repaint)
            {
                this.updateController();
                return;
            }
            
            sccdlib.Event input_event = null;

            // NEXT ARE ALL EVENTS RELATED TO THE MOUSE WHICH SHOULD NOT BE GENERATED IF THE MOUSE IS NOT OVER THE WINDOW
            if (GUIEvent.current == null)
                return;

            // PROCESS MOUSE DOWN EVENT
            if (Event.current.type == EventType.MouseDown)
            {
                string event_name = "";

                if (Event.current.button == 0)
                    event_name = "left-mouse-down";
                else if (Event.current.button == 1)
                    event_name = "right-mouse-down";
                else if (Event.current.button == 2)
                    event_name = "middle-mouse-down";

                if (event_name != "")
                    input_event = new sccdlib.Event(event_name, "input", new object[] {
                        GUIEvent.current.focus_tag,
                        GUIEvent.current.mouse_position
                    });
            }
            // PROCESS MOUSE UP EVENT
            else if (Event.current.type == EventType.MouseUp)
            {
                string event_name = "";

                if (Event.current.button == 0)
                    event_name = "left-mouse-up";
                else if (Event.current.button == 1)
                    event_name = "right-mouse-up";
                else if (Event.current.button == 2)
                    event_name = "middle-mouse-up";

                if (event_name != "")
                    input_event = new sccdlib.Event(event_name, "input", new object[] {
                        GUIEvent.current.focus_tag,
                        GUIEvent.current.mouse_position
                    });
            }
            // PROCESS MOUSE DRAG EVENT
            else if (Event.current.type == EventType.MouseDrag)
            {
                string event_name = "";

                if (Event.current.button == 0)
                    event_name = "left-mouse-drag";
                else if (Event.current.button == 1)
                    event_name = "right-mouse-drag";
                else if (Event.current.button == 2)
                    event_name = "middle-mouse-drag";

                if (event_name != "")
                    input_event = new sccdlib.Event(event_name, "input", new object[] {
                        GUIEvent.current.focus_tag,
                        GUIEvent.current.mouse_position,
                        Event.current.delta
                    });
            }
            // PROCESS MOUSE MOVE EVENT
            else if (Event.current.type == EventType.MouseMove)
            {
                input_event = new sccdlib.Event("mouse-move", "input", new object[] {
                    GUIEvent.current.focus_tag,
                    GUIEvent.current.mouse_position,
                    Event.current.delta
                });
            }

            if (input_event != null)
            {
                this.controller.addInput(input_event);
                this.updateController();
            }
        }

        private void updateController()
        {
            var stdOut = System.Console.Out;
            var consoleOut = new StringWriter();
            System.Console.SetOut(consoleOut);
            this.controller.update(this.update_time);
            string output = consoleOut.ToString();
            if (output != "")
                Debug.Log(output);
            System.Console.SetOut(stdOut);
            this.update_time = 0;
        }

        public void OnGUI()
        {
            GUI.skin = (GUISkin) (Resources.LoadAssetAtPath("Assets/Editor/SCCDSkin.guiskin", typeof(GUISkin)));

            this.window_widget.doOnGUI();
        }

        public void Update()
        {
			this.update_time = Time.deltaTime;
            this.Repaint ();
        }
    }
}