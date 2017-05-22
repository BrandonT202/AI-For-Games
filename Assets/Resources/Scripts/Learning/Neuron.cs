using UnityEngine;
using System.Collections.Generic;

public class Neuron
{
    private float m_trainingRate;
    private float m_momentum;
    private float m_outputValue;
    private float m_gradient;
    private int m_myIndex;

    public struct Connection
    {
        public float weight;
        public float deltaWeight;
    };

    public List<Connection> m_outputWeights;

    public Neuron(int numOutputs, int myIndex)
    {
        m_myIndex = myIndex;
        m_trainingRate = 0.2f;
        m_momentum = 0.5f;
        m_outputWeights = new List<Connection>();
    }

    public void setOutputVal(float val)
    {
        m_outputValue = val;
    }

    public float getOutputVal()
    {
        return m_outputValue;
    }

    public void feedForward(List<Neuron> prevLayer)
    {
        float sum = 0.0f;

        // Sum the previous layer's outputs (i.e. the inputs)
        // including the bias node

        for (int n = 0; n < prevLayer.Count; ++n)
        {
            sum += prevLayer[n].getOutputVal() * prevLayer[n].m_outputWeights[m_myIndex].weight;
        }

        m_outputValue = activationFunction(sum);
    }

    public void calcOutputGradients(float targetValue)
    {
        float delta = targetValue - m_outputValue;
        m_gradient = delta * activationFunctionDerivative(m_outputValue);
    }

    public void calcHiddenGradients(List<Neuron> nextLayer)
    {
        float dow = sumDerivativesOfWeights(nextLayer);
        m_gradient = dow * activationFunctionDerivative(m_outputValue);
    }

    public void updateInputWeights(List<Neuron> prevLayer)
    {
        for (int n = 0; n < prevLayer.Count; ++n)
        {
            Neuron neuron = prevLayer[n];
            float oldDeltaWeight = neuron.m_outputWeights[m_myIndex].deltaWeight;

            float newDeltaWeight =
                // Individual input, magnified by the gradient and train rate
                m_trainingRate
                * neuron.getOutputVal()
                * m_gradient
                // Also add momentum = a fraction of the previous delta weight
                + m_momentum
                * oldDeltaWeight;

            // update the input layer connection
            float newWeight = neuron.m_outputWeights[m_myIndex].weight + newDeltaWeight;
            Connection newConnection = new Connection
            {
                deltaWeight = newDeltaWeight,
                weight = newWeight
            };
            neuron.m_outputWeights[m_myIndex] = newConnection;
        }
    }

    private float activationFunction(float x)
    {
        // log-sigmoid range [0..1]
        return 1.0f / (1.0f + Mathf.Exp(-x));
    }

    private float activationFunctionDerivative(float x)
    {
        return x * (1 - x);
    }

    private float randomWeight()
    {
        return Random.Range(0, 1000) / 1000;
    }

    private float sumDerivativesOfWeights(List<Neuron> nextLayer)
    {
        float sumDOW = 0.0f;

        for (int n = 0; n < nextLayer.Count; n++)
        {
            sumDOW += m_outputWeights[n].weight * nextLayer[n].m_gradient;
        }

        return sumDOW;
    }
}
