using UnityEngine;
using System.Collections;

public class GraphNodeCreator
{

    [SerializeField]
    protected Mesh m_mesh;

    [SerializeField]
    protected Material m_material;

    /// <summary>
    /// Create a visible default node in the game world
    /// </summary>
    /// <param name="node"></param>
    public void newNode(Node node)
    {
        Debug.Log("Found valid node");
        createObj(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), new Vector3(1, 1, 1), m_material, node.NodeId.x + " : " + node.NodeId.y);
    }

    /// <summary>
    /// Create a visible custom node in the game world
    /// </summary>
    /// <param name="node"></param>
    /// <param name="mat"></param>
    /// <param name="tagname"></param>
    public void newNode(Node node, Material mat, string tagname)
    {
        Debug.Log("Found valid node");
        createObj(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), new Vector3(0.6f, 0.6f, 0.6f), mat, node.NodeId.x + " : " + node.NodeId.y, tagname);
    }


    /// <summary>
    /// Create the sphere object
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="scale"></param>
    /// <param name="mat"></param>
    /// <param name="name"></param>
    /// <param name="tagname"></param>
    public void createObj(Vector3 pos, Vector3 scale, Material mat, string name, string tagname = "Path")
    {
        GameObject obj = new GameObject();
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.name = name;
        obj.tag = tagname;
        obj.AddComponent<SphereCollider>();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>();
        obj.GetComponent<MeshFilter>().mesh = m_mesh;
        obj.GetComponent<MeshFilter>().mesh.name = "Sphere";
        obj.GetComponent<MeshRenderer>().material = mat;
    }
}
