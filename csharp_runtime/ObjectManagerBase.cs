using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace sccdlib
{
    public abstract class ObjectManagerBase
    {
        protected ControllerBase controller;
        EventQueue events = new EventQueue();
        Dictionary<RuntimeClassBase,InstanceWrapper> instances_map = new Dictionary<RuntimeClassBase,InstanceWrapper> ();
        
        public ObjectManagerBase (ControllerBase controller)
        {
            this.controller = controller;
        }
       
        
        public void addEvent (Event input_event, double time_offset = 0.0)
        {
            this.events.Add (input_event, time_offset);
        }
        
        public void broadcast (Event new_event)
        {
            foreach (RuntimeClassBase instance in this.instances_map.Keys)
                instance.addEvent(new_event);
        }
        
        public  double getWaitTime()
        {
            //first get waiting time of the object manager's events
            double smallest_time = this.events.getEarliestTime();
            //check all the instances
            foreach (RuntimeClassBase instance in this.instances_map.Keys)
                smallest_time = Math.Min(smallest_time, instance.getEarliestEventTime());
            return smallest_time;
        }
        
        public void step (double delta)
        {
            this.events.decreaseTime(delta);
            foreach( Event e in this.events.popDueEvents())
                this.handleEvent (e);
        }
        
        private void handleEvent (Event handle_event)
        {
            string event_name = handle_event.getName ();
            if (event_name == "narrow_cast") {
                this.handleNarrowCastEvent(handle_event.getParameters());
            } else if (event_name == "broad_cast") {
                this.handleBroadCastEvent(handle_event.getParameters());
            } else if (event_name == "create_instance") {
                this.handleCreateEvent(handle_event.getParameters());
            } else if (event_name == "associate_instance") {
                this.handleAssociateEvent(handle_event.getParameters());
            } else if (event_name == "start_instance") {
                this.handleStartInstanceEvent(handle_event.getParameters());
            }
        }
        
        public void stepAll (double delta)
        {
            this.step(delta);
            foreach (RuntimeClassBase instance in this.instances_map.Keys)
                instance.step(delta);
        }
    
        
        public void start ()
        {
            foreach (RuntimeClassBase instance in this.instances_map.Keys)
                instance.start(); 
        }
        
        /// <summary>
        /// Processes the association reference.
        /// </summary>
        /// <returns>
        /// The association reference.
        /// </returns>
        /// <param name='input_string'>
        /// Input_string.
        /// </param>
        private List<Tuple<string, int>> processAssociationReference (string input_string)
        {
            if (input_string.Length == 0)
                throw new AssociationReferenceException("Empty association reference.");
            string[] path_string = input_string.Split (new char[] {'/'});
            Regex regex = new Regex(@"^([a-zA-Z_]\w*)(?:\[(\d+)\])?$");
            
            var result = new List<Tuple<string, int>>();
            
            foreach (string string_piece in path_string) {
                Match match = regex.Match (string_piece);
                if (match.Success ){
                    string name = match.Groups[1].ToString ();
                    int index;
                    if (match.Groups[2].Success)
                        int.TryParse(match.Groups[2].ToString(), out index);
                    else
                        index = -1;
                    result.Add( new Tuple<string, int>(name,index));
                }else{
                    throw new AssociationReferenceException("Invalid entry in association reference.");
                }   
            }
            return result;
        }
        
        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <returns>
        /// The instances.
        /// </returns>
        /// <param name='source'>
        /// Source.
        /// </param>
        /// <param name='traversal_list'>
        /// Traversal_list.
        /// </param>
        private List<InstanceWrapper> getInstances (RuntimeClassBase source, List<Tuple<string, int>> traversal_list)
        {
            var currents = new List<InstanceWrapper> ();
            currents.Add (this.instances_map [source]);
            foreach (Tuple<string, int> tuple in traversal_list) {
                var nexts = new List<InstanceWrapper> ();
                foreach ( InstanceWrapper current in currents ){
                    Association association = current.getAssociation (tuple.Item1);   
                    if (tuple.Item2 >= 0 )
                        nexts.Add ( association.getInstance(tuple.Item2) );
                    else if (tuple.Item2 == -1)
                        nexts.AddRange ( association.getAllInstances() );
                    else
                        throw new AssociationReferenceException("Incorrect index in association reference.");
                }
                currents = nexts;
            }
            return currents;
        }
        
        /// <summary>
        /// Handles the start instance event.
        /// </summary>
        /// <param name='parameters'>
        /// [0] The instance the event originates from.
        /// [1] An association reference string targeting the instance to start.
        /// </param>
        private void handleStartInstanceEvent (object[] parameters)
        {
            if (parameters.Length != 2) {
                throw new ParameterException ("The start instance event needs 2 parameters.");    
            } else {
                RuntimeClassBase source = (RuntimeClassBase) parameters[0];
                var traversal_list = this.processAssociationReference((string) parameters [1]);
                
                foreach( InstanceWrapper i in this.getInstances (source, traversal_list)){
                    i.getInstance().start();
                }
            }
        }
        
        /// <summary>
        /// Handles the broad cast event.
        /// </summary>
        /// <param name='parameters'>
        /// [0] The event to be broadcasted.
        /// </param>
        private void handleBroadCastEvent(object[] parameters)
        {
            if (parameters.Length != 1 ) 
                throw new ParameterException ("The broadcast event needs 1 parameter.");   
            this.broadcast((Event)parameters[0]); 
        }

        private void handleCreateEvent (object[] parameters)
        {
            if (parameters.Length < 2) {
                throw new ParameterException ("The create event needs at least 2 parameters.");   
            } else {
                RuntimeClassBase source = (RuntimeClassBase)parameters [0];
                string association_name = (string)parameters [1];
                Association association = this.instances_map[source].getAssociation (association_name);
                if (association.allowedToAdd ()){
                    int constructor_parameters_length = parameters.Length -2;
                    object[] constructor_parameters = new object[constructor_parameters_length];
                    Array.Copy(parameters, 2, constructor_parameters, 0, constructor_parameters_length);
                    InstanceWrapper new_instance_wrapper = this.createInstance(association.getClassName (), constructor_parameters);
                    association.addInstance (new_instance_wrapper);
                    source.addEvent(
                        new Event(name: "instance_created", parameters : new object[] {association_name})
                    );
                }else{
                    source.addEvent (
                        new Event(name: "instance_creation_error", parameters : new object[] {association_name})    
                    );
                }    
            }
        }
        
                
        private void handleAssociateEvent (object[] parameters)
        {
            if (parameters.Length != 3) {
                throw new ParameterException ("The associate_instance event needs 3 parameters.");
            } else {
                RuntimeClassBase source = (RuntimeClassBase)parameters [0];
                List<InstanceWrapper> to_copy_list = this.getInstances (source, this.processAssociationReference ((string)parameters [1]));
                if (to_copy_list.Count != 1)
                    throw new AssociationReferenceException ("Invalid source association reference.");
                InstanceWrapper wrapped_to_copy_instance = to_copy_list [0];
                List<Tuple<string,int>> dest_list = this.processAssociationReference ((string)parameters [2]);
                if (dest_list.Count == 0)
                    throw new AssociationReferenceException ("Invalid destination association reference.");
                Tuple<string,int> last_tuple = dest_list [dest_list.Count - 1];
                if (last_tuple.Item2 != -1)
                    throw new AssociationReferenceException ("Last association name in association reference should not be accompanied by an index.");
                dest_list.RemoveAt (dest_list.Count - 1);
                foreach (InstanceWrapper i in this.getInstances(source, dest_list)) {
                    i.getAssociation (last_tuple.Item1).addInstance (wrapped_to_copy_instance);
                }
            }
        }
            
        private void handleNarrowCastEvent(object[] parameters)
        {
            if (parameters.Length != 3){
                throw new ParameterException ("The associate_instance event needs 3 parameters.");
            }else{
                RuntimeClassBase source = (RuntimeClassBase)parameters [0];
                Event cast_event = (Event) parameters[2];
                foreach (InstanceWrapper i in this.getInstances(source, this.processAssociationReference( (string) parameters[1])))
                    i.getInstance ().addEvent(cast_event);
            
            }
        }   
        
        
        protected abstract InstanceWrapper instantiate(string class_name, object[] construct_params);

            
        public InstanceWrapper createInstance(string class_name, object[] construct_params)
        {
            InstanceWrapper iw = this.instantiate(class_name, construct_params);
            if (iw != null)
                this.instances_map[iw.getInstance ()] = iw;
            return iw;
        }
    }
}

