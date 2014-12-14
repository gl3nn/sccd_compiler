/*
    Statecharts + Class Diagram compiler by Glenn De Jonghe
    
    Generated on 2014-12-13 18:45:59.
    
    Model name:   SCCD Editor
    Model author: Glenn De Jonghe
    Model description:
    
        SCCD visual editor for Unity.
*/

using System;
using System.Collections.Generic;
using sccdlib;
using UnityEditor;
using UnityEngine;
using SCCDEditor;
using Event = sccdlib.Event;
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.


public class Canvas : IRuntimeClass
{
    private ControllerBase controller;
    private ObjectManagerBase object_manager;
    private bool active = false;
    private bool state_changed = false;
    private EventQueue events = new EventQueue();
    private Dictionary<int,double> timers = null;
    
    /// <summary>
    /// Enum uniquely representing all statechart nodes.
    /// </summary>
    public enum Node {
        Root,
        Root_waiting,
        Root_creation,
        Root_connecting,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    EditorCanvas canvas;
    Dictionary<int, int> children_map;
    Dictionary<int, int> all_states_map;
    CanvasItem current_item;
    
    /// <summary>
    /// Constructor part that is common for all constructors.
    /// </summary>
    private void commonConstructor(ControllerBase controller)
    {
        this.controller = controller;
        this.object_manager = controller.getObjectManager();
        
        //Initialize statechart :
        this.current_state[Node.Root] = new List<Node>();
    }
    
    public void start()
    {
        if (!this.active) {
            this.active = true;
            this.enter_Root_waiting();
        }
    }
    
    public Canvas(ControllerBase controller, EditorCanvas canvas)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.canvas = canvas;
        this.children_map = new Dictionary<int,int>();
        this.all_states_map = new Dictionary<int,int>();
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_waiting()
    {
        this.current_state[Node.Root].Add(Node.Root_waiting);
    }
    
    private void exit_Root_waiting()
    {
        this.current_state[Node.Root].Remove(Node.Root_waiting);
    }
    
    private void enter_Root_creation()
    {
        this.current_state[Node.Root].Add(Node.Root_creation);
    }
    
    private void exit_Root_creation()
    {
        this.current_state[Node.Root].Remove(Node.Root_creation);
    }
    
    private void enter_Root_connecting()
    {
        this.current_state[Node.Root].Add(Node.Root_connecting);
    }
    
