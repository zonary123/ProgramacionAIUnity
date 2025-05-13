using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PantallaDeInicio : MonoBehaviour{
	public Button botonJugar;

	public Button botonSalir;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	private void Start(){
		botonJugar.onClick.AddListener(() => {
			Debug.Log("Botón Jugar presionado");
			SceneManager.LoadScene("Mapa");
		});

		botonSalir.onClick.AddListener(() => {
			Debug.Log("Botón Salir presionado");
			Application.Quit();
		});
	}
}