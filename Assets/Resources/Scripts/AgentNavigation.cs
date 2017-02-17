using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentNavigation : MonoBehaviour
{
    // Open list for nodes currently able to visit
    private List<Node> m_OpenList = new List<Node>();

    // Closed list for nodes visited
    private List<Node> m_ClosedList = new List<Node>();

    // Starting Node
    private GameObject m_StartNode;

    // End Node
    private GameObject m_EndNode;

    private Node m_CurrentNode;

    public const int m_DirectionCost = 10; //Horizontal/Vertical direction

    private bool direction = true;
    private bool positive = false;

    public Mesh mesh;

    public Vector2 m_currentPos = new Vector2();

    // Use this for initialization
    void Start()
    {
        m_StartNode = GameObject.FindWithTag("StartNode");
        m_EndNode = GameObject.FindWithTag("EndNode");

        m_ClosedList.Add(new Node
        {
            NodeId = new Vector2(10, 10),
            EndNode = false,
            FValue = 42,
            HeuristicCost = 42
        });
        m_CurrentNode = m_ClosedList[0];
    }

    void FixedUpdate()
    {
        Node node = new Node();
        node.HeuristicCost = (int)((m_CurrentNode.NodeId.x - m_EndNode.transform.position.x) + (m_CurrentNode.NodeId.y - m_EndNode.transform.position.z));
        node.FValue = node.HeuristicCost + m_DirectionCost;

        if (direction)
        {
            if (positive)
            {
                node.NodeId = new Vector2(m_CurrentNode.NodeId.x + 1, m_CurrentNode.NodeId.y);
            }
            else
            {
                node.NodeId = new Vector2(m_CurrentNode.NodeId.x - 1, m_CurrentNode.NodeId.y);
            }
        }
        else
        {
            if (positive)
            {
                node.NodeId = new Vector2(m_CurrentNode.NodeId.x, m_CurrentNode.NodeId.y + 1);
            }
            else
            {
                node.NodeId = new Vector2(m_CurrentNode.NodeId.x, m_CurrentNode.NodeId.y - 1);
            }
        }

        m_currentPos = node.NodeId;

        try
        {
            if (Physics.OverlapSphere(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), 0.1f)[0].GetComponent<MeshFilter>().mesh.name == mesh.name)
            {
                Debug.Log(m_currentPos);
                m_CurrentNode = node;
                //newNode(m_CurrentNode);
            }
            //positive = !positive;
        }
        catch
        {
            newNode(node);
            direction = !direction;
        }

        foreach (Node listNode in m_OpenList)
        {

        }

        //m_CurrentNode = node;

    }

    void newNode(Node node)
    {
        //Add new node to open list
        Debug.Log("Found valid node");
        m_OpenList.Add(node);
        GameObject obj = new GameObject();
        obj.transform.position = new Vector3(node.NodeId.x, 0.5f, node.NodeId.y);
        obj.name = node.NodeId.x + " : " + node.NodeId.y;
        obj.AddComponent<SphereCollider>();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshFilter>().mesh.name = "Sphere";
    }
}
