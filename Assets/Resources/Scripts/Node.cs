using UnityEngine;
using System.Collections;

public class Node
{
    public Vector2 NodeId { get; set; }
    public int HeuristicCost { get; set; }
    public int FinalValue { get; set; }
    public bool EndNode { get; set; }
}
