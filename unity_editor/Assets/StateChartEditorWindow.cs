using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace StateChartEditor{
    public class StateChartEditorWindow : EditorWindow
    {
        Vector2 current_mouse_position = Vector3.zero;
        Vector2 mouse_converted = Vector3.zero;
        List<Node> states = new List<Node>();
        List<Edge> edges = new List<Edge>();
        Rect draw_area;
        Rect tool_area;
        Rect total_draw_area = new Rect(0,0,0,0);
        Vector2 scroll_position = Vector2.zero;
        
        Rect without_scroll_bars;
        Node selected = null;

        public bool mouse_is_down { get; private set; }
        public bool get_mouse_outside_window { get; private set; }
        bool dragging = false;

        enum TopMode {  Select ,
                        Connect,
                        Draw};

        enum DrawMode { Basic,
                        Composite};

        string[] top_mode_string_array = Enum.GetValues(typeof(TopMode)).Cast<TopMode>().Select(value => ( Enum.GetName(typeof(TopMode), value))).ToArray();
        string[] draw_mode_string_array = Enum.GetValues(typeof(DrawMode)).Cast<DrawMode>().Select(value => ( Enum.GetName(typeof(DrawMode), value))).ToArray();

        int selected_top_mode = (int)TopMode.Select; 
        int selected_draw_mode = (int)DrawMode.Basic;
        
        [MenuItem("Window/StateChartEditor")]
        public static void Init()
        {
            StateChartEditorWindow window = (StateChartEditorWindow) EditorWindow.GetWindow(typeof(StateChartEditorWindow),false);
            window.wantsMouseMove = true;
            window.title = "StateChart Editor";
            //UnityEngine.Object.DontDestroyOnLoad( window );
        }
    
        public StateChartEditorWindow()
        {
            this.mouse_is_down = false;
            this.get_mouse_outside_window = false;
        }

        private void drawStateChart()
        {
            for(int i = 0; i < states.Count; i++){
                if (!states[i].selected){
                    states[i].draw();
                }
            }
            if(selected != null){
                selected.draw ();
            }            
            DrawEdges();
        }
        
        public Vector2 convertCoordsToChart(Vector2 coords){
            return coords - this.draw_area.TopLeft() + this.total_draw_area.TopLeft () + this.scroll_position;
        }        

        /// <summary>
        /// If scrollbars are added to the draw area, we don't want to use clicks on the scrollbars.
        /// So a new Rect gets created for checking clicks, taking into account the scrollbars. 
        /// </summary>
        private void calculateWithoutScrollBars()
        {
            this.without_scroll_bars = new Rect (this.draw_area.x,
                                                 this.draw_area.y,
                                                 this.draw_area.width,
                                                 this.draw_area.height);
            if (this.total_draw_area.width > this.draw_area.width){
                this.without_scroll_bars.width -= GUI.skin.GetStyle("verticalscrollbar").fixedWidth;
            }

            if (this.total_draw_area.height > this.draw_area.height){
                this.without_scroll_bars.height -= GUI.skin.GetStyle("horizontalscrollbar").fixedHeight;
            }
        }
        
        private void handleEvents()
        {
            this.calculateWithoutScrollBars();
            if (Event.current.button == 0)
            {
                if(Event.current.type == EventType.MouseDown)
                {    
                    this.mouse_is_down = true;
                    checkDrawAreaLeftClick();
                    this.get_mouse_outside_window = true;
                }
                else if(Event.current.rawType == EventType.MouseUp)
                {
                    this.mouse_is_down = false;
                    this.dragging = false;
                }
                else if (Event.current.type == EventType.mouseDrag && (this.dragging))
                {
                    this.selected.move(Event.current.delta);
                    Event.current.Use();
                }
            }
        }
        
        public void unSelect()
        {
            if(selected != null)
            {
                selected.unSelect();
                selected = null;
            }
        }
        
        private void checkDrawAreaLeftClick()
        {
            if (this.without_scroll_bars.Contains(this.current_mouse_position) )
            {                
                //Modes that make use of the previous selection    
                if (this.selected_top_mode == (int)TopMode.Connect)
                {
                    Node clicked = this.catchNodeClick();
                    if(clicked != null && selected != null)
                    {
                        this.addEdge(selected,clicked);
                        unSelect ();
                    }
                    else if(clicked != null)
                    {
                        clicked.setSelected();
                        selected = clicked;
                    }
                    return;
                }
                //Previous selection no longer needed            
                this.unSelect ();

                if (this.selected_top_mode == (int)TopMode.Select )
                {
                    Node clicked = this.catchNodeClick();
                    if(clicked != null)
                    {
                        clicked.setSelected();
                        this.selected = clicked;
                        this.dragging = true;
                    }
                }
                else if(this.selected_top_mode == (int)TopMode.Draw)
                {
                    this.states.Add( new Node(this.mouse_converted,"State" + (states.Count).ToString() ));
                    Event.current.Use();
                }
            }        
        }
        
        public void addEdge(Node start, Node end)
        {
            Debug.Log ("edge added");
            Edge newEdge = new Edge(start,end);
            start.addOutput(newEdge);
            end.addInput(newEdge);
            this.edges.Add (newEdge);
        }
                    
        private Node catchNodeClick()
        {
            for(int i = 0; i < states.Count; i++)
            {
                if (states[i].checkMouseOver(this.mouse_converted))
                    return states[i];
            }
            return null;
        }
        
        private void drawToolArea()
        {
             GUILayout.BeginArea (this.tool_area,"","box");
            GUILayout.BeginVertical();
            this.selected_top_mode    = GUILayout.SelectionGrid(this.selected_top_mode, this.top_mode_string_array, 1);

            if (this.selected_top_mode == (int)TopMode.Draw)
                this.selected_draw_mode = GUILayout.SelectionGrid(this.selected_draw_mode, this.draw_mode_string_array, 2, "toggle");

               GUILayout.EndVertical();
            GUILayout.EndArea ();
        }
        
        private void drawDrawArea()
        {                
            this.calculateScreen ();
            this.scroll_position = GUI.BeginScrollView (this.draw_area, this.scroll_position, this.total_draw_area);                
            this.drawStateChart();
            GUI.EndScrollView();
        }
    
        public void OnGUI()
        {              
            this.current_mouse_position.x = Event.current.mousePosition.x;
            this.current_mouse_position.y = Event.current.mousePosition.y;
            
            this.mouse_converted = convertCoordsToChart(this.current_mouse_position);
        
            this.tool_area = new Rect(0,0,200,Screen.height-22);
            this.draw_area = new Rect(200,0,Screen.width-200,Screen.height-22);
                    
            this.handleEvents ();    
            this.drawToolArea();
            this.drawDrawArea();   
        }
        
        private void calculateScreen()
        {
            float xMin = 0;
            float yMin = 0;
            float xMax = this.draw_area.width;
            float yMax = this.draw_area.height;
            foreach(var state in states)
            {
                if (state.rect.xMin < xMin) xMin = state.rect.xMin;
                if (state.rect.yMin < yMin) yMin = state.rect.yMin;
                if (state.rect.xMax > xMax) xMax = state.rect.xMax;
                if (state.rect.yMax > yMax) yMax = state.rect.yMax;
            }
            this.total_draw_area.Set (xMin,yMin,xMax-xMin, yMax-yMin);
        }
        
        private void DrawEdges()
        {
            for(int i = 0; i < edges.Count; i++)
                edges[i].draw();

            if (this.selected != null && this.selected_top_mode == (int)TopMode.Connect)
                MyTools.drawLine(selected.getPos(), this.mouse_converted);

            //Handles.EndGUI();
        }
        
        private void moveAutonomous()
        {
            if (!this.dragging && this.selected != null)
                this.selected.move( new Vector2(1,1));
        }

        public void Update()
        {
            this.moveAutonomous();
            this.Repaint ();
        }
    }
}