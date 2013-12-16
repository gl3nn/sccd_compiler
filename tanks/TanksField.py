#
# TanksField.py
#
# Field for tanks game
#

import math as Math
import itertools

from PlayerTank import PlayerTank
from AITank import AITank
from Level import Level
from Bullet import Bullet


class TanksField:

    def __init__(self, canvas, nrtanks = 1):
        self.canvas = canvas
        self.bullets = []
        self.bulletSpeed = 10
        self.tanks = []
        self.tanks_map = {}
        
        self.width = int(self.canvas['width'])
        self.height = int(self.canvas['height'])
        
        self.createLevel()
        self.createTanks(nrtanks)
        
        self.level_running = True
        
    def createLevel(self):
        self.level = Level(self)
        self.level.addObstacles([(2,3),(3,2),(3,3),(3,4),(3,5),(4,1),(4,2),(4,3),(4,4),(5,2),
        (7,7),(8,7),(9,7),(7,8),(8,8),(9,8),(7,9),(8,9),(8,10),(9,9),(9,10),(9,11),(10,8),(10,9),(10,10),(10,11),(11,9),(11,10),(11,11),
        (11,0),(12,0),(12,1),(13,0),(13,1),(13,2),(13,3),(14,0),(14,1),(14,2),(14,3),(15,0),(15,1),(15,2),(16,0),(16,1),(16,2),(17,0),(17,1),(18,0),(19,0),(19,1),
        (17,7),(17,8),(16,6),(16,7),(16,8),(15,8),(15,9)])
        
    def createTanks(self,nrtanks):
        self.team_counts = [0,0]
        data = {}
        #Controlled player    
        data["tankLength"] = 20
        data["tankWidth"] = 14
        data["cannonLength"] = 20
        data["cannonFront"] = 3
        data["cannonBack"] = 8
        data["x"] = 30
        data["y"] = 30
        data["angle"] = Math.pi*1.75
        data["moveSpeed"] = 3
        data["rotateSpeed"] = 3
        data["cannonSpeed"] = 4
        data["health"] = 80
        data["damage"] = 10
        data["reloadTime"] = 0.5
        data["team"] = 0
        data['color-fill'] = "DarkOliveGreen4"
        self.player = PlayerTank(self, data)
        self.tanks.append(self.player)
        self.tanks_map[self.player.getImage()] = self.player
        self.team_counts[data["team"]] += 1
        
    #AI
        
        if nrtanks != 0 :
            #1
            data["x"] = self.width  - 30
            data["y"] = self.height - 30
            data["angle"] = Math.pi*0.75
            data["team"] = 1
            data['color-fill'] = "cornsilk4"
            enemy = AITank(self, data)
            self.tanks.append(enemy)
            self.tanks_map[enemy.getImage()] = enemy
            self.team_counts[data["team"]] += 1
            if nrtanks > 1 :
                #2
                data["x"] = self.width - 75
                data["y"] =  75
                data["angle"] = 3*Math.pi/2
                enemy = AITank(self, data)
                self.tanks.append(enemy)
                self.tanks_map[enemy.getImage()] = enemy
                self.team_2_count += 1
                self.team_counts[data["team"]] += 1
    #End
        self.canvas.tag_raise("cannon")
        self.canvas.tag_raise("health")        
    
    def update(self, delta):
        for tank in self.tanks: tank.update(delta)
        for bullet in self.bullets: bullet.update()
        self.tankCollision()
        self.bulletCollision()
        for tank in self.tanks : tank.draw()
        for bullet in self.bullets: bullet.draw()
        
        #check if game over
        if self.level_running :
            for c in self.team_counts :
                if c == 0 :
                    self.level_running = False

        
    def addBullet(self, x, y, team, direction, damage, radius):
        self.bullets.append(Bullet(x, y, team, self.bulletSpeed, direction, damage, radius, self.canvas))

        
    def bulletCollision(self):
        for bullet in self.bullets :
            x = bullet.getX() 
            y = bullet.getY()
            radius = bullet.getRadius()
            hits = self.canvas.find_overlapping(x-radius,y-radius,x+radius,y+radius)
            for hit in hits:
                if "tank" in self.canvas.gettags(hit) :
                    if self.tanks_map[hit].getTeam() != bullet.getTeam() :
                        if self.tanks_map[hit].doDamage(bullet.getDamage()) : 
                            self.tanks.remove(self.tanks_map[hit])
                            del self.tanks_map[hit] 
                    bullet.destroy()
                if "obstacle" in self.canvas.gettags(hit) :
                    bullet.destroy()
            if (x > self.width) or (x < 0) or (y > self.height) or (y < 0) :
                bullet.destroy()
    
        self.bullets[:] = [bullet for bullet in self.bullets if not bullet.getExploded()]
        
    def tankCollision(self):
        #between tanks
        for (tank_a,tank_b) in itertools.combinations(self.tanks,2) :
            result = self.collisionBetweenRectangles(tank_a, tank_b)
            if result : #There is a collision
                (smallestOverlap,axis) = result
                direction = (tank_b.getX() - tank_a.getX(), tank_b.getY() - tank_a.getY())
                #check the direction of the axis between the tank and the object
                if (axis[0] * direction[0] + axis[1] * direction[1]) > 0 : #If not pointing the right way, invert (MTV rectangles)
                    axis = (-axis[0],-axis[1])
                    
                correct_xoffset = Math.ceil((axis[0] * smallestOverlap) / 2.0)
                correct_yoffset = Math.ceil((axis[1] * smallestOverlap) / 2.0)
                    
                tank_a.moveTank(correct_xoffset, correct_yoffset)
                
        for tank in self.tanks :
            corrections = []
            #between tank and obstacles
            for obstacle in self.level.obstacles :
                result = self.collisionBetweenRectangles(tank, obstacle)
                if result : #There is a collision
                    (smallestOverlap,axis) = result
                    direction = (obstacle.getX() - tank.getX(), obstacle.getY() - tank.getY())
                    #check the direction of the axis between the tank and the object
                    if (axis[0] * direction[0] + axis[1] * direction[1]) > 0 : #If not pointing the right way, invert (MTV rectangles)
                        axis = (-axis[0],-axis[1])
                    
                    correct_xoffset = (axis[0] * smallestOverlap)
                    correct_yoffset = (axis[1] * smallestOverlap)       
                    corrections.append((correct_xoffset, correct_yoffset))     
                    
        
            #between tanks and borders
            corrections.extend(self.checkBoundaryMove(tank))
            xCor = 0
            yCor = 0
            for (x,y) in corrections :
                xCor += x
                yCor += y
            tank.moveTank(xCor, yCor)
        
        
    def checkBoundaryMove(self,tank) :
        corrections = []
        for (x,y) in tank.getCoords() :
            if (x > self.width) :
                corrections.append((self.width-x,0))
            if (x < 0) :
                corrections.append((0-x,0))
            if (y > self.height) :
                corrections.append((0,self.height-y))
            if (y < 0) :
                corrections.append((0,0-y))
        return corrections
        
    def collisionBetweenRectangles(self,rec1,rec2):
        smallestOverlap = 0
        axis = (0,0)
        r1 = rec1.getRadius()
        r2 = rec2.getRadius()
        x1 = rec1.getX()
        x2 = rec2.getX()
        y1 = rec1.getY()
        y2 = rec2.getY()
        dsqrd = (y2-y1) * (y2-y1) + (x2-x1) * (x2-x1)
        if (dsqrd < (r1+r2)*(r1+r2)) :
            smallestOverlap = 10000
            coords1 = rec1.getCoords()
            coords2 = rec2.getCoords()
            axiss = [(coords1[1][0] - coords1[0][0],coords1[1][1] - coords1[0][1]),(coords1[1][0] - coords1[2][0],coords1[1][1] - coords1[2][1]),
                (coords2[0][0] - coords2[3][0], coords2[0][1] - coords2[3][1]), ( coords2[0][0] - coords2[1][0], coords2[0][1] - coords2[1][1])]
            for (x,y) in axiss :
                rec1projected=[]
                rec2projected=[]
                xn = x/Math.sqrt(x*x+y*y)
                yn = y/Math.sqrt(x*x+y*y)
                for point in range(4) :
                    rec1projected.append((xn*coords1[point][0]) + (yn*coords1[point][1]))
                for point in range(4) :
                    rec2projected.append((xn*coords2[point][0]) + (yn*coords2[point][1]))
                maxrec1 = max(rec1projected)
                minrec1 = min(rec1projected)
                maxrec2 = max(rec2projected)
                minrec2 = min(rec2projected)
                if minrec1 < minrec2 :
                    overlap = maxrec1 - minrec2
                else : 
                    overlap = maxrec2 - minrec1
                if overlap > 0 :
                    if (overlap < smallestOverlap  ) :
                            smallestOverlap  = overlap
                            axis = (xn,yn)
                    continue
                return False
            return (smallestOverlap,axis)
        return False
        
    def getClosestEnemy(self,tank):
        xpos2 = tank.getX()
        ypos2 = tank.getY()
        inRange = {}
        for enemy in self.tanks :
            if tank.getTeam() != enemy.getTeam() :
                xpos1 = enemy.getX()
                ypos1 = enemy.getY()
                distance =  ( (xpos1 - xpos2) ** 2 + (ypos1 - ypos2) ** 2 ) ** 0.5
                sighted = True
                for obstacle in self.level.getObstacles().values() :
                    if lineIntersectsObstacle(xpos1,ypos1,xpos2,ypos2,obstacle) : 
                        sighted = False
                        continue
                if sighted : inRange[distance] = (xpos1,ypos1)
        if len(inRange.keys()) == 0 :
            return None
        return inRange[min(inRange.keys())]
        

def lineIntersectsObstacle(xpos1,ypos1,xpos2,ypos2,obstacle) :
    width = obstacle.getWidth()
    halfwidth = width/2
    left = obstacle.getX()- halfwidth
    top = obstacle.getY() - halfwidth
    height = width
    minX = xpos1
    maxX = xpos2
    if xpos1 > xpos2 :
        minX = xpos2
        maxX = xpos1
    if maxX > (left + width) : maxX = left + width
    if minX < left : minX = left
    if minX > maxX : return False
    minY = ypos1
    maxY = ypos2
    dx = xpos2 - xpos1
    if Math.fabs(dx) > 0.0000001 :
        a = (ypos2 - ypos1) / dx
        b = ypos1 - a * xpos1
        minY = a * minX + b
        maxY = a * maxX + b 
    if minY > maxY :
        tmp = maxY
        maxY = minY
        minY = tmp  
    if maxY > (top + height) : maxY = top + height
    if minY < top : minY = top
    if minY > maxY : return False
    return True