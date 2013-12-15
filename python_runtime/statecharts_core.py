import abc
import time
import threading
from infinity import INFINITY
from Queue import Queue, Empty

class Association(object):
    #wrapper object for one association relation
    def __init__(self, name, class_name, min_card, max_card):
        self.min_card = min_card
        self.max_card = max_card
        self.name = name
        self.class_name = class_name
        self.instances = [] #list of all instance wrappers
        
    def allowedToAdd(self):
        return self.max_card == -1 or len(self.instances) < self.max_card
        
    def add(self, instance):
        assert self.allowedToAdd()
        self.instances.append(instance)
        
    def get(self, index):
        if index >= 0 :
            return [self.instances[index]]
        elif index == -1 :
            return self.instances
        else :
            raise

class InstanceWrapper(object):
    #wrapper object for an instance and its relevant information needed in the object manager
    def __init__(self):
        self.instance = None
        self.associations = {}#maps association_name to list of InstanceAssociationInfo
        
    def addAssociation(self, name, class_name, min_card, max_card):
        self.associations[name] = Association(name, class_name, min_card, max_card)
        
    def getAssociation(self, association_name):
        return self.associations.get(association_name, None)
    
    def getInstance(self):
        return self.instance
            

class ObjectManagerBase(object):
    __metaclass__  = abc.ABCMeta
    
    def __init__(self, controller):
        self.event_queue = []
        self.controller = controller
        self.all_instances = {} #a dictionary that maps instance_reference to InstanceWrapper
        
        

    def event(self, new_event):
        self.event_queue.append(new_event)
        
    # Broadcast an event to all instances
    def broadcast(self, new_event):
        for i in self.all_instances:
            i.event(new_event)

    def getEarliestEvent(self):
        if self.event_queue:
            return self.event_queue[0].time
        else:
            return None
        
    def getWaitTimes(self):
        wait_times = []
        #first get waiting time of the object manager which acts as statechart too
        t = self.getEarliestEvent()
        if t : wait_times.append(t)
        #check all the instances
        for i in self.all_instances :
            t = i.getEarliestEvent()
            if t is not None and t not in wait_times:
                wait_times.append(t)
        return wait_times
    
    def stepAll(self, delta):
        print len(self.all_instances)
        for i in self.all_instances:
            i.step(delta)
        self.step(delta)

    def step(self, delta):
        if self.event_queue :
            next_queue = []
            for e in self.event_queue :
                e.decTime(delta)   
                if e.getTime() <= 0.0 :
                    self.handleEvent(e)
                else :
                    next_queue.append(e)
            self.event_queue = next_queue
               
    def handleEvent(self, e):
        if e.getName() == "create_instance" :
            self.handleCreateEvent(e.getParameters())
            
        elif e.getName() == "narrow_cast" :
            self.handleNarrowCastEvent(e.getParameters())

    def handleCreateEvent(self, parameters):
        if len(parameters) != 2 :
            print "Wrong number of parameters for the create_instance event."
        else :
            source = parameters[0]
            association_name = parameters[1]
            
            association = self.all_instances[source].getAssociation(association_name)
            if association.allowedToAdd() :
                new_instance_wrapper = self.createInstance(association.class_name)
                association.add(new_instance_wrapper)
                source.event(Event("instance_created", time = 0.0, parameters = [association_name]))
            else :
                source.event(Event("instance_creation_error", time = 0.0, parameters = [association_name]))
                print "Not allowed to add"
        
    def handleNarrowCastEvent(self, parameters):
        if len(parameters) != 3 :
            print  "Wrong number of parameters for the narrow_cast event."
        source = parameters[0]
        traversal_list = parameters[1]
        cast_event = parameters[2]
        for i in self.getInstances(source, traversal_list) :
            i.instance.event(cast_event)
        
    def getInstances(self, source, traversal_list):
        if not traversal_list :
            raise
        currents = [self.all_instances[source]]
        for (name, index) in traversal_list :
            nexts = []
            for current in currents :
                association = current.getAssociation(name)
                nexts.extend(association.get(index))
            currents = nexts
        return currents
            
    
    @abc.abstractmethod
    def instantiate(self, class_name):
        pass

        
    def createInstance(self, class_name):
        instance = self.instantiate(class_name)
        if instance :
            self.all_instances[instance.instance] = instance
        return instance

