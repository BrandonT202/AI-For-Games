using UnityEngine;
using System.Collections.Generic;

class EnvironmentGraph
{
    enum Direction { NORTH = 0, EAST, SOUTH, WEST, NUMOFDIRECTIONS };

    public Mesh mesh;
    public Material nodeMat;
    private List<Node> m_graph;
    Vector2 currentPosition;

    public EnvironmentGraph(Mesh mesh, Material material)
    {
        this.mesh = mesh;
        this.nodeMat = material;
    }

    public void CreateGraphWithoutDiagonals()
    {
        GameObject start = GameObject.FindWithTag("StartNode");
        GameObject end = GameObject.FindWithTag("EndNode");
        Node startNode = new Node();
        Vector3 startPosition = start.transform.position;
        startNode.NodeId = new Vector2(startPosition.x, startPosition.z);

        Node endNode = new Node();
        Vector3 endPosition = end.transform.position;
        endNode.NodeId = new Vector2(endPosition.x, endPosition.z);

        currentPosition = startNode.NodeId;
        List<Connection> connectionList = GetPotentialNodes(currentPosition);
        //while(currentPosition != endNode.NodeId)
        //{
        /* 
         * Currently at start node
         * Find positions to move into from current pos
         * Validate positions with a collision check
         *  if there isnt a collision
         *      Add a connection
         *      Add from node = current
         *      Add to node = possible node
         *      Add connection cost = 10 [Horizonal only]
         *  else Skip
        */
        //}
    }

    private List<Connection> GetPotentialNodes(Vector3 searchPos)
    {
        List<Connection> potentialNodes = new List<Connection>();
        for (Direction m_direction = Direction.NORTH; m_direction < Direction.NUMOFDIRECTIONS; m_direction++)
        {
            Node node = new Node();
            switch (m_direction)
            {
                case Direction.NORTH:
                    node.NodeId = new Vector2(searchPos.x, searchPos.z + 1);
                    break;
                case Direction.EAST:
                    node.NodeId = new Vector2(searchPos.x + 1, searchPos.z);
                    break;
                case Direction.SOUTH:
                    node.NodeId = new Vector2(searchPos.x, searchPos.z - 1);
                    break;
                case Direction.WEST:
                    node.NodeId = new Vector2(searchPos.x - 1, searchPos.z);
                    break;
                default:
                    break;
            }

            if (Physics.OverlapSphere(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), 0.1f).Length == 0)
            {
                // Calculate connection cost
                float newConnectionCost = 10;

                // Set FromNode
                Node fromNode = new Node();
                fromNode.NodeId = new Vector2(searchPos.x, searchPos.z);

                // Set ToNode
                Node toNode = node;

                // Debug: Add sphere to represent to node
                createObj(new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y), new Vector3(0.2f, 0.2f, 0.2f), nodeMat, toNode.NodeId.x + " : " + toNode.NodeId.y);

                // Create new connection node
                Connection newConnection = new Connection(newConnectionCost, fromNode, toNode);

                //Add potential node
                potentialNodes.Add(newConnection);
            }
        }
        return potentialNodes;
    }

    void newNode(Node node)
    {
        Debug.Log("Found valid node");
        createObj(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), new Vector3(1, 1, 1), nodeMat, node.NodeId.x + " : " + node.NodeId.y);
    }

    void createObj(Vector3 pos, Vector3 scale, Material mat, string name)
    {

        GameObject obj = new GameObject();
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.name = name;
        obj.AddComponent<SphereCollider>();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshFilter>().mesh.name = "Sphere";
        obj.GetComponent<MeshRenderer>().material = mat;
    }
}
