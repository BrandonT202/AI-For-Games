using UnityEngine;
using System.Collections.Generic;
using System;

class EnvironmentGraph
{
    enum Direction { NORTH = 0, NORTHEAST, EAST, SOUTHEAST, SOUTH, SOUTHWEST, WEST, NORTHWEST, NUMOFDIRECTIONS };

    public Mesh m_mesh;
    public Material m_material;

    // A list of connections between all the 
    // nodes in the game environment
    private List<Connection> m_graph;

    private Vector3 m_currentPosition;

    // Real-time values
    int m_XIndex = 0;
    int m_ZIndex = 0;
    public bool m_ValidGraph { get; set; }

    public EnvironmentGraph(Mesh mesh, Material material)
    {
        this.m_mesh = mesh;
        this.m_material = material;
    }

    public void Reset()
    {
        m_ValidGraph = false;
        GameObject start = GameObject.Find("Range1");
        Node startNode = new Node();
        Vector3 startPosition = start.transform.position;
        startNode.NodeId = new Vector2(startPosition.x, startPosition.z);

        m_currentPosition = startPosition;
        m_graph = new List<Connection>();
    }

    public void DrawGraph()
    {
        foreach (Connection connection in m_graph)
        {
            Node fromNode = connection.GetFromNode();
            Node toNode = connection.GetToNode();

            //Debug: Add Line to represent the connections
            Vector3 from = new Vector3(fromNode.NodeId.x, 0.5f, fromNode.NodeId.y);
            Vector3 to = new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y);
            Debug.DrawLine(from, to, Color.red);
        }
    }

    public void CreateGraphWithoutDiagonals()
    {
        // Reset graph for new graph creation
        Reset();

        int xCount = 21;//Mathf.Abs(Convert.ToInt32(endPosition.x - startPosition.x));
        int zCount = 21;// Mathf.Abs(Convert.ToInt32(endPosition.z - startPosition.z));
        for (int i = 0; i < xCount; i++)
        {
            List<Connection> newConnections = new List<Connection>();
            for (int j = 0; j < zCount; j++)
            {
                newConnections = GetPotentialNodes(m_currentPosition);

                if (newConnections.Count > 0)
                {
                    m_graph.AddRange(newConnections);
                }

                m_currentPosition.z -= 1f;
            }
            m_currentPosition.z += zCount;
            m_currentPosition.x -= 1f;
        }
        m_ValidGraph = true;
    }

    public void RealTimeCreateGraphWithoutDiagonals()
    {
        GameObject start = GameObject.FindWithTag("StartNode");
        Vector3 startPosition = start.transform.position;

        GameObject end = GameObject.FindWithTag("EndNode");
        Vector3 endPosition = end.transform.position;

        int xCount = 20;//Mathf.Abs(Convert.ToInt32(endPosition.x - startPosition.x));
        int zCount = 20;//Mathf.Abs(Convert.ToInt32(endPosition.z - startPosition.z));

        if (m_XIndex < xCount)
        {
            if (m_ZIndex < zCount)
            {
                List<Connection> newConnections = GetPotentialNodes(m_currentPosition);

                foreach (Connection connection in newConnections)
                {
                    Vector2 from = connection.GetFromNode().NodeId;
                    Vector2 to = connection.GetToNode().NodeId;

                    Debug.Log("Adding Connection Between: From:" + from + " TO:" + to);
                }

                if (newConnections.Count > 0)
                {
                    m_graph.AddRange(newConnections);
                }

                m_currentPosition.z -= 1f;
                m_ZIndex++;
            }
            else
            {
                m_ZIndex = 0;
                m_XIndex++;
                m_currentPosition.z += zCount;
                m_currentPosition.x -= 1f;
            }
        }
        else
        {
            m_ValidGraph = true;
        }
    }

    public List<Connection> GetSurroundingConnections(Node centerNode)
    {
        List<Connection> surroundingConnections = new List<Connection>();
        foreach (Connection connection in m_graph)
        {
            Node fromNode = connection.GetFromNode();
            if (fromNode.NodeId == centerNode.NodeId)
            {
                surroundingConnections.Add(connection);
            }
        }
        return surroundingConnections;
    }

    private bool IsPositionClear(Vector3 position)
    {
        if (Physics.OverlapSphere(position, 0.2f).Length == 0)
        {
            return true;
        }
        else
            return false;
    }

    private bool IsConnectionValid(Vector3 from, Vector3 to)
    {
        if (from == to)
            return false;

        // Check for collision at the TO node
        Collider[] obstructions = Physics.OverlapSphere(to, 0.2f);
        if (obstructions.Length > 0)
        {
            if (obstructions[0].tag != "Grid")
                return false;
        }

        return true;
    }

    private List<Connection> GetPotentialNodes(Vector3 searchPos)
    {
        List<Connection> potentialNodes = new List<Connection>();
        for (Direction m_direction = Direction.NORTH; m_direction < Direction.NUMOFDIRECTIONS; m_direction++)
        {
            Node node = new Node();

            // Calculate connection cost
            float newConnectionCost = ((int)m_direction % 2) == 0 ? 10 : 14;
            
            switch (m_direction)
            {
                case Direction.NORTH:
                    node.NodeId = new Vector2(searchPos.x, searchPos.z + 1);
                    break;
                case Direction.NORTHEAST:
                    node.NodeId = new Vector2(searchPos.x - 1, searchPos.z + 1);
                    break;
                case Direction.EAST:
                    node.NodeId = new Vector2(searchPos.x - 1, searchPos.z);
                    break;
                case Direction.SOUTHEAST:
                    node.NodeId = new Vector2(searchPos.x - 1, searchPos.z - 1);
                    break;
                case Direction.SOUTH:
                    node.NodeId = new Vector2(searchPos.x, searchPos.z - 1);
                    break;
                case Direction.SOUTHWEST:
                    node.NodeId = new Vector2(searchPos.x + 1, searchPos.z - 1);
                    break;
                case Direction.WEST:
                    node.NodeId = new Vector2(searchPos.x + 1, searchPos.z);
                    break;
                case Direction.NORTHWEST:
                    node.NodeId = new Vector2(searchPos.x + 1, searchPos.z + 1);
                    break;
                default:
                    break;
            }

            //BUG: finds a diagonal path through a wall

            if (IsPositionClear(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y)))
            {
                // Set FromNode
                Node fromNode = new Node();
                fromNode.NodeId = new Vector2(searchPos.x, searchPos.z);

                // Set ToNode
                Node toNode = node;

                // Debug: Add sphere to represent TO NODE
                createObj(new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y),
                    new Vector3(0.2f, 0.2f, 0.2f), m_material,
                    toNode.NodeId.x + " : " + toNode.NodeId.y, "Grid");

                // Create new connection node
                Connection newConnection = new Connection(newConnectionCost, fromNode, toNode);

                //Add potential node
                if (Vector2.Distance(fromNode.NodeId, toNode.NodeId) <= 1)
                    potentialNodes.Add(newConnection);
            }
            else
            {
                // Set FromNode
                Node fromNode = new Node();
                fromNode.NodeId = new Vector2(searchPos.x, searchPos.z);

                // Set ToNode
                Node toNode = node;

                bool connection = m_graph.Exists(c => c.GetFromNode().NodeId == fromNode.NodeId &&
                                                            c.GetToNode().NodeId == toNode.NodeId);
                if (!connection)//if wall
                {
                    if (IsConnectionValid(new Vector3(fromNode.NodeId.x, 0.5f, fromNode.NodeId.y),
                        new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y)))
                    {
                        // Create new connection node
                        Connection newConnection = new Connection(newConnectionCost, fromNode, toNode);

                        //Add potential node
                        potentialNodes.Add(newConnection);
                    }
                }
            }
        }
        return potentialNodes;
    }

    public void newNode(Node node)
    {
        Debug.Log("Found valid node");
        createObj(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), new Vector3(1, 1, 1), m_material, node.NodeId.x + " : " + node.NodeId.y);
    }

    public void newNode(Node node, Material mat, string tagname)
    {
        Debug.Log("Found valid node");
        createObj(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), new Vector3(1, 1, 1), mat, node.NodeId.x + " : " + node.NodeId.y, tagname);
    }

    void createObj(Vector3 pos, Vector3 scale, Material mat, string name, string tagname = "Path")
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
