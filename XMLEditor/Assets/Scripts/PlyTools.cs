using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class PlyTools
{
    public class NotPlyFileException : Exception
    { }

    public class UnknownPlyFormatException : Exception
    { }

    public class NoPlyHeaderEndingException : Exception
    { }

    public class UnknownPlyTokenException : Exception
    {
        public UnknownPlyTokenException(string msg) : base(msg)
        { }
    }

    public class NotEnoughCoordinantesException : Exception
    { }

    public class NotTriangleException : Exception
    { }

    private enum PlyPropertyMode
    {
        NONE,
        VERTEX,
        FACE
    }

    public static PlyMesh LoadPly(string path)
    {
        // set localization to . for parsing floats
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

        var sr = new StreamReader(path, true);

        if (sr.ReadLine() != "ply") throw new NotPlyFileException();

        string format = sr.ReadLine();
        if (format == "format ascii 1.0")
        {
            sr = new StreamReader(path, Encoding.ASCII);
        }
        else if (format == "format binary_little_endian 1.0")
        {
            throw new UnknownPlyFormatException();
        }
        else if (format == "format binary_big_endian 1.0")
        {
            throw new UnknownPlyFormatException();
        }
        else
        {
            throw new UnknownPlyFormatException();
        }

        Debug.Log($"Current encoding: {sr.CurrentEncoding}");

        string[] fileLines = sr.ReadToEnd().Split('\n');

        List<string> header = new List<string>();
        List<string> body = new List<string>();

        bool containsHeaderEnding = fileLines.Any(x => x == "end_header");
        if (!containsHeaderEnding) throw new NoPlyHeaderEndingException();

        bool isHeader = true;
        foreach (string line in fileLines)
        {
            if (isHeader)
            {
                if (line == "end_header")
                {
                    isHeader = false;
                    continue;
                }

                header.Add(line.Trim().ToLower());
            }
            else
            {
                body.Add(line.Trim().ToLower());
            }
        }

        PlyPropertyMode mode = PlyPropertyMode.NONE;
        int vertexAmount = 0;
        int faceAmount = 0;

        int propertyInd = 0;

        Dictionary<string, int> vertexPropertyOrder = new Dictionary<string, int>();

        // skipping first two lines beacause they are already processed
        for (int i = 2; i < header.Count; i++)
        {
            var tokens = header[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // if line begins with comment, skip it
            if (header[i].Substring(0, "comment".Length) == "comment") continue;

            switch (tokens[0])
            {
                case "comment": continue;
                case "element":
                    {
                        if (tokens[1] == "vertex")
                        {
                            mode = PlyPropertyMode.VERTEX;
                            vertexAmount = int.Parse(tokens[2]);
                        }
                        else if (tokens[1] == "face")
                        {
                            mode = PlyPropertyMode.FACE;
                            faceAmount = int.Parse(tokens[2]);
                        }
                        else throw new UnknownPlyTokenException($"Unknown element type: {tokens[1]}");
                        propertyInd = 0;
                        break;
                    }
                case "property":
                    {
                        if (mode == PlyPropertyMode.VERTEX)
                        {
                            switch (tokens[2])
                            {
                                case "x":
                                    {
                                        vertexPropertyOrder.Add("x", propertyInd++);
                                        break;
                                    }
                                case "y":
                                    {
                                        vertexPropertyOrder.Add("y", propertyInd++);
                                        break;
                                    }
                                case "z":
                                    {
                                        vertexPropertyOrder.Add("z", propertyInd++);
                                        break;
                                    }
                                case "nx":
                                    {
                                        vertexPropertyOrder.Add("nx", propertyInd++);
                                        break;
                                    }
                                case "ny":
                                    {
                                        vertexPropertyOrder.Add("ny", propertyInd++);
                                        break;
                                    }
                                case "nz":
                                    {
                                        vertexPropertyOrder.Add("nz", propertyInd++);
                                        break;
                                    }
                                case "red":
                                    {
                                        vertexPropertyOrder.Add("r", propertyInd++);
                                        break;
                                    }
                                case "green":
                                    {
                                        vertexPropertyOrder.Add("g", propertyInd++);
                                        break;
                                    }
                                case "blue":
                                    {
                                        vertexPropertyOrder.Add("b", propertyInd++);
                                        break;
                                    }
                                case "alpha":
                                    {
                                        vertexPropertyOrder.Add("a", propertyInd++);
                                        break;
                                    }
                                default:
                                    {
                                        throw new UnknownPlyTokenException($"Unknown property: {tokens[2]}");
                                    }
                            }
                        }
                        else if (mode == PlyPropertyMode.FACE)
                        {
                            // ignore
                        }
                        else throw new UnknownPlyTokenException("Invalid property mode");
                        break;
                    }
                default:
                    {
                        throw new UnknownPlyTokenException($"Unknown syntax: {tokens[0]}");
                    }
            }
        }

        var vertices = new PlyMesh.PlyVertex[vertexAmount];
        // process vertices
        for (int i = 0; i < vertexAmount; i++)
        {
            double[] values = body[i].Split(new char[] { ' ', '\t' }).Select(x => double.Parse(x)).ToArray();

            double x = values[vertexPropertyOrder["x"]];
            double y = values[vertexPropertyOrder["y"]];
            double z = values[vertexPropertyOrder["z"]];

            double nx = vertexPropertyOrder.GetValueOrDefault("nx", -1) == -1 ? 0 : values[vertexPropertyOrder["nx"]];
            double ny = vertexPropertyOrder.GetValueOrDefault("ny", -1) == -1 ? 0 : values[vertexPropertyOrder["ny"]];
            double nz = vertexPropertyOrder.GetValueOrDefault("nz", -1) == -1 ? 0 : values[vertexPropertyOrder["nz"]];

            double r = vertexPropertyOrder.GetValueOrDefault("r", -1) == -1 ? 0 : values[vertexPropertyOrder["r"]];
            double g = vertexPropertyOrder.GetValueOrDefault("g", -1) == -1 ? 0 : values[vertexPropertyOrder["g"]];
            double b = vertexPropertyOrder.GetValueOrDefault("b", -1) == -1 ? 0 : values[vertexPropertyOrder["b"]];
            double a = vertexPropertyOrder.GetValueOrDefault("a", -1) == -1 ? 0 : values[vertexPropertyOrder["a"]];

            vertices[i] = new PlyMesh.PlyVertex(x, y, z, nx, ny, nz, r, g, b, a);
        }

        var faces = new PlyMesh.PlyFace[faceAmount];
        // process faces
        for (int i = vertexAmount; i < vertexAmount + faceAmount; i++)
        {
            int[] values = body[i].Split(new char[] { ' ', '\t' }).Select(x => int.Parse(x)).ToArray();

            if (values[0] != 3) throw new NotTriangleException();

            faces[i - vertexAmount] = new PlyMesh.PlyFace(values[1], values[2], values[3]);
        }

        return new PlyMesh(vertices, faces);
    }

    public static Mesh PlyToMesh(PlyMesh plyMesh)
    {
        Vector3[] vertices = plyMesh.vertices.Select(vert => new Vector3((float)vert.x, (float)vert.z, (float)vert.y)).ToArray();
        Vector3[] normals = plyMesh.vertices.Select(vert => new Vector3((float)vert.nx, (float)vert.nz, (float)vert.ny)).ToArray();
        Color[] colors = plyMesh.vertices.Select(vert => new Color((float)vert.r, (float)vert.g, (float)vert.b, (float)vert.a)).ToArray();
        int[] triangles = plyMesh.faces.SelectMany(face => new int[] { face.v1, face.v2, face.v3 }).ToArray();

        var mesh = new Mesh()
        {
            vertices = vertices,
            //normals = normals,
            colors = colors,
            triangles = triangles
        };

        mesh.RecalculateNormals();

        return mesh;
    }
}

public class PlyMesh
{
    public PlyVertex[] vertices;
    public PlyFace[] faces;

    public PlyMesh(PlyVertex[] vertices, PlyFace[] faces)
    {
        this.vertices = vertices;
        this.faces = faces;
    }

    public class PlyVertex
    {
        public double x, y, z, nx, ny, nz, r, g, b, a;

        public PlyVertex(double x, double y, double z, double nx, double ny, double nz, double r, double g, double b, double a)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.nx = nx;
            this.ny = ny;
            this.nz = nz;
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    public class PlyFace
    {
        public int v1;
        public int v2;
        public int v3;

        public PlyFace(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }
}