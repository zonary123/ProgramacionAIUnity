using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class Guardian : MonoBehaviour{
    [Header("Referencias")] [SerializeField]
    private NavMeshAgent agent;

    [SerializeField] private Animator animator;

    [Header("UI de Alerta")] [SerializeField]
    private GameObject alertUI;

    [SerializeField] private Image alertBar;

    [Header("Patrullaje")] [SerializeField]
    private float patrolStepDistance = 5f;

    [SerializeField] private float waitTime = 2f;

    [Header("Detección del Jugador")] [SerializeField]
    private float viewDistance = 10f;

    [SerializeField] private float viewAngle = 90f;

    [Header("Alertas")] [SerializeField] private float alertThreshold = 3f;

    [SerializeField] private float verificationDistance = 2f;
    [SerializeField] private float alertRadius = 15f;
    [SerializeField] private float searchDuration = 3f;
    private readonly float rotationSpeed = 5f;

    private float alertLevel;
    private bool globalAlertDone;
    private bool isSearching;
    private bool isWaiting;

    private Vector3 lastPatrolPosition;
    private Vector3? lastSeenPlayerPosition;

    private Transform playerTransform;
    private bool returningToPatrol;
    private float searchTimer;
    private bool verificationDone;

    private float waitTimer;

    private void Start(){
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0.5f;
        playerTransform = Player.instance.transform;
        ChooseNextDirection();
    }

    private void Update(){
        UpdateAlertLevel();
        HandleAlertBehavior();
        UpdateUI();
        UpdateAnimations();

        // Si el guardián está en alerta o realizando búsqueda, no patrulla
        if (alertLevel > 0 || returningToPatrol || verificationDone || isSearching) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            PatrolBehavior();
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.yellow;
        var forward = transform.forward * viewDistance;
        var leftRay = Quaternion.Euler(0, -viewAngle / 2, 0);
        var rightRay = Quaternion.Euler(0, viewAngle / 2, 0);
        Gizmos.DrawRay(transform.position + Vector3.up, leftRay * forward);
        Gizmos.DrawRay(transform.position + Vector3.up, rightRay * forward);
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }
    #endif

    private void UpdateUI(){
        alertBar.fillAmount = Mathf.Clamp01(alertLevel / alertThreshold);
        alertUI.SetActive(alertLevel > 0.01f);
    }

    private void UpdateAlertLevel(){
        if (playerTransform != null && IsPlayerInVisionCone() && CanSeePlayer()){
            alertLevel += Time.deltaTime;
            lastSeenPlayerPosition = playerTransform.position;

            if (isSearching){
                isSearching = false;
                agent.isStopped = false;
            }

            RotateTowards(playerTransform.position);
        }
        else{
            alertLevel = Mathf.Max(0f, alertLevel - Time.deltaTime);
            verificationDone = false;

            if (!isSearching && lastSeenPlayerPosition.HasValue){
                isSearching = true;
                searchTimer = searchDuration;

                agent.isStopped = false;
                agent.SetDestination(lastSeenPlayerPosition.Value);
            }
            else if (isSearching && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance){
                searchTimer -= Time.deltaTime;
                if (searchTimer <= 0f){
                    isSearching = false;
                    lastSeenPlayerPosition = null;

                    if (globalAlertDone && !returningToPatrol){
                        returningToPatrol = true;
                        agent.SetDestination(lastPatrolPosition);
                    }
                }
            }

            if (lastSeenPlayerPosition.HasValue)
                RotateTowards(lastSeenPlayerPosition.Value);
        }

        if (returningToPatrol && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance){
            returningToPatrol = false;
            ChooseNextDirection();
        }

        // 🔧 CORRECCIÓN: Si la alerta llega a 0, debe regresar al patrullaje correctamente
        if (alertLevel <= 0f && !returningToPatrol && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting){
            ChooseNextDirection();
        }
    }

    private void HandleAlertBehavior(){
        if (alertLevel <= 0f){
            alertLevel = 0f;
            return;
        }

        var alertPercent = alertLevel / alertThreshold;

        if (alertPercent >= 1f){
            if (!globalAlertDone){
                globalAlertDone = true;
                lastPatrolPosition = transform.position;
                GameManager.instance.AlertNearbyGuardians(transform.position, alertRadius);
            }

            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
        }
        else if (alertPercent >= 0.75f){
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
        }
        else{
            agent.isStopped = true;

            if (!verificationDone && alertPercent >= 0.35f){
                verificationDone = true;

                var direction = (playerTransform.position - transform.position).normalized;
                var checkPoint = transform.position + direction * verificationDistance;

                agent.isStopped = false;
                agent.SetDestination(checkPoint);
            }
        }
    }

    private void PatrolBehavior(){
        if (!isWaiting){
            isWaiting = true;
            waitTimer = waitTime;
            agent.isStopped = true;
        }
        else{
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f){
                isWaiting = false;
                agent.isStopped = false;
                ChooseNextDirection();
            }
        }
    }

    private void ChooseNextDirection(){
        Vector3[] directions ={
            Vector3.forward,
            Vector3.back,
            Vector3.right,
            Vector3.left
        };

        for (var i = 0; i < directions.Length; i++){
            var rnd = Random.Range(i, directions.Length);
            (directions[i], directions[rnd]) = (directions[rnd], directions[i]);
        }

        foreach (var dir in directions){
            var target = transform.position + dir * patrolStepDistance;

            if (!Physics.Raycast(transform.position + Vector3.up, dir, patrolStepDistance))
                if (NavMesh.SamplePosition(target, out var hit, 1f, NavMesh.AllAreas)){
                    lastPatrolPosition = transform.position;
                    agent.SetDestination(hit.position);
                    return;
                }
        }
    }

    public void AlertFrom(Vector3 alertPoint){
        if (alertLevel >= alertThreshold) return;

        alertLevel = alertThreshold * 0.75f;
        lastPatrolPosition = transform.position;

        agent.isStopped = false;
        agent.SetDestination(alertPoint);
    }

    private bool CanSeePlayer(){
        if (playerTransform == null) return false;

        var direction = playerTransform.position - transform.position;
        direction.y = 0f;

        if (Vector3.Angle(transform.forward, direction) <= viewAngle / 2f){
            if (Physics.Raycast(transform.position + Vector3.up, direction.normalized, out var hit, viewDistance)){
                Debug.DrawRay(transform.position + Vector3.up, direction.normalized * viewDistance, Color.green);
                return hit.transform == playerTransform;
            }

            Debug.DrawRay(transform.position + Vector3.up, direction.normalized * viewDistance, Color.red);
        }

        return false;
    }

    private bool IsPlayerInVisionCone(){
        if (playerTransform == null) return false;

        var direction = playerTransform.position - transform.position;
        direction.y = 0f;

        return Vector3.Angle(transform.forward, direction) <= viewAngle / 2f;
    }

    private void RotateTowards(Vector3 targetPosition){
        var direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        var lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void UpdateAnimations(){
        if (animator == null) return;

        var isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;

        animator.SetBool("Idle", !isMoving);
        animator.SetBool("Run", isMoving);
    }
}
