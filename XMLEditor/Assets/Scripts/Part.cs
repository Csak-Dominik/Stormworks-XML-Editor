using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Part : MonoBehaviour
{
    public string filePath;
    public PlyMesh plyMesh;
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        plyMesh = PlyTools.LoadPly(filePath);
        meshFilter.mesh = PlyTools.PlyToMesh(plyMesh);
    }

    private void Update()
    {
    }
}