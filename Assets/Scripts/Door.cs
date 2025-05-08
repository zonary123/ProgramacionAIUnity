using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
	public Transform upperPart;
	public Transform lowerPart;
	public float openDistance = 1.75f;
	public float openSpeed = 2f;
	public float activationRange = 3f; // Distancia mínima para abrir la puerta

	private Vector3 upperClosedPos;
	private Vector3 lowerClosedPos;
	private Vector3 upperOpenPos;
	private Vector3 lowerOpenPos;

	private bool isOpen = false;

	void Start()
	{
		upperClosedPos = upperPart.localPosition;
		lowerClosedPos = lowerPart.localPosition;

		upperOpenPos = upperClosedPos + Vector3.up * openDistance;
		lowerOpenPos = lowerClosedPos + Vector3.down * openDistance;
	}

	void Update()
	{

		float distanceToPlayer = Vector3.Distance(transform.position, Player.instance.transform.position);
		bool shouldOpen = distanceToPlayer <= activationRange;

		if (shouldOpen && !isOpen)
		{
			isOpen = true;
		}
		else if (!shouldOpen && isOpen)
		{
			isOpen = false;
		}

		// Movimiento suave
		Vector3 targetUpper = isOpen ? upperOpenPos : upperClosedPos;
		Vector3 targetLower = isOpen ? lowerOpenPos : lowerClosedPos;

		upperPart.localPosition = Vector3.MoveTowards(upperPart.localPosition, targetUpper, openSpeed * Time.deltaTime);
		lowerPart.localPosition = Vector3.MoveTowards(lowerPart.localPosition, targetLower, openSpeed * Time.deltaTime);
	}
}