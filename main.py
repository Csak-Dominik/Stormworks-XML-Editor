import OpenGL
from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *

ply_file_path = "ply/component_air_intake.ply"

# load the lines of the ply file
with open(ply_file_path, 'r') as f:
    lines = f.readlines()

# get the index of the line that contains the vertex data
for i, line in enumerate(lines):
    if line.startswith('end_header'):
        break

# lines are formatted as: x, y, z, nx, xy, nz, r, g, b, a and are separated by spaces
vertex_data = [[float(x) for x in line.split()] for line in lines[i+1:]]

# vertex class
class Vertex:
    def __init__(self, x, y, z, nx, ny, nz, r, g, b, a):
        self.x = x
        self.y = y
        self.z = z
        self.nx = nx
        self.ny = ny
        self.nz = nz
        self.r = r
        self.g = g
        self.b = b
        self.a = a

class Triangle:
    def __init__(self, id, v1, v2, v3):
        self.id = id
        self.v1 = v1
        self.v2 = v2
        self.v3 = v3

# create a list of vertex objects
vertices = []
triangles = []

for data in vertex_data:
    # if the line is empty, skip it
    if data == []:
        continue

    try:
        print(data)
        vertex = Vertex(*data)
        vertices.append(vertex)
    except TypeError:
        print("Triangle found!")
        triangle = Triangle(*data)
        triangles.append(triangle)

# create a window
glutInit()