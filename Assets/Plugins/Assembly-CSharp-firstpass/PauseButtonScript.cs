using UnityEngine;

public class PauseButtonScript : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OnClick()
	{
		UIHUD hUD = UIManager.instance.GetHUD();
		if (GameController.instance.isPaused)
		{
			SFXManager.instance.PlaySound("UI_Resume");
			UIManager.instance.CloseMenu();
			if (hUD.hasPendingWeaponTutorial && GameController.instance.gameState == GameController.eGameState.GS_OnGround)
			{
				hUD.ShowPendingWeaponTutorial();
				return;
			}
			GameController.instance.Start321Countdown();
			UIManager.instance.GetHUD().Start321Countdown();
		}
		else
		{
			Time.timeScale = 0f;
			SFXManager.instance.StopAllSounds();
			MusicManager.instance.Pause();
			GameController.instance.isPaused = true;
			GameController.instance.inMenu = true;
			UIManager.instance.OpenMenu(UIManager.MenuPanels.PauseMenu);
		}
	}

	private void OnHover(bool isOver)
	{
		GameController.instance.playerController.dontFire = isOver;
	}
}
