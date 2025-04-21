using UnityEngine;

public class GenericTranslate : MonoBehaviour
{
	public Transform startPos;

	public Transform endPos;

	public float translateSpeed;

	private void Start()
	{
		base.transform.position = startPos.position;
	}

	private void Update()
	{
		base.transform.position = Vector3.MoveTowards(base.transform.position, endPos.position, Time.deltaTime * translateSpeed);
		if (Vector3.Distance(base.transform.position, endPos.position) < translateSpeed * Time.deltaTime * 2f)
		{
			base.transform.position = startPos.position;
		}
	}
}
