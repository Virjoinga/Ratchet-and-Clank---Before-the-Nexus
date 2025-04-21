using UnityEngine;

public class UIAchievements : UIScreen
{
	public ChallengeGridScript ListGrid;

	public UIDraggablePanel ScrollPanel;

	public UILabel CompletionStatusLabel;

	public Vector3 StartLoc;

	private void Start()
	{
	}

	public override void Show()
	{
		base.Show();
		EasyAnalytics.Instance.sendView("/Achievements");
		UpdateAchievements();
		ScrollPanel.ResetPosition();
	}

	private void UpdateAchievements()
	{
		ListGrid.UpdateActiveChallenges();
	}
}
