using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour{
	public static GameManager instance;
	public Canvas globalCanvas;
	[SerializeField] private TMP_Text notificationText;

	private void Awake(){
		if (instance == null){
			instance = this;
			DontDestroyOnLoad(gameObject);
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
		Debug.DrawRay(alertOrigin, Vector3.up * 2, Color.red); // Esto sí se puede usar

		var guardians = Physics.OverlapSphere(alertOrigin, alertRadius);
		foreach (var guardian in guardians)
			if (guardian.gameObject != gameObject){
				var other = guardian.GetComponent<Guardian>();
				if (other != null)
					other.AlertFrom(alertOrigin);
			}
	}
}