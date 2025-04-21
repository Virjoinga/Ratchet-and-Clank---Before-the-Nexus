using UnityEngine;

public class GuiManager : MonoBehaviour
{
	private bool[] tog = new bool[5];

	private float ButtonHeight = Screen.height / 5;

	private float ButtonWidth = Screen.width / 2 - 20;

	private void Start()
	{
		tog = new bool[5] { false, false, false, false, true };
	}

	private void Update()
	{
		ButtonHeight = Screen.height / 5;
		ButtonWidth = Screen.width / 2 - 20;
	}

	private void OnGUI()
	{
		GUI.color = Color.black;
		float num = 10f;
		float num2 = 10f;
		GUI.color = Color.white;
		if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Play Video with name"))
		{
			IOSVideoPlayerBinding.instance.PlayVideo("Teaser_Final.mov");
		}
		num2 += ButtonHeight + 10f;
		if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Play Video with path"))
		{
			IOSVideoPlayerBinding.instance.PlayVideo(Application.dataPath + "/Raw/", "Teaser_Final.mov");
		}
		num2 += ButtonHeight + 10f;
		if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Play with Subtitles"))
		{
			IOSVideoPlayerBinding.instance.playSubtitlesFromFile("Teaser_Final_EN.srt");
			IOSVideoPlayerBinding.instance.PlayVideo("Teaser_Final.mov");
		}
		num2 += ButtonHeight + 10f;
		if (!tog[4])
		{
			GUI.color = Color.green;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Should pause unity"))
			{
				IOSVideoPlayerBinding.instance.shouldPauseUnity(true);
				tog[4] = !tog[4];
			}
			GUI.color = Color.white;
			GUI.Label(new Rect(num, num2 + ButtonHeight + 5f, ButtonWidth, ButtonHeight), "Current Status: Unity will keep running during video playback.");
		}
		else
		{
			GUI.color = Color.red;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Should not pause unity"))
			{
				IOSVideoPlayerBinding.instance.shouldPauseUnity(false);
				tog[4] = !tog[4];
			}
			GUI.color = Color.white;
			GUI.Label(new Rect(num, num2 + ButtonHeight + 5f, ButtonWidth, ButtonHeight), "Current Status: Unity will be paused during video playback.");
		}
		num2 = 10f;
		num += ButtonWidth + 10f;
		if (!tog[0])
		{
			GUI.color = Color.green;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Add Play/Pause Button"))
			{
				IOSVideoPlayerBinding.instance.addPlayPauseButton();
				tog[0] = !tog[0];
			}
		}
		else
		{
			GUI.color = Color.red;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Remove Play/Pause Button"))
			{
				IOSVideoPlayerBinding.instance.removePlayPauseButton();
				tog[0] = !tog[0];
			}
		}
		num2 += ButtonHeight + 10f;
		if (!tog[2])
		{
			GUI.color = Color.green;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Add Stop Button"))
			{
				IOSVideoPlayerBinding.instance.addStopButton();
				tog[2] = !tog[2];
			}
		}
		else
		{
			GUI.color = Color.red;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Remove Stop Button"))
			{
				IOSVideoPlayerBinding.instance.removeStopButton();
				tog[2] = !tog[2];
			}
		}
		num2 += ButtonHeight + 10f;
		if (!tog[3])
		{
			GUI.color = Color.green;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Add Seeking Bar"))
			{
				IOSVideoPlayerBinding.instance.addSeekingBar();
				tog[3] = !tog[3];
			}
		}
		else
		{
			GUI.color = Color.red;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Remove Seeking Bar"))
			{
				IOSVideoPlayerBinding.instance.removeSeekingBar();
				tog[3] = !tog[3];
			}
		}
		num2 += ButtonHeight + 10f;
		if (!tog[1])
		{
			GUI.color = Color.green;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Add Skip Button"))
			{
				IOSVideoPlayerBinding.instance.addSkipButton();
				tog[1] = !tog[1];
			}
		}
		else
		{
			GUI.color = Color.red;
			if (GUI.Button(new Rect(num, num2, ButtonWidth, ButtonHeight), "Remove Skip Button"))
			{
				IOSVideoPlayerBinding.instance.removeSkipButton();
				tog[1] = !tog[1];
			}
		}
	}
}
