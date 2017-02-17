using UnityEngine;
using System.Collections;

public class Node
{
    public int NodeId { get; set; }
    public int HeuristicCost { get; set; }
    public int FValue { get; set; }
    public bool EndNode { get; set; }
}
