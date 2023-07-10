using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public string xmlPath = "";
    public List<GameObject> partGameObjects = new List<GameObject>();

    public Vector3 editorPlacementOffset = Vector3.zero;

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
        foreach (XmlNode bodyNode in bodies)
        {
            LoadBody(bodyNode);
        }
        Logger.Log("Bodies loaded!");
    }

    private List<GameObject> LoadBody(XmlNode body)
    {
        XmlNode components = body.SelectSingleNode("components");

        foreach (XmlNode c in components)
        {
            LoadPartMesh(c);
        }

        return null;
    }

    private Mesh LoadPartMesh(XmlNode node)
    {
        // get the mesh file name
        string fileName = node.Attributes["d"] != null ? node.Attributes["d"].Value : "unit_cube";

        // load the mesh
        PlyMesh plyMesh = PlyTools.LoadPly($"Assets/Ply/{fileName}.ply");

        // convert the ply mesh to a unity mesh
        Mesh mesh = PlyTools.PlyToMesh(plyMesh);

        // create a game object for the mesh
        GameObject meshGameObject = new GameObject(fileName);
        meshGameObject.transform.parent = transform;
        meshGameObject.AddComponent<MeshFilter>();
        meshGameObject.AddComponent<MeshRenderer>();
        meshGameObject.GetComponent<MeshFilter>().mesh = mesh;

        // add the game object to the list of part game objects
        partGameObjects.Add(meshGameObject);

        return mesh;
    }
}