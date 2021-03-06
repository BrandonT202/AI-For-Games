﻿using UnityEngine;
using System.Collections;

public class Heuristic
{
    private Node m_goalNode;
    public Heuristic(Node goal)
    {
        this.m_goalNode = goal;
    }

    public float Estimate(Node node)
    {
        // Manhattan distance
        return Mathf.Abs(node.NodeId.x - m_goalNode.NodeId.x) + Mathf.Abs(node.NodeId.y - m_goalNode.NodeId.y);
    }
}