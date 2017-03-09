using UnityEngine;
using System.Collections.Generic;

class PathFinder
{
    struct NodeRecord
    {
        public Node Node { get; set; }
        public Connection Connection { get; set; }
        public float Cost { get; set; }
        public float CostSoFar { get; set; }
        public float EstimatedTotalCost { get; set; }
    }

    private NodeRecord m_Current;

    private List<NodeRecord> m_OpenList;

    private List<NodeRecord> m_ClosedList;

    public PathFinder()
    {
        m_Current = new NodeRecord();
        m_OpenList = new List<NodeRecord>();
        m_ClosedList = new List<NodeRecord>();
    }

    public List<Connection> FindPathAStar(EnvironmentGraph graph, Node start, Node end, Heuristic heuristic)
    {
        m_OpenList.Clear();
        m_ClosedList.Clear();

        // Initialise the start node
        NodeRecord startRecord = new NodeRecord();
        startRecord.Node = start;
        startRecord.Connection = null;
        startRecord.CostSoFar = 0;
        startRecord.EstimatedTotalCost = heuristic.Estimate(start);

        // Add to start node to the open list
        m_OpenList.Add(startRecord);

        // Iterate through all open list nodes
        while (m_OpenList.Count > 0)
        {
            // Find the closest node and make it the current node
            if (m_OpenList.Count == 1)
            {
                m_Current = m_OpenList[0];
            }
            else
            {
                NodeRecord previousRecord = m_OpenList[0];
                foreach (NodeRecord record in m_OpenList)
                {
                    if (record.EstimatedTotalCost <= previousRecord.EstimatedTotalCost)
                    {
                        m_Current = record;
                    }
                }
            }

            // If it is the goal node - terminate
            if (m_Current.Node.NodeId == end.NodeId)
                break;

            // Otherwise get the nodes around the current node
            // TODO: Graph class that needs to return a list of near by nodes
            List<Connection> connections = graph.GetSurroundingConnections(m_Current.Node);

            // Value to be used later on...
            float endNodeHeuristic = 0f;

            // Iterate through all the connections
            foreach (Connection connection in connections)
            {
                Node endNode = connection.GetToNode();
                float endNodeCost = m_Current.CostSoFar + connection.GetCost();

                NodeRecord endNodeRecord = new NodeRecord();
                // If the node is in the closed list we may
                // have to skip or remove it
                if (m_ClosedList.Exists(nr => nr.Node.NodeId == endNode.NodeId))
                {
                    //If the node is in the closed list
                    //get the node
                    endNodeRecord = m_ClosedList.Find(nr => nr.Node.NodeId == endNode.NodeId);

                    //If the route isn't shorter, skip the node
                    if (endNodeRecord.CostSoFar <= endNodeCost)
                        continue;

                    //Otherwise remove it from the closed list
                    m_ClosedList.Remove(endNodeRecord);

                    //Calculate end node heuristic
                    //[Cost to get from the current node to this node]
                    endNodeHeuristic = endNodeRecord.Cost - endNodeRecord.CostSoFar;
                }
                // Skip if the node is open and we've 
                //found a better route
                else if (m_OpenList.Exists(nr => nr.Node.NodeId == endNode.NodeId))
                {
                    //If the node is in the open list
                    //get the node
                    endNodeRecord = m_OpenList.Find(nr => nr.Node.NodeId == endNode.NodeId);

                    //If the route isn't better, then skip the node
                    if (endNodeRecord.CostSoFar <= endNodeCost)
                        continue;

                    //Calculate end node heuristic
                    //[Cost to get from the current node to this node]
                    endNodeHeuristic = endNodeRecord.Cost - endNodeRecord.CostSoFar;
                }
                // Otherwise we know we've got an unvisited node
                else
                {
                    //So make a new node
                    endNodeRecord.Node = endNode;

                    //Calculate end node heuristic using
                    //the heuristic function
                    endNodeHeuristic = heuristic.Estimate(endNode);
                }

                // Update the node
                // Update the cost, estimate and connection
                endNodeRecord.Cost = endNodeCost;
                endNodeRecord.CostSoFar += endNodeCost;
                endNodeRecord.Connection = connection;
                endNodeRecord.EstimatedTotalCost = endNodeCost + endNodeHeuristic;

                if (!m_OpenList.Contains(endNodeRecord))
                    m_OpenList.Add(endNodeRecord);
            }

            // We are finished looking at connections 
            // from the current node therefore we 
            // remove it from the open list and add 
            // it to the closed list
            m_OpenList.Remove(m_Current);
            m_ClosedList.Add(m_Current);
        }

        // Either the goal has been found or
        // there are no more nodes to search
        if (m_Current.Node.NodeId != end.NodeId)
        {
            // Ran out of nodes without finding
            // a solution, thus no solution
            return null;
        }
        else
        {
            // Compile a list of nodes 
            // to create the path
            List<Connection> path = new List<Connection>();
            List<Connection> tempPath = new List<Connection>();

            NodeRecord previousRecord = m_Current;

            // Work back along the path, accumilating the nodes

            foreach (NodeRecord record in m_ClosedList)
            {
                tempPath.Add(record.Connection);
            }

            //while (m_Current.Node.NodeId != start.NodeId)
            //{
            //    tempPath.Add(m_Current.Connection);
            //    m_Current.Node = m_Current.Connection.GetFromNode();

            //    Vector2 previousNodeId = previousRecord.Node.NodeId;
            //    Vector2 currentNodeId = m_Current.Node.NodeId;

            //    if (previousRecord.Node.NodeId == m_Current.Node.NodeId)
            //    {
            //        Debug.Log("Path Error: Record Node");
            //        break;
            //    }
            //    previousRecord = m_Current;
            //    //if (previousRecord.Connection.GetFromNode().NodeId == m_Current.Connection.GetFromNode().NodeId)
            //    //{
            //    //    Debug.Log("Path Error: Record Connection");
            //    //    break;
            //    //}
            //}

            // Reverse the path and return it
            for (int i = tempPath.Count - 1; i >= 0; i--)
            {
                path.Add(tempPath[i]);
            }

            return path;
        }
    }
}
