﻿using UnityEngine;
using System.Collections.Generic;

public class State
{
    private List<Transition> m_transitions;
    private List<Action> m_actions;

    public State()
    {
        m_actions = new List<Action>();
        m_transitions = new List<Transition>();
    }

    public void AddTransition(float min, float max, float target)
    {
        m_transitions.Add(new Transition(min, max, target));
    }

    public List<Transition> GetTransitions()
    {
        return m_transitions;
    }

    public Action GetExitAction()
    {
        return new Action();
    }

    public Action GetEntryAction()
    {
        return new Action();
    }

    public List<Action> GetActions()
    {
        return m_actions;
    }
};
