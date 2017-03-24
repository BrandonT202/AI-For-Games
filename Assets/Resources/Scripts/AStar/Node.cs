using UnityEngine;
using System.Collections;

public class Node
{
    public Vector2 NodeId { get; set; }
    public Connection Connection { get; set; }
    public float CostSoFar { get; set; }
    public float EstimatedTotalCost { get; set; }
}
