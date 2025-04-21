using UnityEngine;

public class MyFPSCounter : MonoBehaviour
{
	private float fpsAvg;

	private float fpsSum;

	private float fps;

	public UILabel fpsLabel;

	public static MyFPSCounter instance;

	private void Start()
	{
		if (fpsLabel != null)
		{
			fpsLabel.text = string.Empty;
		}
		instance = this;
	}

	public void FPSUpdate(float deltaTime)
	{
		if (deltaTime > 0f)
		{
			fps = 1f / deltaTime;
		}
		fpsSum += fps;
		fpsAvg = fpsSum * 0.02f;
		fpsSum -= fpsAvg;
		if (!(fpsLabel != null))
		{
		}
	}

	public void toggleStats()
	{
	}
}
