using UnityEngine;
using System.Collections.Generic;

public class AgentNavigation : MonoBehaviour
{
    enum Direction { NORTH = 0, EAST, SOUTH, WEST, NUMOFDIRECTIONS };

    // Open list for nodes currently able to be visited
    private List<Node> m_OpenList = new List<Node>();

    // Closed list for nodes visited
    public List<Node> m_ClosedList = new List<Node>();

    private EnvironmentGraph m_graph;

    private PathFinder m_pathFinder = new PathFinder();

    public int ListSize;

    // Starting Node
    private Node m_StartNode;

    // End Node
    private Node m_EndNode;

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

    /* 
     * SETUP
     * Add start node to the closed list
     * 
     * ADD NEW OPEN NODES
     * For each direction around the current node
     *      Detect any collisions
     *          if collision
     *              skip that node
     *          else
     *              Calculate heuristic cost and G cost (directional cost)
     *              add node to the open list
     * 
     * CHECK OPEN NODES AND ADD NODE TO CLOSED LIST
     * For each node in the open list 
     *      if current node F cost > node F cost 
     *          closest node = node
     *      else
     *          continue
    */

    void Start()
    {
        //failCounter = maxFailCounter;
        //m_StartNode = GameObject.FindWithTag("StartNode");
        //m_EndNode = GameObject.FindWithTag("EndNode");

        //Add start node to the closed list
        //int finalValue = (int)(Mathf.Abs(m_StartNode.transform.position.x) - Mathf.Abs(m_EndNode.transform.position.x) + Mathf.Abs(m_StartNode.transform.position.z) - Mathf.Abs(m_EndNode.transform.position.z));
        //m_ClosedList.Add(new Node
        //{
        //    NodeId = new Vector2(m_StartNode.transform.position.x, m_StartNode.transform.position.z),
        //    EndNode = false,
        //    FinalValue = finalValue,
        //    HeuristicCost = 1000
        //});
        //m_CurrentNode = m_ClosedList[0];

        m_graph = new EnvironmentGraph(mesh, nodeMat2);
        m_graph.Reset();

        ResetPath();

        CurrentNode = new GameObject();
        CurrentNode.transform.localScale *= 0.5f;
        CurrentNode.name = "CurrentNode";
        CurrentNode.AddComponent<MeshRenderer>();
        CurrentNode.AddComponent<MeshFilter>();
        CurrentNode.GetComponent<MeshFilter>().mesh = mesh;
        CurrentNode.GetComponent<MeshFilter>().mesh.name = "Sphere";
        CurrentNode.GetComponent<MeshRenderer>().material = CurrNodeMat;
    }

    private void ResetPath()
    {
        GameObject start = GameObject.FindWithTag("StartNode");
        GameObject end = GameObject.FindWithTag("EndNode");

        m_StartNode = new Node();
        Vector3 startPosition = start.transform.position;
        m_StartNode.NodeId = new Vector2(startPosition.x, startPosition.z);

        m_EndNode = new Node();
        Vector3 endPosition = end.transform.position;
        m_EndNode.NodeId = new Vector2(endPosition.x, endPosition.z);
    }

    //List<Node> GetPotentialNodes(Node searchFromNode)
    //{
    //    List<Node> potentialNodes = new List<Node>();
    //    for (Direction m_direction = Direction.NORTH; m_direction < Direction.NUMOFDIRECTIONS; m_direction++)
    //    {
    //        Node node = new Node();
    //        switch (m_direction)
    //        {
    //            case Direction.NORTH:
    //                node.NodeId = new Vector2(searchFromNode.NodeId.x, searchFromNode.NodeId.y + 1);
    //                break;
    //            case Direction.EAST:
    //                node.NodeId = new Vector2(searchFromNode.NodeId.x + 1, searchFromNode.NodeId.y);
    //                break;
    //            case Direction.SOUTH:
    //                node.NodeId = new Vector2(searchFromNode.NodeId.x, searchFromNode.NodeId.y - 1);
    //                break;
    //            case Direction.WEST:
    //                node.NodeId = new Vector2(searchFromNode.NodeId.x - 1, searchFromNode.NodeId.y);
    //                break;
    //            default:
    //                break;
    //        }

