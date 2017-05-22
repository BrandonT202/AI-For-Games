using UnityEngine;
using System.Collections.Generic;

public class SafetyNodeGraph
{
    public int m_size { get; private set; }

    private List<SafetyNode> m_graph;
    private PersistenceManager m_persistenceManager;

    public SafetyNodeGraph(int size)
    {
        this.m_size = size;

        m_graph = new List<SafetyNode>();
        createSafetyNodeGraph();
    }

    public void createSafetyNodeGraph()
    {
        Vector3 currentPosition = new Vector3();

        int xCount = m_size;
        int zCount = m_size;

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < zCount; j++)
            {
                SafetyNode newNode = new SafetyNode();
                newNode.NodeId = new Vector2(i, j);
                m_graph.Add(newNode);
                currentPosition.z += 1f;
            }
            currentPosition.z -= zCount;
            currentPosition.x += 1f;
        }
    }
}
