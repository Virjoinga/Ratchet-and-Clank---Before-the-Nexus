using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public enum MenuPanels
	{
		HUD = 0,
		PauseMenu = 1,
		EndRoundScreen = 2,
		PSNLoginMenu = 3,
		AdScreen = 4,
		StartMenu = 5,
		MainMenu = 6,
		Credits = 7,
		Achievements = 8,
		Challenges = 9,
		VendorMenu = 10,
		VendorFrontMenu = 11,
		ProfileMenu = 12,
		TutorialMenu = 13,
		Leaderboards = 14,
		SyncMenu = 15,
		InvalidMenu = 99
	}

	private const float BASE_COST = 375f;

	public static UIManager instance;

	private List<GameObject> m_LoadedMenus;

	private Stack m_CurrentMenus;

	private int numHUDMenus = 5;

	private UIHUD HUD;

	private UIStartMenu StartMenu;

	private UIEndRoundScreen EndRoundScreen;

	private UIPauseMenu PauseMenu;

	private UIVendor VendorMenu;

	private UIVendorFrontMenu VendorFrontMenu;

	public UIPersistent PersistentUI;

	public UIFont JapaneseFont;

	[HideInInspector]
	public bool ShouldSwapFonts;

	private UISplashscreen Splashscreen;

	private bool BGShowing = true;

	private bool HexBGShowing;

	public bool SwipingOn = true;

	public int[] RaritaniumValues = new int[3];

	public int[] RaritaniumCosts = new int[3];

	public int[] ArmorCosts = new int[3];

	public int[,] GadgetCosts = new int[4, 3]
	{
		{ 0, 750, 1500 },
		{ 750, 1500, 3000 },
		{ 937, 1875, 3750 },
		{ 1312, 2625, 5250 }
	};

	public int[,] MegaWeaponCosts = new int[3, 3]
	{
		{ 562, 1125, 2250 },
		{ 1125, 2250, 4500 },
		{ 1500, 3000, 6000 }
	};

	public float CurrentSubtitleStartTime;

	public float CurrentSubtitleEndTime;

	public bool ShowingSubtitle;

	public bool InitialLoad = true;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("UIManager: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/Splashscreen")) as GameObject;
		Splashscreen = gameObject.GetComponent<UISplashscreen>();
		Splashscreen.transform.parent = base.transform;
		Splashscreen.transform.localPosition = Vector3.zero;
		Splashscreen.transform.localScale = Vector3.one;
		PersistentUI.MoviePlane.SetActive(false);
		LoadMenuResources();
	}

	public UISplashscreen GetSplashScreen()
	{
		return Splashscreen;
	}

	public void InitialLoadComplete()
	{
		InitialLoad = false;
		if (!IsMoviePlaying())
		{
			GetStartMenu().gameObject.SetActive(false);
			OpenMenu(MenuPanels.StartMenu);
		}
	}

	private void Start()
	{
		m_CurrentMenus = new Stack();
		if (m_LoadedMenus[5] != null)
		{
			m_CurrentMenus.Push(MenuPanels.StartMenu);
			m_LoadedMenus[5].SetActive(true);
			SwapFont();
		}
	}

	public MenuPanels GetCurrentMenu()
	{
		if (m_CurrentMenus == null)
		{
			return MenuPanels.InvalidMenu;
		}
		return (MenuPanels)(int)m_CurrentMenus.Peek();
	}

	private void LoadMenuResources(bool skipHUD = false)
	{
		if (skipHUD)
		{
			LoadDynamicMenus();
		}
		else
		{
			LoadAllMenus();
		}
	}

	private void LoadDynamicMenus()
	{
		int num = Enum.GetNames(typeof(MenuPanels)).Length;
		for (int i = 0; i < numHUDMenus; i++)
		{
			m_LoadedMenus[i].SetActive(false);
		}
		for (int j = numHUDMenus; j < num - 1; j++)
		{
			string text = ((MenuPanels)j).ToString();
			m_LoadedMenus[j] = UnityEngine.Object.Instantiate(Resources.Load("UI/" + text)) as GameObject;
			if (m_LoadedMenus[j] != null)
			{
				m_LoadedMenus[j].SetActive(false);
				m_LoadedMenus[j].transform.parent = base.transform;
				m_LoadedMenus[j].transform.localPosition = Vector3.zero;
				m_LoadedMenus[j].transform.localScale = Vector3.one;
			}
			else
			{
				Debug.LogError("UIManager: " + text + " not loaded");
			}
		}
	}

	private void LoadAllMenus()
	{
		int num = Enum.GetNames(typeof(MenuPanels)).Length;
		m_LoadedMenus = new List<GameObject>();
		for (int i = 0; i < num - 1; i++)
		{
			string text = ((MenuPanels)i).ToString();
			m_LoadedMenus.Add(UnityEngine.Object.Instantiate(Resources.Load("UI/" + text)) as GameObject);
			if (m_LoadedMenus[i] != null)
			{
				m_LoadedMenus[i].SetActive(false);
				m_LoadedMenus[i].transform.parent = base.transform;
				m_LoadedMenus[i].transform.localPosition = Vector3.zero;
				m_LoadedMenus[i].transform.localScale = Vector3.one;
			}
			else
			{
				Debug.LogError("UIManager: " + text + " not loaded");
			}
		}
	}

	public void OpenMenu(MenuPanels menu)
	{
		if (menu == MenuPanels.Leaderboards)
		{
			SocialManager.instance.ShowLeaderboards();
		}
		else
		{
			if (m_CurrentMenus == null)
			{
				return;
			}
			if ((int)m_CurrentMenus.Peek() == 2 || ((int)m_CurrentMenus.Peek() == 1 && menu == MenuPanels.StartMenu))
			{
				if ((int)m_CurrentMenus.Peek() == 1 && menu == MenuPanels.StartMenu)
				{
					GameController.instance.isPaused = false;
					GameController.instance.inMenu = false;
				}
				LoadMenuResources(true);
				m_CurrentMenus.Pop();
				if (menu != MenuPanels.StartMenu)
				{
					m_CurrentMenus.Push(MenuPanels.StartMenu);
				}
				else
				{
					instance.ShowBG(true);
				}
			}
			if (m_LoadedMenus[(int)menu] == null)
			{
				Debug.LogError("UIManager: " + menu.ToString() + " not loaded");
				return;
			}
			UIScreen component = m_LoadedMenus[(int)m_CurrentMenus.Peek()].GetComponent<UIScreen>();
			if (component != null)
			{
				component.Hide();
			}
			m_LoadedMenus[(int)m_CurrentMenus.Peek()].SetActive(false);
			m_CurrentMenus.Push(menu);
			m_LoadedMenus[(int)m_CurrentMenus.Peek()].SetActive(true);
			component = m_LoadedMenus[(int)m_CurrentMenus.Peek()].GetComponent<UIScreen>();
			SwapFont();
			if (component != null)
			{
				component.Show();
			}
			if (menu == MenuPanels.HUD)
			{
				int num = Enum.GetNames(typeof(MenuPanels)).Length;
				for (int i = numHUDMenus; i < num - 1; i++)
				{
					GameObjectPool.instance.UnloadIndividualObject(m_LoadedMenus[i]);
					m_LoadedMenus[i] = null;
				}
			}
		}
	}

	public void CloseMenu()
	{
		if ((int)m_CurrentMenus.Peek() == 5)
		{
			Debug.LogError("Exit game signal received");
			return;
		}
		m_LoadedMenus[(int)m_CurrentMenus.Peek()].SetActive(false);
		UIScreen component = m_LoadedMenus[(int)m_CurrentMenus.Peek()].GetComponent<UIScreen>();
		if (component != null)
		{
			component.Hide();
		}
		m_CurrentMenus.Pop();
		m_LoadedMenus[(int)m_CurrentMenus.Peek()].SetActive(true);
		component = m_LoadedMenus[(int)m_CurrentMenus.Peek()].GetComponent<UIScreen>();
		if (component != null)
		{
			component.Show();
		}
	}

	public UIHUD GetHUD()
	{
		if (HUD == null)
		{
			HUD = GetComponentInChildren<UIHUD>();
		}
		return HUD;
	}

	public UIStartMenu GetStartMenu()
	{
		if (StartMenu == null)
		{
			StartMenu = GetComponentInChildren<UIStartMenu>();
		}
		return StartMenu;
	}

	public UIEndRoundScreen GetEndRoundScreen()
	{
		if (EndRoundScreen == null)
		{
			EndRoundScreen = GetComponentInChildren<UIEndRoundScreen>();
		}
		return EndRoundScreen;
	}

	public UIPauseMenu GetPauseMenu()
	{
		if (PauseMenu == null)
		{
			PauseMenu = GetComponentInChildren<UIPauseMenu>();
		}
		return PauseMenu;
	}

	public UIVendor GetVendorMenu()
	{
		if (VendorMenu == null)
		{
			VendorMenu = GetComponentInChildren<UIVendor>();
		}
		return VendorMenu;
	}

	public UIVendorFrontMenu GetVendorFrontMenu()
	{
		if (VendorFrontMenu == null)
		{
			VendorFrontMenu = GetComponentInChildren<UIVendorFrontMenu>();
		}
		return VendorFrontMenu;
	}

	public void SetInput(bool bOn)
	{
		UICamera component = GameObject.Find("UICamera").GetComponent<UICamera>();
		component.useTouch = bOn;
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			component.useMouse = bOn;
		}
	}

	public void ShowFriendChallengePopup(bool Victory, int Reward)
	{
		string text = "UI_Menu_160";
		string text2 = "UI_Menu_159";
		string confirmTextLocKey = ((!Victory) ? text2 : text);
		PersistentUI.ShowPopup("PopupFriendChallenge", confirmTextLocKey, true);
		UILabel component = PersistentUI.Popup.transform.Find("ConfirmationBox/YourName").GetComponent<UILabel>();
		UILabel component2 = PersistentUI.Popup.transform.Find("ConfirmationBox/YourScore").GetComponent<UILabel>();
		UILabel component3 = PersistentUI.Popup.transform.Find("ConfirmationBox/FriendName").GetComponent<UILabel>();
		UILabel component4 = PersistentUI.Popup.transform.Find("ConfirmationBox/FriendScore").GetComponent<UILabel>();
		UILabel component5 = PersistentUI.Popup.transform.Find("ConfirmationBox/RewardLabel").GetComponent<UILabel>();
		UILabel component6 = PersistentUI.Popup.transform.Find("ConfirmationBox/PenaltyLabel").GetComponent<UILabel>();
		if (Victory)
		{
			component5.gameObject.SetActive(true);
			component6.gameObject.SetActive(false);
			component5.text = "+" + Reward;
		}
		else
		{
			component5.gameObject.SetActive(false);
			component6.gameObject.SetActive(true);
			component6.text = Reward.ToString();
		}
		component.text = SocialManager.instance.CurrentPlayerName;
		component2.text = GameController.instance.GetScore().ToString();
		component3.text = SocialManager.instance.GetFriendChallengeName();
		component4.text = SocialManager.instance.GetFriendChallengeScore();
		PersistentUI.SetPopupOKButtonCallback(HeroBoltCancelClicked);
		UIEventListener uIEventListener = UIEventListener.Get(PersistentUI.Popup.Overlay);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(HeroBoltCancelClicked));
	}

	public void ShowSocialManagerNotLoggedIn()
	{
		string text = "UI_Menu_166";
		text = "UI_Menu_166";
		PersistentUI.ShowPopup("ConfirmationPanel", text, false);
		string text2 = PersistentUI.Popup.ConfirmText.text;
		PersistentUI.Popup.ConfirmTextLoc.key = "UI_Menu_164";
		PersistentUI.Popup.ConfirmTextLoc.Localize();
		PersistentUI.Popup.ConfirmTextLoc.enabled = false;
		PersistentUI.Popup.ConfirmText.text = text2 + "\n\n" + PersistentUI.Popup.ConfirmText.text;
		PersistentUI.SetPopupOKButtonCallback(SocialManagerLoginClicked);
	}

	public void SocialManagerLoginClicked(GameObject Obj)
	{
		SocialManager.instance.SignIn();
	}

	public void ShowPSNNotLoggedIn()
	{
		PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_138", false);
		string text = PersistentUI.Popup.ConfirmText.text;
		PersistentUI.Popup.ConfirmTextLoc.key = "UI_Menu_164";
		PersistentUI.Popup.ConfirmTextLoc.Localize();
		PersistentUI.Popup.ConfirmTextLoc.enabled = false;
		PersistentUI.Popup.ConfirmText.text = text + "\n\n" + PersistentUI.Popup.ConfirmText.text;
		PersistentUI.SetPopupOKButtonCallback(PSNLoginClicked);
	}

	public void PSNLoginClicked(GameObject Obj)
	{
		OpenMenu(MenuPanels.PSNLoginMenu);
	}

	public void ShowHeroBoltPopup()
	{
		PersistentUI.ShowPopup("PopupUseHeroBolt", "UI_Menu_15", false);
		PersistentUI.SetPopupCancelButtonCallback(HeroBoltCancelClicked);
		UIEventListener.Get(PersistentUI.Popup.Overlay).onClick = HeroBoltCancelClicked;
		UILabel component = PersistentUI.Popup.transform.Find("ConfirmationBox/Quantity").GetComponent<UILabel>();
		component.text = "x" + GameController.instance.playerController.GetHeroBoltsTotal();
	}

	private void HeroBoltCancelClicked(GameObject obj)
	{
		ShowEndRoundScreen();
	}

	public void ShowEndRoundScreen()
	{
		if (GameController.instance.playerController.friendChallengeEnabled)
		{
			GameController.instance.playerController.CheckForFriendChallengeVictory();
		}
		else
		{
			OpenMenu(MenuPanels.EndRoundScreen);
			GetEndRoundScreen().UpdateStats((int)StatsTracker.instance.GetStat(StatsTracker.Stats.boltzCollected), (int)StatsTracker.instance.GetStat(StatsTracker.Stats.distanceTraveled));
			StatsTracker.instance.SaveStatsAndReset();
		}
		GameController.instance.playerController.GetComponent<Rigidbody>().position = Vector3.zero;
	}

	public int GetAffordableVendorItems()
	{
		int num = 0;
		num += GetAffordableWeapons();
		return num + GetAffordableGadgets();
	}

	public int GetAffordableGadgets()
	{
		int num = 0;
		int boltsTotal = GameController.instance.playerController.GetBoltsTotal();
		for (int i = 0; i < 4; i++)
		{
			int num2 = GadgetManager.instance.GetGadgetLevel((GadgetManager.eGadgets)i);
			if (!GadgetManager.instance.HaveBoughtGadget((GadgetManager.eGadgets)i))
			{
				num2 = 0;
			}
			if (num2 != 3 && boltsTotal >= instance.GadgetCosts[i, num2])
			{
				num++;
			}
		}
		for (int j = 0; j < 3; j++)
		{
			int num3 = MegaWeaponManager.instance.GetMegaWeaponLevel((MegaWeaponManager.eMegaWeapons)j);
			if (!MegaWeaponManager.instance.HaveBoughtMegaWeapon((MegaWeaponManager.eMegaWeapons)j))
			{
				num3 = 0;
			}
			if (num3 != 3 && boltsTotal >= instance.MegaWeaponCosts[j, num3])
			{
				num++;
			}
		}
		return num;
	}

	public int GetAffordableWeapons()
	{
		int num = 0;
		int boltsTotal = GameController.instance.playerController.GetBoltsTotal();
		for (int i = 0; i < WeaponsManager.instance.WeapInventory.Length - 1; i++)
		{
			Weapon component = WeaponsManager.instance.WeapInventory[i].GetComponent<Weapon>();
			int num2 = (int)component.GetWeaponUpgradeLevel();
			if (!WeaponsManager.instance.HaveBoughtWeapon((WeaponsManager.WeaponList)i))
			{
				num2 = 0;
			}
			if (num2 != 3 && boltsTotal >= component.upgradeBoltCost[num2])
			{
				num++;
			}
		}
		return num;
	}

	public void ShowBG(bool Instant = false)
	{
		if (!BGShowing)
		{
			BGShowing = true;
			PersistentUI.BG.gameObject.SetActive(true);
			if (Instant)
			{
				PersistentUI.BG.alpha = 1f;
				return;
			}
			PersistentUI.BGTween.enabled = true;
			PersistentUI.BGTween.Play(false);
		}
	}

	public void HideBG(bool Instant = false)
	{
		if (BGShowing)
		{
			BGShowing = false;
			PersistentUI.BG.gameObject.SetActive(true);
			if (Instant)
			{
				PersistentUI.BG.alpha = 0f;
				PersistentUI.BG.gameObject.SetActive(false);
			}
			else
			{
				PersistentUI.BGTween.enabled = true;
				PersistentUI.BGTween.Reset();
				PersistentUI.BGTween.Play(true);
			}
		}
	}

	public void ShowHexBG(bool Instant = false)
	{
		if (!HexBGShowing)
		{
			HexBGShowing = true;
			PersistentUI.HexBGTween.gameObject.SetActive(true);
			if (Instant)
			{
				PersistentUI.HexBGTween.alpha = 0.7f;
				return;
			}
			PersistentUI.HexBGTween.Sample(0f, false);
			PersistentUI.HexBGTween.Play(true);
		}
	}

	public void HideHexBG(bool Instant = false)
	{
		if (HexBGShowing)
		{
			HexBGShowing = false;
			PersistentUI.HexBGTween.gameObject.SetActive(true);
			if (Instant)
			{
				PersistentUI.HexBGTween.alpha = 0f;
				PersistentUI.HexBGTween.gameObject.SetActive(false);
			}
			else
			{
				PersistentUI.HexBGTween.Sample(PersistentUI.HexBGTween.tweenFactor, false);
				PersistentUI.HexBGTween.Play(false);
			}
		}
	}

	public void ShowLoadingGear()
	{
		PersistentUI.LoadingGear.SetActive(true);
	}

	public void HideLoadingGear()
	{
		PersistentUI.LoadingGear.SetActive(false);
	}

	public void SwapFont()
	{
		if (!ShouldSwapFonts)
		{
			return;
		}
		UILabel[] array = NGUITools.FindActive<UILabel>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UILabel uILabel = array[i];
			if (uILabel.font != null)
			{
				uILabel.font = null;
				uILabel.font = JapaneseFont;
			}
		}
	}

	public bool CheckWifi(bool ShowPopup = true)
	{
		bool result = false;
		if (Application.internetReachability != 0)
		{
			result = true;
		}
		else if (ShowPopup)
		{
			PersistentUI.ShowPopup("PopupWifiError", "UI_Menu_187", true);
		}
		return result;
	}

	protected IEnumerator PlayIntroMovieSubtitles()
	{
		yield return new WaitForSeconds(2.5f);
		ShowSubtitle("RCN_MOBILE_FMV_001", 4.5f);
		yield return new WaitForSeconds(5f);
		ShowSubtitle("RCN_MOBILE_FMV_002", 7f);
		yield return new WaitForSeconds(8f);
		ShowSubtitle("RCN_MOBILE_FMV_003", 7f);
		yield return new WaitForSeconds(7f);
		ShowSubtitle("RCN_MOBILE_FMV_004", 4.5f);
		yield return new WaitForSeconds(5f);
		ShowSubtitle("RCN_MOBILE_FMV_005", 2f);
		yield return new WaitForSeconds(4f);
		HideSubtitle();
	}

	private IEnumerator PlayIntroMovieInternal()
	{
		GetStartMenu().gameObject.SetActive(false);
		PersistentUI.MoviePlane.SetActive(true);
		AllPlatformMovieTexture movieTexture = PersistentUI.MoviePlane.GetComponent<AllPlatformMovieTexture>();
		Debug.Log("Unity SystemInfoMemory:" + SystemInfo.systemMemorySize);
		if (movieTexture != null)
		{
			Debug.Log("AllPlatformMovieTexture found!");
			movieTexture.Load("RCN_Mobile_Intro.mp4");
			movieTexture.onFinished += MovieFinished;
			float moviePlayFailTime = 0f;
			while (!movieTexture.Play() && moviePlayFailTime < 5f)
			{
				Debug.Log("AllPlatformMovieTexture failed to play, wait and try again.--Timeout Timer: " + moviePlayFailTime);
				moviePlayFailTime += 0.1f;
				yield return new WaitForSeconds(0.1f);
			}
			if (moviePlayFailTime >= 5f)
			{
				Debug.Log("AllPlatformMovieTexture play FAIL!!! Stopping Movie--Timeout Timer: " + moviePlayFailTime);
				MovieFinished();
			}
			else
			{
				Debug.Log("AllPlatformMovieTexture play success!--Timeout Timer: " + moviePlayFailTime);
				StartCoroutine("PlayIntroMovieSubtitles");
				MusicManager.instance.Play(MusicManager.eMusicTrackType.IntroMovie, true, 0f);
			}
		}
	}

	public void PlayIntroMovie()
	{
		Debug.Log("Playing the movie!");
		StartCoroutine("PlayIntroMovieInternal");
	}

	public bool IsMoviePlaying()
	{
		bool result = false;
		AllPlatformMovieTexture component = PersistentUI.MoviePlane.GetComponent<AllPlatformMovieTexture>();
		if (component != null)
		{
			result = component.IsPlaying();
		}
		return result;
	}

	public void StopIntroMovie()
	{
		AllPlatformMovieTexture component = PersistentUI.MoviePlane.GetComponent<AllPlatformMovieTexture>();
		if (component != null)
		{
			component.Stop();
		}
	}

	private void MovieFinished()
	{
		AllPlatformMovieTexture component = PersistentUI.MoviePlane.GetComponent<AllPlatformMovieTexture>();
		if (component != null)
		{
			component.onFinished -= MovieFinished;
		}
		PersistentUI.MoviePlane.SetActive(false);
		StopCoroutine("PlayIntroMovieSubtitles");
		HideSubtitle();
		MusicManager.instance.Stop();
		if (!InitialLoad)
		{
			GetStartMenu().gameObject.SetActive(false);
			OpenMenu(MenuPanels.StartMenu);
		}
	}

	public void ShowSubtitle(string SubtitleLocKey, float Duration)
	{
		UILocalize component = PersistentUI.Subtitle.GetComponent<UILocalize>();
		component.key = SubtitleLocKey;
		component.Localize();
		PersistentUI.Subtitle.gameObject.SetActive(true);
		CurrentSubtitleStartTime = Time.realtimeSinceStartup;
		CurrentSubtitleEndTime = CurrentSubtitleStartTime + Duration;
		ShowingSubtitle = true;
		SwapFont();
	}

	public void HideSubtitle()
	{
		ShowingSubtitle = false;
		PersistentUI.Subtitle.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (IsMoviePlaying() && (Input.anyKeyDown || Input.touchCount > 0))
		{
			StopIntroMovie();
		}
		if (ShowingSubtitle && Time.realtimeSinceStartup > CurrentSubtitleEndTime)
		{
			HideSubtitle();
		}
		if (!Input.GetKeyDown(KeyCode.Escape))
		{
			return;
		}
		if (TutorialUnlockManager.instance.TutorialShowing)
		{
			TutorialUnlockManager.instance.TutorialComplete();
		}
		else if (PersistentUI.Popup.gameObject.activeSelf)
		{
			if (PersistentUI.CurrentPopupPrefab == "PopupUseHeroBolt")
			{
				HeroBoltCancelClicked(base.gameObject);
			}
			PersistentUI.Popup.Hide();
		}
		else if (GameController.instance.inMenu)
		{
			switch (GetCurrentMenu())
			{
			case MenuPanels.StartMenu:
				if (IsMoviePlaying())
				{
					StopIntroMovie();
				}
				else if (GetSplashScreen() == null || (GetSplashScreen() != null && !GetSplashScreen().gameObject.activeSelf))
				{
					PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_196", false);
					PersistentUI.SetPopupOKButtonCallback(QuitOKPressed);
				}
				break;
			case MenuPanels.HUD:
				break;
			case MenuPanels.PauseMenu:
				GetPauseMenu().OnMenuButtonClicked(GetPauseMenu().MenuButton.gameObject);
				break;
			case MenuPanels.AdScreen:
				UIAdScreen.instance.CloseAdScreen(false);
				break;
			case MenuPanels.EndRoundScreen:
				OpenMenu(MenuPanels.StartMenu);
				break;
			case MenuPanels.VendorFrontMenu:
				CloseMenu();
				HideHexBG();
				break;
			case MenuPanels.MainMenu:
				CloseMenu();
				HideHexBG();
				break;
			default:
				CloseMenu();
				break;
			}
		}
		else if (GetCurrentMenu() == MenuPanels.HUD)
		{
			if (!GetHUD().CountdownLabel.gameObject.activeSelf && GetHUD().HUD.activeSelf && GetHUD().PauseButton.activeSelf)
			{
				GetHUD().PauseButton.GetComponent<PauseButtonScript>().OnClick();
			}
			if (GetHUD().isSkillChallengesShowing)
			{
				GetHUD().HideSkillChallenges();
			}
			if (GetHUD().isSkillDialogueShowing)
			{
				GetHUD().HideSkillDialogue();
			}
		}
	}

	private void QuitOKPressed(GameObject Obj)
	{
		Time.timeScale = 1f;
		EasyAnalytics.Instance.sendTiming("Total Gameplay Time", (long)(GameController.instance.totalGameplayTime * 1000f), "TotalGameplayTime", "Total Time In Gameplay");
		EasyAnalytics.Instance.setCustomMetric(2, (long)GameController.instance.totalGameplayTime);
		EasyAnalytics.Instance.setStartSession(false);
		Debug.Log("Times: " + GameController.instance.totalGameplayTime);
		PersistentUI.Black.SetActive(true);
		StartCoroutine("QuitApplication");
	}

	private IEnumerator QuitApplication()
	{
		yield return new WaitForSeconds(1f);
		Application.Quit();
	}
}
