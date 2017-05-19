using UnityEngine;
using System.Collections.Generic;
using System;

public class AgentNavigation : MonoBehaviour
{
    // Open list for nodes currently able to be visited
    private List<Node> m_OpenList = new List<Node>();

    // Closed list for nodes visited
    public List<Node> m_ClosedList = new List<Node>();

    // A non-negative graph for traversing the environment
    private EnvironmentGraph m_graph;

    // A path finder object that encapsulates the A* algorithm
    private PathFinder m_pathFinder = new PathFinder();

    // Starting Node
    private Node m_StartNode;

    // End Node
    private Node m_EndNode;

    public Mesh mesh;

    private GameObject m_CurrentNode;

    public Material nodeMat;
    public Material nodeMat2;
    public Material CurrNodeMat;
    private float timer;

    List<Connection> m_path;

    public float m_fromDistance;
    public float m_toDistance;

    bool m_IsMovingToDestinationNode = false;
    bool m_IsAtEndNode = false;
    Connection currentConnection = null;

    void Start()
    {
        m_graph = new EnvironmentGraph(mesh, nodeMat2);
        m_graph.Reset();

        ResetPath();

        m_CurrentNode = new GameObject();
        m_CurrentNode.transform.localScale *= 0.5f;
        m_CurrentNode.name = "CurrentNode";
        m_CurrentNode.AddComponent<MeshRenderer>();
        m_CurrentNode.AddComponent<MeshFilter>();
        m_CurrentNode.GetComponent<MeshFilter>().mesh = mesh;
        m_CurrentNode.GetComponent<MeshFilter>().mesh.name = "Sphere";
        m_CurrentNode.GetComponent<MeshRenderer>().material = CurrNodeMat;

        DateTime startTime = DateTime.Now;
        m_graph.CreateGraphWithDiagonals();
        int afterGraphTime = startTime.Millisecond - DateTime.Now.Millisecond;
        Debug.Log("Time to load graph: " + "[" + afterGraphTime + "]");
    }

    private void ResetPath()
    {
        // find the start node
        GameObject start = GameObject.FindWithTag("StartNode");

        m_StartNode = new Node();
        Vector3 startPosition = gameObject.transform.position;
        m_StartNode.NodeId = new Vector2(startPosition.x, startPosition.z);

        // find the end node
        GameObject end = GameObject.FindWithTag("EndNode");

        m_EndNode = new Node();
        Vector3 endPosition = end.transform.position;
        m_EndNode.NodeId = new Vector2(endPosition.x, endPosition.z);
    }

    void Update()
    {
        m_graph.DrawGraph(); // DEBUG GRID

        if (m_path != null)
            drawPath(); // DEBUG PATH
    }

    void FixedUpdate()
    {
        timer += Time.deltaTime;

        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    timer = 0f;
        //    GameObject start = GameObject.FindWithTag("StartNode");
        //    GameObject end = GameObject.FindWithTag("EndNode");
        //    GameObject rangeTop = GameObject.Find("RangeTop");
        //    GameObject rangeBottom = GameObject.Find("RangeBottom");
        //    Vector3 topPos = rangeTop.transform.position;
        //    Vector3 bottomPos = rangeBottom.transform.position;
        //    start.transform.position = new Vector3(Random.Range((int)topPos.x, (int)topPos.z), 1.5f, Random.Range((int)bottomPos.x, (int)bottomPos.z));
        //    end.transform.position = new Vector3(Random.Range((int)bottomPos.x, (int)bottomPos.z), 1.5f, Random.Range((int)topPos.x, (int)topPos.z));

        //    transform.position = new Vector3(start.transform.position.x, transform.position.y, start.transform.position.z);
        //    RegenerateGrid();
        //    ResetPath();
        //}

        if (timer > 0.5f && Input.GetKeyDown(KeyCode.T))
        {
            timer = 0f;
            Debug.Log("Button Pressed");
            //m_graph.CreateGraphWithDiagonals();

            ResetPath();

            if (m_graph.m_ValidGraph)
            {
                Debug.Log("Calculating Path using A*");
                m_path = m_pathFinder.FindPathAStar(m_graph, m_StartNode, m_EndNode, new Heuristic(m_EndNode));
            }

            //Debug.Log("Is graph valid? [" + m_graph.m_ValidGraph + "]");
            //Debug.Log("Is start node valid? [" + (m_StartNode != null) + "]");
            //Debug.Log("Is end node valid? [" + (m_EndNode != null) + "]");
        }

        //if (m_graph.m_ValidGraph && timer > 5f)
        //{
        //    //m_graph.m_ValidGraph = false;
        //    timer = 0f;

        //    //if (!m_IsMovingToDestinationNode)
        //    //{
        //    //    Debug.Log("Deleting Path + Recalulating Path");

        //    //    deleteOldPath();

        //    //    RegenerateGrid();

        //    //    if (m_path == null)
        //    //    {
        //    //        Debug.Log("No Path was found this iteration");
        //    //    }
        //    //    else
        //    //    {
        //    //        Debug.Log("YES! A path was found this iteration");

        //    //        m_path.RemoveAll(c => c == null);

        //    //        foreach (var connection in m_path)
        //    //        {
        //    //            if (connection != null)
        //    //                m_graph.newNode(connection.GetFromNode(), nodeMat, "Path");
        //    //        }

        //    //        // Set agents current position
        //    //        if (m_path[0] != null)
        //    //        {
        //    //            gameObject.transform.position = new Vector3(m_path[0].GetFromNode().NodeId.x, 1.0f, m_path[0].GetFromNode().NodeId.x);
        //    //        }
        //    //    }
        //    //}
        //}
    }

