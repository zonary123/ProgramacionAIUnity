using UnityEngine;

public class Notes : MonoBehaviour
{
    [SerializeField]
    private int id;
    [SerializeField]
    private string noteText;
    [SerializeField]
    private int number;
    [SerializeField]
    private GameObject textDisplay; // Asigna el GameObject hijo con el TextMesh

    private void Awake()
    {
        if (textDisplay != null)
            textDisplay.SetActive(false); // Ocultar al inicio
    }

    public void ShowNote()
    {
        if (textDisplay != null)
        {
            textDisplay.SetActive(true);
            TextMesh mesh = textDisplay.GetComponent<TextMesh>();
            if (mesh != null)
            {
                mesh.text = $"Nota #{number}:\n{noteText}";
            }

            // Opcional: girar el texto hacia el jugador
            Transform cam = Camera.main.transform;
            textDisplay.transform.LookAt(cam);
            textDisplay.transform.Rotate(0, 180, 0); 
        }
    }

    public void HideNote()
    {
        if (textDisplay != null)
            textDisplay.SetActive(false);
    }
}