using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshAgentTest : MonoBehaviour
{
    public Vector3 targetPosition;
    private NavMeshAgent _navMeshAgent;
    
    // Start is called before the first frame update
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void NavigateTo()
    {
        _navMeshAgent.SetDestination(targetPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
