using System;
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

	private RaycastHit hit;
	private Notes currentNote;


	void Update()
	{
		ViewNote();
		LookAround();
		Move();
	}

	private void ViewNote()
	{
		Debug.DrawRay(playerCamera.position, playerCamera.forward * 3f, Color.red);
		if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 3f))
		{
			if (hit.transform.CompareTag("Note"))
			{
				currentNote = hit.transform.GetComponent<Notes>();
				if (Input.GetKeyDown(KeyCode.E))
				{
					currentNote.ShowNote();
				}
			}
			else if (currentNote != null)
			{
				currentNote.HideNote();
				currentNote = null;
			}
		}
		else if (currentNote != null)
		{
			currentNote.HideNote();
			currentNote = null;
		}
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