    private void exit_Root_connecting()
    {
        this.current_state[Node.Root].Remove(Node.Root_connecting);
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_waiting){
                catched = this.transition_Root_waiting(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_creation){
                catched = this.transition_Root_creation(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_connecting){
                catched = this.transition_Root_connecting(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_waiting(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "create" && e.getPort() == "input"){
            enableds.Add(0);
        }
        
        if (e.getName() == "disconnect_child" && e.getPort() == ""){
            enableds.Add(1);
        }
        
        if (e.getName() == "connect_child_to_parent" && e.getPort() == ""){
            enableds.Add(2);
        }
        
        if (e.getName() == "new_child" && e.getPort() == ""){
            enableds.Add(3);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_waiting. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                Vector2 position = (Vector2)parameters[0];
                this.exit_Root_waiting();
                this.current_item = new CanvasItem(position, this.canvas);
                this.object_manager.addEvent(new Event("create_instance", "", new object[] { this, "all_states","BasicState",this.current_item}));
                this.enter_Root_creation();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                CanvasItem child = (CanvasItem)parameters[0];
                this.exit_Root_waiting();
                Debug.Log(string.Format("removing tag {0} from canvas", child.tag));
                this.object_manager.addEvent(new Event("unassociate_instance", "", new object[] { this, String.Format("children[{0}]", this.children_map[child.tag])}));
                this.children_map.Remove(child.tag);
                                	this.canvas.removeChild(child);
                this.enter_Root_waiting();
            } else if (enabled == 2){
                object[] parameters = e.getParameters();
                CanvasItem child = (CanvasItem)parameters[0];
                CanvasItem parent = (CanvasItem)parameters[1];
                this.exit_Root_waiting();
                this.current_item = child;
                                    String parent_path = ".";
                                    if (parent != null)
                                    {
                                        parent_path = String.Format("all_states[{0}]", this.all_states_map[parent.tag]);
                                    }
                                	String child_path = String.Format("all_states[{0}]", this.all_states_map[child.tag]);
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, child_path,parent_path + "/children"}));
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, parent_path,child_path + "/parent"}));
                this.enter_Root_connecting();
            } else if (enabled == 3){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                CanvasItem child = (CanvasItem)parameters[1];
                this.exit_Root_waiting();
                this.canvas.addChild(child);
                                    this.children_map[child.tag] = id;
                                    //this.canvas.adjustSize();
                                    Debug.Log(string.Format("new child with id {0} and tag {1}", id, child.tag));
                this.enter_Root_waiting();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_creation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "instance_created" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_creation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                String association_name = (String)parameters[1];
                this.exit_Root_creation();
                String association_path = String.Format("{0}[{1}]", association_name, id);
                                	//this.children_map[this.current_item.tag] = id;
                                    Debug.Log(string.Format("setting tag {0} to id {1}", this.current_item.tag, id));
                                	this.all_states_map[this.current_item.tag] = id;
                                	//this.current_item = null;
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, association_path}));
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, association_path,"./children"}));
                this.enter_Root_connecting();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_connecting(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "instance_associated" && e.getPort() == ""){
            object[] parameters = e.getParameters();
            int id = (int)parameters[0];
            String association_name = (String)parameters[1];
            if (association_name.EndsWith("/children")){
                enableds.Add(0);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_connecting. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                String association_name = (String)parameters[1];
                this.exit_Root_connecting();
                String parent_path = association_name.Substring(0, association_name.Length-9);
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, parent_path, new Event("new_child", "", new object[] {id,this.current_item})}));
                this.current_item = null;
                this.enter_Root_waiting();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private void transition (Event e = null)
    {
        if (e == null) {
            e = new Event();
        }
        this.state_changed = this.transition_Root(e);
    }
    
    public bool inState(List<Node> nodes)
    {
        foreach(List<Node> actives in current_state.Values){
            foreach(Node node in actives)
                nodes.Remove (node);
            if (nodes.Count == 0){
                return true;
            }
        }
        return false;
    }
    
    
    public void stop()
    {
        this.active = false;
    }
    
    private void microstep ()
    {
        List<Event> due = this.events.popDueEvents();
        if (due.Count == 0) {
            this.transition (null);
        } else {
            foreach (Event e in due)
            {
                this.transition(e);
            }
        }
    }
    
    public void step(double delta)
    {
        if (!this.active) return;
        
        this.events.decreaseTime(delta);
        
        if (this.timers != null && this.timers.Count > 0)
        {
            var next_timers = new Dictionary<int,double>();
            foreach(KeyValuePair<int,double> pair in this.timers)
            {
                double new_time = pair.Value - delta;
                if (new_time <= 0.0)
                    this.addEvent (new Event("_" + pair.Key + "after"), new_time);
                else
                    next_timers[pair.Key] = new_time;
            }
            this.timers = next_timers;
        }
        this.microstep();
        while (this.state_changed)
            this.microstep();
    }
    
    public void addEvent (Event input_event, double time_offset)
    {
        this.events.Add (input_event, time_offset);
    }
    
    public void addEvent (Event input_event)
    {
        this.addEvent(input_event, 0.0);
    }
    
    public double getEarliestEventTime ()
    {
        if (this.timers != null)
        {
            double smallest_timer_value = double.PositiveInfinity;
            foreach (double timer_value in this.timers.Values)
            {
                if (timer_value < smallest_timer_value)
                    smallest_timer_value = timer_value;
            }
            return Math.Min(this.events.getEarliestTime(), smallest_timer_value); 
        }
        return this.events.getEarliestTime();   
    }
}

public class State
{
    //User defined attributes
    protected CanvasItem canvas_item;
    
    public State(CanvasItem canvas_item)
    {
        //constructor body (user-defined)
        this.canvas_item = canvas_item;
    }
}

public class BasicState : State, IRuntimeClass
{
    private ControllerBase controller;
    private ObjectManagerBase object_manager;
    private bool active = false;
    private bool state_changed = false;
    private EventQueue events = new EventQueue();
    private Dictionary<int,double> timers = null;
    
    /// <summary>
    /// Enum uniquely representing all statechart nodes.
    /// </summary>
    public enum Node {
        Root,
        Root_active,
        Root_active_selected,
        Root_active_selected_drop,
        Root_active_not_selected,
        Root_active_selected_not_dragging,
        Root_active_selected_dragging,
        Root_active_selected_drop_drop_window_activation,
        Root_active_selected_drop_wait_for_drop_window,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    Dictionary<int, int> children_map;
    
