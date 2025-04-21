using UnityEngine;

public class GenericRotator : MonoBehaviour
{
	public float rotationRate;

	public Vector3 rotationRange;

	public bool constantRotate;

	public bool slerpRotate = true;

	public bool autoReverse = true;

	private bool reverse;

	private Quaternion targetRot;

	private void Start()
	{
		float x = Random.value * rotationRange.x - rotationRange.x / 2f;
		float y = Random.value * rotationRange.y - rotationRange.y / 2f;
		float z = Random.value * rotationRange.z - rotationRange.z / 2f;
		Vector3 euler = new Vector3(x, y, z);
		base.transform.rotation = Quaternion.Euler(euler);
	}

	private bool approxEqual(Quaternion s, Quaternion t)
	{
		Vector3 eulerAngles = s.eulerAngles;
		Vector3 eulerAngles2 = t.eulerAngles;
		if (Vector3.Distance(eulerAngles, eulerAngles2) < 1f)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (autoReverse)
		{
			if (reverse)
			{
				targetRot = Quaternion.Euler(-rotationRange);
			}
			else
			{
				targetRot = Quaternion.Euler(rotationRange);
			}
		}
		if (constantRotate)
		{
			base.transform.rotation *= Quaternion.Euler(rotationRange * Time.deltaTime * rotationRate);
			return;
		}
		if (slerpRotate)
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, targetRot, Time.deltaTime * rotationRate);
		}
		else
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetRot, Time.deltaTime * rotationRate);
		}
		if (approxEqual(base.transform.rotation, targetRot))
		{
			if (autoReverse)
			{
				reverse = !reverse;
			}
			else
			{
				targetRot *= Quaternion.Euler(rotationRange);
			}
		}
	}
}