    public void drawPath()
    {
        foreach (Connection connection in m_path)
        {
            if (connection == null)
            {
                Debug.Log("Connection in path is null");
            };

            Node fromNode = connection.GetFromNode();
            Node toNode = connection.GetToNode();

            //Debug: Add Line to represent the connections
            Vector3 from = new Vector3(fromNode.NodeId.x, 0.5f, fromNode.NodeId.y);
            Vector3 to = new Vector3(toNode.NodeId.x, 0.5f, toNode.NodeId.y);
            Debug.DrawLine(from, to, Color.green);
        }
    }

    public void followPath()
    {
        if (m_path == null)
        {
            Debug.Log("Path is null.");
            return;
        }

        if (m_graph.m_ValidGraph)
        {
            /*
                find current position
                find connection to start from
                find to node
                lerp current pos to ToNode pos
                check for near ToNode
                    if at ToNode
                        next node
                    else if ToNode == EndNode
                        recalculate path
             */
            Vector3 currentPosition = gameObject.transform.position;
            Debug.Log("Following path using a valid graph");
            if (!m_IsMovingToDestinationNode)
            {
                currentConnection = m_path[0] ?? m_path[1];
                Debug.Log("!m_IsMovingToDestinationNode" + currentConnection);
                foreach (Connection connection in m_path)
                {
                    if (connection == null)
                        continue;

                    Vector2 fromNodePos = connection.GetFromNode().NodeId;
                    Vector2 toNodePos = connection.GetToNode().NodeId;

                    float fromDist = Vector2.Distance(fromNodePos, new Vector2(currentPosition.x, currentPosition.z));
                    float toDist = Vector2.Distance(toNodePos, new Vector2(currentPosition.x, currentPosition.z));

                    if (fromDist < 0.2f && toDist < 1.5f)
                    {
                        m_IsMovingToDestinationNode = true;
                        currentConnection = connection;
                        m_fromDistance = Vector2.Distance(fromNodePos, new Vector2(currentPosition.x, currentPosition.z));
                        m_toDistance = Vector2.Distance(toNodePos, new Vector2(currentPosition.x, currentPosition.z));
                    }
                }
            }

            if (!m_IsAtEndNode && m_IsMovingToDestinationNode && currentConnection != null)
            {
                Debug.Log("!m_IsAtEndNode AND m_IsMovingToDestinationNode AND currentConnection != null");
                Vector2 toNodePosition = currentConnection.GetToNode().NodeId;

                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, new Vector3(toNodePosition.x, 0.5f, toNodePosition.y), Time.deltaTime * 4.0f);

                // Check for closing in on ToNode
                Vector2 fromNodePosition = currentConnection.GetFromNode().NodeId;

                float fromDis = Vector2.Distance(fromNodePosition, new Vector2(currentPosition.x, currentPosition.z));
                float toDis = Vector2.Distance(toNodePosition, new Vector2(currentPosition.x, currentPosition.z)); ;

                if (toDis < 0.2f)
                {
                    gameObject.transform.position = new Vector3(toNodePosition.x, 0.5f, toNodePosition.y);
                    m_IsMovingToDestinationNode = false;
                }

                bool lastConnectionIsDestination = currentConnection.GetToNode().NodeId == m_path[m_path.Count - 1].GetToNode().NodeId;
                bool atEndNode = toDis < 0.05f;
                if (lastConnectionIsDestination && atEndNode)
                {
                    // Set start node as current position
                    // Get random or predicatable end node
                    ResetPath();
                    RegenerateGrid();
                    m_IsAtEndNode = true;
                    Debug.Log("AGENT AT END NODE");
                }
            }
        }
        else
        {
            Debug.Log("Graph is invalid");
        }
    }

    private void deleteOldPath()
    {
        GameObject[] prevPath = GameObject.FindGameObjectsWithTag("Path");

        foreach (GameObject obj in prevPath)
        {
            Destroy(obj);
        }
    }

    public void RegenerateGrid()
    {
        m_graph.CreateGraphWithDiagonals();
    }
}