    /// <summary>
    /// Constructor part that is common for all constructors.
    /// </summary>
    private void commonConstructor(ControllerBase controller)
    {
        this.controller = controller;
        this.object_manager = controller.getObjectManager();
        
        //Initialize statechart :
        this.current_state[Node.Root] = new List<Node>();
        this.current_state[Node.Root_active] = new List<Node>();
        this.current_state[Node.Root_active_selected] = new List<Node>();
        this.current_state[Node.Root_active_selected_drop] = new List<Node>();
    }
    
    public void start()
    {
        if (!this.active) {
            this.active = true;
            this.enterDefault_Root_active();
        }
    }
    
    public BasicState(ControllerBase controller, CanvasItem canvas_item) : base(canvas_item)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.children_map = new Dictionary<int,int>();
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_active()
    {
        this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, "parent","canvas"}));
        this.current_state[Node.Root].Add(Node.Root_active);
    }
    
    private void exit_Root_active()
    {
        if (this.current_state[Node.Root_active].Contains(Node.Root_active_not_selected)){
            this.exit_Root_active_not_selected();
        }
        if (this.current_state[Node.Root_active].Contains(Node.Root_active_selected)){
            this.exit_Root_active_selected();
        }
        this.current_state[Node.Root].Remove(Node.Root_active);
    }
    
    private void enter_Root_active_selected()
    {
        this.canvas_item.setColor(Color.Lerp(GUI.backgroundColor, Color.green, 0.5f));
        this.current_state[Node.Root_active].Add(Node.Root_active_selected);
    }
    
    private void exit_Root_active_selected()
    {
        if (this.current_state[Node.Root_active_selected].Contains(Node.Root_active_selected_not_dragging)){
            this.exit_Root_active_selected_not_dragging();
        }
        if (this.current_state[Node.Root_active_selected].Contains(Node.Root_active_selected_dragging)){
            this.exit_Root_active_selected_dragging();
        }
        if (this.current_state[Node.Root_active_selected].Contains(Node.Root_active_selected_drop)){
            this.exit_Root_active_selected_drop();
        }
        this.current_state[Node.Root_active].Remove(Node.Root_active_selected);
    }
    
    private void enter_Root_active_selected_drop()
    {
        this.object_manager.addEvent(new Event("create_instance", "", new object[] { this, "state_drop","StateDropWindow",this.canvas_item}));
        this.current_state[Node.Root_active_selected].Add(Node.Root_active_selected_drop);
    }
    
    private void exit_Root_active_selected_drop()
    {
        if (this.current_state[Node.Root_active_selected_drop].Contains(Node.Root_active_selected_drop_drop_window_activation)){
            this.exit_Root_active_selected_drop_drop_window_activation();
        }
        if (this.current_state[Node.Root_active_selected_drop].Contains(Node.Root_active_selected_drop_wait_for_drop_window)){
            this.exit_Root_active_selected_drop_wait_for_drop_window();
        }
        this.current_state[Node.Root_active_selected].Remove(Node.Root_active_selected_drop);
    }
    
    private void enter_Root_active_not_selected()
    {
        this.current_state[Node.Root_active].Add(Node.Root_active_not_selected);
    }
    
    private void exit_Root_active_not_selected()
    {
        this.current_state[Node.Root_active].Remove(Node.Root_active_not_selected);
    }
    
    private void enter_Root_active_selected_not_dragging()
    {
        this.current_state[Node.Root_active_selected].Add(Node.Root_active_selected_not_dragging);
    }
    
    private void exit_Root_active_selected_not_dragging()
    {
        this.current_state[Node.Root_active_selected].Remove(Node.Root_active_selected_not_dragging);
    }
    
    private void enter_Root_active_selected_dragging()
    {
        this.current_state[Node.Root_active_selected].Add(Node.Root_active_selected_dragging);
    }
    
    private void exit_Root_active_selected_dragging()
    {
        this.current_state[Node.Root_active_selected].Remove(Node.Root_active_selected_dragging);
    }
    
    private void enter_Root_active_selected_drop_drop_window_activation()
    {
        this.current_state[Node.Root_active_selected_drop].Add(Node.Root_active_selected_drop_drop_window_activation);
    }
    
    private void exit_Root_active_selected_drop_drop_window_activation()
    {
        this.current_state[Node.Root_active_selected_drop].Remove(Node.Root_active_selected_drop_drop_window_activation);
    }
    
    private void enter_Root_active_selected_drop_wait_for_drop_window()
    {
        this.current_state[Node.Root_active_selected_drop].Add(Node.Root_active_selected_drop_wait_for_drop_window);
    }
    
    private void exit_Root_active_selected_drop_wait_for_drop_window()
    {
        this.current_state[Node.Root_active_selected_drop].Remove(Node.Root_active_selected_drop_wait_for_drop_window);
    }
    
    //Statechart enter/exit default method(s) :
    
    private void enterDefault_Root_active()
    {
        this.enter_Root_active();
        this.enterDefault_Root_active_selected();
    }
    
    private void enterDefault_Root_active_selected()
    {
        this.enter_Root_active_selected();
        this.enter_Root_active_selected_not_dragging();
    }
    
    private void enterDefault_Root_active_selected_drop()
    {
        this.enter_Root_active_selected_drop();
        this.enter_Root_active_selected_drop_drop_window_activation();
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_active){
                catched = this.transition_Root_active(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_active(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root_active][0] == Node.Root_active_not_selected){
                catched = this.transition_Root_active_not_selected(e);
            } else if (this.current_state[Node.Root_active][0] == Node.Root_active_selected){
                catched = this.transition_Root_active_selected(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_active_not_selected(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "select" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (this.canvas_item.tag == tag){
                enableds.Add(0);
            }
        }
        
        if (e.getName() == "new_child" && e.getPort() == ""){
            enableds.Add(1);
        }
        
        if (e.getName() == "adjust_size" && e.getPort() == ""){
            enableds.Add(2);
        }
        
        if (e.getName() == "disconnect_child" && e.getPort() == ""){
            enableds.Add(3);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_active_not_selected. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_active_not_selected();
                this.canvas_item.pushToFront();
                this.enterDefault_Root_active_selected();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                CanvasItem child = (CanvasItem)parameters[1];
                this.exit_Root_active_not_selected();
                this.canvas_item.addChild(child);
                                		this.children_map[child.tag] = id;
                                        this.canvas_item.adjustSize();
                this.enter_Root_active_not_selected();
            } else if (enabled == 2){
                this.exit_Root_active_not_selected();
                this.canvas_item.adjustSize();
                this.enter_Root_active_not_selected();
            } else if (enabled == 3){
                object[] parameters = e.getParameters();
                CanvasItem child = (CanvasItem)parameters[0];
                this.exit_Root_active_not_selected();
                this.object_manager.addEvent(new Event("unassociate_instance", "", new object[] { this, String.Format("children[{0}]", this.children_map[child.tag])}));
                this.children_map.Remove(child.tag);
                                        this.canvas_item.removeChild(child);
                this.enter_Root_active_not_selected();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_active_selected(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "select" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (this.canvas_item.tag != tag){
                enableds.Add(0);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_active_selected. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_active_selected();
                this.canvas_item.resetColor();
                this.enter_Root_active_not_selected();
            }
            catched = true;
        }
        
        if (!catched){
            if (this.current_state[Node.Root_active_selected][0] == Node.Root_active_selected_not_dragging){
                catched = this.transition_Root_active_selected_not_dragging(e);
            } else if (this.current_state[Node.Root_active_selected][0] == Node.Root_active_selected_dragging){
                catched = this.transition_Root_active_selected_dragging(e);
            } else if (this.current_state[Node.Root_active_selected][0] == Node.Root_active_selected_drop){
                catched = this.transition_Root_active_selected_drop(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_active_selected_not_dragging(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "drag" && e.getPort() == "input"){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_active_selected_not_dragging. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                Vector2 delta = (Vector2)parameters[1];
                this.exit_Root_active_selected_not_dragging();
                this.canvas_item.move(delta);
                this.enter_Root_active_selected_dragging();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_active_selected_dragging(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "drag" && e.getPort() == "input"){
            enableds.Add(0);
        }
        
        if (e.getName() == "mouse_up" && e.getPort() == "input"){
            enableds.Add(1);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_active_selected_dragging. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                Vector2 delta = (Vector2)parameters[1];
                this.exit_Root_active_selected_dragging();
                this.canvas_item.move(delta);
                this.enter_Root_active_selected_dragging();
            }
            if (enabled == 1){
                this.exit_Root_active_selected_dragging();
                this.enterDefault_Root_active_selected_drop();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_active_selected_drop(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root_active_selected_drop][0] == Node.Root_active_selected_drop_drop_window_activation){
                catched = this.transition_Root_active_selected_drop_drop_window_activation(e);
            } else if (this.current_state[Node.Root_active_selected_drop][0] == Node.Root_active_selected_drop_wait_for_drop_window){
                catched = this.transition_Root_active_selected_drop_wait_for_drop_window(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_active_selected_drop_drop_window_activation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "instance_created" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_active_selected_drop_drop_window_activation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                String association_name = (String)parameters[1];
                this.exit_Root_active_selected_drop_drop_window_activation();
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, association_name}));
                this.enter_Root_active_selected_drop_wait_for_drop_window();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_active_selected_drop_wait_for_drop_window(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "state_drop_response" && e.getPort() == ""){
            object[] parameters = e.getParameters();
            bool do_reconnect = (bool)parameters[0];
            CanvasItem connection = (CanvasItem)parameters[1];
            if (!do_reconnect){
                enableds.Add(0);
            }
        }
        
        if (e.getName() == "state_drop_response" && e.getPort() == ""){
            object[] parameters = e.getParameters();
            bool do_reconnect = (bool)parameters[0];
            CanvasItem connection = (CanvasItem)parameters[1];
            if (do_reconnect){
                enableds.Add(1);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_active_selected_drop_wait_for_drop_window. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                bool do_reconnect = (bool)parameters[0];
                CanvasItem connection = (CanvasItem)parameters[1];
                this.exit_Root_active_selected_drop();
                this.object_manager.addEvent(new Event("delete_instance", "", new object[] { this, "state_drop"}));
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "parent", new Event("adjust_size", "", new object[] {})}));
                this.enter_Root_active_selected_not_dragging();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                bool do_reconnect = (bool)parameters[0];
                CanvasItem connection = (CanvasItem)parameters[1];
                this.exit_Root_active_selected_drop();
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "parent", new Event("disconnect_child", "", new object[] {this.canvas_item})}));
                this.object_manager.addEvent(new Event("unassociate_instance", "", new object[] { this, "parent"}));
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "canvas", new Event("connect_child_to_parent", "", new object[] {this.canvas_item,connection})}));
                this.object_manager.addEvent(new Event("delete_instance", "", new object[] { this, "state_drop"}));
                this.enter_Root_active_selected_not_dragging();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private void transition (Event e = null)
    {
        if (e == null) {
            e = new Event();
        }
        this.state_changed = this.transition_Root(e);
    }
    
    public bool inState(List<Node> nodes)
    {
        foreach(List<Node> actives in current_state.Values){
            foreach(Node node in actives)
                nodes.Remove (node);
            if (nodes.Count == 0){
                return true;
            }
        }
        return false;
    }
    
    
    public void stop()
    {
        this.active = false;
    }
    
    private void microstep ()
    {
        List<Event> due = this.events.popDueEvents();
        if (due.Count == 0) {
            this.transition (null);
        } else {
            foreach (Event e in due)
            {
                this.transition(e);
            }
        }
    }
    
    public void step(double delta)
    {
        if (!this.active) return;
        
        this.events.decreaseTime(delta);
        
        if (this.timers != null && this.timers.Count > 0)
        {
            var next_timers = new Dictionary<int,double>();
            foreach(KeyValuePair<int,double> pair in this.timers)
            {
                double new_time = pair.Value - delta;
                if (new_time <= 0.0)
                    this.addEvent (new Event("_" + pair.Key + "after"), new_time);
                else
                    next_timers[pair.Key] = new_time;
            }
            this.timers = next_timers;
        }
        this.microstep();
        while (this.state_changed)
            this.microstep();
    }
    
    public void addEvent (Event input_event, double time_offset)
    {
        this.events.Add (input_event, time_offset);
    }
    
    public void addEvent (Event input_event)
    {
        this.addEvent(input_event, 0.0);
    }
    
    public double getEarliestEventTime ()
    {
        if (this.timers != null)
        {
            double smallest_timer_value = double.PositiveInfinity;
            foreach (double timer_value in this.timers.Values)
            {
                if (timer_value < smallest_timer_value)
                    smallest_timer_value = timer_value;
            }
            return Math.Min(this.events.getEarliestTime(), smallest_timer_value); 
        }
        return this.events.getEarliestTime();   
    }
}

