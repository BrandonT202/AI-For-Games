using UnityEngine;
using System.Collections.Generic;

public class Agent : MonoBehaviour
{
    private AgentNavigation m_agentNavigation;

    private List<Connection> m_path;

    private float timer;

    // Use this for initialization
    void Start()
    {
        m_agentNavigation = gameObject.GetComponent<AgentNavigation>();
    }


    void Update()
    {
        timer += Time.deltaTime;

		if (timer > 0.01f) {
			timer = 0f;

			// Follow path
			if (m_agentNavigation.m_ValidToMove) 
			{
				m_agentNavigation.followPath ();
			}
		}
    }
}
