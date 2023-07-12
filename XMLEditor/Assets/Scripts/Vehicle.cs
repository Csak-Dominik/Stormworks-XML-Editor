using Assets.Scripts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Unity.Mathematics;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public string xmlPath = "";
    public List<GameObject> partGameObjects = new List<GameObject>();

    public Vector3 editorPlacementOffset = Vector3.zero;

    [SerializeField]
    private Material defaultMat;

    private void Start()
    {
        LoadXML(xmlPath);
    }

    public void LoadXML(string path)
    {
        // set the decimal separator to a period
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        Logger.Log($"Loading XML: {path}...");

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        Logger.Log($"XML loaded: {path}!");

        // load the editor placement offset node
        Logger.Log("Loading editor placement offset...");
        XmlNode editorOffset = xmlDoc.SelectSingleNode("vehicle/editor_placement_offset");

        // make a vector3 from the x, y, z values of the editor offset node's attributes
        // if the value is not found, use the default value of 0
        // the values are stored as strings, so we need to parse them to floats

        editorPlacementOffset = new Vector3(
            editorOffset.Attributes["x"] != null ? float.Parse(editorOffset.Attributes["x"].Value) : 0,
            editorOffset.Attributes["y"] != null ? float.Parse(editorOffset.Attributes["y"].Value) : 0,
            editorOffset.Attributes["z"] != null ? float.Parse(editorOffset.Attributes["z"].Value) : 0
        );
        Logger.Log($"Editor placement offset loaded: {editorPlacementOffset}!");

        Logger.Log("Searching for \"Bodies\" tag...");
        XmlNode bodies = xmlDoc.SelectSingleNode("vehicle/bodies");
        Logger.Log("Found \"Bodies\" tag!");

        Logger.Log("Loading bodies...");
        foreach (XmlNode bodyNode in bodies) LoadBody(bodyNode);
        Logger.Log("Bodies loaded!");
    }

    private void LoadBody(XmlNode body)
    {
        XmlNode components = body.SelectSingleNode("components");

        foreach (XmlNode c in components) CreatePart(c);
    }

    private void CreatePart(XmlNode componentNode)
    {
        // get the mesh file name
        string definitionName = componentNode.Attributes["d"] != null ? componentNode.Attributes["d"].Value : "01_block";

        XmlNode o = componentNode.SelectSingleNode("o");
        XmlNode vp = componentNode.SelectSingleNode("o/vp");

        Vector3 pos = vp != null ? new Vector3(
            vp.Attributes["x"] != null ? float.Parse(vp.Attributes["x"].Value) / 4f : 0,
            vp.Attributes["y"] != null ? float.Parse(vp.Attributes["y"].Value) / 4f : 0,
            vp.Attributes["z"] != null ? float.Parse(vp.Attributes["z"].Value) / 4f : 0
        ) : Vector3.zero;

        Logger.Log("Loading matrix...");
        float[] values = new float[9];
        if (o != null && o.Attributes["r"] != null) values = o.Attributes["r"].Value.Split(',').Select(x => float.Parse(x)).ToArray();
        else
        {
            values[0] = 1;
            values[4] = 1;
            values[8] = 1;
        }

        float3x3 matrix = new float3x3(
            values[0], values[1], values[2],
            values[3], values[4], values[5],
            values[6], values[7], values[8]
       );

        Logger.Log($"Matrix loaded! {matrix}");

        Logger.Log($"Fetching: {definitionName}...");
        string meshName = GetMeshNameFromDefinitionFile(definitionName);
        Logger.Log($"Fetched: {meshName}!");

        if (meshName == "")
        {
            var surfaces = GetSurfacesFromDefinitionFile(definitionName);

            // combine the surfaces into a single mesh
            Mesh genMesh = SurfaceBuilder.Instance.CreateMeshFromSurfaces(surfaces);

            // create new part game object
            var partGenGameObject = CreatePartGameObject(definitionName);
            partGenGameObject.transform.localPosition = pos;
            partGenGameObject.GetComponent<MeshFilter>().mesh = genMesh;
            partGenGameObject.GetComponent<Part>().ApplyMatrix(matrix);
            partGenGameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            return;
        }

        var partGameObject = CreatePartGameObject(definitionName);
        partGameObject.transform.localPosition = pos;
        var part = partGameObject.GetComponent<Part>();
        part.fileName = Path.GetFileName(meshName).Replace(".mesh", ".ply"); // flatten the path to just the file name
        part.LoadMesh();
        part.ApplyMatrix(matrix);
        partGameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }

    private GameObject CreatePartGameObject(string definitionName)
    {
        // create new part game object
        GameObject partGameObject = new GameObject();
        partGameObject.transform.parent = transform;
        partGameObject.name = definitionName;

        // add a mesh filter and renderer
        partGameObject.AddComponent<MeshFilter>();
        var renderer = partGameObject.AddComponent<MeshRenderer>();

        // add the part component
        partGameObject.AddComponent<Part>();

        renderer.material = defaultMat;

        return partGameObject;
    }

    private string GetMeshNameFromDefinitionFile(string definitionName)
    {
        // get the definition file
        XmlDocument definitionFile = new XmlDocument();
        //definitionFile.Load($"Assets/Definitions/{definitionName}.xml");
        try
        {
            definitionFile.LoadXml(XMLTools.FixBadFormatting($"Assets/Definitions/{definitionName}.xml"));
        }
        catch (XmlException)
        {
            Logger.LogError($"Failed to load definition file: {definitionName}.xml");
            Logger.Log($"File contents: {XMLTools.FixBadFormatting($"Assets/Definitions/{definitionName}.xml")}");
        }

        // get the mesh name
        XmlNode meshNameNode = definitionFile.SelectSingleNode("definition");
        string meshName = meshNameNode.Attributes["mesh_data_name"] != null ? meshNameNode.Attributes["mesh_data_name"].Value : "";

        return meshName;
    }

    private List<Surface> GetSurfacesFromDefinitionFile(string definitionName)
    {
        // get the definition file
        XmlDocument definitionFile = new XmlDocument();
        //definitionFile.Load($"Assets/Definitions/{definitionName}.xml");
        definitionFile.LoadXml(XMLTools.FixBadFormatting($"Assets/Definitions/{definitionName}.xml"));

        // get the surfaces node
        XmlNode surfacesNode = definitionFile.SelectSingleNode("definition/surfaces");

        // create a list of surfaces
        List<Surface> surfaces = new List<Surface>();

        // loop through each surface
        foreach (XmlNode surfaceNode in surfacesNode)
        {
            // create a new surface
            Surface surface = new Surface();

            // get the orientation
            surface.orientation = surfaceNode.Attributes["orientation"] != null ? int.Parse(surfaceNode.Attributes["orientation"].Value) : 0;

            // get the rotation
            surface.rotation = surfaceNode.Attributes["rotation"] != null ? int.Parse(surfaceNode.Attributes["rotation"].Value) : 0;

            // get the shape
            surface.shape = surfaceNode.Attributes["shape"] != null ? int.Parse(surfaceNode.Attributes["shape"].Value) : 1;

            // add the surface to the list
            surfaces.Add(surface);
        }

        Debug.Log($"Surfaces: {surfaces}");

        return surfaces;
    }
}