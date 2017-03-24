using UnityEngine;

public class Transition
{
    private Condition m_condition;

    private Transition(Condition condition)
    {
        m_condition = condition;
    }

    public Transition(float max, float min, float targetValue)
    {
        m_condition = new FloatCondition(min, max);
        ((FloatCondition)m_condition).targetValue = targetValue;
    }

    public bool IsTriggered()
    {
        return m_condition.Test();
    }

    public State GetTargetState()
    {
        return new State();
    }

    public Action GetAction()
    {
        return new Action();
    }
}
