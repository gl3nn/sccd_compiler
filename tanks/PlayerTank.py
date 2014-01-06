from Tank import Tank
from player_controller import Controller

class PlayerTank(Tank):
	def __init__(self, field, data):		
		Tank.__init__(self, field, data)
		self.controller = Controller(self)
		self.controller.start()
		
	def destroy(self):
		Tank.destroy(self)
				
	def addListener(self, ports):
		return self.controller.addOutputListener(ports)
								
	def event(self, event_name, port, time = 0.0, parameters = []):
		self.controller.addInput(event_name, port, time, parameters)
			
	def update(self, delta):
		self.controller.addInput("update","engine")
		self.controller.update(delta)
		
		