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

        private const float BLOCK_SIZE = 0.25f;
        private const float HALF_BLOCK_SIZE = BLOCK_SIZE / 2f;

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
        public Mesh RotateMesh(Mesh mesh, int orientation, int rotation)
        {
            var vertices = mesh.vertices;

            // rotate the mesh in the correct orientation
            // orientation is from 0 to 5
            // 0 is up
            // 1 is right
            // 2 is left
            // 3 is down
            // 4 is forward
            // 5 is back

            switch (orientation)
            {
                case 0:
                    // no orientation needed

                    // rotation
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        vertices[i] = RotateVertex(mesh.vertices[i], rotation, Vector3.up);
                    }
                    break;

                case 1:
                    // make the top face the x+ direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].z, vertices[i].y, -vertices[i].x);
                    }

                    // rotation
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        vertices[i] = RotateVertex(mesh.vertices[i], rotation, Vector3.right);
                    }
                    break;

                case 2:
                    // make the top face the x- direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(-vertices[i].z, vertices[i].y, vertices[i].x);
                    }

                    // rotation
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        vertices[i] = RotateVertex(mesh.vertices[i], rotation, Vector3.left);
                    }
                    break;

                case 3:
                    // make the top face the y- direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].x, -vertices[i].z, vertices[i].y);
                    }

                    // rotation
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        vertices[i] = RotateVertex(mesh.vertices[i], rotation, Vector3.down);
                    }
                    break;

                case 4:
                    // make the top face the z+ direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].x, vertices[i].z, -vertices[i].y);
                    }

                    // rotation
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        vertices[i] = RotateVertex(mesh.vertices[i], rotation, Vector3.forward);
                    }
                    break;

                case 5:
                    // make the top face the z- direction
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(-vertices[i].x, vertices[i].z, vertices[i].y);
                    }

                    // rotation
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        vertices[i] = RotateVertex(mesh.vertices[i], rotation, Vector3.back);
                    }
                    break;

                default:
                    break;
            }

            mesh.vertices = vertices;

            return mesh;
        }

        // Rotate the vertex around the specified axis in 90 degree increments clockwise
        private Vector3 RotateVertex(Vector3 vertex, int rotation, Vector3 axis)
        {
            switch (rotation)
            {
                case 0:
                    // no rotation needed
                    break;

                case 1:
                    // rotate 90 degrees clockwise
                    vertex = Quaternion.AngleAxis(90f, axis) * vertex;
                    break;

                case 2:
                    // rotate 180 degrees clockwise
                    vertex = Quaternion.AngleAxis(180f, axis) * vertex;
                    break;

                case 3:
                    // rotate 270 degrees clockwise
                    vertex = Quaternion.AngleAxis(270f, axis) * vertex;
                    break;

                default:
                    break;
            }

            return vertex;
        }

        public Mesh CombineMeshes(List<Mesh> meshes)
        {
            var combinedMesh = new Mesh();

            // combine instances
            var combine = new CombineInstance[meshes.Count];

            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = Matrix4x4.identity;
            }

            // assign combined mesh
            combinedMesh.CombineMeshes(combine);

            return combinedMesh;
        }

        public Mesh CreateMeshFromSurfaces(List<Surface> surfaces)
        {
            var meshes = new List<Mesh>();

            foreach (var surface in surfaces)
            {
                meshes.Add(CreateSurface(surface.orientation, surface.rotation, surface.shape));
            }

            return CombineMeshes(meshes);
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
            vertices[0] = new Vector3(-HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE);
            vertices[1] = new Vector3(-HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, HALF_BLOCK_SIZE);
            vertices[2] = new Vector3(HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, HALF_BLOCK_SIZE);
            vertices[3] = new Vector3(HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE);
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