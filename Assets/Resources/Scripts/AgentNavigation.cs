using UnityEngine;
using System.Collections.Generic;

public class AgentNavigation : MonoBehaviour
{
    enum Direction { NORTH = 0, EAST, SOUTH, WEST, NUMOFDIRECTIONS };

    // Open list for nodes currently able to be visited
    private List<Node> m_OpenList = new List<Node>();

    // Closed list for nodes visited
    public List<Node> m_ClosedList = new List<Node>();
    public int ListSize;

    // Starting Node
    private GameObject m_StartNode;

    // End Node
    private GameObject m_EndNode;

    private Node m_CurrentNode;

    //private Node m_Last

    public int m_currentHeuristicCost = 0;

    public const int m_DirectionCost = 10; // Horizontal / Vertical direction

    public Mesh mesh;

    public Vector2 m_currentPos = new Vector2();

    private int failCounter;
    private int maxFailCounter = 2;

    private GameObject CurrentNode;

    public Material nodeMat;
    public Material nodeMat2;
    public Material CurrNodeMat;
    float timer;

    // Use this for initialization
    void Start()
    {
        failCounter = maxFailCounter;
        m_StartNode = GameObject.FindWithTag("StartNode");
        m_EndNode = GameObject.FindWithTag("EndNode");

        // Add start node to the closed list
        m_ClosedList.Add(new Node
        {
            NodeId = new Vector2(10, 10),
            EndNode = false,
            FinalValue = 42,
            HeuristicCost = 42
        });
        m_CurrentNode = m_ClosedList[0];

        CurrentNode = new GameObject();
        CurrentNode.transform.localScale *= 0.5f;
        CurrentNode.name = "CurrentNode";
        CurrentNode.AddComponent<MeshRenderer>();
        CurrentNode.AddComponent<MeshFilter>();
        CurrentNode.GetComponent<MeshFilter>().mesh = mesh;
        CurrentNode.GetComponent<MeshFilter>().mesh.name = "Sphere";
        CurrentNode.GetComponent<MeshRenderer>().material = CurrNodeMat;
    }

    void FixedUpdate()
    {
        /* 
         * Add start node to the closed list
         * Find the next node
         * Add that node to the open list
         * Calculate heuristic cost and G cost (directional cost)
         * 
        */

        timer += Time.deltaTime;
        if (timer >= 0.05f)
        {
            timer = 0;
            for (Direction m_direction = Direction.NORTH; m_direction < Direction.NUMOFDIRECTIONS; m_direction++)
            {
                Node node = new Node();
                switch (m_direction)
                {
                    case Direction.NORTH:
                        node.NodeId = new Vector2(m_CurrentNode.NodeId.x, m_CurrentNode.NodeId.y + 1);
                        break;
                    case Direction.EAST:
                        node.NodeId = new Vector2(m_CurrentNode.NodeId.x + 1, m_CurrentNode.NodeId.y);
                        break;
                    case Direction.SOUTH:
                        node.NodeId = new Vector2(m_CurrentNode.NodeId.x, m_CurrentNode.NodeId.y - 1);
                        break;
                    case Direction.WEST:
                        node.NodeId = new Vector2(m_CurrentNode.NodeId.x - 1, m_CurrentNode.NodeId.y);
                        break;
                    default:
                        break;
                }

                try
                {
                    if (Physics.OverlapSphere(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), 0.1f)[0])
                    {
                    }
                }
                catch
                {
                    node.HeuristicCost = (int)((node.NodeId.x - m_EndNode.transform.position.x) + (node.NodeId.y - m_EndNode.transform.position.z));
                    node.FinalValue = node.HeuristicCost + m_DirectionCost;
                    m_OpenList.Add(node);
                }

            }

            Node closestNode = new Node();
            closestNode.FinalValue = int.MaxValue;
            closestNode.NodeId = m_ClosedList[m_ClosedList.Count - 1].NodeId;
            foreach (Node listNode in m_OpenList)
            {
                // Find closest to end node
                if (closestNode.FinalValue >= listNode.FinalValue)
                {
                    closestNode = listNode;
                }
            }

            // Add current node to closed list
            if (m_CurrentNode.NodeId != new Vector2(m_EndNode.transform.position.x, m_EndNode.transform.position.z))
            {
                newNode(closestNode);
                RemoveNodeFromOpenList(closestNode);
                if (/*m_CurrentNode.NodeId != m_ClosedList[m_ClosedList.Count - 1].NodeId*/m_OpenList.Count > 0 || m_ClosedList.Count < 2)
                {
                }
                /*else//dead end
                {
                    Debug.Log("Dead End");
                    GameObject tempObj =  GameObject.Find(m_ClosedList[m_ClosedList.Count - 1].NodeId.x + " : " + m_ClosedList[m_ClosedList.Count - 1].NodeId.y);
                    tempObj.GetComponent<MeshRenderer>().material = nodeMat2;
                    m_ClosedList.Remove(m_ClosedList[m_ClosedList.Count - 1]);
                    closestNode = m_ClosedList[m_ClosedList.Count - 1];


                }*/
            }
            else//reached end goal [ Check the open list for a valid contender for closest node ]
                this.enabled = false;

            // Make closest node current node
            m_CurrentNode = closestNode;

            m_currentHeuristicCost = m_CurrentNode.HeuristicCost;
            m_currentPos = m_CurrentNode.NodeId;
            ListSize = m_ClosedList.Count;
            //m_OpenList.Clear();
        }
    }

    void RemoveNodeFromOpenList(Node node)
    {
        m_ClosedList.Add(node);

        //Debug.Log(m_OpenList.Count);
        while (m_OpenList.Count > 20)
        {
            GameObject tempObj = GameObject.Find(m_OpenList[0].NodeId.x + " : " + m_OpenList[0].NodeId.y);
            Destroy(tempObj);
            m_OpenList.Remove(m_OpenList[0]);//potential for shorter memmory but needs work

        }


        foreach (var listNode in m_OpenList) //TODO: Identified problem with the open list - redundant node inside REMOVE
        {
            if (listNode.NodeId == node.NodeId)
            {
                GameObject tempObj = GameObject.Find(listNode.NodeId.x + " : " + listNode.NodeId.y);
                tempObj.GetComponent<MeshRenderer>().material = nodeMat2;
                m_OpenList.Remove(listNode);
                break;
            }
        }
    }

    void newNode(Node node)
    {
        //Add new node to open list
        Debug.Log("Found valid node");
        GameObject obj = new GameObject();
        obj.transform.position = new Vector3(node.NodeId.x, 0.5f, node.NodeId.y);
        obj.name = node.NodeId.x + " : " + node.NodeId.y;
        obj.AddComponent<SphereCollider>();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshFilter>().mesh.name = "Sphere";
        obj.GetComponent<MeshRenderer>().material = nodeMat;
    }
}
