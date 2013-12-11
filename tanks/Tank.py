#
# Tank.py
#
# base class Tank
#
import math as Math

D1 = Math.pi / 180
D360 = 2 * Math.pi

class Tank:

    def __init__(self, field, data):
        self.field = field
        self.canvas = field.canvas

        self.team = data["team"]
        self.tankLength = data["tankLength"] 
        self.tankWidth = data["tankWidth"]
        self.cannonLength = data["cannonLength"]
        self.cannonFront = data["cannonFront"]
        self.cannonBack = data["cannonBack"]
        self.x = data["x"]
        self.y = data["y"]
        self.angle = 0 #later on set
        self.cannonAngle = 0 #later on set
        self.moveSpeed = data["moveSpeed"]
        self.rotateSpeed = data["rotateSpeed"]
        self.cannonSpeed = data["cannonSpeed"]
        self.health = data["health"]
        self.damage = data["damage"]
        self.reloadTime  = data["reloadTime"]
        borderwidth = 1
        self.radius = Math.sqrt(Math.pow(self.tankWidth+borderwidth,2)+Math.pow(self.tankLength+borderwidth,2))
        
        self.bodyCoords = [(self.x-self.tankLength, self.y-self.tankWidth), (self.x+self.tankLength, self.y-self.tankWidth),
        (self.x+self.tankLength, self.y+self.tankWidth), (self.x-self.tankLength, self.y+self.tankWidth)]
        
        self.cannonCoords = [(self.x-self.cannonLength*0.5, self.y-self.cannonBack), (self.x+self.cannonLength*0.5, self.y-self.cannonBack),
        (self.x+self.cannonLength*0.5, self.y-self.cannonFront), (self.x+self.cannonLength*1.5, self.y-self.cannonFront),
        (self.x+self.cannonLength*1.5, self.y+self.cannonFront), (self.x+self.cannonLength*0.5, self.y+self.cannonFront),
        (self.x+self.cannonLength*0.5, self.y+self.cannonBack), (self.x-self.cannonLength*0.5, self.y+self.cannonBack)]
        self.rotateBody(data["angle"])
        self.rotateCannon(data["angle"])
        
        self.bodyImage = self.canvas.create_polygon([(0,0),(0,0)],outline="black",width=borderwidth,tags="tank", fill=data["color-fill"])
        self.cannonImage = self.canvas.create_polygon([(0,0),(0,0)],outline="black",width=borderwidth,tags="cannon", fill=data["color-fill"])
        self.healthImage = self.canvas.create_text(0, 0, text=self.health, fill="green", font=("Helvectica", "8"), tags="health")
        self.draw()
        
        
    def getTeam(self):
        return self.team
        
    def getReloadTime(self):
        return self.reloadTime
        
    def getImage(self):
        return self.bodyImage
        
    def getX(self):
        return self.x
        
    def getY(self):
        return self.y
        
    def getAngle(self):
        return self.angle
        
    def getHealth(self):
        return self.health
        
    def doDamage(self,damage):
        oldHealth = self.health
        self.health -= damage
        if self.health <= 0 : 
            self.destroy()
            return True
        elif self.health <= 10 and oldHealth > 10 :
            self.canvas.itemconfig(self.healthImage,fill="red")
        elif self.health <= 30 and oldHealth > 30 :
            self.canvas.itemconfig(self.healthImage,fill="orange")
        self.canvas.itemconfig(self.healthImage,text=self.health)
        return False
        
    def getCoords(self):
        return self.bodyCoords
        
    def getRadius(self):
        return self.radius
        
    def destroy(self):
        self.canvas.delete(self.bodyImage)
        self.canvas.delete(self.cannonImage)
        self.canvas.delete(self.healthImage)
        
    def draw(self):
        self.drawBody()
        self.drawCannon()
        self.drawHealth()
            
    def drawBody(self):
        self.canvas.coords(self.bodyImage,*[item for sublist in self.bodyCoords for item in sublist])    
        
    def drawCannon(self):
        self.canvas.coords(self.cannonImage,*[item for sublist in self.cannonCoords for item in sublist])
        
    def drawHealth(self):
        self.canvas.coords(self.healthImage,self.x,self.y)
        
    #---------------------------------#
    
                
    def moveTank(self,xoffset,yoffset):
        #Move body first for collision detection
        self.moveBody(xoffset,yoffset)
        self.moveCannon(xoffset,yoffset)
        self.x += xoffset
        self.y += yoffset
            
    def moveBody(self,xoffset,yoffset):
        if xoffset != 0 or yoffset !=0 :
            newxy = []
            for x, y in self.bodyCoords:
                newxy.append((x+xoffset,y+yoffset))
            self.bodyCoords = newxy
            
    def moveCannon(self,xoffset,yoffset):
        if xoffset != 0 or yoffset !=0 :
            newxy = []
            for x, y in self.cannonCoords:
                newxy.append((x+xoffset,y+yoffset))
            self.cannonCoords = newxy    
            
    #------------------------------#
            
    def rotateTank(self,angleOffset):
        self.rotateBody(angleOffset)
        self.rotateCannon(angleOffset)        
        
    def rotateBody(self,angleOffset):
        newxy = []
        for x, y in self.bodyCoords:
            newxy.append(self.rotate(x,y,angleOffset))
        self.bodyCoords = newxy
        self.angle += angleOffset
        self.angle = self.angle % D360
        
    def rotateCannon(self,angleOffset):
        newxy = []
        for x, y in self.cannonCoords:
            newxy.append(self.rotate(x,y,angleOffset))
            
        self.cannonCoords = newxy
        self.cannonAngle += angleOffset
        self.cannonAngle = self.cannonAngle % D360
        
    #------------------------------#
    
    def turnCannon(self,angleOffset):
        self.rotateCannon(angleOffset)
    
    #-------------------------------#

    def shoot(self):
        angle = self.cannonAngle
        self.field.addBullet(self.x+(Math.cos(angle)*(self.cannonLength*1.5)),self.y-(Math.sin(angle)*(self.cannonLength*1.5)), self.team, angle, self.damage, self.cannonFront-1)

    def moveUp(self):
        x = Math.cos(self.angle)*self.moveSpeed
        y = - Math.sin(self.angle)*self.moveSpeed
        self.moveTank(x,y)

    def turnRight(self):
        angle = -(self.rotateSpeed * D1)
        self.rotateTank(angle)

    def moveDown(self):
        y = Math.sin(self.angle)*self.moveSpeed
        x = -Math.cos(self.angle)*self.moveSpeed
        self.moveTank(x,y)

    def turnLeft(self):
        angle = (self.rotateSpeed * D1)
        self.rotateTank(angle)
        
    def turnCannonLeft(self):    
        angle = self.cannonSpeed * D1
        self.turnCannon(angle)

    def turnCannonRight(self):    
        angle = -(self.cannonSpeed * D1)
        self.turnCannon(angle)

    def rotate(self,x,y,angle):
        #note: the rotation is done in the opposite fashion from for a right-handed coordinate system due to the left-handedness of computer coordinates
        x -= self.x
        y -= self.y
        _x = x * Math.cos(angle) + y * Math.sin(angle)
        _y = -x * Math.sin(angle) + y * Math.cos(angle)
        return [_x + self.x, _y + self.y]
    
    def update(self, delta):
        pass