class Event(object):
    def __init__(self, event_name, time = 0.0, port = "", parameters = []):
        self.name = event_name
        self.time = time
        self.parameters = parameters
        self.port = port

    def getName(self):
        return self.name

    def getPort(self):
        return self.port

    def getTime(self):
        return self.time
    
    def decTime(self, delta):
        self.time -= delta

    def getParameters(self):
        return self.parameters
    
class OutputListener(object):
    def __init__(self, port_names):
        self.port_names = port_names
        self.queue = Queue()

    def add(self, event):
        if event.getPort() in self.port_names :
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
        self.input_queue = []

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
        pass
    
    def stop(self):
        pass
    
    def addInput(self, event_name, port, time = 0.0, parameters = []):
        self.input_queue.append(Event(event_name, time, port, parameters))

    def outputEvent(self, event):
        for listener in self.output_listeners :
            listener.add(event)
        
    def addOutputListener(self, listener):
        self.output_listeners.append(listener)
        
class GameLoopControllerBase(ControllerBase):
    def __init__(self, object_manager, keep_running):
        super(GameLoopControllerBase, self).__init__(object_manager, keep_running)
        
    def update(self, delta):
        if self.input_queue :
            next_input_queue = []
            for event in self.input_queue :
                event.decTime(delta)
                if event.getTime() <= 0 :
                    self.broadcast(event)
                else :
                    next_input_queue.append(event)
            self.input_queue = next_input_queue
        self.object_manager.stepAll(delta)
        
class ThreadsControllerBase(ControllerBase):
    def __init__(self, object_manager, keep_running):
        super(ThreadsControllerBase, self).__init__(object_manager, keep_running)
        self.input_condition = threading.Condition()
        self.stop_thread = False
        self.run_semaphore = threading.Semaphore()
        self.thread = threading.Thread(target=self.run)
        
    def handleInput(self, delta):
        self.input_condition.acquire()
        if self.input_queue :
            next_input_queue = []
            for event in self.input_queue :
                event.decTime(delta)
                if event.getTime() <= 0 :
                    self.broadcast(event)
                else :
                    next_input_queue.append(event)
                    
            self.input_queue = next_input_queue
        self.input_condition.release()   
        
    # Compute time untill earliest next event
    def getNextTime(self):
        #fetch the statecharts waiting times
        wait_times = self.object_manager.getWaitTimes()             
        #fetch input waiting time
        self.input_condition.acquire()
        if len(self.input_queue) > 0 :
            for event in self.input_queue :
                wait_times.append(event.getTime())
        self.input_condition.release()

        if wait_times:
            wait_times.sort()
            return wait_times[0]
        elif not self.done:
            self.done = True
            return 0
        elif self.done:
            self.done = False
            return INFINITY

    def handleWaiting(self):
        timeout = self.getNextTime()
        if(timeout <= 0.0):
            return 0
        self.input_condition.acquire()
        begin_time = time.time()
        if timeout == INFINITY :
            if self.keep_running :
                self.input_condition.wait()
            else :
                self.input_condition.release()
                self.stop()
                return 0
        else :
            self.input_condition.wait(timeout)    
        timeout = min(timeout, time.time() - begin_time)
        self.input_condition.release()
        return timeout

    def run(self):
        last_iteration_time = 0.0
        while True:
            self.handleInput(last_iteration_time)
            # Compute the new state based on internal events
            self.object_manager.stepAll(last_iteration_time)
            last_iteration_time = self.handleWaiting()
            
            self.input_condition.acquire()
            if self.stop_thread : 
                break
            self.input_condition.release()

    def start(self):
        self.thread.start()

    def stop(self):
        self.input_condition.acquire()
        self.stop_thread = True
        self.input_condition.notifyAll()
        self.input_condition.release()
        
    def join(self, timeout = None):
        self.thread.join(timeout)

    def addInput(self, event_name, port, time = 0.0, parameters = []):
        self.input_condition.acquire()
        self.input_queue.append(Event(event_name, time, port, parameters))
        self.input_condition.notifyAll()
        self.input_condition.release()

    def addEventList(self, event_list):
        self.input_condition.acquire()
        for event in event_list :
            self.input_queue.append(event)
        self.input_condition.release()
        