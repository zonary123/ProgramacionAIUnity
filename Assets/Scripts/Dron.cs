using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DronePatrol : MonoBehaviour{
	[Header("Patrullaje")] public Transform[] waypoints;

	public float speed = 3f;
	public float rotationSpeed = 5f;
	public int startIndex;
	public float stopTime = 0.5f;

	[Header("Alerta")] public float detectionRadius = 5f;

	public float alertIncreaseRate = 1f;
	public float alertDecreaseRate = 1.5f;
	public float alertThreshold = 3f;
	public float visionAngle = 90f;
	public AudioClip alarm;
	public GameObject alertUI;
	public Image alertBar;
	private float alertLevel;

	private AudioSource audioSource;
	private int currentIndex;
	private bool isAlerted;
	private bool isPaused;
	private Quaternion savedWaypointRotation;

	private void Start(){
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null){
			Debug.LogError("Falta AudioSource.");
			enabled = false;
			return;
		}

		if (waypoints == null || waypoints.Length == 0){
			Debug.LogError("No hay waypoints.");
			enabled = false;
			return;
		}

		currentIndex = Mathf.Clamp(startIndex, 0, waypoints.Length - 1);
		foreach (var waypoint in waypoints)
		{
			waypoint.GetComponent<SpriteRenderer>().enabled = false;
		}
		transform.position = waypoints[currentIndex].position;
		StartCoroutine(Patrol());
	}

	private void Update(){
		HandleDetection();
		UpdateUI();
		HandleSound();
	}

	private void HandleDetection(){
		var distanceToPlayer = Vector3.Distance(transform.position, Player.instance.transform.position);
		var canSeePlayer = distanceToPlayer <= detectionRadius && IsPlayerInVisionCone();

		if (canSeePlayer){
			alertLevel += alertIncreaseRate * Time.deltaTime;
			isPaused = true;
			LookAtPlayer();
		}
		else{
			alertLevel -= alertDecreaseRate * Time.deltaTime;
			if (alertLevel <= 0f){
				alertLevel = 0f;
				isPaused = false;
				isAlerted = false;
			}

			if (!isPaused)
				RestoreRotationToWaypoint();
		}

		alertLevel = Mathf.Clamp(alertLevel, 0f, alertThreshold);
	}

	private bool IsPlayerInVisionCone(){
		// Obtener la dirección hacia el jugador
		var directionToPlayer = Player.instance.transform.position - transform.position;
		directionToPlayer.y = 0f; // Ignorar la altura (solo visión en el plano horizontal)

		// Dirección del dron
		var droneForward = transform.forward;

		// Calcular el producto punto entre la dirección del dron y la dirección al jugador
		var dotProduct = Vector3.Dot(droneForward, directionToPlayer.normalized);

		// Ángulo de visión en radianes
		var angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

		// Verificar si el jugador está dentro del cono de visión
		return angle <= visionAngle / 2f;
	}

	private void LookAtPlayer(){
		var playerDir = (Player.instance.transform.position - transform.position).normalized;
		if (playerDir != Vector3.zero){
			var targetRot = Quaternion.LookRotation(playerDir);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
		}
	}

	private void RestoreRotationToWaypoint(){
		if (Quaternion.Angle(transform.rotation, savedWaypointRotation) > 1f)
			transform.rotation = Quaternion.Slerp(transform.rotation, savedWaypointRotation, rotationSpeed * Time.deltaTime);
	}

	private void UpdateUI(){
		alertBar.fillAmount = alertLevel / alertThreshold;
		alertUI.SetActive(alertLevel > 0f);
	}

	private void HandleSound(){
		if (alertLevel >= alertThreshold && !isAlerted){
			isAlerted = true;
			Debug.Log("🔊 ¡ALARMA ACTIVADA!");
			if (!audioSource.isPlaying)
				audioSource.Play();
		}

		if (alertLevel <= 0f && audioSource.isPlaying)
			audioSource.Stop();
	}

	private IEnumerator Patrol(){
		while (true){
			var nextIndex = (currentIndex + 1) % waypoints.Length;
			var targetPos = waypoints[nextIndex].position;

			var direction = (targetPos - transform.position).normalized;
			if (direction != Vector3.zero){
				var targetRotation = Quaternion.LookRotation(direction);
				savedWaypointRotation = targetRotation;
				while (Quaternion.Angle(transform.rotation, targetRotation) > 1f){
					if (!isPaused)
						transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
					yield return null;
				}

				transform.rotation = targetRotation;
			}

			while (Vector3.Distance(transform.position, targetPos) > 0.05f){
				if (!isPaused)
					transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
				yield return null;
			}

			yield return new WaitForSeconds(stopTime);
			currentIndex = nextIndex;
		}
	}
}