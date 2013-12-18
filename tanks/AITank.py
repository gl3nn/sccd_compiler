from Tank import Tank
from random import choice
import heapq
import math

D1 = math.pi / 180
D15 = math.pi/8
D30 = math.pi/6
D45 = math.pi/4
D60 = math.pi/3
D90 = math.pi/2
D105 = D90 + D15
D360 = 2 * math.pi
D270 = D360 - D90

"""
	Old code that will be used as reference		
	#Interface for static part	
		
	def newDestination(self):
		self.statechart.event('newDestination')
		
	def enemyPosChanged(self):
		self.statechart.event('enemyPosChanged')
		
	def stop(self):
		self.statechart.event('stop')
		self.statechart.event('stopTurn')

	def turnRight(self):
		self.statechart.event('turnRight')
		
	#Interface for statechart
		
	def pointReached(self):
		return self.static.pointReached()
		
	def pointAhead(self):
		return self.static.pointAhead()
		
	def pointBehind(self):
		return self.static.pointBehind()
		
	def pointLeft(self):
		return self.static.pointLeft()
		
	def pointRight(self):
		return self.static.pointRight()
		
	def pointStraight(self):
		return self.static.pointStraight()
		
	def newExplore(self):
		self.static.newExplore()

	def calculatePath(self):
		self.static.calculatePath()

	def morePoints(self):
		return self.static.morePoints()
		
	def getReactionTime(self):
		return self.static.getReactionTime()
		
	def pointLeftCannon(self):
		return self.static.pointLeftCannon()
		
	def pointRightCannon(self):
		return self.static.pointRightCannon()
		
	def cannonCorrect(self):
		return self.static.cannonCorrect()
		
	def focusEnemy(self):
		self.static.focusEnemy()
		
	def enemyPresent(self):
		return self.static.enemyPresent()
		
	def checkEnemy(self):
		self.static.checkEnemy()
		
	def cannonReset(self):
		return self.static.cannonReset()
"""		
		
class AITank(Tank):
	def __init__(self, field, data):			
		Tank.__init__(self, field, data)
		self.reactionTime = 0.05
		self.fieldMap = Map(field.level)
		self.cell = self.fieldMap.calculateCell(self.x,self.y)
		self.points = []
		self.destination = None
		self.count = 0
		self.margin = 0.2
		self.cannonMargin = self.cannonSpeed * D1
		self.maxMoveSpeed = self.moveSpeed
		self.angleToTarget = 0
		self.angleToGoal = 0
		self.enemyPos = None
		self.enemyXY = None
	
		
	def getReactionTime(self):
		return self.reactionTime
		
	#-------Overloading------#
		
	def moveTankUp(self):
		Tank.moveTankUp(self)
		self.cell = self.fieldMap.calculateCell(self.x,self.y)
		
		
	#-------Radar-------------#
	
	def enemyPresent(self):
		sighted_list = self.field.getSightedEnemies(self.tank, self.range)
		if len(sighted_list) > 0 :
			return True
		return False
	
	def getEnemyPos(self):
		sighted_list = self.field.getSightedEnemies(self.tank, self.range)
		if len(sighted_list) > 0 :
			sighted_list.sort(key=lambda x: x[1])
			return sighted_list[0]
		else :
			return (0,0)
		
	def checkEnemy(self):
		enemyXY = self.fieldController.getClosestEnemy(self.controller)
		if enemyXY != None :
			self.enemyXY = enemyXY
			enemy = self.fieldMap.calculateCell(enemyXY[0],enemyXY[1])
			if enemy != self.enemyPos :
				self.enemyPos = enemy
				self.controller.enemyPosChanged()
			self.angleToTarget = self.fieldMap.getAngleToDest((self.x,self.y),self.enemyXY)
						
	#------Strategy------#
	
	
	def newExplore(self):
		self.destination = self.fieldMap.getExplore(self.cell, self.angle)
		self.controller.newDestination()

	
	def focusEnemy(self):
		self.destination = self.enemyPos
		self.controller.newDestination()
	
	#------PathFinding----#
	
	def calculatePath(self):
		self.controller.stop()

		class PriorityQueue:
			"""
			Built in AI Course
			"""  
			def  __init__(self):  
				self.heap = []

			def push(self, item, priority):
				pair = (priority,item)
				heapq.heappush(self.heap,pair)

			def pop(self):
				(priority,item) = heapq.heappop(self.heap)
				return item

			def isEmpty(self):
				return len(self.heap) == 0	
		#A*
		start = self.cell
		explored = {}
		fringe = PriorityQueue();
		startNode = (start, 0, 0) #(state,parent,cost to get to this state)
		fringe.push(startNode, startNode[2])
		while (True) :
			while (True) :
				currentNode = fringe.pop()
				if currentNode[0] not in explored : #same node on the fringe multiple times. we only want the lowest cost node expanded.
					break
			explored[currentNode[0]] = currentNode
			if currentNode[0] == self.destination :
				break
			successors = self.fieldMap.getSuccessors(currentNode[0])
			for (successor,cost) in successors :
				totalcost = currentNode[2] + cost			
				if successor not in explored : #nodes that are not expanded yet, but are already on the fringe will still be added.
					node = (successor, currentNode, totalcost)
					heuristic =  ((successor[0] - self.destination[0]) ** 2 + (successor[1] - self.destination[1]) ** 2 ) ** 0.5
					fringe.push(node,(totalcost+heuristic))	
					
		newpoints = []
		while ( currentNode[0] != start) :
			newpoints.insert(0, (currentNode[0],self.fieldMap.calculateCoords(currentNode[0])))
			currentNode = currentNode[1]
		self.points = newpoints
		
		
	def morePoints(self):
		return len(self.points) > 0
		
	#------Steering--------#
	
	def pointReached(self) :
		if len(self.points) == 0 : return False
		if self.cell == self.points[0][0] :
			self.points.pop(0)
			self.controller.stop()
			return True
		return False
		
	def calculateAngle(self) :
		self.angleToGoal = self.fieldMap.getAngleToDest((self.x,self.y),self.points[0][1])
		
	
	def pointAhead(self) :
		if len(self.points) == 0 : return False
		self.calculateAngle()
		diff = math.fabs(self.angle - self.angleToGoal)
		if diff <= (D45) :
			self.moveSpeed = int(math.ceil(((D45 - diff) / D45) * self.maxMoveSpeed))
			return 1
		if diff >= (D360 - D45) :
			self.moveSpeed = int(math.ceil(((diff - (D360 - D45)) / D45) * self.maxMoveSpeed))
			return 1
		return 0
		
	def pointBehind(self) :
		if len(self.points) == 0 : return True
		self.calculateAngle()
		diff = math.fabs(self.angle - self.angleToGoal)
		if diff < (D45) or diff > (D360 - D45) :
			return 0
		return 1
		
	def pointRight(self) :
		if len(self.points) == 0 : return False
		angle = self.angleToGoal
		diff = (self.angle - angle) % D360
		if diff >= self.margin and diff <= math.pi:
			return 1
		return 0
		
	def pointLeft(self) : 
		if len(self.points) == 0 : return False
		angle = self.angleToGoal
		diff = (angle - self.angle) % D360
		if diff >= self.margin and diff <= math.pi:
			return 1
		return 0
		
	def pointStraight(self):
		if len(self.points) == 0 : return True
		angle = self.angleToGoal
		diff = math.fabs(self.angle - angle)
		if diff < self.margin or diff > (D360- self.margin) :
			return 1
		return 0 
		
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
		
		
		
		
FREE, OBSTACLE = range(2)	
		
