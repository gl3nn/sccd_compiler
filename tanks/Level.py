from Obstacle import Obstacle

class Level():
    def __init__(self, field):

        self.cellSize = 50
        self.totalWidth = field.width
        self.totalHeight = field.height
        self.canvas = field.canvas
        self.cellsX = self.totalWidth // self.cellSize
        self.cellsY = self.totalHeight // self.cellSize
        self.structure = [[0 for x in xrange(self.cellsY)] for x in xrange(self.cellsX)] 
        self.obstacles = []
        
    def addObstacles(self, obstacle_list):
        for (x,y) in obstacle_list :
            self.structure[x][y] = 1
            obstacle = Obstacle(self.canvas, self.cellSize * (x+0.5),self.cellSize * (y+0.5),self.cellSize,self.cellSize)
            self.obstacles.append(obstacle)