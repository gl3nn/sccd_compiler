import math as Math

class Bullet():

    def __init__(self, x, y, team, speed, direction, damage, radius, canvas):
        self.x = x
        self.y = y
        self.speed = speed
        self.direction = direction
        self.damage = damage
        self.radius = radius
        self.canvas = canvas
        self.team = team
        self.image = self.canvas.create_oval(self.x-self.radius, self.y-self.radius, self.x+self.radius, self.y+self.radius, outline='black', fill="firebrick1",stipple='gray25', tags="bullet")
        self.exploded = False

    def getTeam(self):
        return self.team 
        
    def getX(self):
        return self.x
        
    def getY(self):
        return self.y
                
    def getRadius(self):
        return self.radius
        
    def getExploded(self):
        return self.exploded
        
    def getDamage(self):
        return self.damage
        
    def getImage(self):
        return self.image
        
    def move(self):
        self.x += Math.cos(self.direction)*self.speed
        self.y -= Math.sin(self.direction)*self.speed
        
    def draw(self):
        self.canvas.coords(self.image, self.x-self.radius, self.y-self.radius, self.x+self.radius, self.y+self.radius)
   
    def destroy(self):
        self.exploded = True
        self.canvas.delete(self.image)
        
    def update(self):
        self.move()