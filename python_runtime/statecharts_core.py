import abc
import time
import threading
from infinity import INFINITY
from Queue import Queue, Empty

class Object(object):
    def __init__(self, class_name, created_from = None):
        self.associations = {} #class_name to Association
        self.class_name = class_name
        self.created_from = created_from
        self.reference= None

    def get(self):
        return self.reference

    def getCreator(self):
        return self.created_from

    def getClass(self):
        return self.class_name

    def getAssociation(self, class_name):
        if(class_name in self.associations):
            return self.associations[class_name]
        else :
            return None

class ObjectManagerBase(object):
    __metaclass__  = abc.ABCMeta
    
    def __init__(self, controller):
        self.eventQueue = []
        self.all_objects = {}
        self.currentTime = 0.0
        self.associations_info = {}
        self.controller = controller

    def event(self, newEvent):
        self.eventQueue.append(newEvent)
        
    # Broadcast an event to all instances
    def broadcast(self, newEvent):
        assert newEvent.getTime() >= self.currentTime
        for i in self.all_objects:
            i.event(newEvent)

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
        for o in self.all_objects :
            t = o.getEarliestEvent()
            if t is not None and t not in wait_times:
                wait_times.append(t)
        return wait_times
    
    def stepAll(self, global_time):
        for o in self.all_objects:
            o.step(global_time)
        self.step(global_time)

    def step(self, currentTime):
        self.currentTime = currentTime

        while self.eventQueue:
            current = self.eventQueue.pop(0)
            if current.getName() == "createInstance" :
                self.createInstance(current.getParameters())

    def createInstance(self, parameters):
        if len(parameters) != 2 :
            error_message = "Wrong number of parameters for the createInstance event."
        else :
            class_name = parameters[0]
            from_reference = parameters[1]

            if from_reference in self.all_objects :
                association = self.all_objects[from_reference].getAssociation(class_name)
                if association != None:
                    if association.allowedToAdd() :
                        new_object = Object(class_name, from_reference)
                        self.setupObject(new_object)
                        new_reference = new_object.get()
                        self.all_objects[new_reference] = new_object
                        association.add(new_reference)
                        from_reference.event(Event("createdInstance", time = self.currentTime, parameters = [True, class_name, new_reference]))
                        return
                    else :
                        error_message = "Not allowed to add a new instance of this class"
                else :
                    error_message = "Class is not an association according to class diagram"
            else :
                error_message = "Passed object reference is unknown"
        print error_message
        from_reference.event(Event("createdInstance",  time = self.currentTime, parameters = [False, class_name, error_message]))
    
    @abc.abstractmethod
    def setupObject(self, new_object):
        pass

    def deleteInstance(self, parameters):
        if len(parameters) != 1 :
            error_message = "Wrong number of parameters for the deleteInstance event."
        else :
            to_delete = parameters[0]
            if to_delete in self.all_objects :
                instance = self.all_objects[to_delete]
                creator = instance.getCreator()
                if creator != None:
                    association = creator.getAssociation(instance.getClassName())
                    association.remove(to_delete)
                self.all_objects.pop(instance, None) #FIXME 
            else :
                error_message = "Passed object reference is unknown"

        print error_message;
        
    def createDefaultInstance(self, class_name):
        new_object = Object(class_name)        
        self.setupObject(new_object)
        new_reference = new_object.get()
        self.all_objects[new_reference] = new_object
        
class AssociationInfo(object):
    def __init__(self, class_name, min_card, max_card):
        self.min = min_card
        self.max = max_card
        self.class_name = class_name

class Association(object):

    def __init__(self, association_info):
        self.references = []
        self.info = association_info

    def getClassName(self):
        return self.info.class_name

    def allowedToAdd(self):
        return len(self.references) < self.info.max

    def add(self, reference):
        self.references.append(reference)

    def remove(self, reference):
        self.references = [x for x in self.references if x != reference]

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
    
    def setTime(self, time):
        self.time = time

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
    def fetch(self, timeout = -1):     
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

    def __init__(self, object_manager, friend, keep_running, loopMax):
        self.object_manager = object_manager
        self.keep_running = keep_running
        self.friend = friend #reference accessible inside class diagram through FRIEND

        # Keep local track of global time
        self.globalTime = 0.0

        # Maximum loops allowed before run time error/aborting
        self.loopMax = loopMax

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

    def broadcast(self, newEvent):
        self.object_manager.broadcast(newEvent)
        
    def start(self):
        pass
    
    def stop(self):
        pass

    def outputEvent(self, event):
        for listener in self.output_listeners :
            listener.add(event)

        
    def addOutputListener(self, listener):
        self.output_listeners.append(listener)
        
class GameLoopControllerBase(ControllerBase):
    def __init__(self, object_manager, friend, keep_running, loopMax):
        ControllerBase.__init__(object_manager, friend, keep_running, loopMax)
        
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
    def __init__(self, object_manager, friend, keep_running, loopMax):
        super(ThreadsControllerBase, self).__init__(object_manager, friend, keep_running, loopMax)
        self.inputCondition = threading.Condition()
        self.outputCondition = threading.Condition()
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
        