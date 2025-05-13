using TMPro;
using UnityEngine;

public class NoteUIDisplay : MonoBehaviour{
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private TMP_Text contentText;

	public void SetNoteInfo(string title, string content){
		if (titleText != null)
			titleText.text = title;

		if (contentText != null)
			contentText.text = content;
	}
}