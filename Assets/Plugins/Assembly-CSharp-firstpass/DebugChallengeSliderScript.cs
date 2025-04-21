using UnityEngine;

public class DebugChallengeSliderScript : MonoBehaviour
{
	public int ChallengeIndex;

	public UILabel Label;

	public int SliderIndex;

	private void Start()
	{
		int[] challengeIDsDebug = ChallengeSystem.instance.GetChallengeIDsDebug();
		for (int i = 0; i < challengeIDsDebug.Length; i++)
		{
			GetComponent<UISlider>().numberOfSteps = challengeIDsDebug.Length;
		}
	}

	private void OnSliderChange(float newValue)
	{
		int[] challengeIDsDebug = ChallengeSystem.instance.GetChallengeIDsDebug();
		UISlider component = GetComponent<UISlider>();
		SliderIndex = (int)(component.sliderValue * (float)component.numberOfSteps);
		SliderIndex = Mathf.Clamp(SliderIndex, 0, ChallengeSystem.instance.challengeList.Count - 1);
		ChallengeSystem.ChallengeInfo challengeInfo = ChallengeSystem.instance.challengeList[challengeIDsDebug[SliderIndex]];
		Label.text = "ID: " + challengeInfo.UID + " " + challengeInfo.Description;
	}
}
