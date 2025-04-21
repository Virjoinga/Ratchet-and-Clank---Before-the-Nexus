using UnityEngine;

public class DebugFullReset : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnClick()
	{
		StatsTracker.instance.FullReset();
	}
}
