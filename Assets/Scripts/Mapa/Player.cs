using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour{
	public static Player instance;
	public float speed = 5f;

	public Transform playerCamera;
	public float mouseSensitivity = 100f;
	public bool isHidden;
	private Notes currentNote;
	private RaycastHit hit;

	private Rigidbody rb;
	private float xRotation;

	private void Awake(){
		if (instance == null)
			instance = this;
		else
			Destroy(gameObject);
	}

	private void Start(){
		rb = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update(){
		ViewNote();
		LookAround();
		Move();
	}

	private void ViewNote(){
		Debug.DrawRay(playerCamera.position, playerCamera.forward * 3f, Color.red);
		if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 3f))
			if (hit.transform.CompareTag("HidingPlace")){ }
			else if (hit.transform.CompareTag("Note")){
				currentNote = hit.transform.GetComponent<Notes>();
				if (Input.GetKeyDown(KeyCode.E)) currentNote.ShowNote();
			}
	}

	private void LookAround(){
		var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		transform.Rotate(Vector3.up * mouseX);
	}

	private void Move(){
		var moveX = Input.GetAxis("Horizontal");
		var moveZ = Input.GetAxis("Vertical");

		var move = transform.right * moveX + transform.forward * moveZ;
		var newVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

		rb.linearVelocity = newVelocity;
	}
}