using UnityEngine;
using System.Collections;

public class NaiveBayesController : MonoBehaviour
{
    private PersistenceManager m_persistenceManager;

	// Use this for initialization
	void Start ()
    {
        m_persistenceManager = new PersistenceManager();
        m_persistenceManager.Connect();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
