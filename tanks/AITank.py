from Tank import Tank
import math
from mymath import D360
from ai_controller import Controller

class AITank(Tank):
	def __init__(self, field, data):			
		Tank.__init__(self, field, data)
		self.controller = Controller(self)
		self.controller.start()		
			
	def update(self, delta):
		self.controller.addInput("update","engine")
		self.controller.update(delta)
		
	def angleToDest(self, (dest_x,dest_y)):
		return math.atan2(self.y- dest_y, dest_x-self.x) % D360
