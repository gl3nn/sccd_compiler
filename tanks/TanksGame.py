import sys
import time
import Tkinter as tk
from TanksField import TanksField


class TanksGame(tk.Tk):
    def __init__(self):
        tk.Tk.__init__(self)
        self.resizable(width="NO", height="NO")
        self.title("Paper Warfare")
        
        #GUI
        self.setupGUI()

        #Other entities
        self.field = TanksField(self.canvas,nrtanks=1)
        self.player = self.field.player
        self.player_gui_listener = self.player.addListener(["gui"])
        
        #lift overlay
        self.canvas.tag_raise(self.overlay)
                
        #initial event binding
        self.bind("p", self.pausePressed)
        
        #engine
        self.fixed_update_time = 20 #ms
        self.is_paused = True
        
        self.elapsed = 0.0
        
        self.game_running = True 
        
        #these two don't need to be set if we start in the paused state
        #self.schedule_time = dt.datetime.now()
        #self.after(self.fixed_update_time, self.update)
        
    """GUI Related"""
        
    def setupGUI(self):
        self.protocol("WM_DELETE_WINDOW", self.window_close)
        self.sidebar = tk.Frame(self)
        self.sidebar.pack(side=tk.LEFT, fill=tk.Y)
        w = tk.Label(self.sidebar, text="Menu", relief=tk.GROOVE, width=12)
        w.pack(side=tk.TOP, padx = 2, pady=2)
        
        self.keyBoard = []
        self.keyBoardVar = tk.IntVar()
        modes = [("Azerty", 0, True),("Qwerty", 1, False)]
        for text, mode, active in modes:
            b = tk.Radiobutton(self.sidebar, text=text, value=mode, variable=self.keyBoardVar, command=self.keyBoardChanged)
            if active : b.select()
            b.pack(anchor=tk.W, side=tk.TOP)
        self.keyBoardChanged()
        
        w = tk.Label(self.sidebar, text="State", relief=tk.GROOVE, width=12)
        w.pack(side=tk.TOP, padx = 2, pady=2)
        self.reloadState = tk.StringVar()
        w = tk.Label(self.sidebar, textvariable=self.reloadState, width=12)
        w.pack(side=tk.TOP, padx = 2, pady=2)
        self.setLoaded()    
        
        #set up canvas, this is the actual game area
        canvas_width = 1000
        canvas_height = 600
        self.canvas = tk.Canvas(master=self,
                    takefocus=1,
                    width=canvas_width, height=canvas_height,
                    background="white")
        self.canvas.pack(fill=tk.BOTH, expand=1)
        self.canvas.focus_set()
        self.background = tk.PhotoImage(file="./paper2.gif")
        self.canvas.create_image(0, 0, image=self.background, anchor='nw')
        
        self.overlay = self.canvas.create_text(
           canvas_width/2, 
           canvas_height/2,
           text="Press P to Play/Pause\n\nSteering :\nAzerty = Z+S+Q+D\nQwerty = W+S+A+D\n\nCannon :\nRotate = Left + Right Arrow\nShoot = Up Arrow", 
           font=("Helvetica", 18, "bold"), fill ="Red", justify=tk.CENTER)
        
    def hideOverlay(self):
        self.canvas.itemconfig(self.overlay,text="")
        
    def showPauseOverlay(self):
        self.canvas.itemconfig(self.overlay,text="Press P to Resume") 
        
    def showEndOverlay(self):
        self.canvas.itemconfig(self.overlay,text="Game Over") 
        
    def keyBoardChanged(self):
        if self.keyBoardVar.get() == 1 : self.keyBoard = ['w','s','a','d'] #qwerty
        else : self.keyBoard = ['z','s','q','d'] #azerty
        
    def setLoaded(self):
        self.reloadState.set("Ready")
        
    def setReloading(self):
        self.reloadState.set("Reloading...")
        
    """Event handling"""
    
    def keyPressed(self, event):
        event_name = None
        if (event.char == self.keyBoard[0]):
            event_name = "up-pressed"
        elif (event.char== self.keyBoard[1]):
            event_name = "down-pressed"
        elif (event.char == self.keyBoard[2]):
            event_name = "left-pressed"
        elif (event.char == self.keyBoard[3]):
            event_name = "right-pressed"
        elif (event.keysym == "Up"):
            event_name = "shoot-pressed"
        elif (event.keysym == "Left"):
            event_name = "cannon-left-pressed"
        elif (event.keysym == "Right"):
            event_name = "cannon-right-pressed"
        elif (event.char == 'p') :
            self.pausePressed(event)
            
        if self.player and event_name :    
            self.player.event(event_name, "input")
            
    def keyReleased(self, event):
        event_name = None
        if (event.char == self.keyBoard[0]):
            event_name = "up-released"
        elif (event.char== self.keyBoard[1]):
            event_name = "down-released"
        elif (event.char == self.keyBoard[2]):
            event_name = "left-released"
        elif (event.char == self.keyBoard[3]):
            event_name = "right-released"
        elif (event.keysym == "Up"):
            event_name = "shoot-released"
        elif (event.keysym == "Left"):
            event_name = "cannon-left-released"
        elif (event.keysym == "Right"):
            event_name = "cannon-right-released"
            
        if self.player and event_name :    
            self.player.event(event_name, "input")
        
    """Engine related"""    
    
    def update(self):
        self.schedule_time = time.time()
        self.scheduled_update_id = self.after(self.fixed_update_time, self.update)
        
        #update child elements
        self.field.update(self.fixed_update_time/1000.0)
        
        #GUI update
        while(True) :
            fetched = self.player_gui_listener.fetch()
            if fetched is None :
                break
            else :
                if fetched.getName() == "reloading" :
                    self.setReloading()
                elif fetched.getName() == "loaded" :
                    self.setLoaded()
                    
        if self.game_running == True :
            if not self.field.level_running :
                self.showEndOverlay()
                self.game_running = False
        
    def pausePressed(self, event):
        if self.is_paused :
            self.scheduled_update_id = self.after(self.fixed_update_time-(int(self.elapsed)*1000), self.update)
            self.hideOverlay()
            self.is_paused = False
            self.bind("<KeyPress>", self.keyPressed)
            self.bind("<KeyRelease>", self.keyReleased)
            self.unbind("p")
        else :
            self.elapsed = time.time() - self.schedule_time
            self.after_cancel(self.scheduled_update_id)
            self.showOverlay()
            self.is_paused = True
            self.bind("p", self.pausePressed)
            self.unbind("<KeyPress>")
            self.unbind("<KeyRelease>")
        
    def window_close(self):
        #destroy code?
        sys.exit(0)

if __name__=="__main__":
    game = TanksGame()
    game.mainloop()
    


        


