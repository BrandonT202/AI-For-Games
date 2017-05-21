using UnityEngine;
using System.Collections.Generic;
using System;

class EnvironmentGraph
{
    enum Direction { NORTH = 0, NORTHEAST, EAST, SOUTHEAST, SOUTH, SOUTHWEST, WEST, NORTHWEST, NUMOFDIRECTIONS };

    [SerializeField]
    private Mesh m_mesh;

    [SerializeField]
    private Material m_material;

    /// <summary>
    /// A list of connections between all the nodes in the game environment
    /// </summary>
    private List<Connection> m_graph;

    private Vector3 m_currentPosition;
    private Vector3 startPosition;
    private Vector3 endPosition;

    // Real-time values
    int m_XIndex = 0;
    int m_ZIndex = 0;

    public bool m_ValidGraph { get; set; }

    public EnvironmentGraph(Mesh mesh, Material material)
    {
        this.m_mesh = mesh;
        this.m_material = material;
        m_graph = new List<Connection>();
    }

    /// <summary>
    /// Reset the start and end range nodes, clear the graph and set current position to the start 
    /// </summary>
    public void Reset()
    {
        m_ValidGraph = false;

        // find the bottom range value
        startPosition = GameObject.Find("RangeBottom").transform.position;

        // find the top range value
        endPosition = GameObject.Find("RangeTop").transform.position;

        // set current position to the start position
        m_currentPosition = startPosition;

        // remove any graph connections
        m_graph.Clear();
    }

    /// <summary>
    /// Render the connections in the graph in the scene view (for debugging)
    /// </summary>
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

