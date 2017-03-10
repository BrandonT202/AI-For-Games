using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        m_graph.CreateGraphWithoutDiagonals();
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
        
        if (!m_graph.m_ValidGraph && timer > 0.01f)
        {
            timer = 0;
            //m_graph.RealTimeCreateGraphWithoutDiagonals();
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
            reMap();
     
            ResetPath();
            timer = 0f;
        }

        if (m_graph.m_ValidGraph && timer > .1f)
        {
            //m_graph.m_ValidGraph = false;
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
    public void reMap()
    {
        GameObject[] prevPath = GameObject.FindGameObjectsWithTag("Path");

        foreach (GameObject obj in prevPath)
        {
            Destroy(obj);
        }
        m_graph.CreateGraphWithoutDiagonals();
    }
}