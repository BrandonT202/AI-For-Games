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

    // Use this for initialization
    void Start ()
    {
        m_StartNode = GameObject.FindWithTag("StartNode");
        m_EndNode = GameObject.FindWithTag("EndNode");

        m_ClosedList.Add(new Node
        {
            NodeId = 0,
            EndNode = false,
            FValue = 0,
            HeuristicCost = 0
        });
	}
	
	void FixedUpdate ()
    {
        
	}
}