    /// <summary>
    /// Creates the graph from a start position and an end position
    /// </summary>
    public void CreateGraphWithDiagonals()
    {
        // Reset graph for new graph creation
        Reset();

        int xCount = Mathf.Abs(Convert.ToInt32(endPosition.x - startPosition.x)) + 1;
        int zCount = Mathf.Abs(Convert.ToInt32(endPosition.z - startPosition.z)) + 1;
        Debug.Log("X-Count: " + xCount + " || " + "Z - Count: " + zCount);

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

                m_currentPosition.z += 1f;
            }
            m_currentPosition.z -= zCount;
            m_currentPosition.x += 1f;
        }
        m_ValidGraph = true;
    }

    /// <summary>
    /// Gets all the potential connections from the search position in eight directions
    /// </summary>
    /// <param name="searchPos"></param>
    /// <returns></returns>
    private List<Connection> GetPotentialNodes(Vector3 searchPos)
    {
        List<Connection> potentialNodes = new List<Connection>();
        for (Direction m_direction = Direction.NORTH; m_direction < Direction.NUMOFDIRECTIONS; m_direction++)
        {
            Node node = new Node();

            // Calculate connection cost
            float newConnectionCost = ((int)m_direction % 2) == 0 ? 10 : 14;

			// TODO: Connection Cost Scaling with Naive Bayes
			// index the prob table
			// if over prob threshold
			// scale the connection cost

            switch (m_direction)
            {
                case Direction.NORTH:
                    node.NodeId = new Vector2(searchPos.x, searchPos.z + 1);
                    break;
                case Direction.NORTHEAST:
                    node.NodeId = new Vector2(searchPos.x + 1, searchPos.z + 1);
                    break;
                case Direction.EAST:
                    node.NodeId = new Vector2(searchPos.x + 1, searchPos.z);
                    break;
                case Direction.SOUTHEAST:
                    node.NodeId = new Vector2(searchPos.x + 1, searchPos.z - 1);
                    break;
                case Direction.SOUTH:
                    node.NodeId = new Vector2(searchPos.x, searchPos.z - 1);
                    break;
                case Direction.SOUTHWEST:
                    node.NodeId = new Vector2(searchPos.x - 1, searchPos.z - 1);
                    break;
                case Direction.WEST:
                    node.NodeId = new Vector2(searchPos.x - 1, searchPos.z);
                    break;
                case Direction.NORTHWEST:
                    node.NodeId = new Vector2(searchPos.x - 1, searchPos.z + 1);
                    break;
                default:
                    break;
            }

            // take away any node that is not facing forward or to the right side of the starting node
            if ((node.NodeId.x < startPosition.x || node.NodeId.y < startPosition.z)
                || (node.NodeId.x > endPosition.x || node.NodeId.y > endPosition.z))
                continue;

            // Set FromNode
            Node fromNode = new Node();
            fromNode.NodeId = new Vector2(searchPos.x, searchPos.z);

            // Set ToNode
            Node toNode = node;

            if (IsPositionClear(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y)))
            {
                Debug.Log("Position CLEAR: " + node.NodeId.x + " : " + node.NodeId.y);

                // Debug: Add sphere to represent TO NODE
                createObj(new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y),
                    new Vector3(0.2f, 0.2f, 0.2f), m_material,
                    toNode.NodeId.x + " : " + toNode.NodeId.y, "Grid");

                Collider[] colliders = Physics.OverlapSphere(new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y), 0.3f);
                if (colliders.Length > 0)
                {
                    if (colliders[0].tag != "Grid")
                        break;
                }

                // Create new connection node
                Connection newConnection = new Connection(newConnectionCost, fromNode, toNode);

                //Add potential node
                potentialNodes.Add(newConnection);
            }
            else
            {
                bool connection = m_graph.Exists(c => c.GetFromNode().NodeId == fromNode.NodeId &&
                                                            c.GetToNode().NodeId == toNode.NodeId);
                if (!connection)//if wall
                {
                    if (IsConnectionValid(new Vector3(fromNode.NodeId.x, 0.5f, fromNode.NodeId.y),
                        new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y)))
                    {
                        Collider[] colliders = Physics.OverlapSphere(new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y), 0.3f);
                        if (colliders.Length > 0)
                        {
                            if ((colliders[0].tag != "Grid") & (colliders[0].tag != "Player"))
                                break;
                        }

                        colliders = Physics.OverlapSphere(new Vector3(fromNode.NodeId.x, 0.5f, fromNode.NodeId.y), 0.3f);
                        if (colliders.Length > 0)
                        {
                            if ((colliders[0].tag != "Grid") & (colliders[0].tag != "Player"))
                                break;
                        }

                        if (fromNode.NodeId.x == 2 && fromNode.NodeId.y == 2 && toNode.NodeId.x == 1 && toNode.NodeId.y == 1)
                        {
                            Debug.Log("unbelievable");
                        }

                        //Debug.Log("Position not CLEAR but added connection: " + node.NodeId.x + " : " + node.NodeId.y);

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

	/// <summary>
	/// Reals the time create graph without diagonals.
	/// </summary>
    public void RealTimeCreateGraphWithoutDiagonals()
    {
        GameObject start = GameObject.Find("RangeBottom");
        Vector3 startPosition = start.transform.position;

        GameObject end = GameObject.Find("RangeTop");
        Vector3 endPosition = end.transform.position;

        int xCount = Mathf.Abs(Convert.ToInt32(endPosition.x - startPosition.x));
        int zCount = Mathf.Abs(Convert.ToInt32(endPosition.z - startPosition.z));

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

    /// <summary>
    /// Checks if there is any object in the way
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool IsPositionClear(Vector3 position)
    {
        if (Physics.OverlapSphere(position, 0.2f).Length == 0)
        {
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Determines if the connection from->to is valid looking 
    /// for a grid tag on the collision object
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    private bool IsConnectionValid(Vector3 from, Vector3 to)
    {
        if (from == to)
            return false;

        // Check for collision at the TO node
        Vector3 direction = 2 * (to - from);
        RaycastHit hit;

        if (Physics.Raycast(from, direction, out hit))
        {
            if (hit.transform.tag != "Grid" && hit.transform.tag != "Player")
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get all surrounding connection from the center node
    /// </summary>
    /// <param name="centerNode"></param>
    /// <returns></returns>
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

	public Node FindClosestEstimatedNode(Node endNode, Heuristic heuristic)
	{
		Node returnNode = null;

		int prevEstimatedCost = 1000;
		foreach (var connection in m_graph) 
		{
			Node toNode = connection.GetToNode ();

			if (endNode.NodeId == toNode.NodeId) 
			{
				returnNode = toNode;
				break;
			}

			int estimate = (int)heuristic.Estimate (toNode);
			if (estimate < prevEstimatedCost) 
			{
				returnNode = toNode;
				prevEstimatedCost = estimate;
			}
		}

		Debug.Log ("Return Node: " + returnNode.NodeId.x + ":" + returnNode.NodeId.y);

		return returnNode;
	}

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
