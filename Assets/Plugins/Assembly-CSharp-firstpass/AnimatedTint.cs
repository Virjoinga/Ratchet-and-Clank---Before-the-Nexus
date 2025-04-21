using UnityEngine;

public class AnimatedTint : MonoBehaviour
{
	public bool animateRed = true;

	public AnimationCurve Red = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public bool animateGreen = true;

	public AnimationCurve Green = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public bool animateBlue = true;

	public AnimationCurve Blue = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public bool animateAlpha = true;

	public AnimationCurve Alpha = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public float duration = 1f;

	public GameObject objectToTint;

	protected float endTime = -1f;

	protected Color oldColor;

	protected bool isPlaying;

	private void Start()
	{
		if (objectToTint == null)
		{
			objectToTint = base.gameObject;
		}
		oldColor = objectToTint.GetComponent<Renderer>().material.color;
		isPlaying = false;
	}

	private void PlayTintAnimation()
	{
		endTime = Time.realtimeSinceStartup + duration;
		oldColor = objectToTint.GetComponent<Renderer>().material.color;
		isPlaying = true;
	}

	private void Update()
	{
		float num = endTime - Time.realtimeSinceStartup;
		if (Time.time <= endTime)
		{
			float time = Mathf.Clamp01(num / duration);
			Color color = new Color((!animateRed) ? oldColor.r : Red.Evaluate(time), (!animateGreen) ? oldColor.g : Green.Evaluate(time), (!animateBlue) ? oldColor.b : Blue.Evaluate(time), (!animateAlpha) ? oldColor.a : Alpha.Evaluate(time));
			objectToTint.GetComponent<Renderer>().material.color = color;
		}
		else if (isPlaying)
		{
			isPlaying = false;
			objectToTint.GetComponent<Renderer>().material.color = oldColor;
		}
	}
}
