import abc
import re
import time
import threading
from infinity import INFINITY
from event_queue import EventQueue
from Queue import Queue, Empty


class RuntimeException(Exception):
    def __init__(self, message):
        self.message = message
    def __str__(self):
        return repr(self.message)

class AssociationException(RuntimeException):
    pass

class AssociationReferenceException(RuntimeException):
    pass

class ParameterException(RuntimeException):
    pass

class InputException(RuntimeException):
    pass

class Association(object):
    #wrapper object for one association relation
    def __init__(self, name, class_name, min_card, max_card):
        self.min_card = min_card
        self.max_card = max_card
        self.name = name
        self.class_name = class_name
        self.instances = [] #list of all instance wrappers
        
    def getName(self):
        return self.name
    
    def getClassName(self):
        return self.class_name
        
    def allowedToAdd(self):
        return self.max_card == -1 or len(self.instances) < self.max_card
        
    def addInstance(self, instance):
        if self.allowedToAdd() :
            self.instances.append(instance)
        else :
            raise AssociationException("Not allowed to add the instance to the association.")
        
    def getInstance(self, index):
        try :
            return self.instances[index]
        except IndexError :
            raise AssociationException("Invalid index for fetching instance(s) from association.")

class InstanceWrapper(object):
    #wrapper object for an instance and its relevant information needed in the object manager
    def __init__(self, instance, associations):
        self.instance = instance
        self.associations = {}
        for association in associations :
            self.associations[association.getName()] = association  
        
    def getAssociation(self, name):
        try :
            return self.associations[name]
        except KeyError :
            raise AssociationReferenceException("Unknown association.")
    
    def getInstance(self):
        return self.instance

class ObjectManagerBase(object):
    __metaclass__  = abc.ABCMeta
    
    def __init__(self, controller):
        self.controller = controller
        self.events = EventQueue()
        self.instances_map = {} #a dictionary that maps RuntimeClassBase to InstanceWrapper
        
    def addEvent(self, event, time_offset = 0.0):
        self.events.add(event, time_offset)
        
    # Broadcast an event to all instances
    def broadcast(self, new_event):
        for i in self.instances_map:
            i.addEvent(new_event)
        
    def getWaitTime(self):  
        #first get waiting time of the object manager's events
        smallest_time = self.events.getEarliestTime();
        #check all the instances
        for instance in self.instances_map.iterkeys() :
            smallest_time = min(smallest_time, instance.getEarliestEventTime())
        return smallest_time;
    
    def stepAll(self, delta):
        self.step(delta)
        for i in self.instances_map.iterkeys():
            i.step(delta)

    def step(self, delta):
        self.events.decreaseTime(delta);
        for event in self.events.popDueEvents() :
            self.handleEvent(event)
               
    def start(self):
        for i in self.instances_map:
            i.start()           
               
    def handleEvent(self, e):   
        if e.getName() == "narrow_cast" :
            self.handleNarrowCastEvent(e.getParameters())
            
        elif e.getName() == "broad_cast" :
            self.handleBroadCastEvent(e.getParameters())
            
        elif e.getName() == "create_instance" :
            self.handleCreateEvent(e.getParameters())
            
        elif e.getName() == "associate_instance" :
            self.handleAssociateEvent(e.getParameters())
            
        elif e.getName() == "start_instance" :
            self.handleStartInstanceEvent(e.getParameters())
            
    def processAssociationReference(self, input_string):
        if len(input_string) == 0 :
            raise AssociationReferenceException("Empty association reference.")
        regex_pattern = re.compile("^([a-zA-Z_]\w*)(?:\[(\d+)\])?$");
        path_string =  input_string.split("/")
        result = []
        for piece in path_string :
            match = regex_pattern.match(piece)
            if match :
                name = match.group(1)
                index = match.group(2)
                if index is None :
                    index = -1
                result.append((name,index))
            else :
                raise AssociationReferenceException("Invalid entry in association reference.")
        return result
    
    def handleStartInstanceEvent(self, parameters):
        if len(parameters) != 2 :
            raise ParameterException ("The start instance event needs 2 parameters.")  
        else :
            source = parameters[0]
            traversal_list = self.processAssociationReference(parameters[1])
            for i in self.getInstances(source, traversal_list) :
                i.instance.start()
        
    def handleBroadCastEvent(self, parameters):
        if len(parameters) != 1 :
            raise ParameterException ("The broadcast event needs 1 parameter.")
        self.broadcast(parameters[0])

    def handleCreateEvent(self, parameters):
        if len(parameters) < 2 :
            raise ParameterException ("The create event needs at least 2 parameters.")
        else :
            source = parameters[0]
            association_name = parameters[1]
            
            association = self.instances_map[source].getAssociation(association_name)
            if association.allowedToAdd() :
                new_instance_wrapper = self.createInstance(association.class_name, parameters[2:])
                association.addInstance(new_instance_wrapper)
                source.addEvent(Event("instance_created", parameters = [association_name]))
            else :
                source.addEvent(Event("instance_creation_error", parameters = [association_name]))
                
    def handleAssociateEvent(self, parameters):
        if len(parameters) != 3 :
            raise ParameterException ("The associate_instance event needs 3 parameters.");
        else :
            source = parameters[0]
            to_copy_list = self.getInstances(source,self.processAssociationReference(parameters[1]))
            if len(to_copy_list) != 1 :
                raise AssociationReferenceException ("Invalid source association reference.")
            wrapped_to_copy_instance = to_copy_list[0]
            dest_list = self.processAssociationReference(parameters[2])
            if len(dest_list) == 0 :
                raise AssociationReferenceException ("Invalid destination association reference.")
            last = dest_list.pop()
            if last[1] != -1 :
                raise AssociationReferenceException ("Last association name in association reference should not be accompanied by an index.")
                
            for i in self.getInstances(source, dest_list) :
                i.getAssociation(last[0]).addInstance(wrapped_to_copy_instance)
        
    def handleNarrowCastEvent(self, parameters):
        if len(parameters) != 3 :
            raise ParameterException ("The associate_instance event needs 3 parameters.")
        source = parameters[0]
        traversal_list = self.processAssociationReference(parameters[1])
        cast_event = parameters[2]
        for i in self.getInstances(source, traversal_list) :
            i.instance.addEvent(cast_event)
        
    def getInstances(self, source, traversal_list):
        currents = [self.instances_map[source]]
        for (name, index) in traversal_list :
            nexts = []
            for current in currents :
                association = current.getAssociation(name)
                if (index >= 0 ) :
                    nexts.append ( association.getInstance(index) );
                elif (index == -1) :
                    nexts.extend ( association.instances );
                else :
                    raise AssociationReferenceException("Incorrect index in association reference.")
            currents = nexts
        return currents
            
    @abc.abstractmethod
    def instantiate(self, class_name, construct_params):
        pass
        
    def createInstance(self, class_name, construct_params = []):
        instance_wrapper = self.instantiate(class_name, construct_params)
        if instance_wrapper:
            self.instances_map[instance_wrapper.getInstance()] = instance_wrapper
        return instance_wrapper
    
