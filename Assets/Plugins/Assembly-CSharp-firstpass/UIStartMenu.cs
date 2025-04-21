using System;
using UnityEngine;

public class UIStartMenu : UIScreen
{
	public GameObject PlayButton;

	public GameObject ShopButton;

	public GameObject MenuButton;

	public UITweener PlayTween;

	public UITweener TutorialTween;

	public UITweener MenuTween;

	public UITweener GameLogoTween;

	public UITweener GameLogoBrightTween;

	public UITweener HexBGTween;

	public UILabel ChallengeFriendName;

	public UILabel ChallengeFriendScore;

	public UILabel ChallengeFriendLabel;

	public GameObject IOSSignInLabel;

	public GameObject AndroidSignInLabel;

	public GameObject FriendChallengeBox;

	public UIDebugMenu DebugMenu;

	public bool showDebug;

	private MuteMusicScript MuteMusic;

	private MuteSFxScript MuteSFx;

	public GameObject AffordableItemIndicator;

	public GameObject PrivacyPolicyButton;

	public GameObject LicenseButton;

	private void Start()
	{
		if (showDebug)
		{
			DebugMenu.gameObject.SetActive(true);
		}
		else
		{
			DebugMenu.gameObject.SetActive(false);
		}
		UIEventListener uIEventListener = UIEventListener.Get(PlayButton);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPlayButtonPressed));
		UIEventListener uIEventListener2 = UIEventListener.Get(ShopButton);
		uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnShopButtonPressed));
		UIEventListener uIEventListener3 = UIEventListener.Get(MenuButton);
		uIEventListener3.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onPress, new UIEventListener.BoolDelegate(OnMenuButtonPressed));
		UIEventListener uIEventListener4 = UIEventListener.Get(PrivacyPolicyButton);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnPrivacyButtonClicked));
		UIEventListener uIEventListener5 = UIEventListener.Get(LicenseButton);
		uIEventListener5.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener5.onClick, new UIEventListener.VoidDelegate(OnLicenseButtonClicked));
	}

	private void OnLicenseButtonClicked(GameObject obj)
	{
		Application.OpenURL("http://us.playstation.com/support/terms/mobile.htm");
	}

	private void OnPrivacyButtonClicked(GameObject obj)
	{
		Application.OpenURL("http://us.playstation.com/support/privacypolicy/mobile.htm");
	}

	private void OnPlayButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			MenuButton.GetComponent<BoxCollider>().collider.enabled = false;
			ShopButton.GetComponent<BoxCollider>().collider.enabled = false;
		}
		else
		{
			MenuButton.GetComponent<BoxCollider>().collider.enabled = true;
			ShopButton.GetComponent<BoxCollider>().collider.enabled = true;
		}
	}

	private void OnShopButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			PlayButton.GetComponent<BoxCollider>().collider.enabled = false;
			MenuButton.GetComponent<BoxCollider>().collider.enabled = false;
		}
		else
		{
			PlayButton.GetComponent<BoxCollider>().collider.enabled = true;
			MenuButton.GetComponent<BoxCollider>().collider.enabled = true;
		}
	}

	private void OnMenuButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			PlayButton.GetComponent<BoxCollider>().collider.enabled = false;
			ShopButton.GetComponent<BoxCollider>().collider.enabled = false;
		}
		else
		{
			PlayButton.GetComponent<BoxCollider>().collider.enabled = true;
			ShopButton.GetComponent<BoxCollider>().collider.enabled = true;
		}
	}

	public override void Show()
	{
		base.Show();
		EasyAnalytics.Instance.sendView("/StartMenu");
		SocialManager.instance.LoadFriendChallengeScore(SocialManager.instance.isSignedIn);
		PlayTween.Reset();
		TutorialTween.Reset();
		MenuTween.Reset();
		HexBGTween.Reset();
		GameLogoTween.Reset();
		GameLogoBrightTween.Reset();
		PlayTween.Play(true);
		TutorialTween.Play(true);
		MenuTween.Play(true);
		HexBGTween.Play(true);
		GameLogoTween.Play(true);
		GameLogoBrightTween.Play(true);
		MusicManager.instance.Play(MusicManager.eMusicTrackType.Menu, false, 2f);
		UpdateAffordableItemIndicator();
		UIManager.instance.SwapFont();
		MuteMusic = base.transform.Find("StartMenuContainer/Anchor - Bottom Left").Find("MusicMuteOffset").Find("Checkbox")
			.gameObject.GetComponent<MuteMusicScript>();
		MuteSFx = base.transform.Find("StartMenuContainer/Anchor - Bottom Left").Find("SFXMuteOffset").Find("Checkbox")
			.gameObject.GetComponent<MuteSFxScript>();
		MuteMusic.UpdateButton();
		MuteSFx.UpdateButton();
		ChallengeSystem.instance.SelectNewChallenges();
	}

	public void UpdateFriendChallengeButton()
	{
		if (SocialManager.instance.isSignedIn)
		{
			AndroidSignInLabel.SetActive(false);
			IOSSignInLabel.SetActive(false);
			if (SocialManager.instance.GetFriendScoreToBeat() > 0)
			{
				ChallengeFriendScore.gameObject.SetActive(true);
				ChallengeFriendName.gameObject.SetActive(true);
				ChallengeFriendLabel.gameObject.SetActive(true);
				ChallengeFriendName.text = SocialManager.instance.GetFriendChallengeName();
				ChallengeFriendScore.text = SocialManager.instance.GetFriendChallengeScore();
				Debug.Log(string.Format("challenge friend {0} with score {1}", ChallengeFriendName.text, ChallengeFriendScore.text));
				UIEventListener.Get(FriendChallengeBox).onClick = FriendChallengeBoxClickedLoggedIn;
			}
			else
			{
				FriendChallengeBox.gameObject.SetActive(false);
				ChallengeFriendScore.gameObject.SetActive(false);
				ChallengeFriendName.gameObject.SetActive(false);
				ChallengeFriendLabel.gameObject.SetActive(false);
				UIEventListener.Get(FriendChallengeBox).onClick = null;
			}
		}
		else
		{
			AndroidSignInLabel.SetActive(true);
			IOSSignInLabel.SetActive(false);
			ChallengeFriendLabel.gameObject.SetActive(false);
			ChallengeFriendScore.gameObject.SetActive(false);
			ChallengeFriendName.gameObject.SetActive(false);
			UIEventListener.Get(FriendChallengeBox).onClick = FriendChallengeBoxClickedLoggedOut;
		}
	}

	private void FriendChallengeBoxClickedLoggedIn(GameObject obj)
	{
		if (SocialManager.instance.GetFriendScoreToBeat() > 0)
		{
			GameController.instance.playerController.friendChallengeEnabled = true;
		}
		UIManager.instance.HideBG();
		GameController.instance.InitialStartGame();
	}

	private void FriendChallengeBoxClickedLoggedOut(GameObject obj)
	{
		if (UIManager.instance.CheckWifi())
		{
			SocialManager.instance.SignIn();
		}
	}

	public void UpdateAffordableItemIndicator()
	{
		int affordableVendorItems = UIManager.instance.GetAffordableVendorItems();
		if (affordableVendorItems > 0)
		{
			AffordableItemIndicator.SetActive(true);
		}
		else
		{
			AffordableItemIndicator.SetActive(false);
		}
	}

	private void Update()
	{
		SFXManager.instance.StopAllLoopingSounds();
	}
}
