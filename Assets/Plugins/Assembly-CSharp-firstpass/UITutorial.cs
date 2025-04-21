using UnityEngine;

public class UITutorial : UIScreen
{
	public GameObject StartButton;

	public GameObject BackButton;

	private void Start()
	{
	}

	public override void Show()
	{
		EasyAnalytics.Instance.sendView("/Tutorial");
		StartButton.SetActive(false);
		BackButton.SetActive(true);
		PlayerPrefs.SetInt(GameController.instance.forceInstructions.ToString(), 1);
		PlayerPrefs.Save();
	}
}
