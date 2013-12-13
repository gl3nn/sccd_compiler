import abc
import time
import threading
from infinity import INFINITY
from Queue import Queue, Empty

class ObjectManagerBase(object):
    __metaclass__  = abc.ABCMeta
    
    def __init__(self, controller):
        self.event_queue = []
        self.controller = controller
        self.class_names = []
        self.all_instances = []

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
        self.step(delta)
        for i in self.all_instances:
            i.step(delta)

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
            self.handleCreateInstanceEvent(e.getParameters())

    def handleCreateInstanceEvent(self, parameters):
        if len(parameters) != 2 :
            error_message = "Wrong number of parameters for the create_instance event."
        else :
            class_name = parameters[0]
            from_reference = parameters[1]
            new_reference = self.createInstance(class_name)
            if not new_reference :
                if class_name not in self.class_names :
                    error_message = "Provided class name is not part of the class diagram."
                else :
                    error_message = "Unexpected error occured during creation."
                print error_message
                from_reference.event(Event("creation_error", time = 0.0, parameters = [error_message, class_name]))
            else :
                from_reference.event(Event("created_instance", time = 0.0, parameters = [new_reference, class_name]))        
    
    @abc.abstractmethod
    def instantiate(self, class_name):
        pass
        
    def createInstance(self, class_name):
        instance = self.instantiate(class_name)
        if instance :
            self.all_instances.append(instance)
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
        