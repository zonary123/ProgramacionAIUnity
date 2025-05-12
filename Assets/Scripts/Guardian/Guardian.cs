using UnityEngine;
using UnityEngine.AI;

public class Guardian : MonoBehaviour{
	[Header("Referencias")] [SerializeField]
	private NavMeshAgent agent;

	[Header("Patrullaje")] [SerializeField]
	private float patrolStepDistance = 5f;

	[SerializeField] private float waitTime = 2f;

	[Header("Detección del Jugador")] [SerializeField]
	private float viewDistance = 10f;

	[SerializeField] private float viewAngle = 90f;
	[SerializeField] private LayerMask playerLayer;

	[Header("Alertas")] [SerializeField] private float alertThreshold = 3f;

	[SerializeField] private float verificationDistance = 2f;
	[SerializeField] private float alertRadius = 15f;
	private float alertLevel;
	private bool globalAlertDone;
	private bool isWaiting;

	private Transform playerTransform;
	private bool verificationDone;
	private float waitTimer;

	private void Start(){
		playerTransform = Player.instance.transform;
		Debug.Log("Guardian iniciado. Comienza patrullaje.");
		ChooseNextDirection();
	}

	private void Update(){
		if (playerTransform != null && IsPlayerInVisionCone()){
			Debug.Log("Jugador dentro del cono de visión del guardián.");
			if (CanSeePlayer()){
				alertLevel += Time.deltaTime;
				Debug.Log($"Alerta aumentando: {alertLevel:F2}");
			}
		}
		else{
			if (alertLevel > 0) Debug.Log("Jugador fuera de visión. Alerta bajando.");
			alertLevel = Mathf.Max(0f, alertLevel - Time.deltaTime);
			verificationDone = false;
			globalAlertDone = false;
		}

		HandleAlertBehavior();

		if (alertLevel == 0 && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) PatrolBehavior();
	}

	#if UNITY_EDITOR
	private void OnDrawGizmosSelected(){
		// Dibuja el cono de visión en la escena
		Gizmos.color = Color.yellow;
		var forward = transform.forward * viewDistance;
		var leftRayRotation = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up);
		var rightRayRotation = Quaternion.AngleAxis(viewAngle / 2, Vector3.up);

		var leftRayDirection = leftRayRotation * forward;
		var rightRayDirection = rightRayRotation * forward;

		Gizmos.DrawRay(transform.position + Vector3.up, leftRayDirection);
		Gizmos.DrawRay(transform.position + Vector3.up, rightRayDirection);
		Gizmos.DrawWireSphere(transform.position, alertRadius);
	}
	#endif

	private void HandleAlertBehavior(){
		var alertPercent = alertLevel / alertThreshold;

		if (alertLevel > 0 && alertLevel < alertThreshold){
			agent.isStopped = true;

			if (alertPercent >= 0.35f && !verificationDone){
				verificationDone = true;
				var dir = (playerTransform.position - transform.position).normalized;
				var checkPoint = transform.position + dir * verificationDistance;

				Debug.Log("Iniciando verificación de la posición del jugador.");
				agent.SetDestination(checkPoint);
				agent.isStopped = false;
			}
		}
		else if (alertLevel >= alertThreshold && !globalAlertDone){
			globalAlertDone = true;
			Debug.Log("ALERTA MÁXIMA: jugador confirmado. Avisando a otros guardianes.");
			GameManager.instance.AlertNearbyGuardians(transform.position, alertRadius);
			agent.SetDestination(playerTransform.position);
			agent.isStopped = false;
		}
	}

	private void PatrolBehavior(){
		if (!isWaiting){
			isWaiting = true;
			waitTimer = waitTime;
			agent.isStopped = true;
			Debug.Log("Guardian en espera antes de continuar patrullaje.");
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
			transform.forward,
			-transform.forward,
			transform.right,
			-transform.right
		};

		for (var i = 0; i < 10; i++){
			var dir = directions[Random.Range(0, directions.Length)];
			var target = transform.position + dir * patrolStepDistance;

			if (!Physics.Raycast(transform.position + Vector3.up, dir, patrolStepDistance)){
				Debug.Log($"Nueva dirección de patrullaje elegida: {dir}");
				agent.SetDestination(target);
				return;
			}
		}

		Debug.Log("No se encontró una dirección libre para patrullar.");
	}

	public void AlertFrom(Vector3 alertPoint){
		Debug.Log($"Guardian alertado externamente desde {alertPoint}. Dirigiéndose al punto.");
		alertLevel = alertThreshold * 0.75f;
		agent.SetDestination(alertPoint);
		agent.isStopped = false;
	}

	private bool CanSeePlayer(){
		if (playerTransform == null) return false;

		var directionToPlayer = playerTransform.position - transform.position;
		directionToPlayer.y = 0f;

		var angle = Vector3.Angle(transform.forward, directionToPlayer);
		if (angle <= viewAngle / 2f)
			if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out var hit, viewDistance,
				    playerLayer))
				return hit.transform == playerTransform;

		return false;
	}

	private bool IsPlayerInVisionCone(){
		if (playerTransform == null) return false;

		var directionToPlayer = playerTransform.position - transform.position;
		directionToPlayer.y = 0f;

		var angle = Vector3.Angle(transform.forward, directionToPlayer);
		return angle <= viewAngle / 2f;
	}
}