    //        if (Physics.OverlapSphere(new Vector3(node.NodeId.x, 0.5f, node.NodeId.y), 0.1f).Length == 0)
    //        {
    //            // Calculate h-value
    //            int hValue = (int)(Mathf.Abs(node.NodeId.x - m_EndNode.transform.position.x) + Mathf.Abs(node.NodeId.y - m_EndNode.transform.position.z));
    //            if (hValue < 0)
    //                Debug.Log("HVALUE: " + hValue);
    //            node.HeuristicCost = hValue;

    //            // Calculate final value
    //            node.FinalValue = node.HeuristicCost + m_DirectionCost;

    //            //Add potential node
    //            potentialNodes.Add(node);
    //        }
    //    }
    //    return potentialNodes;
    //}

    //void AddValidNodesToOpenList(List<Node> potentialNodes)
    //{
    //    List<Node> nodesToAdd = new List<Node>();
    //    foreach (Node potentialNode in potentialNodes)
    //    {
    //        // Add an "open node"
    //        createObj(new Vector3(potentialNode.NodeId.x, 0.5f, potentialNode.NodeId.y), new Vector3(0.2f, 0.2f, 0.2f), nodeMat2, "OPEN " + potentialNode.NodeId.x + " : " + potentialNode.NodeId.y);

    //        // Add open node to the list if it doesn't exist
    //        if (m_OpenList.Count != 0)
    //        {
    //            bool nodeExists = false;
    //            foreach (Node openNode in m_OpenList)
    //            {
    //                if (openNode.NodeId == potentialNode.NodeId)
    //                    nodeExists = true;
    //            }

    //            if (!nodeExists)
    //                nodesToAdd.Add(potentialNode);
    //        }
    //        else
    //        {
    //            m_OpenList.Add(potentialNode);
    //        }
    //    }

    //    // Add new unique nodes
    //    m_OpenList.AddRange(nodesToAdd);
    //}

    void FixedUpdate()
    {
        timer += Time.deltaTime;
        m_graph.DrawGraph();

        if (!m_graph.m_ValidGraph && timer > 0.01f)
        {
            timer = 0;
            m_graph.RealTimeCreateGraphWithoutDiagonals();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject start = GameObject.FindWithTag("StartNode");
                GameObject end = GameObject.FindWithTag("EndNode");
                start.transform.position = new Vector3(Random.Range(-10, 10), 0.5f, Random.Range(-10, 10));
                end.transform.position = new Vector3(Random.Range(-10, 10), 0.5f, Random.Range(-10, 10));
            }
            ResetPath();
            timer = 0f;
        }

