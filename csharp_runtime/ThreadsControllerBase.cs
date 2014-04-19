using System;
using System.Threading;
using System.Collections.Generic;

namespace sccdlib
{
    public class ThreadsControllerBase : ControllerBase
    {
        bool stop_thread = false;
        bool keep_running;
        Thread thread = null;
        Mutex input_mutex = new Mutex(false);
        Mutex stop_thread_mutex = new Mutex(false);
        EventWaitHandle wait_handle = new AutoResetEvent(false);
        DateTime last_recorded_time = DateTime.UtcNow;
        
        public ThreadsControllerBase (bool keep_running = true)
            : base()
        {          
            this.keep_running = keep_running;
            this.thread = new Thread (new ThreadStart (this.run));
        }
        
        private void handleInput(double delta)
        {
            this.input_mutex.WaitOne(-1);
            this.input_queue.decreaseTime(delta);
            foreach(Event e in this.input_queue.popDueEvents())
                this.broadcast (e);
            this.input_mutex.ReleaseMutex();
        }
        
        public override void start()
        {
            this.thread.Start ();
        }
    
        public override void stop()
        {
            this.stop_thread_mutex.WaitOne(-1);
            this.stop_thread = true;
            this.stop_thread_mutex.ReleaseMutex ();
            this.wait_handle.Set();
        }
        
        
        private double getWaitTime ()
        {
            this.input_mutex.WaitOne (-1);
            double wait_time = Math.Min (this.object_manager.getWaitTime (), this.input_queue.getEarliestTime ());
            this.input_mutex.ReleaseMutex ();
    
            if (double.IsPositiveInfinity (wait_time)) {
                if (this.done) {
                    this.done = false;
                } else {
                    this.done = true;
                    return 0.0;
                }
            }
            return wait_time;
        }
    
        private void handleWaiting ()
        {
            double wait_time = this.getWaitTime ();
            if (wait_time <= 0.0)
                return;
            if (double.IsPositiveInfinity(wait_time)) {
                if (this.keep_running) {
                    this.wait_handle.WaitOne (-1); //Wait until signal
                } else {
                    this.stop_thread_mutex.WaitOne(-1);
                    this.stop_thread = true;
                    this.stop_thread_mutex.ReleaseMutex ();
                }
            } else {
                //Calculate how much wait time is left.
                TimeSpan actual_wait_time = TimeSpan.FromMilliseconds(wait_time) - DateTime.UtcNow.Subtract(this.last_recorded_time);
                if (actual_wait_time > TimeSpan.Zero)
                    this.wait_handle.WaitOne (actual_wait_time);
            }
        }
    
        private void run()
        {
            base.start ();
            DateTime previous_recorded_time;
            double last_iteration_time = 0.0;
            this.last_recorded_time = DateTime.UtcNow;
            
            while (true)
            {
                this.handleInput(last_iteration_time);
                //Compute the new state based on internal events
                this.object_manager.stepAll(last_iteration_time);
                
                this.handleWaiting();
                
                this.stop_thread_mutex.WaitOne (-1);
                if (this.stop_thread) 
                    break;
                this.stop_thread_mutex.ReleaseMutex ();
                
                previous_recorded_time = last_recorded_time;
                this.last_recorded_time = DateTime.UtcNow;
                last_iteration_time = this.last_recorded_time.Subtract(previous_recorded_time).TotalMilliseconds;
            }
        }
    
        public void join()
        {
            this.thread.Join ();
        }
    
        public override void addInput(Event input_event, double time_offset = 0.0)
        {
            this.input_mutex.WaitOne (-1);
            base.addInput (input_event, time_offset);
            this.input_mutex.ReleaseMutex ();
        }
    
        public override void addEventList(List<Tuple<Event,double>> event_list)
        {
            this.input_mutex.WaitOne (-1);
            base.addEventList (event_list);
            this.input_mutex.ReleaseMutex ();
        }
        
        protected override IOutputListener createOutputListener (string[] ports)
        {
            return new ConcurrentOutputListener(ports);   
        }
    }
}

