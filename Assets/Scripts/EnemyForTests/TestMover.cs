using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Tests
{
    internal class TestMover : MonoBehaviour
    {
       // [Inject] private MainBuilding _mainBuilding;

        private NavMeshAgent _navMeshAgent;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
           // _navMeshAgent.destination = _mainBuilding.transform.position;
        }
    }
}
