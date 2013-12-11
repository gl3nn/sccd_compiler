import math as Math

class Obstacle():
    def __init__(self, canvas, x,y, width, height, color = "LightGray"):
        self.canvas = canvas
        self.x = x
        self.y = y
        self.width = width
        self.height = height
        self.radius = Math.sqrt(Math.pow(self.width+2,2)+Math.pow(self.height+2,2))
        halfwidth = width/2
        halfheight = height/2
        self.coords =[(self.x-halfwidth, self.y-halfheight), (self.x+halfwidth, self.y-halfheight),
        (self.x+halfwidth, self.y+halfheight), (self.x-halfwidth, self.y+halfheight)]
        self.image = self.canvas.create_polygon(self.coords,outline="SlateGray4",width=0, stipple="gray50", tags="obstacle", fill=color)
        
    def getRadius(self):
        return self.radius
        
    def getX(self):
        return self.x
        
    def getY(self):
        return self.y
        
    def getWidth(self):
        return self.width
        
    def getCoords(self):
        return self.coords
        
    def getImage(self):
        return self.image