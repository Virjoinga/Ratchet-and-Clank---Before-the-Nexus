using System;
using UnityEngine;

public class UISplashscreen : MonoBehaviour
{
	public GameObject Overlay;

	public bool AllowSkip;

	public GameObject DarksideLogo;

	public GameObject InsomniacLogo;

	public GameObject UKFlag;

	public GameObject USFlag;

	public GameObject NOFlag;

	private bool FlagsShowing;

	private bool ActiveSplash;

	private void Start()
	{
		ActiveSplash = true;
		if (AllowSkip)
		{
			UIEventListener uIEventListener = UIEventListener.Get(Overlay);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OverlayClicked));
		}
	}

	public void LoadingFinished()
	{
		if (!ActiveSplash)
		{
		}
	}

	private void LogosFinished()
	{
		FlagsShowing = true;
		DarksideLogo.SetActive(false);
		UIManager.instance.PlayIntroMovie();
		if (GameController.instance.ShouldShowFlags())
		{
			UIManager.instance.PersistentUI.ShowPopup("PopupFlags", "UI_Menu_129", true);
			PopupBoxScript popup = UIManager.instance.PersistentUI.Popup;
			UIEventListener.Get(popup.Overlay.gameObject).onClick = null;
			USFlag = popup.transform.Find("ConfirmationBox/FlagGrid/1US").gameObject;
			UKFlag = popup.transform.Find("ConfirmationBox/FlagGrid/2UK").gameObject;
			NOFlag = popup.transform.Find("ConfirmationBox/3NO").gameObject;
			UIEventListener uIEventListener = UIEventListener.Get(USFlag);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(USFlagClicked));
			UIEventListener uIEventListener2 = UIEventListener.Get(USFlag);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(popup.OverlayClicked));
			UIEventListener uIEventListener3 = UIEventListener.Get(UKFlag);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(UKFlagClicked));
			UIEventListener uIEventListener4 = UIEventListener.Get(UKFlag);
			uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(popup.OverlayClicked));
			UIEventListener uIEventListener5 = UIEventListener.Get(NOFlag);
			uIEventListener5.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener5.onClick, new UIEventListener.VoidDelegate(NOFlagClicked));
			UIEventListener uIEventListener6 = UIEventListener.Get(NOFlag);
			uIEventListener6.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener6.onClick, new UIEventListener.VoidDelegate(popup.OverlayClicked));
		}
		else
		{
			Hide();
		}
	}

	private void USFlagClicked(GameObject obj)
	{
		GameController.instance.SetAmericanEnglish();
		Hide();
	}

	private void UKFlagClicked(GameObject obj)
	{
		GameController.instance.SetBritishEnglish();
		Hide();
	}

	private void NOFlagClicked(GameObject obj)
	{
		GameController.instance.SetNorwegian();
		Hide();
	}

	private void OverlayClicked(GameObject obj)
	{
		if (!FlagsShowing)
		{
			LogosFinished();
		}
	}

	public void Hide()
	{
		UIManager.instance.PersistentUI.HidePopup();
		GameObjectPool.instance.UnloadIndividualObject(base.gameObject);
		ActiveSplash = false;
		UIStartMenu startMenu = UIManager.instance.GetStartMenu();
		if (startMenu != null)
		{
			UIManager.instance.HideHexBG(true);
		}
	}
}
