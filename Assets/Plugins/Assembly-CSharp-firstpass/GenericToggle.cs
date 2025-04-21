using UnityEngine;

public class GenericToggle : MonoBehaviour
{
	public GameObject otherObj;

	public float sustainTime;

	public bool onAtStart;

	private float currentSustainTime;

	private void Start()
	{
		currentSustainTime = sustainTime;
		if (!onAtStart)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (base.gameObject.activeSelf)
		{
			currentSustainTime -= Time.deltaTime;
			if (currentSustainTime <= 0f)
			{
				currentSustainTime = sustainTime;
				otherObj.SetActive(true);
				base.gameObject.SetActive(false);
			}
		}
	}
}
