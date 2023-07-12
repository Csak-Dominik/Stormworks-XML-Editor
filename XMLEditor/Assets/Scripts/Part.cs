using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Part : MonoBehaviour
{
    private const string PLY_DIRECTORY = "Assets/Ply/";

    public string fileName;
    public PlyMesh plyMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Material material;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material = material;
    }

    private void Start()
    {
    }

    public void LoadMesh()
    {
        string filePath = PLY_DIRECTORY + fileName;

        try
        {
            Logger.Log($"Loading {filePath}...");
            plyMesh = PlyTools.LoadPly(filePath);
            meshFilter.mesh = PlyTools.PlyToMesh(plyMesh);
            Logger.Log($"Loading {filePath} succeeded!");
        }
        catch (FileNotFoundException)
        {
            Logger.Log($"Loading {filePath} failed! Trying to load multiple files...");

            int counter = 0;

            List<Mesh> meshes = new List<Mesh>();

            while (counter < 20)
            {
                filePath = PLY_DIRECTORY + fileName.Replace(".ply", $".{FillNumberToLength(counter, 3)}.ply");

                try
                {
                    Logger.Log($"Loading {filePath}...");
                    plyMesh = PlyTools.LoadPly(filePath);
                    meshes.Add(PlyTools.PlyToMesh(plyMesh));
                    counter++;
                    Logger.Log($"Loading {filePath} succeeded!");
                }
                catch (FileNotFoundException)
                {
                    Logger.LogWarning($"Loading {filePath} failed!");
                    break;
                }
                catch (Exception e)
                {
                    Logger.LogError($"Loading {filePath} failed! Exception: {e}");
                    break;
                }
            }

            CombineInstance[] combine = new CombineInstance[meshes.Count];

            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = transform.localToWorldMatrix;
            }

            meshFilter.mesh = new Mesh();
            meshFilter.mesh.CombineMeshes(combine);

            Logger.Log($"Loading {PLY_DIRECTORY + fileName} succeeded!");
        }
        catch (Exception e)
        {
            Logger.LogError($"Loading {filePath} failed! Exception: {e}");
        }
    }

    private string FillNumberToLength(int number, int length)
    {
        string numStr = number.ToString();

        while (numStr.Length < length)
        {
            numStr = "0" + numStr;
        }

        return numStr;
    }

    public void ApplyMatrix(float3x3 matrix)
    {
        var vertices = meshFilter.mesh.vertices;
        var normals = meshFilter.mesh.normals;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = math.mul(vertices[i], matrix);
        }

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.normals = normals;
    }
}