class Map():

	def __init__(self, level):
		self.totalWidth = level.totalWidth
		self.totalHeight = level.totalHeight
		self.cellSize = level.cellSize
		obstacleMap = level.structure
		self.cellsX = self.totalWidth // self.cellSize
		self.cellsY = self.totalHeight // self.cellSize
		self.structure = [[ OBSTACLE if obstacleMap[x][y] else FREE for y in xrange(self.cellsY)] for x in xrange(self.cellsX)] 	
		
	def getExplore(self, cell, tankAngle):
		successors = self.getSuccessors(cell)
		good = []
		for (successor,wildcard) in successors :
			#successorCoords = self.calculateCoords(successor)
			diffAngle = math.fabs(tankAngle - self.getAngleToDest(cell,successor))
			if diffAngle > math.pi :
				diffAngle = D360-diffAngle
			value = len(self.getSuccessors(successor))
			if diffAngle <= D60 :
				good.append((successor,value+7))
			elif diffAngle <= D105 :
				good.append((successor,value+4))
			else :
				good.append((successor,value))
		max_value = 0

		result = []
		for (pos,value) in good :
			if value > max_value :
				result = [pos]
				max_value = value
			elif value == max_value :
				result.append(pos)
		return choice(result)
		
		
	def getSuccessors(self, (xpos,ypos)):
		successors = []
		i = 0
		for y in range(ypos-1,ypos+2):
			for x in range(xpos-1,xpos+2):
				i = i + 1
				if x >= 0 and x < self.cellsX and y >= 0 and y < self.cellsY  :
					if i == 5 :
						continue
					elif self.structure[x][y] == FREE :
						if (x == xpos or y == ypos) : 
							successors.append(((x,y),1))							
						elif i == 1 :
							if self.structure[x+1][y] == FREE and self.structure[x][y+1] == FREE : successors.append(((x,y),1.4))
						elif i == 3 :
							if self.structure[x-1][y] == FREE and self.structure[x][y+1] == FREE : successors.append(((x,y),1.4))
						elif i == 7 :
							if self.structure[x+1][y] == FREE and self.structure[x][y-1] == FREE : successors.append(((x,y),1.4))
						elif i == 9 :
							if self.structure[x-1][y] == FREE and self.structure[x][y-1] == FREE : successors.append(((x,y),1.4))
		return successors

		
	def addObstacles(self,o_list):
		for (x,y) in o_list :
			self.structure[x][y] = 1

	def calculateCell(self,xp,yp):
		return ( int(xp / self.cellSize) , int(yp / self.cellSize) )
		
	def calculateCoords(self,cell):
		return (self.cellSize * (cell[0]+0.5),self.cellSize * (cell[1]+0.5))
	
	def getAngleToDest(self,(xpos,ypos),(xdes,ydes)):		
		return math.atan2(ypos-ydes, xdes-xpos) % D360
	
