using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {
    [SerializeField] public Transform playerTransform;

    private NavMeshAgent navMeshAgent;

    void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update() {
        navMeshAgent.destination = playerTransform.position;
    }
}
