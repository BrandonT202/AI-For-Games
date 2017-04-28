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

    public Mesh mesh;

    private GameObject CurrentNode;

    public Material nodeMat;
    public Material nodeMat2;
    public Material CurrNodeMat;
    float timer;

    List<Connection> m_path;

    public float fromDistance;
    public float toDistance;

    bool m_IsMovingToDestinationNode = false;
    bool m_IsAtEndNode = false;
    Connection currentConnection = null;

    void Start()
    {
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
        m_graph.CreateGraphWithDiagonals();
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


    void FixedUpdate()
    {
        timer += Time.deltaTime;
        m_graph.DrawGraph(); // DEBUG GRID

        if (Input.GetKeyDown(KeyCode.R))
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject start = GameObject.FindWithTag("StartNode");
                GameObject end = GameObject.FindWithTag("EndNode");
                start.transform.position = new Vector3(Random.Range(-10, 10), 0.5f, Random.Range(-10, 10));
                end.transform.position = new Vector3(Random.Range(-10, 10), 0.5f, Random.Range(-10, 10));
            }
            RegenerateGrid();

            ResetPath();
            timer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            timer = 0f;

            if (m_graph.m_ValidGraph)
            {
                Debug.Log("Calculating Path using A*");
                m_path = m_pathFinder.FindPathAStar(m_graph, m_StartNode, m_EndNode, new Heuristic(m_EndNode));
            }

            Debug.Log("Is graph valid? [" + m_graph.m_ValidGraph + "]");
            Debug.Log("Is start node valid? [" + (m_StartNode != null) + "]");
            Debug.Log("Is end node valid? [" + (m_EndNode != null) + "]");
        }

        //if (m_graph.m_ValidGraph && timer > 5f)
        //{
        //    //m_graph.m_ValidGraph = false;
        //    timer = 0f;

        //    if (!m_IsMovingToDestinationNode)
        //    {
        //        Debug.Log("Deleting Path + Recalulating Path");

        //        deleteOldPath();

        //        RegenerateGrid();

        //        if (m_path == null)
        //        {
        //            Debug.Log("No Path was found this iteration");
        //        }
        //        else
        //        {
        //            Debug.Log("YES! A path was found this iteration");

        //            m_path.RemoveAll(c => c == null);

        //            foreach (var connection in m_path)
        //            {
        //                if (connection != null)
        //                    m_graph.newNode(connection.GetFromNode(), nodeMat, "Path");
        //            }

        //            // Set agents current position
        //            if (m_path[0] != null)
        //            {
        //                gameObject.transform.position = new Vector3(m_path[0].GetFromNode().NodeId.x, 1.0f, m_path[0].GetFromNode().NodeId.x);
        //            }
        //        }
        //    }
        //}
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
                Debug.Log("!m_IsMovingToDestinationNode");
                foreach (Connection connection in m_path)
                {
                    if (connection == null)
                    {
                        Debug.Log("Connection is null...");
                        continue;
                    }

                    if (connection.GetFromNode() == null)
                        continue;

                    if (connection.GetToNode() == null)
                        return;

                    Vector2 fromNodePos = connection.GetFromNode().NodeId;
                    Vector2 toNodePos = connection.GetToNode().NodeId;

                    float fromDist = Vector2.Distance(fromNodePos, new Vector2(currentPosition.x, currentPosition.z));
                    float toDist = Vector2.Distance(toNodePos, new Vector2(currentPosition.x, currentPosition.z));

                    if (fromDist < 1.5f && toDist < 1.5f)
                    {
                        m_IsMovingToDestinationNode = true;
                        currentConnection = connection;
                        fromDistance = Vector2.Distance(fromNodePos, new Vector2(currentPosition.x, currentPosition.z));
                        toDistance = Vector2.Distance(toNodePos, new Vector2(currentPosition.x, currentPosition.z));
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

                if (currentConnection.GetToNode().NodeId == m_path[m_path.Count - 1].GetToNode().NodeId)
                {
                    // Set start node as current position
                    // Get random or predicatable end node
                    RegenerateGrid();
                    ResetPath();
                    m_IsAtEndNode = true;
                    Debug.Log("AGENT AT END NODE");
                }
            }
            else
            {
                Debug.Log("Current Connection is null...");
            }
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