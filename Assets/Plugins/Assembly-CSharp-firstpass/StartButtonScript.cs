using UnityEngine;

public class StartButtonScript : MonoBehaviour
{
	public bool InitialStart;

	public bool FromTutorial;

	private void Start()
	{
	}

	private void OnClick()
	{
		if (PlayerPrefs.GetInt(GameController.instance.forceInstructions.ToString()) == 1)
		{
			GameController.instance.forceInstructions = false;
		}
		GameController.instance.playerController.friendChallengeEnabled = false;
		if (GameController.instance.forceInstructions)
		{
			UIManager.instance.OpenMenu(UIManager.MenuPanels.TutorialMenu);
			UITutorial componentInChildren = UIManager.instance.GetComponentInChildren<UITutorial>();
			componentInChildren.StartButton.SetActive(true);
			componentInChildren.BackButton.SetActive(false);
		}
		else if (InitialStart)
		{
			UIManager.instance.HideBG();
			GameController.instance.InitialStartGame();
			StatsTracker.instance.SaveStatsAndReset();
		}
		else
		{
			UIManager.instance.HideBG(true);
			ChallengeSystem.instance.ActiveChallengesDirty = true;
			ChallengeSystem.instance.SelectNewChallenges();
			GameController.instance.StartGame();
			UIManager.instance.GetHUD().HUD.SetActive(false);
			UIManager.instance.GetHUD().StartSkillPresentation();
		}
	}
}