class Event(object):
    def __init__(self, event_name, port = "", parameters = []):
        self.name = event_name
        self.parameters = parameters
        self.port = port

    def getName(self):
        return self.name

    def getPort(self):
        return self.port

    def getParameters(self):
        return self.parameters
    
    def __repr__(self):
        representation = "(event name : " + str(self.name) + "; port : " + str(self.port)
        if self.parameters :
            representation += "; parameters : " + str(self.parameters)
        representation += ")"
        return representation
    
class OutputListener(object):
    def __init__(self, port_names):
        self.port_names = port_names
        self.queue = Queue()

    def add(self, event):
        if len(self.port_names) == 0 or event.getPort() in self.port_names :
            self.queue.put_nowait(event)
            
    """ Tries for timeout seconds to fetch an event, returns None if failed.
        0 as timeout means no blocking.
        -1 as timeout means blocking until an event can be fetched. """
    def fetch(self, timeout = 0):     
        try :
            if timeout == 0 :
                return self.queue.get(False)
            elif timeout < 0 :
                return self.queue.get(True, None)
            else :
                return self.queue.get(True, timeout)
        except Empty:
            return None
        
class ControllerBase(object):

    def __init__(self, object_manager, keep_running):
        self.object_manager = object_manager
        self.keep_running = keep_running

        # Keep track of input ports
        self.input_ports = []
        self.input_queue = EventQueue();

        # Keep track of output ports
        self.output_ports = []
        self.output_listeners = []

        # Let statechart run one last time before stopping
        self.done = False
            
    def addInputPort(self, port_name):
        self.input_ports.append(port_name)
        
    def addOutputPort(self, port_name):
        self.output_ports.append(port_name)

    def broadcast(self, new_event):
        self.object_manager.broadcast(new_event)
        
    def start(self):
        self.object_manager.start()
    
    def stop(self):
        pass
    
    def addInput(self, input_event, time_offset = 0.0):
        if input_event.getName() == ""  :
            raise InputException("Input event can't have an empty name.")
        
        if input_event.getPort() not in self.input_ports :
            raise InputException("Input port mismatch.")
        
        self.input_queue.add(input_event, time_offset)

    def outputEvent(self, event):
        for listener in self.output_listeners :
            listener.add(event)
        
    def addOutputListener(self, ports):
        listener = OutputListener(ports)
        self.output_listeners.append(listener)
        return listener
    
    def addEventList(self, event_list):
        for (event, time_offset) in event_list :
            self.addInput(event, time_offset)
            
    def getObjectManager(self):
        return self.object_manager;
        
