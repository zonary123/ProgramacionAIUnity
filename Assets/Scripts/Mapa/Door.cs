using UnityEngine;

public class Door : MonoBehaviour{
	public Transform upperPart;
	public Transform lowerPart;
	public float openDistance = 3f;
	public float openSpeed = 4.25f;

	protected bool isOpen;
	protected Vector3 lowerClosedPos;
	protected Vector3 lowerOpenPos;

	protected Vector3 upperClosedPos;
	protected Vector3 upperOpenPos;

	protected virtual void Start(){
		openSpeed = 4.25f;
		openDistance = 3f;
		upperClosedPos = upperPart.localPosition;
		lowerClosedPos = lowerPart.localPosition;

		upperOpenPos = upperClosedPos + Vector3.up * openDistance;
		lowerOpenPos = lowerClosedPos + Vector3.down * openDistance;
	}

	protected virtual void Update(){
		var isPlayerInRange = Vector3.Distance(transform.position, Player.instance.transform.position) <= openDistance;
		if (isPlayerInRange && !isOpen)
			OpenDoor();
		else if (!isPlayerInRange && isOpen) CloseDoor();

		MoveDoor();
	}

	protected void MoveDoor(){
		var targetUpper = isOpen ? upperOpenPos : upperClosedPos;
		var targetLower = isOpen ? lowerOpenPos : lowerClosedPos;

		upperPart.localPosition = Vector3.MoveTowards(upperPart.localPosition, targetUpper, openSpeed * Time.deltaTime);
		lowerPart.localPosition = Vector3.MoveTowards(lowerPart.localPosition, targetLower, openSpeed * Time.deltaTime);
	}

	public void OpenDoor(){
		isOpen = true;
	}

	public void CloseDoor(){
		isOpen = false;
	}
}