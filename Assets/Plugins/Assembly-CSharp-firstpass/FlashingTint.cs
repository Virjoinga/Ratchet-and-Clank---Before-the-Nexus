using UnityEngine;

public class FlashingTint : MonoBehaviour
{
	public float duration = 1f;

	public float frequency = 1f;

	public Color targetColor;

	public GameObject objectToTint;

	protected float endTime = -1f;

	protected Color oldColor;

	protected bool isPlaying;

	private void Start()
	{
		isPlaying = false;
	}

	private void FlashTint()
	{
		if (!isPlaying)
		{
			if (objectToTint == null)
			{
				objectToTint = base.gameObject;
			}
			oldColor = objectToTint.GetComponent<Renderer>().material.color;
			endTime = Time.realtimeSinceStartup + duration;
			isPlaying = true;
		}
	}

	private void Update()
	{
		if (isPlaying)
		{
			float num = endTime - Time.realtimeSinceStartup;
			if (Time.realtimeSinceStartup <= endTime)
			{
				float num2 = Mathf.Clamp01(num / duration);
				float t = Mathf.PingPong((1f - num2) * 2f * frequency, 1f);
				objectToTint.GetComponent<Renderer>().material.color = Color.Lerp(oldColor, targetColor, t);
			}
			else if (isPlaying)
			{
				isPlaying = false;
				objectToTint.GetComponent<Renderer>().material.color = oldColor;
			}
		}
	}

	private void ResetTint()
	{
		objectToTint.GetComponent<Renderer>().material.color = oldColor;
		endTime = Time.time - 1f;
		isPlaying = false;
	}
}
