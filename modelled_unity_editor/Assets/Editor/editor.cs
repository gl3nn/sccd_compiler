/*
    Statecharts + Class Diagram compiler by Glenn De Jonghe
    
    Generated on 2014-11-07 17:03:54.
    
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


public class StateChartEditor : RuntimeClassBase
{
    
    /// <summary>
    /// Enum uniquely representing all statechart nodes.
    /// </summary>
    public enum Node {
        Root,
        Root_waiting,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    CanvasItem canvas;
    
    /// <summary>
    /// Constructor part that is common for all constructors.
    /// </summary>
    private void commonConstructor(ControllerBase controller = null)
    {
        this.controller = controller;
        this.object_manager = controller.getObjectManager();
        
        //Initialize statechart :
        
        this.current_state[Node.Root] = new List<Node>();
    }
    
    public override void start()
    {
        if (!this.active) {
            base.start();
            this.enter_Root_waiting();
        }
    }
    
    public StateChartEditor(ControllerBase controller, CanvasItem canvas)
    {
        this.commonConstructor(controller);
        
        //constructor body (user-defined)
        this.canvas = canvas;
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
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_waiting){
                catched = this.transition_Root_waiting(e);
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
        
        if (e.getName() == "instance_created" && e.getPort() == ""){
            enableds.Add(1);
        }
        
        if (e.getName() == "draw" && e.getPort() == "engine"){
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
                CanvasItem new_canvas_item = new CanvasItem(position, this.canvas);
                                        this.canvas.addChild(new_canvas_item);
                this.object_manager.addEvent(new Event("create_instance", "", new object[] { this, "states","State","name",new_canvas_item}));
                this.enter_Root_waiting();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                String association_name = (String)parameters[0];
                this.exit_Root_waiting();
                //this.nr_of_windows += 1
                                        //print "in create_window %s" % self.nr_of_windows
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, association_name}));
                this.enter_Root_waiting();
            } else if (enabled == 2){
                this.exit_Root_waiting();
                this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, "states", new Event("draw", "", new object[] {})}));
                this.enter_Root_waiting();
            }
            catched = true;
        }
        
        return catched;
    }
    
    protected override void transition (Event e = null)
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
    
}


public class State : RuntimeClassBase
{
    
    /// <summary>
    /// Enum uniquely representing all statechart nodes.
    /// </summary>
    public enum Node {
        Root,
        Root_selected,
        Root_not_selected,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    String name;
    CanvasItem canvas_item;
    
    /// <summary>
    /// Constructor part that is common for all constructors.
    /// </summary>
    private void commonConstructor(ControllerBase controller = null)
    {
        this.controller = controller;
        this.object_manager = controller.getObjectManager();
        
        //Initialize statechart :
        
        this.current_state[Node.Root] = new List<Node>();
    }
    
    public override void start()
    {
        if (!this.active) {
            base.start();
            this.enter_Root_not_selected();
        }
    }
    
    public State(ControllerBase controller, String name, CanvasItem canvas_item)
    {
        this.commonConstructor(controller);
        
        //constructor body (user-defined)
        this.name = name;
        this.canvas_item = canvas_item;
    }
    
    public void draw(bool is_selected)
    {
            if (is_selected)
            {
                var old_color = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp(GUI.backgroundColor,Color.green,0.5f);
                GUI.Box(this.canvas_item.rect, name);    
                GUI.backgroundColor = old_color;
            }
            else
            {
                GUI.Box(this.canvas_item.rect, name);
            }
    }
    
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_selected()
    {
        this.current_state[Node.Root].Add(Node.Root_selected);
    }
    
    private void exit_Root_selected()
    {
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
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_selected){
                catched = this.transition_Root_selected(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_not_selected){
                catched = this.transition_Root_not_selected(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_selected(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "draw" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (e.getName() == "drag" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            Vector2 delta = (Vector2)parameters[1];
            if (true/*this.canvas_item.tag == tag*/){
                enableds.Add(1);
            }
        }
        
        if (e.getName() == "select" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (this.canvas_item.tag != tag){
                enableds.Add(2);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_selected. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_selected();
                this.draw(true);
                this.enter_Root_selected();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                Vector2 delta = (Vector2)parameters[1];
                this.exit_Root_selected();
                this.canvas_item.move(delta);
                this.enter_Root_selected();
            } else if (enabled == 2){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_selected();
                this.enter_Root_not_selected();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_not_selected(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "draw" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (e.getName() == "select" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (this.canvas_item.tag == tag){
                enableds.Add(1);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_not_selected. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_not_selected();
                this.draw(false);
                this.enter_Root_not_selected();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_not_selected();
                this.enter_Root_selected();
            }
            catched = true;
        }
        
        return catched;
    }
    
    protected override void transition (Event e = null)
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
    
}

public class ObjectManager : ObjectManagerBase
{
    public ObjectManager(ControllerBase controller): base(controller)
    {
    }
    
    protected override InstanceWrapper instantiate(string class_name, object[] construct_params)
    {
        RuntimeClassBase instance = null;
        List<Association> associations = new List<Association>();
        if (class_name == "StateChartEditor" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (RuntimeClassBase) Activator.CreateInstance(typeof(StateChartEditor), new_parameters);
            associations.Add(new Association("states", "State", 0, -1));
        }else if (class_name == "State" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (RuntimeClassBase) Activator.CreateInstance(typeof(State), new_parameters);
        }
        if (instance != null) {
            return new InstanceWrapper(instance, associations);
        }
        return null;
    }
}

public class Controller : GameLoopControllerBase
{
    public Controller(CanvasItem canvas) : base()
    {
        this.addInputPort("input");
        this.addInputPort("engine");
        this.object_manager = new ObjectManager(this);
        this.object_manager.createInstance("StateChartEditor", new object[]{canvas});
    }
}