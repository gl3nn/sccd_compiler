/*
    Statecharts + Class Diagram compiler by Glenn De Jonghe
    
    Generated on 2015-01-17 00:59:20.
    
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


public class StateChartWindow : IRuntimeClass
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
        Root_toolbar_creation,
        Root_toolbar_activation,
        Root_canvas_creation,
        Root_canvas_activation,
        Root_main,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    GUITopLevel window_widget;
    
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
            this.enter_Root_toolbar_creation();
        }
    }
    private void narrowCast(string parent_path, Event send_event){
        this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, parent_path, send_event}));
    }
    
    private void broadCast(Event send_event){
        this.object_manager.addEvent(new Event("broad_cast", "", new object[] {send_event}));
    }
    
    
    public StateChartWindow(ControllerBase controller, GUITopLevel window_widget)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.window_widget = window_widget;
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_toolbar_creation()
    {
        this.current_state[Node.Root].Add(Node.Root_toolbar_creation);
    }
    
    private void exit_Root_toolbar_creation()
    {
        this.current_state[Node.Root].Remove(Node.Root_toolbar_creation);
    }
    
    private void enter_Root_toolbar_activation()
    {
        this.current_state[Node.Root].Add(Node.Root_toolbar_activation);
    }
    
    private void exit_Root_toolbar_activation()
    {
        this.current_state[Node.Root].Remove(Node.Root_toolbar_activation);
    }
    
    private void enter_Root_canvas_creation()
    {
        this.current_state[Node.Root].Add(Node.Root_canvas_creation);
    }
    
    private void exit_Root_canvas_creation()
    {
        this.current_state[Node.Root].Remove(Node.Root_canvas_creation);
    }
    
    private void enter_Root_canvas_activation()
    {
        this.current_state[Node.Root].Add(Node.Root_canvas_activation);
    }
    
    private void exit_Root_canvas_activation()
    {
        this.current_state[Node.Root].Remove(Node.Root_canvas_activation);
    }
    
    private void enter_Root_main()
    {
        this.current_state[Node.Root].Add(Node.Root_main);
    }
    
    private void exit_Root_main()
    {
        this.current_state[Node.Root].Remove(Node.Root_main);
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_toolbar_creation){
                catched = this.transition_Root_toolbar_creation(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_toolbar_activation){
                catched = this.transition_Root_toolbar_activation(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_canvas_creation){
                catched = this.transition_Root_canvas_creation(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_canvas_activation){
                catched = this.transition_Root_canvas_activation(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_main){
                catched = this.transition_Root_main(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_toolbar_creation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        enableds.Add(0);
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_toolbar_creation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_toolbar_creation();
                this.object_manager.addEvent(new Event("create_instance", "", new object[] { this, "toolbar","Toolbar",this.window_widget}));
                this.enter_Root_toolbar_activation();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_toolbar_activation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "instance_created" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_toolbar_activation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_toolbar_activation();
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, ".","toolbar/window"}));
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, "toolbar"}));
                this.enter_Root_canvas_creation();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_canvas_creation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        enableds.Add(0);
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_canvas_creation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_canvas_creation();
                this.object_manager.addEvent(new Event("create_instance", "", new object[] { this, "canvas","Canvas",this.window_widget}));
                this.enter_Root_canvas_activation();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_canvas_activation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "instance_created" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_canvas_activation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_canvas_activation();
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, ".","canvas/window"}));
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, "canvas","toolbar/canvas"}));
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, "canvas"}));
                this.enter_Root_main();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main(Event e)
    {
        bool catched = false;
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

public class Toolbar : IRuntimeClass
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
        Root_buttons_creation_loop,
        Root_button_activation,
        Root_listening,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    GUIHorizontalGroup toolbar_widget;
    List<GUIButtonInformation> buttons_information;
    int button_count = 0;
    
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
            this.enter_Root_buttons_creation_loop();
        }
    }
    private void narrowCast(string parent_path, Event send_event){
        this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, parent_path, send_event}));
    }
    
    private void broadCast(Event send_event){
        this.object_manager.addEvent(new Event("broad_cast", "", new object[] {send_event}));
    }
    
    
    public Toolbar(ControllerBase controller, GUITopLevel window_widget)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.toolbar_widget = new GUIHorizontalGroup();
        window_widget.addChild(this.toolbar_widget);
        this.buttons_information = new List<GUIButtonInformation>
        {
            new GUIButtonInformation("Basic", "set-basic"),
            new GUIButtonInformation("Parallel", "set-parallel"),
            new GUIButtonInformation("History", "set-history")
        };
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_buttons_creation_loop()
    {
        this.current_state[Node.Root].Add(Node.Root_buttons_creation_loop);
    }
    
    private void exit_Root_buttons_creation_loop()
    {
        this.current_state[Node.Root].Remove(Node.Root_buttons_creation_loop);
    }
    
    private void enter_Root_button_activation()
    {
        this.current_state[Node.Root].Add(Node.Root_button_activation);
    }
    
    private void exit_Root_button_activation()
    {
        this.current_state[Node.Root].Remove(Node.Root_button_activation);
    }
    
    private void enter_Root_listening()
    {
        this.current_state[Node.Root].Add(Node.Root_listening);
    }
    
    private void exit_Root_listening()
    {
        this.current_state[Node.Root].Remove(Node.Root_listening);
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_buttons_creation_loop){
                catched = this.transition_Root_buttons_creation_loop(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_button_activation){
                catched = this.transition_Root_button_activation(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_listening){
                catched = this.transition_Root_listening(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_buttons_creation_loop(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (this.button_count < this.buttons_information.Count){
            enableds.Add(0);
        }
        
        if (this.button_count >= this.buttons_information.Count){
            enableds.Add(1);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_buttons_creation_loop. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_buttons_creation_loop();
                this.object_manager.addEvent(new Event("create_instance", "", new object[] { this, "buttons","Button",this.toolbar_widget,this.buttons_information[this.button_count]}));
                this.enter_Root_button_activation();
            }
            if (enabled == 1){
                this.exit_Root_buttons_creation_loop();
                this.enter_Root_listening();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_button_activation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "instance_created" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_button_activation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                String association_name = (String)parameters[1];
                this.exit_Root_button_activation();
                String association_path = String.Format("{0}[{1}]", association_name, id);
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, ".",association_path + "/toolbar"}));
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, association_path}));
                this.button_count++;
                this.enter_Root_buttons_creation_loop();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_listening(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "button_pressed" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_listening. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                string button_action = (string)parameters[0];
                int button_tag = (int)parameters[1];
                this.exit_Root_listening();
                this.narrowCast("buttons", new Event("reset", "", new object[] {button_tag}));
                this.narrowCast("canvas", new Event("toolbar_action", "", new object[] {button_action}));
                this.enter_Root_listening();
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

public class Button : IRuntimeClass
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
        Root_off,
        Root_on,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    GUIButton button_widget;
    
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
            this.enter_Root_off();
        }
    }
    private void narrowCast(string parent_path, Event send_event){
        this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, parent_path, send_event}));
    }
    
    private void broadCast(Event send_event){
        this.object_manager.addEvent(new Event("broad_cast", "", new object[] {send_event}));
    }
    
    
    public Button(ControllerBase controller, GUIHorizontalGroup toolbar_widget, GUIButtonInformation button_information)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.button_widget = new GUIButton(button_information);
        toolbar_widget.addChild(this.button_widget);
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_off()
    {
        this.button_widget.is_on = false;
        this.current_state[Node.Root].Add(Node.Root_off);
    }
    
    private void exit_Root_off()
    {
        this.current_state[Node.Root].Remove(Node.Root_off);
    }
    
    private void enter_Root_on()
    {
        this.button_widget.is_on = true;
        this.current_state[Node.Root].Add(Node.Root_on);
    }
    
    private void exit_Root_on()
    {
        this.current_state[Node.Root].Remove(Node.Root_on);
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_off){
                catched = this.transition_Root_off(e);
            } else if (this.current_state[Node.Root][0] == Node.Root_on){
                catched = this.transition_Root_on(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_off(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "left-mouse-down" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (this.button_widget.tag == tag){
                enableds.Add(0);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_off. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_off();
                this.narrowCast("toolbar", new Event("button_pressed", "", new object[] {this.button_widget.properties.action,tag}));
                this.enter_Root_on();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_on(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "reset" && e.getPort() == ""){
            object[] parameters = e.getParameters();
            int except_tag = (int)parameters[0];
            if (this.button_widget.tag != except_tag){
                enableds.Add(0);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_on. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int except_tag = (int)parameters[0];
                this.exit_Root_on();
                this.enter_Root_off();
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
        Root_main,
        Root_main_event_processing,
        Root_main_execution,
        Root_main_event_processing_listening,
        Root_main_execution_idle,
        Root_main_execution_creation,
        Root_main_execution_connecting,
    };
    
    Dictionary<Node,List<Node>> current_state = new Dictionary<Node,List<Node>>();
    
    //User defined attributes
    GUICanvas canvas_widget;
    Dictionary<int, int> children_map;
    Dictionary<int, int> all_states_map;
    GUICanvasElement current_item;
    string creation_type = "bla";
    
    /// <summary>
    /// Constructor part that is common for all constructors.
    /// </summary>
    private void commonConstructor(ControllerBase controller)
    {
        this.controller = controller;
        this.object_manager = controller.getObjectManager();
        
        //Initialize statechart :
        this.current_state[Node.Root] = new List<Node>();
        this.current_state[Node.Root_main] = new List<Node>();
        this.current_state[Node.Root_main_event_processing] = new List<Node>();
        this.current_state[Node.Root_main_execution] = new List<Node>();
    }
    
    public void start()
    {
        if (!this.active) {
            this.active = true;
            this.enterDefault_Root_main();
        }
    }
    private void narrowCast(string parent_path, Event send_event){
        this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, parent_path, send_event}));
    }
    
    private void broadCast(Event send_event){
        this.object_manager.addEvent(new Event("broad_cast", "", new object[] {send_event}));
    }
    
    
    public Canvas(ControllerBase controller, GUITopLevel window_widget)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.canvas_widget = new GUICanvas();
        window_widget.addChild(this.canvas_widget);
        this.children_map = new Dictionary<int,int>();
        this.all_states_map = new Dictionary<int,int>();
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_main()
    {
        this.current_state[Node.Root].Add(Node.Root_main);
    }
    
    private void exit_Root_main()
    {
        this.exit_Root_main_event_processing();
        this.exit_Root_main_execution();
        this.current_state[Node.Root].Remove(Node.Root_main);
    }
    
    private void enter_Root_main_event_processing()
    {
        this.current_state[Node.Root_main].Add(Node.Root_main_event_processing);
    }
    
    private void exit_Root_main_event_processing()
    {
        if (this.current_state[Node.Root_main_event_processing].Contains(Node.Root_main_event_processing_listening)){
            this.exit_Root_main_event_processing_listening();
        }
        this.current_state[Node.Root_main].Remove(Node.Root_main_event_processing);
    }
    
    private void enter_Root_main_execution()
    {
        this.current_state[Node.Root_main].Add(Node.Root_main_execution);
    }
    
    private void exit_Root_main_execution()
    {
        if (this.current_state[Node.Root_main_execution].Contains(Node.Root_main_execution_idle)){
            this.exit_Root_main_execution_idle();
        }
        if (this.current_state[Node.Root_main_execution].Contains(Node.Root_main_execution_creation)){
            this.exit_Root_main_execution_creation();
        }
        if (this.current_state[Node.Root_main_execution].Contains(Node.Root_main_execution_connecting)){
            this.exit_Root_main_execution_connecting();
        }
        this.current_state[Node.Root_main].Remove(Node.Root_main_execution);
    }
    
    private void enter_Root_main_event_processing_listening()
    {
        this.current_state[Node.Root_main_event_processing].Add(Node.Root_main_event_processing_listening);
    }
    
    private void exit_Root_main_event_processing_listening()
    {
        this.current_state[Node.Root_main_event_processing].Remove(Node.Root_main_event_processing_listening);
    }
    
    private void enter_Root_main_execution_idle()
    {
        this.current_state[Node.Root_main_execution].Add(Node.Root_main_execution_idle);
    }
    
    private void exit_Root_main_execution_idle()
    {
        this.current_state[Node.Root_main_execution].Remove(Node.Root_main_execution_idle);
    }
    
    private void enter_Root_main_execution_creation()
    {
        this.current_state[Node.Root_main_execution].Add(Node.Root_main_execution_creation);
    }
    
    private void exit_Root_main_execution_creation()
    {
        this.current_state[Node.Root_main_execution].Remove(Node.Root_main_execution_creation);
    }
    
    private void enter_Root_main_execution_connecting()
    {
        this.current_state[Node.Root_main_execution].Add(Node.Root_main_execution_connecting);
    }
    
    private void exit_Root_main_execution_connecting()
    {
        this.current_state[Node.Root_main_execution].Remove(Node.Root_main_execution_connecting);
    }
    
    //Statechart enter/exit default method(s) :
    
    private void enterDefault_Root_main()
    {
        this.enter_Root_main();
        this.enterDefault_Root_main_event_processing();
        this.enterDefault_Root_main_execution();
    }
    
    private void enterDefault_Root_main_event_processing()
    {
        this.enter_Root_main_event_processing();
        this.enter_Root_main_event_processing_listening();
    }
    
    private void enterDefault_Root_main_execution()
    {
        this.enter_Root_main_execution();
        this.enter_Root_main_execution_idle();
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_main){
                catched = this.transition_Root_main(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_main(Event e)
    {
        bool catched = false;
        if (!catched){
            catched = this.transition_Root_main_event_processing(e) || catched;
            catched = this.transition_Root_main_execution(e) || catched;
        }
        return catched;
    }
    
    private bool transition_Root_main_event_processing(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root_main_event_processing][0] == Node.Root_main_event_processing_listening){
                catched = this.transition_Root_main_event_processing_listening(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_main_event_processing_listening(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "set_creation_type" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (e.getName() == "left-mouse-down" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (tag == this.canvas_widget.tag){
                enableds.Add(1);
            }
        }
        
        if (e.getName() == "middle-mouse-down" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            Vector2 position = (Vector2)parameters[1];
            if (tag == this.canvas_widget.tag){
                enableds.Add(2);
            }
        }
        
        if (e.getName() == "right-mouse-down" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (tag == this.canvas_widget.tag){
                enableds.Add(3);
            }
        }
        
        if (e.getName() == "unselect" && e.getPort() == ""){
            enableds.Add(4);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_event_processing_listening. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                string creation_type = (string)parameters[0];
                this.exit_Root_main_event_processing_listening();
                this.creation_type = creation_type;
                this.enter_Root_main_event_processing_listening();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_main_event_processing_listening();
                this.narrowCast("./all_states", new Event("unselect", "", new object[] {tag}));
                this.enter_Root_main_event_processing_listening();
            } else if (enabled == 2){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                Vector2 position = (Vector2)parameters[1];
                this.exit_Root_main_event_processing_listening();
                this.narrowCast("./all_states", new Event("unselect", "", new object[] {tag}));
                this.addEvent(new Event("create", "", new object[] {position}));
                this.enter_Root_main_event_processing_listening();
            } else if (enabled == 3){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_main_event_processing_listening();
                this.narrowCast("./all_states", new Event("unselect", "", new object[] {tag}));
                this.enter_Root_main_event_processing_listening();
            } else if (enabled == 4){
                object[] parameters = e.getParameters();
                int except_tag = (int)parameters[0];
                this.exit_Root_main_event_processing_listening();
                this.narrowCast("./all_states", new Event("unselect", "", new object[] {except_tag}));
                this.enter_Root_main_event_processing_listening();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_execution(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root_main_execution][0] == Node.Root_main_execution_idle){
                catched = this.transition_Root_main_execution_idle(e);
            } else if (this.current_state[Node.Root_main_execution][0] == Node.Root_main_execution_creation){
                catched = this.transition_Root_main_execution_creation(e);
            } else if (this.current_state[Node.Root_main_execution][0] == Node.Root_main_execution_connecting){
                catched = this.transition_Root_main_execution_connecting(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_main_execution_idle(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "create" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (e.getName() == "new_child" && e.getPort() == ""){
            enableds.Add(1);
        }
        
        if (e.getName() == "disconnect_child" && e.getPort() == ""){
            enableds.Add(2);
        }
        
        if (e.getName() == "connect_child_to_parent" && e.getPort() == ""){
            enableds.Add(3);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_execution_idle. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                Vector2 position = (Vector2)parameters[0];
                this.exit_Root_main_execution_idle();
                this.current_item = new GUICanvasElement(canvas_widget, position);
                this.object_manager.addEvent(new Event("create_instance", "", new object[] { this, "./all_states","BasicState",this.current_item}));
                this.enter_Root_main_execution_creation();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                GUICanvasElement child = (GUICanvasElement)parameters[1];
                this.exit_Root_main_execution_idle();
                this.canvas_widget.addElement(child);
                                            this.children_map[child.tag] = id;
                this.enter_Root_main_execution_idle();
            } else if (enabled == 2){
                object[] parameters = e.getParameters();
                GUICanvasElement child = (GUICanvasElement)parameters[0];
                this.exit_Root_main_execution_idle();
                //Debug.Log(string.Format("removing tag {0} from canvas", child.tag));
                this.object_manager.addEvent(new Event("unassociate_instance", "", new object[] { this, String.Format("children[{0}]", this.children_map[child.tag])}));
                this.children_map.Remove(child.tag);
                                            this.canvas_widget.removeElement(child);
                this.enter_Root_main_execution_idle();
            } else if (enabled == 3){
                object[] parameters = e.getParameters();
                GUICanvasElement child = (GUICanvasElement)parameters[0];
                GUICanvasElement parent = (GUICanvasElement)parameters[1];
                this.exit_Root_main_execution_idle();
                this.current_item = child;
                                            String parent_path = ".";
                                            if (parent != null)
                                            {
                                                parent_path = String.Format("all_states[{0}]", this.all_states_map[parent.tag]);
                                            }
                                            String child_path = String.Format("all_states[{0}]", this.all_states_map[child.tag]);
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, parent_path,child_path + "/parent"}));
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, child_path,parent_path + "/children"}));
                this.enter_Root_main_execution_connecting();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_execution_creation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "instance_created" && e.getPort() == ""){
            object[] parameters = e.getParameters();
            int id = (int)parameters[0];
            String association_name = (String)parameters[1];
            if (association_name.EndsWith("/all_states")){
                enableds.Add(0);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_execution_creation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                String association_name = (String)parameters[1];
                this.exit_Root_main_execution_creation();
                String association_path = String.Format("{0}[{1}]", association_name, id);
                                            //Debug.Log(string.Format("setting tag {0} to id {1}", this.current_item.tag, id));
                                            this.all_states_map[this.current_item.tag] = id;
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, ".",association_path + "/parent"}));
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, ".",association_path + "/canvas"}));
                this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, association_path,"./children"}));
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, association_path}));
                this.enter_Root_main_execution_connecting();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_execution_connecting(Event e)
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
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_execution_connecting. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                String association_name = (String)parameters[1];
                this.exit_Root_main_execution_connecting();
                String parent_path = association_name.Substring(0, association_name.Length-9);
                this.narrowCast(parent_path, new Event("new_child", "", new object[] {id,this.current_item}));
                this.current_item = null;
                this.enter_Root_main_execution_idle();
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
    protected GUICanvasElement widget;
    
    public State(GUICanvasElement widget)
    {
        //constructor body (user-defined)
        this.widget = widget;
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
        Root_main,
        Root_main_event_processing,
        Root_main_selection_state,
        Root_main_selection_state_selected,
        Root_main_selection_state_selected_drop,
        Root_main_event_processing_listening,
        Root_main_selection_state_setup,
        Root_main_selection_state_not_selected,
        Root_main_selection_state_selected_not_dragging,
        Root_main_selection_state_selected_dragging,
        Root_main_selection_state_selected_drop_drop_window_creation,
        Root_main_selection_state_selected_drop_wait_for_drop_window,
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
        this.current_state[Node.Root_main] = new List<Node>();
        this.current_state[Node.Root_main_event_processing] = new List<Node>();
        this.current_state[Node.Root_main_selection_state] = new List<Node>();
        this.current_state[Node.Root_main_selection_state_selected] = new List<Node>();
        this.current_state[Node.Root_main_selection_state_selected_drop] = new List<Node>();
    }
    
    public void start()
    {
        if (!this.active) {
            this.active = true;
            this.enterDefault_Root_main();
        }
    }
    private void narrowCast(string parent_path, Event send_event){
        this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, parent_path, send_event}));
    }
    
    private void broadCast(Event send_event){
        this.object_manager.addEvent(new Event("broad_cast", "", new object[] {send_event}));
    }
    
    
    public BasicState(ControllerBase controller, GUICanvasElement widget) : base(widget)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.children_map = new Dictionary<int,int>();
    }
    //Statechart enter/exit action method(s) :
    
    private void enter_Root_main()
    {
        this.current_state[Node.Root].Add(Node.Root_main);
    }
    
    private void exit_Root_main()
    {
        this.exit_Root_main_event_processing();
        this.exit_Root_main_selection_state();
        this.current_state[Node.Root].Remove(Node.Root_main);
    }
    
    private void enter_Root_main_event_processing()
    {
        this.current_state[Node.Root_main].Add(Node.Root_main_event_processing);
    }
    
    private void exit_Root_main_event_processing()
    {
        if (this.current_state[Node.Root_main_event_processing].Contains(Node.Root_main_event_processing_listening)){
            this.exit_Root_main_event_processing_listening();
        }
        this.current_state[Node.Root_main].Remove(Node.Root_main_event_processing);
    }
    
    private void enter_Root_main_selection_state()
    {
        this.current_state[Node.Root_main].Add(Node.Root_main_selection_state);
    }
    
    private void exit_Root_main_selection_state()
    {
        if (this.current_state[Node.Root_main_selection_state].Contains(Node.Root_main_selection_state_setup)){
            this.exit_Root_main_selection_state_setup();
        }
        if (this.current_state[Node.Root_main_selection_state].Contains(Node.Root_main_selection_state_not_selected)){
            this.exit_Root_main_selection_state_not_selected();
        }
        if (this.current_state[Node.Root_main_selection_state].Contains(Node.Root_main_selection_state_selected)){
            this.exit_Root_main_selection_state_selected();
        }
        this.current_state[Node.Root_main].Remove(Node.Root_main_selection_state);
    }
    
    private void enter_Root_main_selection_state_selected()
    {
        this.widget.setColor(Color.Lerp(GUI.backgroundColor, Color.green, 0.5f));
        this.current_state[Node.Root_main_selection_state].Add(Node.Root_main_selection_state_selected);
    }
    
    private void exit_Root_main_selection_state_selected()
    {
        if (this.current_state[Node.Root_main_selection_state_selected].Contains(Node.Root_main_selection_state_selected_not_dragging)){
            this.exit_Root_main_selection_state_selected_not_dragging();
        }
        if (this.current_state[Node.Root_main_selection_state_selected].Contains(Node.Root_main_selection_state_selected_dragging)){
            this.exit_Root_main_selection_state_selected_dragging();
        }
        if (this.current_state[Node.Root_main_selection_state_selected].Contains(Node.Root_main_selection_state_selected_drop)){
            this.exit_Root_main_selection_state_selected_drop();
        }
        this.current_state[Node.Root_main_selection_state].Remove(Node.Root_main_selection_state_selected);
    }
    
    private void enter_Root_main_selection_state_selected_drop()
    {
        this.current_state[Node.Root_main_selection_state_selected].Add(Node.Root_main_selection_state_selected_drop);
    }
    
    private void exit_Root_main_selection_state_selected_drop()
    {
        if (this.current_state[Node.Root_main_selection_state_selected_drop].Contains(Node.Root_main_selection_state_selected_drop_drop_window_creation)){
            this.exit_Root_main_selection_state_selected_drop_drop_window_creation();
        }
        if (this.current_state[Node.Root_main_selection_state_selected_drop].Contains(Node.Root_main_selection_state_selected_drop_wait_for_drop_window)){
            this.exit_Root_main_selection_state_selected_drop_wait_for_drop_window();
        }
        this.current_state[Node.Root_main_selection_state_selected].Remove(Node.Root_main_selection_state_selected_drop);
    }
    
    private void enter_Root_main_event_processing_listening()
    {
        this.current_state[Node.Root_main_event_processing].Add(Node.Root_main_event_processing_listening);
    }
    
    private void exit_Root_main_event_processing_listening()
    {
        this.current_state[Node.Root_main_event_processing].Remove(Node.Root_main_event_processing_listening);
    }
    
    private void enter_Root_main_selection_state_setup()
    {
        this.current_state[Node.Root_main_selection_state].Add(Node.Root_main_selection_state_setup);
    }
    
    private void exit_Root_main_selection_state_setup()
    {
        this.current_state[Node.Root_main_selection_state].Remove(Node.Root_main_selection_state_setup);
    }
    
    private void enter_Root_main_selection_state_not_selected()
    {
        this.current_state[Node.Root_main_selection_state].Add(Node.Root_main_selection_state_not_selected);
    }
    
    private void exit_Root_main_selection_state_not_selected()
    {
        this.current_state[Node.Root_main_selection_state].Remove(Node.Root_main_selection_state_not_selected);
    }
    
    private void enter_Root_main_selection_state_selected_not_dragging()
    {
        this.current_state[Node.Root_main_selection_state_selected].Add(Node.Root_main_selection_state_selected_not_dragging);
    }
    
    private void exit_Root_main_selection_state_selected_not_dragging()
    {
        this.current_state[Node.Root_main_selection_state_selected].Remove(Node.Root_main_selection_state_selected_not_dragging);
    }
    
    private void enter_Root_main_selection_state_selected_dragging()
    {
        this.current_state[Node.Root_main_selection_state_selected].Add(Node.Root_main_selection_state_selected_dragging);
    }
    
    private void exit_Root_main_selection_state_selected_dragging()
    {
        this.current_state[Node.Root_main_selection_state_selected].Remove(Node.Root_main_selection_state_selected_dragging);
    }
    
    private void enter_Root_main_selection_state_selected_drop_drop_window_creation()
    {
        this.object_manager.addEvent(new Event("create_instance", "", new object[] { this, "state_drop","StateDrop",this.widget}));
        this.object_manager.addEvent(new Event("associate_instance", "", new object[] { this, ".","state_drop/dropped_state"}));
        this.current_state[Node.Root_main_selection_state_selected_drop].Add(Node.Root_main_selection_state_selected_drop_drop_window_creation);
    }
    
    private void exit_Root_main_selection_state_selected_drop_drop_window_creation()
    {
        this.current_state[Node.Root_main_selection_state_selected_drop].Remove(Node.Root_main_selection_state_selected_drop_drop_window_creation);
    }
    
    private void enter_Root_main_selection_state_selected_drop_wait_for_drop_window()
    {
        this.current_state[Node.Root_main_selection_state_selected_drop].Add(Node.Root_main_selection_state_selected_drop_wait_for_drop_window);
    }
    
    private void exit_Root_main_selection_state_selected_drop_wait_for_drop_window()
    {
        this.current_state[Node.Root_main_selection_state_selected_drop].Remove(Node.Root_main_selection_state_selected_drop_wait_for_drop_window);
    }
    
    //Statechart enter/exit default method(s) :
    
    private void enterDefault_Root_main()
    {
        this.enter_Root_main();
        this.enterDefault_Root_main_event_processing();
        this.enterDefault_Root_main_selection_state();
    }
    
    private void enterDefault_Root_main_event_processing()
    {
        this.enter_Root_main_event_processing();
        this.enter_Root_main_event_processing_listening();
    }
    
    private void enterDefault_Root_main_selection_state()
    {
        this.enter_Root_main_selection_state();
        this.enter_Root_main_selection_state_setup();
    }
    
    private void enterDefault_Root_main_selection_state_selected()
    {
        this.enter_Root_main_selection_state_selected();
        this.enter_Root_main_selection_state_selected_not_dragging();
    }
    
    private void enterDefault_Root_main_selection_state_selected_drop()
    {
        this.enter_Root_main_selection_state_selected_drop();
        this.enter_Root_main_selection_state_selected_drop_drop_window_creation();
    }
    
    //Statechart transitions :
    
    private bool transition_Root(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root][0] == Node.Root_main){
                catched = this.transition_Root_main(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_main(Event e)
    {
        bool catched = false;
        if (!catched){
            catched = this.transition_Root_main_event_processing(e) || catched;
            catched = this.transition_Root_main_selection_state(e) || catched;
        }
        return catched;
    }
    
    private bool transition_Root_main_event_processing(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root_main_event_processing][0] == Node.Root_main_event_processing_listening){
                catched = this.transition_Root_main_event_processing_listening(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_main_event_processing_listening(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "new_child" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (e.getName() == "adjust_size" && e.getPort() == ""){
            enableds.Add(1);
        }
        
        if (e.getName() == "disconnect_child" && e.getPort() == ""){
            enableds.Add(2);
        }
        
        if (e.getName() == "left-mouse-down" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (tag == this.widget.tag){
                enableds.Add(3);
            }
        }
        
        if (e.getName() == "middle-mouse-down" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (tag == this.widget.tag){
                enableds.Add(4);
            }
        }
        
        if (e.getName() == "right-mouse-down" && e.getPort() == "input"){
            object[] parameters = e.getParameters();
            int tag = (int)parameters[0];
            if (tag == this.widget.tag){
                enableds.Add(5);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_event_processing_listening. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                GUICanvasElement child = (GUICanvasElement)parameters[1];
                this.exit_Root_main_event_processing_listening();
                this.widget.addElement(child);
                                            this.children_map[child.tag] = id;
                                            this.widget.adjustSize();
                this.enter_Root_main_event_processing_listening();
            }
            if (enabled == 1){
                this.exit_Root_main_event_processing_listening();
                this.widget.adjustSize();
                this.enter_Root_main_event_processing_listening();
            } else if (enabled == 2){
                object[] parameters = e.getParameters();
                GUICanvasElement child = (GUICanvasElement)parameters[0];
                this.exit_Root_main_event_processing_listening();
                this.object_manager.addEvent(new Event("unassociate_instance", "", new object[] { this, String.Format("children[{0}]", this.children_map[child.tag])}));
                this.children_map.Remove(child.tag);
                                            this.widget.removeElement(child);
                this.enter_Root_main_event_processing_listening();
            } else if (enabled == 3){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_main_event_processing_listening();
                this.narrowCast("canvas", new Event("unselect", "", new object[] {this.widget.tag}));
                this.addEvent(new Event("set_selected", "", new object[] {}));
                this.enter_Root_main_event_processing_listening();
            } else if (enabled == 4){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_main_event_processing_listening();
                this.narrowCast("canvas", new Event("unselect", "", new object[] {this.widget.tag}));
                this.enter_Root_main_event_processing_listening();
            } else if (enabled == 5){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                this.exit_Root_main_event_processing_listening();
                this.narrowCast("canvas", new Event("unselect", "", new object[] {this.widget.tag}));
                this.enter_Root_main_event_processing_listening();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_selection_state(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root_main_selection_state][0] == Node.Root_main_selection_state_setup){
                catched = this.transition_Root_main_selection_state_setup(e);
            } else if (this.current_state[Node.Root_main_selection_state][0] == Node.Root_main_selection_state_not_selected){
                catched = this.transition_Root_main_selection_state_not_selected(e);
            } else if (this.current_state[Node.Root_main_selection_state][0] == Node.Root_main_selection_state_selected){
                catched = this.transition_Root_main_selection_state_selected(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_main_selection_state_setup(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        enableds.Add(0);
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_selection_state_setup. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_main_selection_state_setup();
                this.enter_Root_main_selection_state_selected();
                this.enterDefault_Root_main_selection_state_selected_drop();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_selection_state_not_selected(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "set_selected" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_selection_state_not_selected. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                this.exit_Root_main_selection_state_not_selected();
                this.widget.pushToFront();
                this.enterDefault_Root_main_selection_state_selected();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_selection_state_selected(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "unselect" && e.getPort() == ""){
            object[] parameters = e.getParameters();
            int except_tag = (int)parameters[0];
            if (this.widget.tag != except_tag){
                enableds.Add(0);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_selection_state_selected. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int except_tag = (int)parameters[0];
                this.exit_Root_main_selection_state_selected();
                this.widget.resetColor();
                this.enter_Root_main_selection_state_not_selected();
            }
            catched = true;
        }
        
        if (!catched){
            if (this.current_state[Node.Root_main_selection_state_selected][0] == Node.Root_main_selection_state_selected_not_dragging){
                catched = this.transition_Root_main_selection_state_selected_not_dragging(e);
            } else if (this.current_state[Node.Root_main_selection_state_selected][0] == Node.Root_main_selection_state_selected_dragging){
                catched = this.transition_Root_main_selection_state_selected_dragging(e);
            } else if (this.current_state[Node.Root_main_selection_state_selected][0] == Node.Root_main_selection_state_selected_drop){
                catched = this.transition_Root_main_selection_state_selected_drop(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_main_selection_state_selected_not_dragging(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "left-mouse-drag" && e.getPort() == "input"){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_selection_state_selected_not_dragging. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                Vector2 position = (Vector2)parameters[1];
                Vector2 delta = (Vector2)parameters[2];
                this.exit_Root_main_selection_state_selected_not_dragging();
                this.widget.move(delta);
                this.enter_Root_main_selection_state_selected_dragging();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_selection_state_selected_dragging(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "left-mouse-drag" && e.getPort() == "input"){
            enableds.Add(0);
        }
        
        if (e.getName() == "left-mouse-up" && e.getPort() == "input"){
            enableds.Add(1);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_selection_state_selected_dragging. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int tag = (int)parameters[0];
                Vector2 position = (Vector2)parameters[1];
                Vector2 delta = (Vector2)parameters[2];
                this.exit_Root_main_selection_state_selected_dragging();
                this.widget.move(delta);
                this.enter_Root_main_selection_state_selected_dragging();
            }
            if (enabled == 1){
                this.exit_Root_main_selection_state_selected_dragging();
                this.enterDefault_Root_main_selection_state_selected_drop();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_selection_state_selected_drop(Event e)
    {
        bool catched = false;
        if (!catched){
            if (this.current_state[Node.Root_main_selection_state_selected_drop][0] == Node.Root_main_selection_state_selected_drop_drop_window_creation){
                catched = this.transition_Root_main_selection_state_selected_drop_drop_window_creation(e);
            } else if (this.current_state[Node.Root_main_selection_state_selected_drop][0] == Node.Root_main_selection_state_selected_drop_wait_for_drop_window){
                catched = this.transition_Root_main_selection_state_selected_drop_wait_for_drop_window(e);
            }
        }
        return catched;
    }
    
    private bool transition_Root_main_selection_state_selected_drop_drop_window_creation(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "instance_associated" && e.getPort() == ""){
            enableds.Add(0);
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_selection_state_selected_drop_drop_window_creation. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                int id = (int)parameters[0];
                String association_name = (String)parameters[1];
                this.exit_Root_main_selection_state_selected_drop_drop_window_creation();
                this.object_manager.addEvent(new Event("start_instance", "", new object[] { this, "state_drop"}));
                this.enter_Root_main_selection_state_selected_drop_wait_for_drop_window();
            }
            catched = true;
        }
        
        return catched;
    }
    
    private bool transition_Root_main_selection_state_selected_drop_wait_for_drop_window(Event e)
    {
        bool catched = false;
        List<int> enableds = new List<int>();
        if (e.getName() == "state_drop_response" && e.getPort() == ""){
            object[] parameters = e.getParameters();
            bool do_reconnect = (bool)parameters[0];
            GUICanvasElement connection = (GUICanvasElement)parameters[1];
            if (!do_reconnect){
                enableds.Add(0);
            }
        }
        
        if (e.getName() == "state_drop_response" && e.getPort() == ""){
            object[] parameters = e.getParameters();
            bool do_reconnect = (bool)parameters[0];
            GUICanvasElement connection = (GUICanvasElement)parameters[1];
            if (do_reconnect){
                enableds.Add(1);
            }
        }
        
        if (enableds.Count > 1){
            Console.WriteLine("Runtime warning : indeterminism detected in a transition from node Root_main_selection_state_selected_drop_wait_for_drop_window. Only the first in document order enabled transition is executed.");
        }
        if (enableds.Count > 0){
            int enabled = enableds[0];
            
            if (enabled == 0){
                object[] parameters = e.getParameters();
                bool do_reconnect = (bool)parameters[0];
                GUICanvasElement connection = (GUICanvasElement)parameters[1];
                this.exit_Root_main_selection_state_selected_drop();
                this.object_manager.addEvent(new Event("delete_instance", "", new object[] { this, "state_drop"}));
                this.narrowCast("parent", new Event("adjust_size", "", new object[] {}));
                this.enter_Root_main_selection_state_selected_not_dragging();
            }
            if (enabled == 1){
                object[] parameters = e.getParameters();
                bool do_reconnect = (bool)parameters[0];
                GUICanvasElement connection = (GUICanvasElement)parameters[1];
                this.exit_Root_main_selection_state_selected_drop();
                this.narrowCast("parent", new Event("disconnect_child", "", new object[] {this.widget}));
                this.object_manager.addEvent(new Event("unassociate_instance", "", new object[] { this, "parent"}));
                this.narrowCast("canvas", new Event("connect_child_to_parent", "", new object[] {this.widget,connection}));
                this.object_manager.addEvent(new Event("delete_instance", "", new object[] { this, "state_drop"}));
                this.enter_Root_main_selection_state_selected_not_dragging();
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

public class StateDrop : IRuntimeClass
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
    List<GUICanvasElement> connection_options;
    string[] connection_options_strings;
    int selected_option = 0;
    GUICanvasElement canvas_member;
    
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
    private void narrowCast(string parent_path, Event send_event){
        this.object_manager.addEvent(new Event("narrow_cast", "", new object[] {this, parent_path, send_event}));
    }
    
    private void broadCast(Event send_event){
        this.object_manager.addEvent(new Event("broad_cast", "", new object[] {send_event}));
    }
    
    
    public StateDrop(ControllerBase controller, GUICanvasElement canvas_member)
    {
        this.commonConstructor(controller);
        //constructor body (user-defined)
        this.canvas_member = canvas_member;
        this.connection_options = new List<GUICanvasElement>();
        List<GUICanvasElement> overlappings = canvas_member.getOverlappings();
        if (overlappings.Count == 0)
        {
        	if (this.canvas_member.parent != this.canvas_member.canvas)
        		this.connection_options.Add(null);
        }
        else
        {
        	for (int i = overlappings.Count - 1; i >= 0; i--)
        	{
                if (overlappings[i] == this.canvas_member.parent)
                    break;
        		this.connection_options.Add(overlappings[i]);
        		if (overlappings[i].completelyContains(this.canvas_member))
        			break;
        	}
        }
    }
    
    public void drawModalWindow(GUIModalWindow modal_window)
    {
        EditorGUILayout.Space();
        GUILayout.Label("Connect to:");
        this.selected_option = GUILayout.SelectionGrid(this.selected_option, this.connection_options_strings, 1, "MenuItem");
        EditorGUILayout.Space();
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
                                    this.connection_options_strings[i] = "Canvas(disconnecting)";
                            }
                            new GUIModalWindow("State Drop", this.canvas_member.canvas, this.drawModalWindow);
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
                GUICanvasElement connection = null;
                                    bool do_reconnect = false;
                                    if (this.connection_options.Count == 1)
                                    {
                                        do_reconnect = true;
                                        connection = this.connection_options[0];
                                    }
                this.narrowCast("dropped_state", new Event("state_drop_response", "", new object[] {do_reconnect,connection}));
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
                GUICanvasElement connection = (GUICanvasElement)parameters[1];
                this.exit_Root_popup();
                this.narrowCast("dropped_state", new Event("state_drop_response", "", new object[] {do_reconnect,connection}));
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
        if (class_name == "StateChartWindow" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(StateChartWindow), new_parameters);
            associations.Add(new Association("toolbar", "Toolbar", 0, 1));
            associations.Add(new Association("canvas", "Canvas", 0, 1));
        }else if (class_name == "Toolbar" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(Toolbar), new_parameters);
            associations.Add(new Association("window", "StateChartEditor", 0, 1));
            associations.Add(new Association("canvas", "Canvas", 0, 1));
            associations.Add(new Association("buttons", "Button", 0, -1));
        }else if (class_name == "Button" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(Button), new_parameters);
            associations.Add(new Association("toolbar", "Toolbar", 0, 1));
        }else if (class_name == "Canvas" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(Canvas), new_parameters);
            associations.Add(new Association("window", "StateChartEditor", 0, 1));
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
            associations.Add(new Association("state_drop", "StateDrop", 0, 1));
        }else if (class_name == "StateDrop" ){
            object[] new_parameters = new object[construct_params.Length + 1];
            new_parameters[0] = this.controller;
            Array.Copy(construct_params, 0, new_parameters, 1, construct_params.Length);
            instance = (IRuntimeClass) Activator.CreateInstance(typeof(StateDrop), new_parameters);
            associations.Add(new Association("dropped_state", "State", 0, 1));
        }
        if (instance != null) {
            return new InstanceWrapper(instance, associations);
        }
        throw new RunTimeException(string.Format("Tried to instantiate class '{0}', which is not part of the class diagram.", class_name));
    }
}

public class Controller : GameLoopControllerBase
{
    public Controller(GUITopLevel window_widget) : base()
    {
        this.addInputPort("input");
        this.addInputPort("engine");
        this.addInputPort("windows");
        this.object_manager = new ObjectManager(this);
        this.object_manager.createInstance("StateChartWindow", new object[]{window_widget});
    }
}
