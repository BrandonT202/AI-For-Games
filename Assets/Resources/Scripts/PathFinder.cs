using UnityEngine;
using System.Collections.Generic;

class PathFinder : MonoBehaviour
{
    private Node m_CurrentNode;
    
    private List<Node> m_OpenList;

    private List<Node> m_ClosedList;

    public PathFinder()
    {
        m_OpenList = new List<Node>();
        m_ClosedList = new List<Node>();
    }

    public List<Node> FindPathAStar(List<Vector2> graph, Node start, Node end, Heuristic heuristic)
    {
        // Initialise the start node
        Node startRecord = start;
        startRecord.ConnectionNode = null;
        startRecord.CostSoFar = 0;
        startRecord.EstimatedTotalCost = heuristic.Estimate(startRecord);

        // Add to start node to the open list
        m_OpenList.Add(startRecord);

        // Iterate through all open list nodes
        while (m_OpenList.Count > 0)
        {
            // Find the closest node and make it the current node
            foreach (Node node in m_OpenList)
            {
                if (m_OpenList.Count == 1 && m_CurrentNode == null)
                {
                    m_CurrentNode = node;
                    break;
                }
                else if (m_CurrentNode.EstimatedTotalCost > node.EstimatedTotalCost)
                {
                    m_CurrentNode = node;
                }
            }

            // If it is the goal node - terminate
            if (m_CurrentNode.NodeId == end.NodeId)
                break;

            // Otherwise get the nodes around the current node
            // TODO: Graph class that needs to return a list of near by nodes
            List<Node> connections = new List<Node>();

            // Value to be used later on...
            float endNodeHeuristic = 0f;

            // Iterate through all the connections
            foreach (Node node in connections)
            {
                Node endNode = node;
                float endNodeCost = m_CurrentNode.CostSoFar + node.CostSoFar;

                // If the node is in the closed list we may
                // have to skip or remove it
                if (m_ClosedList.Contains(endNode))
                {
                    // If the node is in the closed list
                    // get the node
                    Node closedNode = m_ClosedList.Find(n => n.NodeId == endNode.NodeId);

                    // If the route isn't shorter, skip the node
                    if (closedNode.CostSoFar <= endNodeCost)
                        continue;

                    // Otherwise remove it from the closed list
                    m_ClosedList.Remove(closedNode);

                    // Calculate end node heuristic
                    // [Cost to get from the current node to this node
                    float cost = m_CurrentNode.CostSoFar + 10f;
                    endNodeHeuristic = cost - node.CostSoFar;
                }
                // Skip if the node is open and we've 
                //found a better route
                else if (m_OpenList.Contains(endNode))
                {
                    // If the node is in the open list
                    // get the node
                    endNode = m_OpenList.Find(n => n.NodeId == endNode.NodeId);

                    // If the route isn't better, then skip the node
                    if (endNode.CostSoFar <= endNodeCost)
                        continue;

                    // Calculate end node heuristic
                    // [Cost to get from the current node to this node
                    float cost = m_CurrentNode.CostSoFar + 10f;
                    endNodeHeuristic = cost - node.CostSoFar;
                }
                // Otherwise we know we've got an unvisited node
                else
                {
                    // So make new node
                    endNode = node;

                    // Calculate end node heuristic using 
                    // the heuristic function
                    endNodeHeuristic = heuristic.Estimate(endNode);
                }

                // Update the node
                // Update the cost, estimate and connection
                endNode.EstimatedTotalCost = endNodeCost + endNodeHeuristic;

                if (!m_OpenList.Contains(endNode))
                    m_OpenList.Add(endNode);
            }

            // We are finished looking at connection 
            // from the current node therefore we 
            // remove it from the open list and add 
            // it to the closed list
            m_OpenList.Remove(m_CurrentNode);
            m_ClosedList.Add(m_CurrentNode);
        }

        // Either the goal has been found or
        // there are no more nodes to search
        if(m_CurrentNode.NodeId != end.NodeId)
        {
            // Ran out of nodes without finding
            // a solution, thus no solution
            return null;
        }
        else
        {
            // Compile a list of nodes 
            // to create the path
            List<Node> path = new List<Node>();
            List<Node> tempPath = new List<Node>();

            // Work back along the path, accumilating the nodes
            while (m_CurrentNode.NodeId != start.NodeId)
            {
                tempPath.Add(m_CurrentNode);
                m_CurrentNode = m_CurrentNode.ConnectionNode;
            }

            // Reverse the path and return it
            for (int i = tempPath.Count - 1; i >= 0; i--)
            {
                path.Add(tempPath[i]);
            }

            return path;
        }
    }
}
