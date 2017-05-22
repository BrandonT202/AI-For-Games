using UnityEngine;
using System.Collections.Generic;

public class NeuralNetworkController
{
    private NeuralNetwork m_network;
    private SafetyNodeGraph m_safetyGraph;
    private PersistenceManager m_persistenceManager;

    private const float m_BOOLEANTHRESHOLD = 0.7f;

    // Use this for initialization
    void init(int numWorldNodes)
    {
        m_safetyGraph = new SafetyNodeGraph(numWorldNodes);

        m_persistenceManager = new PersistenceManager();

        List<int> topology = new List<int> { 1, 4, 1 };
        m_network = new NeuralNetwork(topology);
    }

    public void train(Vector2 position)
    {
        int worldNodeIndex = (int)position.y * m_safetyGraph.m_size + (int)position.x;

        // Attain the training set data from the database
        float output = m_persistenceManager.SelectOutput("PositionSafety", worldNodeIndex);

        List<float> outputs = new List<float> { output };
        List<float> inputs = new List<float> { position.x, position.y };

        // propagate
        m_network.feedForward(inputs);

        // backpropagate
        m_network.backpropagate(outputs);
    }

    public bool determineIfSafe(Vector2 position)
    {
        // Use the position as the input
        // The input will indentify a node in the safety graph
        // The safety graph has a value to indicate whether the node is safe to 
        // pass or not.
        List<float> inputs = new List<float> { position.x, position.y };
        m_network.feedForward(inputs);

        // The neural network will output a value between 0..1
        // This will indicate whether it has learned that the node 
        // at <position> will be safe to pass or not
        List<float> results = new List<float>();
        m_network.getNetworkResults(ref results);

        // if the output exceeds the threshold then true else false
        return results[0] > m_BOOLEANTHRESHOLD;
    }
}
