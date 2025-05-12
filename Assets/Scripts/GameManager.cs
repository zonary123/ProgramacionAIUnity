using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour{
	public static GameManager instance;
	[SerializeField] private TMP_Text notificationText;

	private void Awake(){
		if (instance == null){
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else{
			Destroy(gameObject);
		}
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	private void Start(){ }

	// Update is called once per frame
	private void Update(){ }

	public void ShowNotification(string message){
		if (notificationText != null) notificationText.text = message;
	}

	public void HideNotification(){
		if (notificationText != null) notificationText.text = "";
	}

	public void AlertNearbyGuardians(Vector3 alertOrigin, float alertRadius){
		// Mostrar visualmente el radio de alerta en el editor (para depuración)
		Debug.DrawRay(alertOrigin, Vector3.up * 2, Color.red); // Mostrar el origen del alerta
		Gizmos.color = Color.green; // Color para representar el área de alerta
		Gizmos.DrawWireSphere(alertOrigin, alertRadius); // Dibuja la esfera de alerta

		var guardians = Physics.OverlapSphere(alertOrigin, alertRadius);
		foreach (var guardian in guardians)
			if (guardian.gameObject != gameObject){
				var other = guardian.GetComponent<Guardian>();
				if (other != null) other.AlertFrom(alertOrigin);
			}
	}
}