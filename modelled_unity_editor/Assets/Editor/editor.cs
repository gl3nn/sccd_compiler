/*
    Statecharts + Class Diagram compiler by Glenn De Jonghe
    
    Generated on 2014-11-22 18:31:29.
    
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
                                	String parent_path = String.Format("all_states[{0}]", this.all_states_map[parent.tag]);
                                	String child_path = String.Format("all_states[{0}]", this.all_states_map[child.tag]);
                                    Debug.Log("connect_child_to_parent");
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, child_path,parent_path + "/children"}));
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, parent_path,child_path + "/parent"}));
                this.enter_Root_connecting();
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
                                	this.children_map[this.current_item.tag] = id;
                                	this.all_states_map[this.current_item.tag] = id;
                                	this.current_item = null;
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, association_path}));
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, association_path,"children"}));
                this.enter_Root_waiting();
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
        Root_selected,
        Root_not_selected,
        Root_selected_not_dragging,
        Root_selected_dragging,
        Root_selected_layout,
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
        this.current_state[Node.Root_selected] = new List<Node>();
    }
    
    public void start()
    {
        if (!this.active) {
            this.active = true;
            this.enterDefault_Root_selected();
        }
    }
    
    public BasicState(ControllerBase controller, CanvasItem canvas_item) : base(canvas_item)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.children_map = new Dictionary<int,int>();
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_selected()
    {
        this.canvas_item.setColor(Color.Lerp(GUI.backgroundColor, Color.green, 0.5f));
        this.current_state[Node.Root].Add(Node.Root_selected);
    }
    
    private void exit_Root_selected()
    {
        if (this.current_state[Node.Root_selected].Contains(Node.Root_selected_not_dragging)){
            this.exit_Root_selected_not_dragging();
        }
        if (this.current_state[Node.Root_selected].Contains(Node.Root_selected_dragging)){
            this.exit_Root_selected_dragging();
        }
        if (this.current_state[Node.Root_selected].Contains(Node.Root_selected_layout)){
            this.exit_Root_selected_layout();
        }
        this.current_state[Node.Root].Remove(Node.Root_selected);
    }
    
    private void enter_Root_not_selected()
    {
        this.current_state[Node.Root].Add(Node.Root_not_selected);
    }
    
    private void exit_Root_not_selected()
    {
        this.current_state[Node.Root].Remove(Node.Root_not_selected);
    }
    
    private void enter_Root_selected_not_dragging()
    {
        this.current_state[Node.Root_selected].Add(Node.Root_selected_not_dragging);
    }
    
    private void exit_Root_selected_not_dragging()
    {
        this.current_state[Node.Root_selected].Remove(Node.Root_selected_not_dragging);
    }
    
    private void enter_Root_selected_dragging()
    {
        this.current_state[Node.Root_selected].Add(Node.Root_selected_dragging);
    }
    
    private void exit_Root_selected_dragging()
    {
        this.current_state[Node.Root_selected].Remove(Node.Root_selected_dragging);
    }
    
    private void enter_Root_selected_layout()
    {
        this.current_state[Node.Root_selected].Add(Node.Root_selected_layout);
    }
    
    private void exit_Root_selected_layout()
    {
        this.current_state[Node.Root_selected].Remove(Node.Root_selected_layout);
    }
    
    //Statechart enter/exit default method(s) :
    
    private void enterDefault_Root_selected()
    {
        this.enter_Root_selected();
        this.enter_Root_selected_not_dragging();
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_not_selected){
                catched = this.transition_Root_not_selected(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_selected){
                catched = this.transition_Root_selected(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_not_selected(Event e)
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
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_not_selected. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_not_selected();
                this.canvas_item.pushToFront();
                this.enterDefault_Root_selected();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                CanvasItem child = (CanvasItem)parameters[1];
                this.exit_Root_not_selected();
                this.canvas_item.addChild(child);
                            		this.children_map[child.tag] = id;
                                    this.canvas_item.adjustSize();
                this.enter_Root_not_selected();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_selected(Event e)
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
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_selected. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_selected();
                this.canvas_item.resetColor();
                this.enter_Root_not_selected();
            }
            catched = true;
        }
        
        if (!catched){
            if (this.current_state[Node.Root_selected][0] == Node.Root_selected_not_dragging){
                catched = this.transition_Root_selected_not_dragging(e);
            } else if (this.current_state[Node.Root_selected][0] == Node.Root_selected_dragging){
                catched = this.transition_Root_selected_dragging(e);
            } else if (this.current_state[Node.Root_selected][0] == Node.Root_selected_layout){
                catched = this.transition_Root_selected_layout(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_selected_not_dragging(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "drag" && e.getPort() == "input"){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_selected_not_dragging. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                Vector2 delta = (Vector2)parameters[1];
                this.exit_Root_selected_not_dragging();
                this.canvas_item.move(delta);
                this.enter_Root_selected_dragging();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_selected_dragging(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "drag" && e.getPort() == "input"){
            enableds.Add(0);
        }
        
        if (e.getName() == "mouse_up" && e.getPort() == "input"){
            if (this.canvas_item.getOverlappings().Count == 0){
                enableds.Add(1);
            }
        }
        
        if (e.getName() == "mouse_up" && e.getPort() == "input"){
            if (this.canvas_item.getOverlappings().Count > 0){
                enableds.Add(2);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_selected_dragging. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                Vector2 delta = (Vector2)parameters[1];
                this.exit_Root_selected_dragging();
                this.canvas_item.move(delta);
                this.enter_Root_selected_dragging();
            }
            if (enabled == 1){
                this.exit_Root_selected_dragging();
                this.enter_Root_selected_layout();
            } else if (enabled == 2){
                this.exit_Root_selected_dragging();
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "parent", new Event("disconnect_child", "", new object[] {this.canvas_item})}));
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "parent", new Event("connect_child_to_parent", "", new object[] {this.canvas_item,this.canvas_item.getOverlappings()[0]})}));
                this.enter_Root_selected_not_dragging();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_selected_layout(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        enableds.Add(0);
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_selected_layout. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_selected_layout();
                //grootte veranderen
                this.enter_Root_selected_not_dragging();
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
            associations.Add(new Association("parent", "IRuntimeClass", 0, -1));
            associations.Add(new Association("canvas", "Canvas", 0, -1));
        }
        if (instance != null) {
            return new InstanceWrapper(instance, associations);
        }
        return null;
    }
}

public class Controller : GameLoopControllerBase
{
    public Controller(EditorCanvas canvas) : base()
    {
        this.addInputPort("input");
        this.addInputPort("engine");
        this.object_manager = new ObjectManager(this);
        this.object_manager.createInstance("Canvas", new object[]{canvas});
    }
}
