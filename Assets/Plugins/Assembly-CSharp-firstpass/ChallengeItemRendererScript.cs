using UnityEngine;

public class ChallengeItemRendererScript : MonoBehaviour
{
	public UILabel DescLabel;

	public UILabel CompletionStatusLabel;

	public UICheckbox CompletedIcon;

	public UISlider ProgressBar;

	public IconScript Icon;

	public ChallengeSystem.ChallengeInfo Challenge;

	public UISprite Background;

	public ParticleSystem FX;

	public GameObject Reward;

	public UILabel RewardLabel;

	public int ActiveChallengeIndex;

	private void Start()
	{
	}

	public void Init(ChallengeSystem.ChallengeInfo TheChallenge, int ChallengeIndex, Transform Parent, bool DoTransform = true)
	{
		Challenge = TheChallenge;
		ActiveChallengeIndex = ChallengeIndex;
		if (DoTransform)
		{
			base.transform.parent = Parent.transform;
			base.transform.localPosition = Vector3.zero;
			base.transform.localScale = Vector3.one;
		}
	}

	public void SetParentUI(UIPauseMenu PauseMenu)
	{
	}
}
