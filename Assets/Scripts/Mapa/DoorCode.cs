using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorCode : Door{
	public string correctCode = "1234";
	public GameObject codePanelPrefab;
	public float activationRange = 3f;

	[SerializeField] private bool win;

	[SerializeField] private GameObject currentPanel;
	[SerializeField] private bool isUnlocked;

	private void Awake(){
		CloseDoor();
	}

	protected override void Update(){
		var distanceToPlayer = Vector3.Distance(transform.position, Player.instance.transform.position);

		if (!isUnlocked){
			if (distanceToPlayer <= activationRange){
				// Abrir el panel solo al presionar E si aún no está abierto
				if (currentPanel == null && Input.GetKeyDown(KeyCode.E))
					ShowCodePanel();
				// Salir del panel con Escape
				else if (currentPanel != null && Input.GetKeyDown(KeyCode.Escape)) CloseCodePanel();
			}
			else if (currentPanel != null){
				CloseCodePanel(); // Cerrar automáticamente si se aleja
			}
		}
		else{
			if (distanceToPlayer <= activationRange) OpenDoor();
			else CloseDoor();
		}

		MoveDoor();
	}

	private void ShowCodePanel(){
		currentPanel = Instantiate(codePanelPrefab, FindObjectOfType<Canvas>().transform);
		var ui = currentPanel.GetComponent<CodePanelUI>();

		// Mostrar cursor y pausar el tiempo
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 0;

		// Asignar callbacks
		ui.OnConfirm += OnCodeEntered;
	}

	private void OnCodeEntered(string enteredCode){
		if (enteredCode == correctCode){
			isUnlocked = true;
			CloseCodePanel();
			if (win) StartCoroutine(LoadWinScene());
		}
		else{
			var ui = currentPanel.GetComponent<CodePanelUI>();
			ui.ShowError("Código incorrecto");
		}
	}

	private IEnumerator LoadWinScene(){
		yield return new WaitForSeconds(5f);
		SceneManager.LoadScene("WinScene");
	}

	private void CloseCodePanel(){
		if (currentPanel != null){
			var ui = currentPanel.GetComponent<CodePanelUI>();
			ui.OnConfirm -= OnCodeEntered;
			Destroy(currentPanel);
		}

		// Ocultar cursor y reanudar el tiempo
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1;
	}
}