public class UIChallenges : UIScreen
{
	public ChallengeGridScript ListGrid;

	public UILabel CompletionStatusLabel;

	private bool Populated;

	private void Start()
	{
		if (!Populated)
		{
			ListGrid.PopulateList();
			Populated = true;
		}
		UpdateAchievements();
	}

	public override void Show()
	{
		EasyAnalytics.Instance.sendView("/Challenges");
		if (!Populated)
		{
			ListGrid.PopulateList();
			Populated = true;
		}
		UpdateAchievements();
	}

	private void UpdateAchievements()
	{
		ListGrid.UpdateActiveChallenges();
		UpdateCompletionStatus();
	}

	private void UpdateCompletionStatus()
	{
		CompletionStatusLabel.text = ListGrid.numCompleted + "/" + ChallengeSystem.instance.challengeList.Count;
	}
}
