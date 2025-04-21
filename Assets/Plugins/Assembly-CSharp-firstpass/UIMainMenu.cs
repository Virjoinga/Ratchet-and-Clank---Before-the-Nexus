using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : UIScreen
{
	public List<UITweener> ShowTweens;

	public UIButton SyncButton;

	public UIButton BackButton;

	public UIButton PSNLoginButton;

	public IconScript SyncIcon;

	public bool loginSuccess;

	public GameObject AndroidLeaderboardLabel;

	public GameObject IOSLeaderboardLabel;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(SyncButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(SyncButtonClicked));
		UIEventListener uIEventListener2 = UIEventListener.Get(SyncButton.gameObject);
		uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(SyncButtonPressed));
		UIEventListener.Get(PSNLoginButton.gameObject).onClick = PSNButtonClicked;
		UIEventListener uIEventListener3 = UIEventListener.Get(PSNLoginButton.gameObject);
		uIEventListener3.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onPress, new UIEventListener.BoolDelegate(PSNButtonPressed));
		UIEventListener.Get(BackButton.gameObject).onClick = BackButtonClicked;
		UIEventListener.Get(BackButton.gameObject).onPress = BackButtonPressed;
		AndroidLeaderboardLabel.SetActive(true);
		IOSLeaderboardLabel.SetActive(false);
		UIManager.instance.SwapFont();
	}

	private void PSNButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			BackButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
		}
		else
		{
			BackButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
		}
	}

	private void SyncButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			BackButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
		}
		else
		{
			BackButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
		}
	}

	private void BackButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			SyncButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
			PSNLoginButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
		}
		else
		{
			SyncButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
			PSNLoginButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
		}
	}

	public void BackButtonClicked(GameObject Obj)
	{
		UIManager.instance.HideHexBG();
	}

	public void SyncButtonClicked(GameObject obj)
	{
		if (UIManager.instance.CheckWifi())
		{
			if (loginSuccess || ServerConnection.IsAuthorized())
			{
				UIManager.instance.OpenMenu(UIManager.MenuPanels.SyncMenu);
			}
			else
			{
				UIManager.instance.ShowPSNNotLoggedIn();
			}
		}
	}

	public void PSNButtonClicked(GameObject obj)
	{
		if (UIManager.instance.CheckWifi())
		{
			if (loginSuccess || ServerConnection.IsAuthorized())
			{
				UIManager.instance.PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_157", true);
			}
			else
			{
				UIManager.instance.OpenMenu(UIManager.MenuPanels.PSNLoginMenu);
			}
		}
	}

	public void OnLoginFlowSuccess()
	{
		loginSuccess = true;
		UpdateSyncButton();
	}

	public void OnLoginFlowFail()
	{
		UIManager.instance.PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_140", true);
	}

	public override void Show()
	{
		base.Show();
		EasyAnalytics.Instance.sendView("/MainMenu");
		for (int i = 0; i < ShowTweens.Count; i++)
		{
			ShowTweens[i].Reset();
			ShowTweens[i].Play(true);
		}
		UpdateSyncButton();
		UIManager.instance.ShowHexBG();
	}

	public void UpdateSyncButton()
	{
		if (loginSuccess || ServerConnection.IsAuthorized())
		{
			SyncIcon.SetIconSprite("icon_menu_rarsync", IconScript.HexLevel.HEX_V3);
		}
	}
}