public class StateDropWindow : IRuntimeClass
{
    private ControllerBase controller;
    private ObjectManagerBase object_manager;
    private bool active = false;
    private bool state_changed = false;
    private EventQueue events = new EventQueue();
    private Dictionary<int,double> timers = null;
    
    /// <summary>
    /// Enum uniquely representing all statechart nodes.
    /// </summary>
    public enum Node {
        Root,
        Root_root,
        Root_final,
        Root_popup,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    List<CanvasItem> connection_options;
    string[] connection_options_strings;
    int selected_option = 0;
    CanvasItem canvas_item;
    
    /// <summary>
    /// Constructor part that is common for all constructors.
    /// </summary>
    private void commonConstructor(ControllerBase controller)
    {
        this.controller = controller;
        this.object_manager = controller.getObjectManager();
        
        //Initialize statechart :
        this.current_state[Node.Root] = new List<Node>();
    }
    
    public void start()
    {
        if (!this.active) {
            this.active = true;
            this.enter_Root_root();
        }
    }
    
    public StateDropWindow(ControllerBase controller, CanvasItem canvas_item)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.canvas_item = canvas_item;
        this.connection_options = new List<CanvasItem>();
        CanvasItem container = canvas_item.getImmediateContainer();
        if (container == null)
        {
            this.connection_options.AddRange(canvas_item.getOverlappings());
            if (canvas_item.parent != null)
            {
                //disconnect is an option
                Debug.Log("disconnect is an option");
                this.connection_options.Add(null);
            }          
        }
        else if (container != canvas_item.parent)
        {
            this.connection_options.Add(container);
        }
    }
    
