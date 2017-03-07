using UnityEngine;
using System.Collections;

public class Heuristic : MonoBehaviour
{
    private Node m_goalNode;
    public Heuristic(Node goal)
    {
        this.m_goalNode = goal;
    }

    public float Estimate(Node node)
    {
        return Vector2.Distance(m_goalNode.NodeId, node.NodeId);
    }
}