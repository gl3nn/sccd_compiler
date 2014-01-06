from random import choice
import heapq
from mymath import D60, D105, D360
import math

FREE, OBSTACLE = range(2)    
        
class AIMap():

    def __init__(self, level):
        self.totalWidth = level.totalWidth
        self.totalHeight = level.totalHeight
        self.cellSize = level.cellSize
        obstacleMap = level.structure
        self.cellsX = self.totalWidth // self.cellSize
        self.cellsY = self.totalHeight // self.cellSize
        self.structure = [[ OBSTACLE if obstacleMap[x][y] else FREE for y in xrange(self.cellsY)] for x in xrange(self.cellsX)]     
        
    def getNewExplore(self, position, tankAngle):
        cell = self.calculateCell(position)
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
        return self.calculateCoords(choice(result))
        
        
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

    def calculateCell(self, (xp,yp)):
        return ( int(xp / self.cellSize) , int(yp / self.cellSize) )
        
    def calculateCoords(self,cell):
        return (self.cellSize * (cell[0]+0.5),self.cellSize * (cell[1]+0.5))
    
    def getAngleToDest(self,(xpos,ypos),(xdes,ydes)):        
        return math.atan2(ypos-ydes, xdes-xpos) % D360
    
    #------PathFinding----#
    
    def calculatePath(self, start, destination):
        #self.controller.stop()

        class PriorityQueue:
            def  __init__(self):  
                self.heap = []

            def push(self, item, priority):
                pair = (priority,item)
                heapq.heappush(self.heap,pair)

            def pop(self):
                popped = heapq.heappop(self.heap)
                return popped[1]#item

            def isEmpty(self):
                return len(self.heap) == 0    
            
        #A*
        start_cell = self.calculateCell(start)
        destination_cell = self.calculateCell(destination)
        explored = {}
        fringe = PriorityQueue();
        startNode = (start_cell, 0, 0) #(state,parent,cost to get to this state)
        fringe.push(startNode, startNode[2])
        while (True) :
            while (True) :
                currentNode = fringe.pop()
                if currentNode[0] not in explored : #same node on the fringe multiple times. we only want the lowest cost node expanded.
                    break
            explored[currentNode[0]] = currentNode
            if currentNode[0] == destination_cell :
                break
            successors = self.getSuccessors(currentNode[0])
            for (successor,cost) in successors :
                totalcost = currentNode[2] + cost            
                if successor not in explored : #nodes that are not expanded yet, but are already on the fringe will still be added.
                    node = (successor, currentNode, totalcost)
                    heuristic =  ((successor[0] - destination[0]) ** 2 + (successor[1] - destination[1]) ** 2 ) ** 0.5
                    fringe.push(node,(totalcost+heuristic))    
                    
        newpoints = []
        while ( currentNode[0] != start_cell) :
            newpoints.insert(0, self.calculateCoords(currentNode[0]))
            currentNode = currentNode[1]
            
        return newpoints
        
    
    