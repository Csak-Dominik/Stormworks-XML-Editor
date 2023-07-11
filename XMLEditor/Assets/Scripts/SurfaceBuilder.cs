using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public struct Surface
    {
        public int orientation;
        public int rotation;
        public int shape;
    }

    // Singleton class
    public class SurfaceBuilder
    {
        private static SurfaceBuilder instance = null;
        public static SurfaceBuilder Instance => instance ??= new SurfaceBuilder();

        private Dictionary<int, Mesh> shapeDict = new Dictionary<int, Mesh>();

        private SurfaceBuilder()
        {
            InitMeshDictionary();
        }

        public Mesh CreateSurface(int orientation, int rotation, int shape)
        {
            if (!shapeDict.ContainsKey(shape)) return shapeDict[1]; // fix later pls, change back to null

            // rotate the mesh to the correct orientation
            return RotateMesh(shapeDict[shape], orientation, rotation);
        }

        // the orientation is the direction the mesh is facing
        // the rotation is the number of 90 degree rotations to apply
        private Mesh RotateMesh(Mesh mesh, int orientation, int rotation)
        {
            var vertices = new Vector3[mesh.vertices.Length];

            // first rotate the vertices
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                vertices[i] = RotateVertex(mesh.vertices[i], rotation);
            }

            // rotate the mesh in the correct orientation
            // orientation is from 0 to 5
            // 0 is up
            // 1 is right
            // 2 is down
            // 3 is left
            // 4 is forward
            // 5 is back

            switch (orientation)
            {
                case 0:
                    // no rotation needed
                    break;

                case 1:
                    // make the original y+ face the x+ direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].y, vertices[i].z, vertices[i].x);
                    }
                    break;

                case 2:
                    // make the original y+ face the y- direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].x, -vertices[i].y, vertices[i].z);
                    }
                    break;

                case 3:
                    // make the original y+ face the x- direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(-vertices[i].y, vertices[i].z, -vertices[i].x);
                    }
                    break;

                case 4:
                    // make the original y+ face the z+ direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].x, vertices[i].z, -vertices[i].y);
                    }
                    break;

                case 5:
                    // make the original y+ face the z- direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(-vertices[i].x, vertices[i].z, vertices[i].y);
                    }
                    break;

                default:
                    break;
            }

            return mesh;
        }

        // Rotate the vertex around the y axis
        private Vector3 RotateVertex(Vector3 vertex, int rotation)
        {
            var newVertex = vertex;
            for (int i = 0; i < rotation; i++)
            {
                newVertex = new Vector3(newVertex.z, newVertex.y, -newVertex.x);
            }
            return newVertex;
        }

        public Mesh CombineSurfaces(List<Surface> surfaces)
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            // combine all the vertices and triangles
            foreach (var surface in surfaces)
            {
                var newMesh = CreateSurface(surface.orientation, surface.rotation, surface.shape);
                var newVertices = newMesh.vertices;
                var newTriangles = newMesh.triangles;

                // add the vertices
                foreach (var vertex in newVertices)
                {
                    vertices.Add(vertex);
                }

                // add the triangles
                foreach (var triangle in newTriangles)
                {
                    triangles.Add(triangle);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            return mesh;
        }

        private void InitMeshDictionary()
        {
            InitSquare();
        }

        private void InitSquare()
        {
            // create square mesh on the xz plane above the origin
            var squareMesh = new Mesh();
            var vertices = new Vector3[4];
            var triangles = new int[6];
            vertices[0] = new Vector3(-0.5f, 0.5f, -0.5f);
            vertices[1] = new Vector3(-0.5f, 0.5f, 0.5f);
            vertices[2] = new Vector3(0.5f, 0.5f, 0.5f);
            vertices[3] = new Vector3(0.5f, 0.5f, -0.5f);
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            squareMesh.vertices = vertices;
            squareMesh.triangles = triangles;

            shapeDict.Add(1, squareMesh);
        }
    }
}