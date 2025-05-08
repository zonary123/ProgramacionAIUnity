using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
	public static Player instance;
	public float speed = 5f;

	public Transform playerCamera;
	public float mouseSensitivity = 100f;
	private float xRotation = 0f;

	private Rigidbody rb;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}
	
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
	{
		LookAround();
		Move();
	}

	void LookAround()
	{
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		transform.Rotate(Vector3.up * mouseX);
	}

	void Move()
	{
		float moveX = Input.GetAxis("Horizontal");
		float moveZ = Input.GetAxis("Vertical");

		Vector3 move = transform.right * moveX + transform.forward * moveZ;
		Vector3 newVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

		rb.linearVelocity = newVelocity;
	}
}