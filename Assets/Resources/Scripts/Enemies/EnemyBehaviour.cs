using Panda;
using UnityEngine;
using System.Collections.Generic;

public class EnemyBehaviour : MonoBehaviour
{
    private List<Vector3> m_positions;
    private Vector3 m_centre;

    private float m_speed;
    private int m_currentPositionIndex;

    void Start ()
    {
        m_positions = new List<Vector3>();
        m_centre = transform.FindChild("Centre").position;

        m_positions.Add(new Vector3(m_centre.x - 3, m_centre.y, m_centre.z - 3));
        m_positions.Add(new Vector3(m_centre.x + 3, m_centre.y, m_centre.z - 3));
        m_positions.Add(new Vector3(m_centre.x + 3, m_centre.y, m_centre.z + 3));
        m_positions.Add(new Vector3(m_centre.x - 3, m_centre.y, m_centre.z + 3));

        m_currentPositionIndex = 0;
        m_speed = 1.5f;
    }

    [Task]
    void MoveToNextPosition()
    {
        Vector3 destination = m_positions[m_currentPositionIndex];
        Vector3 delta = (destination - transform.position);
        Vector3 velocity = m_speed * delta.normalized;

        transform.position = transform.position + velocity * Time.deltaTime;

        Vector3 newDelta = (destination - transform.position);
        float d = newDelta.magnitude;

        if (Task.isInspected)
            Task.current.debugInfo = string.Format("d={0:0.000}", d);

        if (Vector3.Dot(delta, newDelta) <= 0.0f || d < 1e-3)
        {
            m_currentPositionIndex++;
            transform.position = destination;
            m_currentPositionIndex = m_currentPositionIndex > m_positions.Count - 1 ? 0 : m_currentPositionIndex;
            Task.current.Succeed();
            d = 0.0f;
            Task.current.debugInfo = "d=0.000";
        }
    }

}
