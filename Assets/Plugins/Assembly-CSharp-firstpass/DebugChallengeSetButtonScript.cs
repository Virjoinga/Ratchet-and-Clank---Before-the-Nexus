using UnityEngine;

public class DebugChallengeSetButtonScript : MonoBehaviour
{
	public int ChallengeIndex;

	public DebugChallengeSliderScript ChallengeSlider0;

	public DebugChallengeSliderScript ChallengeSlider1;

	public DebugChallengeSliderScript ChallengeSlider2;

	private void OnClick()
	{
		int[] challengeIDsDebug = ChallengeSystem.instance.GetChallengeIDsDebug();
		Debug.Log("CLICK " + challengeIDsDebug[ChallengeSlider0.SliderIndex] + "," + challengeIDsDebug[ChallengeSlider1.SliderIndex] + "," + challengeIDsDebug[ChallengeSlider2.SliderIndex]);
	}
}
