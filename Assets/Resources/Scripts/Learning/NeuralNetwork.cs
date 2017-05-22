using UnityEngine;
using System.Collections.Generic;

public class NeuralNetwork
{
    public List<List<Neuron>> m_layers;

    float m_error;
    float m_recentAverageError;
    float m_averageSmoothingFactor;

    public NeuralNetwork(List<int> topology)
    {
        m_error = 0.0f;
        m_recentAverageError = 0.0f;
        m_averageSmoothingFactor = 50.0f;

        m_layers = new List<List<Neuron>>();
    }

    public void feedForward(List<float> inputs)
    {
        // Assign the input values into the input neurons
        for (int i = 0; i < inputs.Count; ++i)
        {
            m_layers[0][i].setOutputVal(inputs[i]);
        }

        // Forward propagate
        for (int layerNum = 1; layerNum < m_layers.Count; ++layerNum)
        {
            List<Neuron> prevLayer = m_layers[layerNum - 1];
            for (int n = 0; n < m_layers[layerNum].Count - 1; ++n)
            {
                m_layers[layerNum][n].feedForward(prevLayer);
            }
        }
    }

    public void backpropagate(List<float> targetValues)
    {
        // Calculate overall net error (RMS of output neuron errors)
        List<Neuron> outputLayer = m_layers[m_layers.Count - 1];
        m_error = 0.0f;

        for (int n = 0; n < outputLayer.Count - 1; ++n)
        {
            float delta = targetValues[n] - outputLayer[n].getOutputVal();
            m_error += delta * delta;
        }
        m_error /= outputLayer.Count - 1; // get average error squared
        m_error = Mathf.Sqrt(m_error); // RMS

        // An recent average measurement
        m_recentAverageError =
            (m_recentAverageError * m_averageSmoothingFactor + m_error)
            / (m_averageSmoothingFactor + 1.0f);

        // Calculate output layer gradients
        for (int n = 0; n < outputLayer.Count - 1; ++n)
        {
            outputLayer[n].calcOutputGradients(targetValues[n]);
        }

        // Calculate hidden layer gradients
        for (int layerNum = m_layers.Count - 2; layerNum > 0; --layerNum)
        {
            List<Neuron> hiddenLayer = m_layers[layerNum];
            List<Neuron> nextLayer = m_layers[layerNum + 1];

            for (int n = 0; n < hiddenLayer.Count;  ++n)
            {
                hiddenLayer[n].calcHiddenGradients(nextLayer);
            }
        }

        // For all layers from outputs to first hidden layer,
        // update connection weights
        for (int layerNum = m_layers.Count - 1; layerNum > 0; --layerNum)
        {
            List<Neuron> layer = m_layers[layerNum];
            List<Neuron> prevLayer = m_layers[layerNum - 1];

            for (int n = 0; n < layer.Count - 1; ++n)
            {
                layer[n].updateInputWeights(prevLayer);
            }
        }
    }

    public void getNetworkResults(ref List<float> results)
    {
        results.Clear();

        List<Neuron> backLayer = m_layers[m_layers.Count - 1];
        for (int n = 0; n < backLayer.Count - 1; ++n)
        {
            results.Add(backLayer[n].getOutputVal());
        }
    }

    public float getRecentAverageError()
    {
        return m_recentAverageError;
    }
}