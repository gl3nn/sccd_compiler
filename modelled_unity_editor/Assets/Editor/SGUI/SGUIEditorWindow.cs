using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{
    public abstract class SGUIEditorWindow : EditorWindow
    {
		// Imported from compiled model.
        protected sccdlib.GameLoopControllerBase    controller;

        private   double                            update_time = 0;

        protected SGUITopLevel                      top_level_widget;

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

            if (SGUIEvent.current != null)
            {
                // NEXT ARE ALL EVENTS RELATED TO THE MOUSE WHICH SHOULD NOT BE GENERATED IF THE MOUSE IS NOT OVER THE WINDOW

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
                    {
                        input_event = new sccdlib.Event(event_name, "input", new object[] {
                            SGUIEvent.current.focus_tag,
                            SGUIEvent.current.mouse_position
                        });
                        GUI.FocusControl("");
                    }
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
                            SGUIEvent.current.focus_tag,
                            SGUIEvent.current.mouse_position
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
                            SGUIEvent.current.focus_tag,
                            SGUIEvent.current.mouse_position,
                            Event.current.delta
                        });
                }
                // PROCESS MOUSE MOVE EVENT
                else if (Event.current.type == EventType.MouseMove)
                {
                    input_event = new sccdlib.Event("mouse-move", "input", new object[] {
                        SGUIEvent.current.focus_tag,
                        SGUIEvent.current.mouse_position,
                        Event.current.delta
                    });
                }
            }
            if (input_event == null)
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    input_event = new sccdlib.Event("key-down", "input", new object[] {
                        Event.current.keyCode
                    });
                }
            }

            if (input_event != null)
            {
                this.controller.addInput(input_event);
                this.updateController();
                this.Repaint ();
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

        public void generateEvent(string event_name, string port, params object[] parameters)
        {
            this.controller.addInput(new sccdlib.Event(event_name, port, parameters));
            this.updateController();
        }

        public void OnGUI()
        {
            GUI.skin = (GUISkin) (Resources.LoadAssetAtPath("Assets/Editor/SCCDSkin.guiskin", typeof(GUISkin)));
            this.top_level_widget.doOnGUI();
        }

        public void Update()
        {
            this.update_time += Time.deltaTime;
            this.top_level_widget.Update();
        }

        public virtual void restart()
        {
        }
    }
}