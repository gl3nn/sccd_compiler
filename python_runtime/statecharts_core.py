import abc
import time
import threading
from infinity import INFINITY
from Queue import Queue, Empty

class ObjectManagerBase(object):
    __metaclass__  = abc.ABCMeta
    
    def __init__(self, controller):
        self.eventQueue = []
        self.currentTime = 0.0
        self.controller = controller
        self.class_names = []
        self.all_instances = []

    def event(self, newEvent):
        self.eventQueue.append(newEvent)
        
    # Broadcast an event to all instances
    def broadcast(self, new_event):
        assert new_event.getTime() >= self.currentTime
        for i in self.all_instances:
            i.event(new_event)

    def getEarliestEvent(self):
        if self.eventQueue:
            return self.eventQueue[0].getTime()
        else:
            return None
        
    def getWaitTimes(self):
        wait_times = []
        #first get waiting time of the object manager which acts as statechart too
        t = self.getEarliestEvent()
        if t : wait_times.append(t)
        #check all the instances
        for o in self.all_instances :
            t = o.getEarliestEvent()
            if t is not None and t not in wait_times:
                wait_times.append(t)
        return wait_times
    
    def stepAll(self, global_time):
        for o in self.all_instances:
            o.step(global_time)
        self.step(global_time)

    def step(self, currentTime):
        self.currentTime = currentTime

        while self.eventQueue:
            current = self.eventQueue.pop(0)
            if current.getName() == "create_instance" :
                self.createInstance(current.getParameters())

    def createInstance(self, parameters):
        if len(parameters) != 2 :
            error_message = "Wrong number of parameters for the create_instance event."
        else :
            class_name = parameters[0]
            from_reference = parameters[1]
            new_reference = self.instantiate(class_name)

            if not new_reference :
                if class_name not in self.class_names :
                    error_message = "Provided class name is not part of the class diagram."
                else :
                    error_message = "Unexpected error occured during creation."
                print error_message
                from_reference.event(Event("creation_error",  time = self.currentTime, parameters = [error_message, class_name]))
            else :
                self.all_instances.append(new_reference)
                from_reference.event(Event("created_instance", time = self.currentTime, parameters = [new_reference, class_name]))

        
    
    @abc.abstractmethod
    def instantiate(self, class_name):
        pass
        
    def createDefaultInstance(self, class_name):  
        self.all_instances.append(self.instantiate(class_name))

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
    
    """def decTime(self, delta):
        self.time -= delta"""

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

    def __init__(self, object_manager, friend, keep_running):
        self.object_manager = object_manager
        self.keep_running = keep_running
        self.friend = friend #reference accessible inside class diagram through FRIEND

        # Keep local track of global time
        self.globalTime = 0.0

        # Keep track of input ports
        self.input_ports = []
        self.inputQueue = []

        # Keep track of output ports
        self.output_ports = []
        self.output_listeners = []

        # Let statechart run one last time before stopping
        self.done = False
            
    def addInputPort(self, port_name):
        self.input_ports.append(port_name)
        
    def addOutputPort(self, port_name):
        self.output_ports.append(port_name)

    def broadcast(self, newEvent):
        self.object_manager.broadcast(newEvent)
        
    def start(self):
        pass
    
    def stop(self):
        pass
    
    def addInput(self, event_name, port, time = 0.0, parameters = []):
        self.inputQueue.append(Event(event_name, time + self.globalTime, port, parameters))

    def outputEvent(self, event):
        for listener in self.output_listeners :
            listener.add(event)

        
    def addOutputListener(self, listener):
        self.output_listeners.append(listener)
        
class GameLoopControllerBase(ControllerBase):
    def __init__(self, object_manager, friend, keep_running):
        ControllerBase.__init__(object_manager, friend, keep_running)
        
    def update(self, delta):
        self.globalTime += delta
        if self.inputQueue :
            next_input_queue = []
            for event in self.inputQueue :
                if event.getTime() <= self.globalTime :
                    self.broadcast(event)
                else :
                    next_input_queue.append(event)
            self.inputQueue = next_input_queue
        self.object_manager.stepAll()
        
class ThreadsControllerBase(ControllerBase):
    def __init__(self, object_manager, friend, keep_running):
        super(ThreadsControllerBase, self).__init__(object_manager, friend, keep_running)
        self.inputCondition = threading.Condition()
        self.stop_thread = False
        self.run_semaphore = threading.Semaphore()
        self.thread = threading.Thread(target=self.run)
        
    def handleInput(self):
        self.inputCondition.acquire()
        if self.inputQueue :
            next_input_queue = []
            for event in self.inputQueue :
                if event.getTime() <= self.globalTime :
                    self.broadcast(event)
                else :
                    next_input_queue.append(event)
                    
            self.inputQueue = next_input_queue
        self.inputCondition.release()   
        
            # Compute time untill earliest next event
    def getNextTime(self):
        #fetch the statecharts waiting times
        waitTimes = self.object_manager.getWaitTimes()
                
        #fetch input waiting time
        self.inputCondition.acquire()
        if len(self.inputQueue) > 0 :
            for event in self.inputQueue :
                waitTimes.append(event.getTime())
        self.inputCondition.release()

        if waitTimes:
            waitTimes.sort()
            return waitTimes[0] - self.globalTime
        elif not self.done:
            self.done = True
            return 0
        elif self.done:
            self.done = False
            return INFINITY

    def handleWaiting(self):
        timeout = self.getNextTime()
        if(timeout <= 0):
            return 0
        self.inputCondition.acquire()
        begin_time = time.time()
        if timeout == INFINITY :
            if self.keep_running :
                self.inputCondition.wait()
            else :
                self.inputCondition.release()
                self.stop()
                return 0
        else :
            self.inputCondition.wait(timeout)    
        timeout = min(timeout, time.time() - begin_time)
        self.inputCondition.release()
        return timeout

    def run(self):
        while True:
            self.handleInput()
            # Compute the new state based on internal events
            self.object_manager.stepAll(self.globalTime)
            self.globalTime += self.handleWaiting()
            
            self.inputCondition.acquire()
            if self.stop_thread : 
                break
            self.inputCondition.release()

    def start(self):
        self.thread.start()

    def stop(self):
        self.inputCondition.acquire()
        self.stop_thread = True
        self.inputCondition.notifyAll()
        self.inputCondition.release()
        
    def join(self, timeout = None):
        self.thread.join(timeout)

    def addInput(self, event_name, port, time = 0.0, parameters = []):
        self.inputCondition.acquire()
        self.inputQueue.append(Event(event_name, time + self.globalTime, port, parameters))
        self.inputCondition.notifyAll()
        self.inputCondition.release()

    def addAbsoluteEventList(self, event_list):
        self.inputCondition.acquire()
        for event in event_list :
            self.inputQueue.append(event)
        self.inputCondition.release()
        