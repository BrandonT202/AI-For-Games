﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AgentNavigation : MonoBehaviour
{
	private const int m_defaultSearchRange = 2;

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

	public bool m_ValidToMove
	{
		get 
		{
			return (m_path != null && m_path.Count > 0);
		}
		private set{ }
	}


	public int m_searchRange;

    public float m_fromDistance;
    public float m_toDistance;

    bool m_IsMovingToDestinationNode = false;
    bool m_IsAtEndNode = false;
    Connection currentConnection = null;

    void Start()
    {
        m_graph = new EnvironmentGraph(mesh, nodeMat2);
        m_graph.Reset();

        m_CurrentNode = new GameObject();
        m_CurrentNode.transform.localScale *= 0.5f;
        m_CurrentNode.name = "CurrentNode";
        m_CurrentNode.AddComponent<MeshRenderer>();
        m_CurrentNode.AddComponent<MeshFilter>();
        m_CurrentNode.GetComponent<MeshFilter>().mesh = mesh;
        m_CurrentNode.GetComponent<MeshFilter>().mesh.name = "Sphere";
        m_CurrentNode.GetComponent<MeshRenderer>().material = CurrNodeMat;

		StartCoroutine(RegenerateGrid());

		m_searchRange = 2;
    }

	private IEnumerator ResetPath()
    {
		yield return deleteOldPath();

		// path becomes invalid
		if (m_path != null) 
		{
			m_path.Clear ();
			m_path.RemoveAll(c => c == null);
		}

		m_IsAtEndNode = false;

        // set start node as the current agent position
        m_StartNode = new Node();
        Vector3 startPosition = gameObject.transform.position;
        m_StartNode.NodeId = new Vector2(startPosition.x, startPosition.z);

        // find the end node
        GameObject end = GameObject.FindWithTag("EndNode");

        m_EndNode = new Node();
        Vector3 endPosition = end.transform.position;
        m_EndNode.NodeId = new Vector2(endPosition.x, endPosition.z);
    }

    void FixedUpdate()
    {
        timer += Time.deltaTime;

		/*
		 * if at end node, the path has reached a point where it needs a new 
		 * destination and to recalculate the desired path
		*/

		if (m_IsAtEndNode) 
		{
			Debug.Log ("Need a new destination");

			// find new destination

			StartCoroutine(RegenerateGrid());
		}

		if (timer > 1f)
        {
            timer = 0f;

			if (m_graph.m_ValidGraph && !m_ValidToMove) 
			{
				m_path = m_pathFinder.FindPathRealTimeAStar (m_graph, m_StartNode, m_EndNode, new Heuristic (m_EndNode));

				if (m_path != null) 
				{
					m_searchRange = m_defaultSearchRange;
				}
			}

			if(m_path == null)
			{
				m_searchRange++;
				StartCoroutine (RegenerateGrid ());
			}
        }

		// Rendering
		if(m_graph.m_ValidGraph)
		{
			m_graph.DrawGraph(); // DEBUG GRID
		}

		if (m_path != null)
		{
			if(m_path.Count > 1)
			{
				drawPath(); // DEBUG PATH
			}
		}
	}

    public void drawPath()
    {
        foreach (Connection connection in m_path)
        {
            if (connection == null)
            {
                Debug.Log("Connection in path is null");
				continue;
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
			Debug.Log ("Path is null.");
			return;
		} 
		else if(m_path.Count > 0) 
		{
			bool containsNulls = m_path.Contains (null);
			if (containsNulls) 
			{
				return;
			}
		}

		if (m_IsAtEndNode) 
		{
			Debug.Log ("Agent is at end node...");
			return;
		}

		if (m_ValidToMove)
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
            if (!m_IsMovingToDestinationNode)
            {
				currentConnection = m_path [0];

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

			if (!m_IsAtEndNode && m_IsMovingToDestinationNode)
            {
                Vector2 toNodePosition = currentConnection.GetToNode().NodeId;

                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, new Vector3(toNodePosition.x, 0.5f, toNodePosition.y), Time.deltaTime * 4.0f);

				// Check for closing in on ToNode
                float toDis = Vector2.Distance(toNodePosition, new Vector2(currentPosition.x, currentPosition.z)); ;

				bool lastConnectionIsDestination = currentConnection.GetToNode().NodeId == m_path[m_path.Count - 1].GetToNode().NodeId;
				float toEndDis = Vector2.Distance(m_path[m_path.Count - 1].GetToNode().NodeId, new Vector2(currentPosition.x, currentPosition.z));
				bool atEndNode = toEndDis < 0.05f;
                if (lastConnectionIsDestination && atEndNode)
                {
                    // Set start node as current position
                    // Get random or predicatable end node
                    m_IsAtEndNode = true;
                }

				if (toDis < 0.05f)
				{
					gameObject.transform.position = new Vector3(toNodePosition.x, 0.5f, toNodePosition.y);
					m_IsMovingToDestinationNode = false;
				}
            }
        }
        else
        {
            Debug.Log("Graph is invalid");
        }
    }

	private IEnumerator deleteOldPath()
    {
        GameObject[] prevGrid = GameObject.FindGameObjectsWithTag("Grid");

		foreach (var node in prevGrid) 
		{
			Destroy (node);
		}

		yield return new WaitForEndOfFrame();
    }

	public IEnumerator RegenerateGrid()
    {
		yield return ResetPath ();

		Vector3 currentPosition = gameObject.transform.position;

		// find the bottom range value
		GameObject rangeBottom = GameObject.Find("RangeBottom");

		// find the top range value
		GameObject rangeTop = GameObject.Find("RangeTop");

		rangeBottom.transform.position = new Vector3 (currentPosition.x - m_searchRange, currentPosition.y, currentPosition.z - m_searchRange);

		rangeTop.transform.position = new Vector3 (currentPosition.x + m_searchRange, currentPosition.y, currentPosition.z + m_searchRange);

        m_graph.CreateGraphWithDiagonals();
    }
}