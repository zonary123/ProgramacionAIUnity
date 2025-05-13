using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodePanelUI : MonoBehaviour{
	public bool active = true;
	public TMP_InputField inputDisplay;
	public TMP_Text feedbackText;
	public Button[] numberButtons;
	public Button clearButton;
	public Button confirmButton;

	private string currentInput = "";

	public Action<string> OnConfirm;

	private void Start(){
		// Asegurar que se captura bien el valor dentro del bucle
		foreach (var btn in numberButtons){
			var digit = btn.GetComponentInChildren<TMP_Text>().text;
			btn.onClick.AddListener(() => AddDigit(digit));
		}

		clearButton.onClick.AddListener(ClearInput);
		confirmButton.onClick.AddListener(() => OnConfirm?.Invoke(currentInput));
	}

	private void LateUpdate(){ }

	private void AddDigit(string digit){
		currentInput += digit;
		inputDisplay.text = currentInput;
	}

	private void ClearInput(){
		currentInput = "";
		inputDisplay.text = "";
		feedbackText.text = "";
	}

	public void ShowError(string message){
		feedbackText.text = message;
	}
}