        if (m_graph.m_ValidGraph && timer > 5f)
        {
            timer = 0f;

            GameObject[] prevPath = GameObject.FindGameObjectsWithTag("Path");

            foreach (GameObject obj in prevPath)
            {
                Destroy(obj);
            }

            List<Connection> path = m_pathFinder.FindPathAStar(m_graph, m_StartNode, m_EndNode, new Heuristic(m_EndNode));

            if (path == null)
            {
                Debug.Log("No Path was found this iteration");
            }
            else
            {
                Debug.Log("YES! A path was found this iteration");
                foreach (var connection in path)
                {
                    if (connection != null)
                        m_graph.newNode(connection.GetFromNode(), nodeMat, "Path");
                }
            }

            //    m_currentHeuristicCost = m_CurrentNode.HeuristicCost;

            //    timer += Time.deltaTime;
            //    if (timer >= 0.05f)
            //    {
            //        timer = 0;

            //        // Find potential nodes around the current node
            //        List<Node> potentialNodes = GetPotentialNodes(m_CurrentNode);

            //        // Only add nodes that aren't headed into a wall
            //        AddValidNodesToOpenList(potentialNodes);

            //        // CHECK OPEN NODES
            //        Node closestNode = new Node();
            //        closestNode.FinalValue = int.MaxValue;
            //        closestNode.NodeId = m_ClosedList[m_ClosedList.Count - 1].NodeId;
            //        foreach (Node listNode in m_OpenList)
            //        {
            //            // Find closest to end node
            //            if (listNode.FinalValue <= closestNode.FinalValue)
            //            {
            //                // If there isn't any node in the way
            //                if (Physics.OverlapSphere(new Vector3(listNode.NodeId.x, 0.5f, listNode.NodeId.y), 0.1f).Length == 0)
            //                {
            //                    closestNode = listNode;
            //                }
            //            }
            //        }

            //        // Remove the closest node from the open list 
            //        RemoveNodeFromOpenList(m_CurrentNode);

            //        // Have we reached the end node??
            //        if (m_CurrentNode.NodeId != new Vector2(m_EndNode.transform.position.x, m_EndNode.transform.position.z))
            //        {
            //            if (Physics.OverlapSphere(new Vector3(closestNode.NodeId.x, 0.5f, closestNode.NodeId.y), 0.1f).Length == 0)
            //            {
            //                // Add new node to scene
            //                newNode(closestNode);
            //            }
            //        }
            //        else//reached end goal [ Check the open list for a valid contender for closest node ]
            //            this.enabled = false;

            //        // Make closest node current node
            //        m_CurrentNode = closestNode;

            //        // Delete old current node indicator 
            //        GameObject tempObj = GameObject.Find("Current Position");
            //        if (tempObj != null)
            //            Destroy(tempObj);

            //        // Add new current node indicator
            //        createObj(new Vector3(m_currentPos.x, 1.5f, m_currentPos.y), new Vector3(0.4f, 0.4f, 0.4f), CurrNodeMat, "Current Position");

            //        m_currentPos = m_CurrentNode.NodeId;
            //        ListSize = m_ClosedList.Count;
            //    }
        }
    }

    void RemoveNodeFromOpenList(Node node)
    {
        m_ClosedList.Add(node);

        Node removeNode = new Node();
        foreach (var listNode in m_OpenList) //TODO: Identified problem with the open list - redundant node inside REMOVE
        {
            if (listNode.NodeId == node.NodeId)
            {
                GameObject tempObj = GameObject.Find(listNode.NodeId.x + " : " + listNode.NodeId.y);

                if (tempObj != null)
                {
                    tempObj.GetComponent<MeshRenderer>().material = nodeMat2;
                    Destroy(tempObj);
                }
                removeNode = listNode;
                break;
            }
        }

        m_OpenList.Remove(removeNode);
    }
}


//            // Add current node to closed list
//            if (m_CurrentNode.NodeId != new Vector2(m_EndNode.transform.position.x, m_EndNode.transform.position.z))
//            {
//                newNode(closestNode);

//                if (/*m_CurrentNode.NodeId != m_ClosedList[m_ClosedList.Count - 1].NodeId*/m_OpenList.Count > 0 || m_ClosedList.Count< 2)
//                {
//                }
//                /*else//dead end
//                {
//                    Debug.Log("Dead End");
//                    GameObject tempObj =  GameObject.Find(m_ClosedList[m_ClosedList.Count - 1].NodeId.x + " : " + m_ClosedList[m_ClosedList.Count - 1].NodeId.y);
//                    tempObj.GetComponent<MeshRenderer>().material = nodeMat2;
//                    m_ClosedList.Remove(m_ClosedList[m_ClosedList.Count - 1]);
//                    closestNode = m_ClosedList[m_ClosedList.Count - 1];
//                }*/
//            }
//            else//reached end goal [ Check the open list for a valid contender for closest node ]
//                this.enabled = false;


//        }