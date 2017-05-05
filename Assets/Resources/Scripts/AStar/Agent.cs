using UnityEngine;
using System.Collections.Generic;

public class Agent : MonoBehaviour 
{
    private AgentNavigation m_agentNavigation;

    private float timer;

    private List<Connection> m_path;

    // Use this for initialization
    void Start () 
    {
        m_agentNavigation = gameObject.GetComponent<AgentNavigation>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        timer += Time.deltaTime;

        if(timer > 0.1f)
        {
            timer = 0f;
            // Follow path
            if (Input.GetKey(KeyCode.F))
            {
                m_agentNavigation.followPath();
            }
        }

        if (timer > 5f) // Potentially a state change or decision change
        {
            // Get new path
        }
    }
}
