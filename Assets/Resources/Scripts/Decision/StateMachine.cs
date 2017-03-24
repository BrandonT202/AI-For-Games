using UnityEngine;
using System.Collections.Generic;

public class StateMachine
{
    private List<State> m_states;

    private List<Action> m_actions;

    private State m_initialState;

    private State m_currentState;

    public StateMachine()
    {
        m_states = new List<State>();
        m_actions = new List<Action>();
    }

    public void AddState (float min, float max, float target)
    {
        State newState = new State();
        newState.AddTransition(min, max, target);
    }

    public List<Action> Update()
    {
        Transition m_triggeredTransition = null;

        // Check all transitions and store the first one that triggers
        foreach (Transition transition in m_currentState.GetTransitions())
        {
            if (transition.IsTriggered())
            {
                m_triggeredTransition = transition;
                break;
            }
        }

        State targetState = null;

        // Check for a transition to fire
        if (m_triggeredTransition != null)
        {
            // Find for target state
            targetState = m_triggeredTransition.GetTargetState();

            m_actions.Add(m_currentState.GetExitAction());
            m_actions.Add(m_triggeredTransition.GetAction());
            m_actions.Add(targetState.GetEntryAction());

            m_currentState = targetState;
            return m_actions;
        }
        else
        {
            return m_currentState.GetActions();
        }
    }
}
