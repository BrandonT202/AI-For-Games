using UnityEngine;

public class Connection
{
    private float m_ConnectionCost;
    private Node m_FromNode;
    private Node m_ToNode;

    public Connection()
    {
        m_ConnectionCost = 0f;
        m_FromNode = null;
        m_ToNode = null;
    }

    public Connection(float cost, Node from, Node to)
    {
        this.m_ConnectionCost = cost;
        this.m_FromNode = from; 
        this.m_ToNode = to;
    }

    //Returns non-negative cost of the connection
    public float GetCost()
    {
        return m_ConnectionCost;
    }

    // Returns the node that the connection ends on
    public Node GetToNode()
    {
        return m_ToNode;
    }

    public Node GetFromNode()
    {
        return m_FromNode;
    }
}