class GameLoopControllerBase(ControllerBase):
    def __init__(self, object_manager, keep_running):
        super(GameLoopControllerBase, self).__init__(object_manager, keep_running)
        
    def update(self, delta):
        self.input_queue.decreaseTime(delta)
        for event in self.input_queue.popDueEvents() :
            self.broadcast(event)
        self.object_manager.stepAll(delta)
        
class ThreadsControllerBase(ControllerBase):
    def __init__(self, object_manager, keep_running):
        super(ThreadsControllerBase, self).__init__(object_manager, keep_running)
        self.input_condition = threading.Condition()
        self.stop_thread = False
        self.thread = threading.Thread(target=self.run)
        
    def handleInput(self, delta):
        self.input_condition.acquire()
        self.input_queue.decreaseTime(delta)
        for event in self.input_queue.popDueEvents():
            self.broadcast(event)
        self.input_condition.release()   
        
    def start(self):
        self.thread.start()
        
    def stop(self):
        self.input_condition.acquire()
        self.stop_thread = True
        self.input_condition.notifyAll()
        self.input_condition.release()
        
    def getWaitTime(self):
        """Compute time untill earliest next event"""
        self.input_condition.acquire()
        wait_time = min(self.object_manager.getWaitTime(), self.input_queue.getEarliestTime())
        self.input_condition.release()

        if wait_time == INFINITY :
            if self.done :
                self.done = False
            else :
                self.done = True
                return 0.0
        return wait_time

    def handleWaiting(self):
        wait_time = self.getWaitTime()
        if(wait_time <= 0.0):
            return
        
        self.input_condition.acquire()
        if wait_time == INFINITY :
            if self.keep_running :
                self.input_condition.wait() #Wait for a signals
            else :
                self.stop_thread = True
        
        elif wait_time != 0.0 :
            actual_wait_time = wait_time - (time.time() - self.last_recorded_time)
            if actual_wait_time > 0.0 :
                self.input_condition.wait(actual_wait_time)    
        self.input_condition.release()

    def run(self):
        self.last_recorded_time  = time.time()
        super(ThreadsControllerBase, self).start()
        last_iteration_time = 0.0
        
        while True:
            self.handleInput(last_iteration_time)
            #Compute the new state based on internal events
            self.object_manager.stepAll(last_iteration_time)
            
            self.handleWaiting()
            
            self.input_condition.acquire()
            if self.stop_thread : 
                break
            self.input_condition.release()
            
            previous_recorded_time = self.last_recorded_time
            self.last_recorded_time = time.time()
            last_iteration_time = self.last_recorded_time - previous_recorded_time
        
    def join(self):
        self.thread.join()

    def addInput(self, input_event, time_offset = 0.0):
        self.input_condition.acquire()
        super(ThreadsControllerBase, self).addInput(input_event, time_offset)
        self.input_condition.notifyAll()
        self.input_condition.release()

    def addEventList(self, event_list):
        self.input_condition.acquire()
        super(ThreadsControllerBase, self).addEventList(event_list)
        self.input_condition.release()

class RuntimeClassBase(object):
    __metaclass__  = abc.ABCMeta
    
    def __init__(self):
        self.active = False
        self.state_changed = False
        self.events = EventQueue();
        self.timers = None;
        
    def addEvent(self, event, time_offset = 0.0):
        self.events.add(event, time_offset);
        
    def getEarliestEventTime(self) :
        if self.timers :
            return min(self.events.getEarliestTime(), min(self.timers.itervalues()))
        return self.events.getEarliestTime();

    def step(self, delta):
        if not self.active :
            return
        
        self.events.decreaseTime(delta);
        
        if self.timers :
            next_timers = {}
            for (key,value) in self.timers.iteritems() :
                time = value - delta
                if time <= 0.0 :
                    self.addEvent( Event("_" + str(key) + "after"), time);
                else :
                    next_timers[key] = time
            self.timers = next_timers;

        self.microstep()
        while (self.state_changed) :
            self.microstep()
            
    def microstep(self):
        due = self.events.popDueEvents()
        if (len(due) == 0) :
            self.transition()
        else :
            for event in due :
                self.transition(event);
    
    @abc.abstractmethod
    def transition(self, event = None):
        pass
    
    def start(self):
        self.active = True
        
        