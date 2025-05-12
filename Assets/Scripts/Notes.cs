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

	private void Update(){
		isPlayerInRange = Vector3.Distance(transform.position, Player.instance.transform.position) <= 3f;

		if (isPlayerInRange && !noteIsVisible){
			GameManager.instance.ShowNotification($"Presiona E para leer la nota #{id}");

			if (Input.GetKeyDown(KeyCode.E)) ShowNote();
		}
		else if (!isPlayerInRange && noteIsVisible){
			HideNote();
		}
		else if (isPlayerInRange && noteIsVisible && Input.GetKeyDown(KeyCode.E)){
			HideNote();
		}
	}

	public void ShowNote(){
		if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
		notificationCoroutine = StartCoroutine(HideNotificationCoroutine());

		ShowNoteUI();
		noteIsVisible = true;
	}

	private void HideNote(){
		HideNoteUI();
		noteIsVisible = false;

		if (notificationCoroutine != null){
			StopCoroutine(notificationCoroutine);
			notificationCoroutine = null;
		}

		GameManager.instance.HideNotification();
	}

	private void ShowNoteUI(){
		if (noteUIPrefab == null || currentNoteUI != null) return;

		var canvas = FindObjectOfType<Canvas>();
		if (canvas == null){
			Debug.LogWarning("No se encontró un Canvas en la escena.");
			return;
		}

		currentNoteUI = Instantiate(noteUIPrefab, canvas.transform);

		var display = currentNoteUI.GetComponent<NoteUIDisplay>();
		if (display != null) display.SetNoteInfo($"Nota #{id}", number.ToString());
	}

	private void HideNoteUI(){
		if (currentNoteUI != null){
			Destroy(currentNoteUI);
			currentNoteUI = null;
		}
	}

	private IEnumerator HideNotificationCoroutine(){
		yield return new WaitForSeconds(0.5f);
		GameManager.instance.HideNotification();
		notificationCoroutine = null;
	}
}