from Tank import Tank
import math

D1 = math.pi / 180
D45 = math.pi/4
D90 = math.pi/2
D360 = 2 * math.pi

"""
		
	def stop(self):
		self.statechart.event('stop')
		self.statechart.event('stopTurn')


"""		
		
class AITank(Tank):
	def __init__(self, field, data):			
		Tank.__init__(self, field, data)

		self.points = []
		self.destination = None
		self.margin = 0.2
		self.cannonMargin = self.cannonSpeed * D1
		self.maxMoveSpeed = self.moveSpeed
		self.angleToTarget = 0
		self.angleToGoal = 0
		
		
		
	def angleToDest(self, (dest_x,dest_y)):
		return math.atan2(self.tank.y- dest_y, dest_x-self.tank.x) % D360
			
	#------Strategy------#
	
	
	def newExplore(self):
		self.destination = self.fieldMap.getExplore(self.cell, self.angle)
		self.controller.newDestination()

	
	def focusEnemy(self):
		self.destination = self.enemyPos
		self.controller.newDestination()
	
	
		
	#-----Aiming-------#
	
	
	def pointRightCannon(self) :
		diff = (self.cannonAngle - self.angleToTarget) % D360
		if diff >= self.cannonMargin and diff <= math.pi:
			return 1
		return 0
		
	def pointLeftCannon(self) :
		diff = (self.angleToTarget - self.cannonAngle) % D360
		if diff >= self.cannonMargin and diff <= math.pi:
			return 1
		return 0
		
	def cannonCorrect(self):
		diff = math.fabs(self.cannonAngle - self.angleToTarget)
		if diff < self.cannonMargin or diff > (D360- self.cannonMargin) :
			return 1
		return 0 
		
	def cannonReset(self):
		self.angleToTarget = self.angle
		if self.cannonCorrect() :
			return True
		return False
	
