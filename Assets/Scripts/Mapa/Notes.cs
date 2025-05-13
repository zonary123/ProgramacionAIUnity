using System.Collections;
using UnityEngine;

public class Notes : MonoBehaviour{
	[SerializeField] private int id;
	[SerializeField] private int number;
	[SerializeField] private GameObject noteUIPrefab;

	private GameObject currentNoteUI;
	private bool isPlayerInRange;
	private bool noteIsVisible;
	private Coroutine notificationCoroutine;
	private bool notificationShown;

	private void Update(){
		var distanceToPlayer = Vector3.Distance(transform.position, Player.instance.transform.position);
		var currentlyInRange = distanceToPlayer <= 3f;

		if (currentlyInRange && !isPlayerInRange){
			isPlayerInRange = true;
			HandlePlayerEnterRange();
		}
		else if (!currentlyInRange && isPlayerInRange){
			isPlayerInRange = false;
			HandlePlayerExitRange();
		}

		if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)){
			if (!noteIsVisible) ShowNote();
			else HideNote();
		}
	}

	private void HandlePlayerEnterRange(){
		if (!noteIsVisible && !notificationShown){
			GameManager.instance.ShowNotification($"Presiona E para leer la nota #{id}");
			notificationShown = true;
		}
	}

	private void HandlePlayerExitRange(){
		if (noteIsVisible) HideNote();

		if (notificationCoroutine != null)
			StopCoroutine(notificationCoroutine);

		notificationCoroutine = StartCoroutine(HideNotificationCoroutine());
	}

	public void ShowNote(){
		if (notificationCoroutine != null)
			StopCoroutine(notificationCoroutine);

		GameManager.instance.HideNotification();
		notificationCoroutine = null;

		ShowNoteUI();
		noteIsVisible = true;
		notificationShown = false;
	}

	private void HideNote(){
		HideNoteUI();
		noteIsVisible = false;

		if (isPlayerInRange){
			GameManager.instance.ShowNotification($"Presiona E para leer la nota #{id}");
			notificationShown = true;
		}
		else{
			if (notificationCoroutine != null)
				StopCoroutine(notificationCoroutine);

			notificationCoroutine = StartCoroutine(HideNotificationCoroutine());
		}
	}

	private void ShowNoteUI(){
		if (noteUIPrefab == null || currentNoteUI != null) return;

		var canvas = GameManager.instance.globalCanvas;
		if (canvas == null){
			Debug.LogWarning("No se encontró un Canvas en la escena.");
			return;
		}

		currentNoteUI = Instantiate(noteUIPrefab, canvas.transform);

		var display = currentNoteUI.GetComponent<NoteUIDisplay>();
		if (display != null)
			display.SetNoteInfo($"Nota #{id}", number.ToString());
	}

	private void HideNoteUI(){
		if (currentNoteUI != null){
			Destroy(currentNoteUI);
			currentNoteUI = null;
		}
	}

	private IEnumerator HideNotificationCoroutine(){
		yield return new WaitForSeconds(0.25f);
		GameManager.instance.HideNotification();
		notificationCoroutine = null;
		notificationShown = false;
	}
}