    public void drawModalWindow(SCCDModalWindow modal_window)
    {
        GUILayout.Label("Connect to:");
        this.selected_option = GUILayout.SelectionGrid(this.selected_option, this.connection_options_strings, 1, "MenuItem");
        EditorGUILayout.BeginHorizontal();          
        if (GUILayout.Button("Yes"))
        {
            this.addEvent(new sccdlib.Event("state_drop_popup_response", "", new object[] {true, this.connection_options[this.selected_option]}));
            modal_window.close();
        }
        if (GUILayout.Button("No"))
        {
            this.addEvent(new sccdlib.Event("state_drop_popup_response", "", new object[] {false, null}));
            modal_window.close();
        }
        EditorGUILayout.EndHorizontal();
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_root()
    {
        this.current_state[Node.Root].Add(Node.Root_root);
    }
    
    private void exit_Root_root()
    {
        this.current_state[Node.Root].Remove(Node.Root_root);
    }
    
    private void enter_Root_final()
    {
        this.current_state[Node.Root].Add(Node.Root_final);
    }
    
    private void exit_Root_final()
    {
        this.current_state[Node.Root].Remove(Node.Root_final);
    }
    
    private void enter_Root_popup()
    {
        this.connection_options_strings = new string[this.connection_options.Count];
                            for (int i = 0; i < this.connection_options.Count; i++)
                            {
                                if (this.connection_options[i] != null)
                                    this.connection_options_strings[i] = this.connection_options[i].label;
                                else
                                    this.connection_options_strings[i] = "Canvas(Disconnect)";
                            }
                            this.canvas_item.canvas.createModalWindow("State Drop", this.drawModalWindow);
        this.current_state[Node.Root].Add(Node.Root_popup);
    }
    
    private void exit_Root_popup()
    {
        this.current_state[Node.Root].Remove(Node.Root_popup);
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_root){
                catched = this.transition_Root_root(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_final){
                catched = this.transition_Root_final(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_popup){
                catched = this.transition_Root_popup(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_root(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (this.connection_options.Count == 0 || (this.connection_options.Count == 1 && this.connection_options[0] != null)){
            enableds.Add(0);
        }
        
        if (this.connection_options.Count > 1 || (this.connection_options.Count == 1 && this.connection_options[0] == null)){
            enableds.Add(1);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_root. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_root();
                CanvasItem connection = null;
                                    bool do_reconnect = false;
                                    if (this.connection_options.Count == 1)
                                    {
                                        do_reconnect = true;
                                        connection = this.connection_options[0];
                                    }
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "parent", new Event("state_drop_response", "", new object[] {do_reconnect,connection})}));
                this.enter_Root_final();
            }
            if (enabled == 1){
                this.exit_Root_root();
                this.enter_Root_popup();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_final(Event e)
    {
        bool catched = false;
        return catched;
    }
    
    private bool transition_Root_popup(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "state_drop_popup_response" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_popup. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                bool do_reconnect = (bool)parameters[0];
                CanvasItem connection = (CanvasItem)parameters[1];
                this.exit_Root_popup();
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "parent", new Event("state_drop_response", "", new object[] {do_reconnect,connection})}));
                this.enter_Root_final();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private void transition (Event e = null)
    {
        if (e == null) {
            e = new Event();
        }
        this.state_changed = this.transition_Root(e);
    }
    
    public bool inState(List<Node> nodes)
    {
        foreach(List<Node> actives in current_state.Values){
            foreach(Node node in actives)
                nodes.Remove (node);
            if (nodes.Count == 0){
                return true;
            }
        }
        return false;
    }
    
    
    public void stop()
    {
        this.active = false;
    }
    
    private void microstep ()
    {
        List<Event> due = this.events.popDueEvents();
        if (due.Count == 0) {
            this.transition (null);
        } else {
            foreach (Event e in due)
            {
                this.transition(e);
            }
        }
    }
    
    public void step(double delta)
    {
        if (!this.active) return;
        
        this.events.decreaseTime(delta);
        
        if (this.timers != null && this.timers.Count > 0)
        {
            var next_timers = new Dictionary<int,double>();
            foreach(KeyValuePair<int,double> pair in this.timers)
            {
                double new_time = pair.Value - delta;
                if (new_time <= 0.0)
                    this.addEvent (new Event("_" + pair.Key + "after"), new_time);
                else
                    next_timers[pair.Key] = new_time;
            }
            this.timers = next_timers;
        }
        this.microstep();
        while (this.state_changed)
            this.microstep();
    }
    
    public void addEvent (Event input_event, double time_offset)
    {
        this.events.Add (input_event, time_offset);
    }
    
    public void addEvent (Event input_event)
    {
        this.addEvent(input_event, 0.0);
    }
    
    public double getEarliestEventTime ()
    {
        if (this.timers != null)
        {
            double smallest_timer_value = double.PositiveInfinity;
            foreach (double timer_value in this.timers.Values)
            {
                if (timer_value < smallest_timer_value)
                    smallest_timer_value = timer_value;
            }
            return Math.Min(this.events.getEarliestTime(), smallest_timer_value); 
        }
        return this.events.getEarliestTime();   
    }
}

public class ObjectManager : ObjectManagerBase
{
    public ObjectManager(ControllerBase controller): base(controller)
    {
    }
    
    protected override InstanceWrapper instantiate(string class_name, object[] construct_params)
    {
        IRuntimeClass instance = null;
        List<Association> associations = new List<Association>();
        if (class_name == "Canvas" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(Canvas), new_parameters);
            associations.Add(new Association("children", "State", 0, -1));
            associations.Add(new Association("all_states", "State", 0, -1));
        }else if (class_name == "State" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(State), new_parameters);
        }else if (class_name == "BasicState" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(BasicState), new_parameters);
            associations.Add(new Association("children", "State", 0, -1));
            associations.Add(new Association("parent", "IRuntimeClass", 0, 1));
            associations.Add(new Association("canvas", "Canvas", 0, 1));
            associations.Add(new Association("state_drop", "StateDropWindow", 0, 1));
        }else if (class_name == "StateDropWindow" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(StateDropWindow), new_parameters);
            associations.Add(new Association("parent", "State", 0, 1));
        }
        if (instance != null) {
            return new InstanceWrapper(instance, associations);
        }
        throw new RunTimeException(string.Format("Tried to instantiate class '{0}', which is not part of the class diagram.", class_name));
    }
}

public class Controller : GameLoopControllerBase
{
    public Controller(EditorCanvas canvas) : base()
    {
        this.addInputPort("input");
        this.addInputPort("engine");
        this.addInputPort("windows");
        this.object_manager = new ObjectManager(this);
        this.object_manager.createInstance("Canvas", new object[]{canvas});
    }
}
