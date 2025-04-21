using System.Collections;
using UnityEngine;

public class UIHUD : UIScreen
{
	public GameObject HUD;

	public GameObject SkillPointPresentation;

	public GameObject SkillPointDialogue;

	public GameObject SkillPointChallenges;

	public UITweener SkillPointTweener;

	public GameObject SkillPointDialogueText;

	public GameObject SkillPointDialogueTitle;

	public IconScript SkillPointDialogueCharacter;

	public UITweener SkillPointCharacterTweener;

	public ChallengeGridScript SkillPointChallengeGrid;

	public GameObject GadgetButton;

	public UILabel txt_DistanceTraveled;

	public UILabel txt_BoltsCollected;

	public UILabel txt_BoltsAdded;

	public UILabel txt_Ammo;

	public UILabel txt_BoltMultiplier;

	public UILabel txt_Score;

	public UILabel txt_RaritaniumCollected;

	public UILabel txt_HeroBolts;

	public UILabel txt_Terachnoids;

	public GameObject TerachnoidAlternateReward;

	public UILabel TerachnoidRewardBolts;

	public UILabel txt_GodMode;

	public UILabel txt_FriendChallengeName;

	public UILabel txt_FriendChallengeScore;

	public UILabel AmmoCostLabel;

	public UILabel BuyAmmoLabel;

	public UILabel CountdownLabel;

	public TweenAlpha CountdownAlphaTween;

	public TweenScale CountdownScaleTween;

	public UIButton BuyAmmoButton;

	public UITweener BoltsTween;

	public UITweener RaritaniumTween;

	public UITweener HeroBoltsTween;

	public UITweener TerachnoidsTween;

	public UITweener FriendChallengeTween;

	public UIGrid HPGrid;

	public UIGrid ReflectorHPGrid;

	public IconScript WeaponIcon;

	public GameObject AmmoContainer;

	public GameObject OutOfAmmoContainer;

	public UISlider AmmoBar;

	private bool isBoltsShowing;

	private bool isBoltsAddedShowing;

	private bool isRaritaniumShowing;

	private bool isTerachnoidsShowing;

	private bool isHeroBoltsShowing;

	private bool isFriendChallengeShowing;

	private bool friendChallengeEnabled;

	public bool isSkillDialogueShowing;

	public bool isSkillChallengesShowing;

	private float SkillPresentationDialogueDuration = 6f;

	private float SkillPresentationDialogueEndTime;

	private float SkillPresentationChallengesDuration = 6f;

	private float SkillPresentationChallengesEndTime;

	public float BoltsShowTimeDuration = 4f;

	private float BoltsTimerEndTime;

	public float BoltsAddedShowTimeDuration = 2f;

	private float BoltsAddedTimerEndTime;

	public float RaritaniumShowTimeDuration = 4f;

	private float RaritaniumTimerEndTime;

	public float HeroBoltsShowTimeDuration = 4f;

	private float HeroBoltsTimerEndTime;

	public float TerachnoidsShowTimeDuration = 4f;

	private float TerachnoidsTimerEndTime;

	public float FriendChallengeTimeDuration = 4f;

	private float FriendChallengeTimerEndTime;

	private string LastCountdownTick;

	public GameObject PauseButton;

	public UISlider BossHealth;

	public bool BossHealthShowing;

	public UITweener BossHealthTween;

	private int BoltsLast;

	private int BoltsNew;

	private bool bNeedsShowBolts;

	public bool hasPendingWeaponTutorial;

	public TutorialUnlockManager.TutorialLock PendingWeaponTutorial;

	public GameObject ShotgunSpread;

	public GameObject ShotgunSpreadLeft;

	public GameObject ShotgunSpreadRight;

	public GameObject ShotgunSpreadMiddle;

	public GameObject GadgetEMP;

	public Camera uiCamera;

	private uint AmmoToBuy;

	private uint AmmoBoltCost;

	public bool isHudHiding;

	private void Start()
	{
		UpdateAll();
		UIEventListener.Get(WeaponIcon.gameObject).onClick = WeaponIconClicked;
		UIEventListener.Get(OutOfAmmoContainer.gameObject).onClick = WeaponIconClicked;
		UIEventListener.Get(BuyAmmoButton.gameObject).onClick = BuyAmmoButtonClicked;
		UIEventListener.Get(BuyAmmoButton.gameObject).onHover = OnHoverCheckAmmo;
		UIEventListener.Get(GadgetButton).onClick = GadgetButtonClicked;
		UIEventListener.Get(GadgetButton).onHover = OnHoverCheckGadget;
		UIEventListener.Get(SkillPointDialogue).onClick = DialogueClicked;
		UIEventListener.Get(SkillPointChallenges).onClick = ChallengesClicked;
		uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		isHudHiding = false;
	}

	public override void Hide()
	{
		base.Hide();
		isHudHiding = true;
		HideShotgunCrosshairs();
		ResetSkillChallengePresentation();
	}

	private void DialogueClicked(GameObject obj)
	{
		HideSkillDialogue();
	}

	private void ChallengesClicked(GameObject obj)
	{
		HideSkillChallenges();
	}

	public void Start321Countdown()
	{
		CountdownLabel.gameObject.SetActive(true);
		CountdownLabel.text = "3";
		HUD.SetActive(false);
	}

	public void CountdownChanged()
	{
		CountdownAlphaTween.Reset();
		CountdownAlphaTween.enabled = true;
		CountdownAlphaTween.Sample(0f, false);
		CountdownAlphaTween.Play(true);
		CountdownScaleTween.Reset();
		CountdownScaleTween.enabled = true;
		CountdownScaleTween.Sample(0f, false);
		CountdownScaleTween.Play(true);
	}

	public void UpdateCountdownTimer()
	{
		if (!CountdownLabel.gameObject.activeSelf)
		{
			return;
		}
		CountdownLabel.text = GameController.instance.GetCountdownTime().ToString();
		if (LastCountdownTick != CountdownLabel.text)
		{
			CountdownChanged();
			if (CountdownLabel.text != "0")
			{
				SFXManager.instance.PlaySound("UI_Countdown");
			}
		}
		LastCountdownTick = CountdownLabel.text;
		if (GameController.instance.GetCountdownTime() <= 0)
		{
			CountdownLabel.gameObject.SetActive(false);
			Time.timeScale = 1f;
			GameController.instance.inMenu = false;
			GameController.instance.isPaused = false;
			MusicManager.instance.Resume();
			if (GadgetManager.instance.GetJetpackStatus())
			{
				SFXManager.instance.PlaySound("cha_Ratchet_Jet_Idle");
			}
			ShowHUD();
			EasyAnalytics.Instance.sendView("/HUD");
			EnableGadgets();
		}
	}

	public void GadgetButtonClicked(GameObject obj)
	{
		GameController.instance.playerController.dontFire = true;
		Pickup currentHeldPickup = GameController.instance.playerController.CurrentHeldPickup;
		if (currentHeldPickup != null)
		{
			GameController.instance.playerController.ActivateGadget();
			SFXManager.instance.PlaySound("cha_ratchet_Pickup");
		}
	}

	private void OnHoverCheckGadget(GameObject obj, bool isOver)
	{
		GameController.instance.playerController.dontFire = isOver;
	}

	private void OnHoverCheckAmmo(GameObject obj, bool isOver)
	{
		GameController.instance.playerController.dontFire = isOver;
	}

	public void UpdateHeldGadget()
	{
		IconScript component = GadgetButton.GetComponent<IconScript>();
		GameObject gameObject = GadgetButton.transform.Find("IconSpriteContainer").gameObject;
		gameObject.SetActive(true);
		switch (GameController.instance.playerController.CurrentPickupType)
		{
		case PlayerController.PickupTypes.None:
			gameObject.SetActive(false);
			component.SetIconSprite("icon_weapon_pistol", IconScript.HexLevel.HEX_V0, false);
			break;
		case PlayerController.PickupTypes.Magnetizer:
			component.SetIconSprite(GadgetManager.instance.GadgetSpriteNames[0], (IconScript.HexLevel)GadgetManager.instance.GetGadgetLevel(GadgetManager.eGadgets.g_Magnetizer), true, true);
			break;
		case PlayerController.PickupTypes.Reflector:
			component.SetIconSprite(GadgetManager.instance.GadgetSpriteNames[2], (IconScript.HexLevel)GadgetManager.instance.GetGadgetLevel(GadgetManager.eGadgets.g_Reflector), true, true);
			break;
		case PlayerController.PickupTypes.Multiplier:
			component.SetIconSprite(GadgetManager.instance.GadgetSpriteNames[1], (IconScript.HexLevel)GadgetManager.instance.GetGadgetLevel(GadgetManager.eGadgets.g_Multiplier), true, true);
			break;
		case PlayerController.PickupTypes.Jetpack:
			component.SetIconSprite(GadgetManager.instance.GadgetSpriteNames[3], (IconScript.HexLevel)GadgetManager.instance.GetGadgetLevel(GadgetManager.eGadgets.g_Jetpack), true, true);
			break;
		case PlayerController.PickupTypes.Rift:
			component.SetIconSprite(MegaWeaponManager.instance.MegaWeaponSpriteNames[1], (IconScript.HexLevel)MegaWeaponManager.instance.GetMegaWeaponLevel(MegaWeaponManager.eMegaWeapons.mw_RiftInducer), true, true);
			break;
		case PlayerController.PickupTypes.Tornado:
			component.SetIconSprite(MegaWeaponManager.instance.MegaWeaponSpriteNames[2], (IconScript.HexLevel)MegaWeaponManager.instance.GetMegaWeaponLevel(MegaWeaponManager.eMegaWeapons.mw_Tornado), true, true);
			break;
		case PlayerController.PickupTypes.Groove:
			component.SetIconSprite(MegaWeaponManager.instance.MegaWeaponSpriteNames[0], (IconScript.HexLevel)MegaWeaponManager.instance.GetMegaWeaponLevel(MegaWeaponManager.eMegaWeapons.mw_Groovitron), true, true);
			break;
		default:
			Debug.LogWarning("Unknown Gadget type");
			break;
		}
	}

	public void StartSkillPresentation()
	{
		int num = Random.Range(0, 4);
		string key = string.Empty;
		string key2 = string.Empty;
		string newSpriteName = string.Empty;
		switch (num)
		{
		case 0:
			newSpriteName = "tal";
			key = "UI_Menu_193";
			key2 = "SP_Talwyn";
			break;
		case 1:
			newSpriteName = "smuggler";
			key = "UI_Menu_192";
			key2 = "SP_Smuggler";
			break;
		case 2:
			newSpriteName = "plumber";
			key = "UI_Menu_190";
			key2 = "SP_Plumber";
			break;
		case 3:
			newSpriteName = "qwark";
			key = "UI_Menu_191";
			key2 = "SP_Qwark";
			break;
		}
		SkillPointDialogueTitle.GetComponent<UILocalize>().key = key;
		SkillPointDialogueText.GetComponent<UILocalize>().key = key2;
		SkillPointDialogueText.GetComponent<UILocalize>().Localize();
		SkillPointDialogueCharacter.SetIconSprite(newSpriteName, IconScript.HexLevel.HEX_V1);
		SkillPointDialogueText.GetComponent<TypewriterEffect>().UpdateOriginalText(SkillPointDialogueText.GetComponent<UILabel>().text);
		SkillPointPresentation.SetActive(true);
		SkillPointDialogue.SetActive(true);
		isSkillDialogueShowing = true;
		SkillPointTweener.Reset();
		SkillPointTweener.Play(true);
		SkillPointCharacterTweener.Reset();
		SkillPointCharacterTweener.Play(true);
		SkillPointDialogueText.GetComponent<TypewriterEffect>().Reset();
		SkillPresentationDialogueEndTime = Time.time + SkillPresentationDialogueDuration;
		SFXManager.instance.PlaySound("UI_Transmission_Start");
		SFXManager.instance.GetAudioSource("UI_Transmission_Loop").pitch = 1.25f;
		SFXManager.instance.PlaySound("UI_Transmission_Loop");
		UIManager.instance.SwapFont();
	}

	public void ResetSkillChallengePresentation()
	{
		HideSkillDialogue();
		HideSkillChallenges();
	}

	public void HideSkillDialogue()
	{
		SkillPointDialogue.SetActive(false);
		ShowSkillChallenges();
		isSkillDialogueShowing = false;
		SFXManager.instance.StopSound("UI_Transmission_Loop");
	}

	public void ShowSkillChallenges()
	{
		SkillPointChallenges.SetActive(true);
		isSkillChallengesShowing = true;
		SkillPresentationChallengesEndTime = Time.time + SkillPresentationChallengesDuration;
	}

	public void HideSkillChallenges()
	{
		isSkillChallengesShowing = false;
		SkillPointPresentation.SetActive(false);
		SkillPointChallenges.SetActive(false);
		ShowHUD(true);
		if (!isHudHiding)
		{
			EasyAnalytics.Instance.sendView("/HUD");
		}
	}

	public void ShowHUD(bool ShowAll = false)
	{
		HUD.SetActive(true);
		if (ShowAll)
		{
			ShowHeroBolts();
			ShowRaritanium();
			ShowTerachnoids();
			ShowBolts(-1f);
		}
	}

	public void NotifyIntroStart()
	{
		HUD.SetActive(false);
	}

	public void NotifyIntroOver()
	{
		StartSkillPresentation();
	}

	public void SetPendingWeaponTutorial(TutorialUnlockManager.TutorialLock Tut)
	{
		PendingWeaponTutorial = Tut;
		hasPendingWeaponTutorial = true;
		UIPauseMenu pauseMenu = UIManager.instance.GetPauseMenu();
		if ((pauseMenu == null || (pauseMenu != null && !pauseMenu.gameObject.activeSelf)) && (GameController.instance.gameState == GameController.eGameState.GS_TransitToGnd || GameController.instance.gameState == GameController.eGameState.GS_OnGround))
		{
			ShowPendingWeaponTutorial();
		}
	}

	public void ShowPendingWeaponTutorial()
	{
		hasPendingWeaponTutorial = false;
		TutorialUnlockManager.instance.OpenTutorial(PendingWeaponTutorial);
	}

	private void Update()
	{
		float time = Time.time;
		if (Time.frameCount % 6 == 0 && bNeedsShowBolts)
		{
			bNeedsShowBolts = false;
			ShowBoltsAdded();
			UpdateBoltsCollected();
		}
		if (isBoltsShowing && time >= BoltsTimerEndTime)
		{
			HideBolts();
		}
		if (isBoltsAddedShowing && time >= BoltsAddedTimerEndTime)
		{
			HideBoltsAdded();
		}
		if (isRaritaniumShowing && time >= RaritaniumTimerEndTime)
		{
			HideRaritanium();
		}
		if (isHeroBoltsShowing && time >= HeroBoltsTimerEndTime)
		{
			HideHeroBolts();
		}
		if (isTerachnoidsShowing && time >= TerachnoidsTimerEndTime)
		{
			HideTerachnoids();
		}
		if (isFriendChallengeShowing && time >= FriendChallengeTimerEndTime)
		{
			HideFriendChallenge();
		}
		if (isSkillDialogueShowing && time >= SkillPresentationDialogueEndTime)
		{
			HideSkillDialogue();
		}
		if (isSkillChallengesShowing && time >= SkillPresentationChallengesEndTime)
		{
			HideSkillChallenges();
		}
		if (!isSkillChallengesShowing && !isSkillDialogueShowing && !isRaritaniumShowing && !isBoltsShowing && !isFriendChallengeShowing && !isBoltsAddedShowing && !isHeroBoltsShowing && !isTerachnoidsShowing && !bNeedsShowBolts)
		{
			base.enabled = false;
		}
	}

	public void UpdateAll()
	{
		txt_GodMode.text = string.Empty;
		UpdateBoltMultiplier();
		UpdateAmmo();
		UpdateBoltsCollected(true);
		txt_BoltsAdded.text = string.Empty;
		UpdateWeaponIcon();
		UpdateHP();
		UpdateRaritaniumCollected();
		UpdateHeroBoltsCollected();
		UpdateTerachnoidsCollected();
		UpdateScore();
		UpdateFriendChallenge();
		UpdateHeldGadget();
		GameController.instance.playerController.UpdateArmorLevel();
		if (GameController.instance.gameState == GameController.eGameState.GS_Grinding)
		{
			UIManager.instance.GetHUD().AmmoContainer.SetActive(false);
		}
		else if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			UIManager.instance.GetHUD().AmmoContainer.SetActive(true);
		}
		PauseButton.SetActive(true);
		HideBossHealth(true);
		HideShotgunCrosshairs();
		EnableGadgets();
		bNeedsShowBolts = false;
	}

	public void GetWeaponTutorial()
	{
		Weapon component = WeaponsManager.instance.WeapInventory[WeaponsManager.instance.curWeapIndex].GetComponent<Weapon>();
		switch (component.weaponName)
		{
		case "ConstructoPistol":
			if (TutorialUnlockManager.instance.tutorialLocks[7])
			{
				SetPendingWeaponTutorial(TutorialUnlockManager.TutorialLock.PistolTut);
			}
			break;
		case "ConstructoShotgun":
			if (TutorialUnlockManager.instance.tutorialLocks[8])
			{
				SetPendingWeaponTutorial(TutorialUnlockManager.TutorialLock.ShotgunTut);
			}
			break;
		case "BuzzBlades":
			if (TutorialUnlockManager.instance.tutorialLocks[9])
			{
				SetPendingWeaponTutorial(TutorialUnlockManager.TutorialLock.BuzzBladesTut);
			}
			break;
		case "PredatorLauncher":
			if (TutorialUnlockManager.instance.tutorialLocks[10])
			{
				SetPendingWeaponTutorial(TutorialUnlockManager.TutorialLock.PredatorTut);
			}
			break;
		case "RynoM":
			if (TutorialUnlockManager.instance.tutorialLocks[11])
			{
				SetPendingWeaponTutorial(TutorialUnlockManager.TutorialLock.RynoTut);
			}
			break;
		default:
			Debug.LogError("Weapon: Unknown weapon");
			break;
		}
	}

	private void WeaponIconClicked(GameObject obj)
	{
		Time.timeScale = 0f;
		SFXManager.instance.StopAllSounds();
		MusicManager.instance.Pause();
		GameController.instance.isPaused = true;
		GameController.instance.inMenu = true;
		UIManager.instance.OpenMenu(UIManager.MenuPanels.PauseMenu);
	}

	private void ShowBolts(float Duration = -1f)
	{
		if (Duration == -1f)
		{
			Duration = BoltsShowTimeDuration;
		}
		if (!(BoltsTimerEndTime >= Time.time + Duration))
		{
			BoltsTimerEndTime = Time.time + Duration;
		}
		if (!isBoltsShowing)
		{
			BoltsTween.Play(true);
		}
		isBoltsShowing = true;
		base.enabled = true;
	}

	private void HideBolts()
	{
		isBoltsShowing = false;
		BoltsTween.Play(false);
	}

	public void DisableGadgets()
	{
		IconScript component = GadgetButton.GetComponent<IconScript>();
		GadgetEMP.SetActive(true);
		component.SetIconSprite(string.Empty, IconScript.HexLevel.HEX_V0, false);
		GadgetButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
	}

	public void EnableGadgets()
	{
		if ((GameController.instance.playerController.CurrentPickupType != PlayerController.PickupTypes.Jetpack || TileSpawnManager.instance.floorTileList[1].GetComponent<TileInfo>().spawnedState != TileSpawnManager.TileSpawnState.Tunnel) && TileSpawnManager.instance.floorTileList[1].GetComponent<TileInfo>().spawnedState != TileSpawnManager.TileSpawnState.Hero)
		{
			UpdateHeldGadget();
			GadgetEMP.SetActive(false);
			GadgetButton.SetActive(true);
			GadgetButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
		}
	}

	public void ShowBossHealth()
	{
		BossHealthShowing = true;
		BossHealth.gameObject.SetActive(true);
	}

	public void UpdateBossHealth(float Current, float Max)
	{
		BossHealth.sliderValue = Current / Max;
	}

	public void HideBossHealth(bool Instant = false)
	{
		BossHealthShowing = false;
		BossHealth.gameObject.SetActive(false);
	}

	public void UpdateBoltsCollected(bool ImmediateUpdate = false)
	{
		int num = GameController.instance.playerController.GetBolts();
		if ((int)StatsTracker.instance.GetStat(StatsTracker.Stats.boltzSpent) > GameController.instance.playerController.GetBoltsTotal() - GameController.instance.playerController.GetBolts() + (int)StatsTracker.instance.GetStat(StatsTracker.Stats.boltzSpent))
		{
			num = GameController.instance.playerController.GetBoltsTotal();
		}
		ShowBolts(-1f);
		if (ImmediateUpdate)
		{
			HideBoltsAdded();
			txt_BoltsCollected.GetComponent<CounterScript>().FinishCounter();
			txt_BoltsCollected.GetComponent<CounterScript>().StartCounter(num, num);
		}
		else
		{
			txt_BoltsCollected.GetComponent<CounterScript>().StartCounter(BoltsLast, num);
		}
		BoltsLast = num;
		UpdateAmmo();
	}

	private void ShowBoltsAdded()
	{
		BoltsAddedTimerEndTime = Time.time + BoltsAddedShowTimeDuration;
		if (!isBoltsAddedShowing)
		{
			txt_BoltsAdded.gameObject.SetActive(true);
		}
		isBoltsAddedShowing = true;
		base.enabled = true;
		ShowBolts(-1f);
	}

	private void HideBoltsAdded()
	{
		isBoltsAddedShowing = false;
		txt_BoltsAdded.gameObject.SetActive(false);
		BoltsNew = 0;
	}

	public void UpdateBoltsAdded(int Bolts)
	{
		BoltsNew += Bolts;
		txt_BoltsAdded.text = "+" + BoltsNew;
		bNeedsShowBolts = true;
		base.enabled = true;
	}

	private void ShowRaritanium()
	{
		RaritaniumTimerEndTime = Time.time + RaritaniumShowTimeDuration;
		if (!isRaritaniumShowing)
		{
			RaritaniumTween.Play(true);
		}
		isRaritaniumShowing = true;
		base.enabled = true;
	}

	private void HideRaritanium()
	{
		isRaritaniumShowing = false;
		RaritaniumTween.Play(false);
	}

	public void UpdateRaritaniumCollected()
	{
		int lifetimeRaritaniumTotal = GameController.instance.playerController.GetLifetimeRaritaniumTotal();
		txt_RaritaniumCollected.text = lifetimeRaritaniumTotal.ToString();
		ShowRaritanium();
	}

	private void ShowHeroBolts()
	{
		HeroBoltsTimerEndTime = Time.time + HeroBoltsShowTimeDuration;
		if (!isHeroBoltsShowing)
		{
			HeroBoltsTween.Play(true);
		}
		isHeroBoltsShowing = true;
		base.enabled = true;
	}

	private void HideHeroBolts()
	{
		isHeroBoltsShowing = false;
		HeroBoltsTween.Play(false);
	}

	public void UpdateHeroBoltsCollected()
	{
		int heroBoltsTotal = GameController.instance.playerController.GetHeroBoltsTotal();
		txt_HeroBolts.text = heroBoltsTotal.ToString();
		ShowHeroBolts();
	}

	private void ShowTerachnoids()
	{
		TerachnoidsTimerEndTime = Time.time + TerachnoidsShowTimeDuration;
		if (!isTerachnoidsShowing)
		{
			TerachnoidsTween.Play(true);
		}
		isTerachnoidsShowing = true;
		base.enabled = true;
	}

	private void HideTerachnoids()
	{
		isTerachnoidsShowing = false;
		TerachnoidsTween.Play(false);
	}

	public void UpdateTerachnoidsCollected()
	{
		int terachnoidsTotal = GameController.instance.playerController.GetTerachnoidsTotal();
		if (terachnoidsTotal > 10)
		{
			TerachnoidAlternateReward.SetActive(true);
			TerachnoidRewardBolts.text = "+" + GameController.instance.playerController.TerachnoidBoltValue;
			txt_Terachnoids.text = terachnoidsTotal.ToString();
		}
		else
		{
			TerachnoidAlternateReward.SetActive(false);
			txt_Terachnoids.text = terachnoidsTotal + "/10";
		}
		ShowTerachnoids();
	}

	private void ShowFriendChallenge()
	{
		if (base.gameObject.activeSelf)
		{
			FriendChallengeTimerEndTime = Time.time + FriendChallengeTimeDuration;
			if (!isFriendChallengeShowing)
			{
				FriendChallengeTween.Play(true);
			}
			isFriendChallengeShowing = true;
			base.enabled = true;
			StartCoroutine("ReshowFriendChallenge");
		}
	}

	private IEnumerator ReshowFriendChallenge()
	{
		yield return new WaitForSeconds(20f);
		ShowFriendChallenge();
	}

	private void HideFriendChallenge()
	{
		isFriendChallengeShowing = false;
		FriendChallengeTween.Play(false);
	}

	public void UpdateFriendChallenge()
	{
		friendChallengeEnabled = GameController.instance.playerController.friendChallengeEnabled;
		if (friendChallengeEnabled)
		{
			txt_FriendChallengeName.text = SocialManager.instance.GetFriendChallengeName();
			txt_FriendChallengeScore.text = SocialManager.instance.GetFriendChallengeScore();
			ShowFriendChallenge();
		}
		else
		{
			HideFriendChallenge();
		}
	}

	public void UpdateScore()
	{
		txt_Score.text = GameController.instance.GetScore().ToString();
	}

	public void UpdateDistanceTraveled()
	{
		int num = (int)GameController.instance.playerController.GetTravelDist();
		txt_DistanceTraveled.text = num + "m";
	}

	public void BuyAmmoButtonClicked(GameObject obj)
	{
		Weapon myWeap = GameController.instance.playerController.myWeap;
		uint ammo = myWeap.ammo;
		SFXManager.instance.PlaySound("UI_Purchase");
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.boltzSpent, AmmoBoltCost);
		if (myWeap.weaponName == "ConstructoPistol")
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.pistolReloads);
		}
		myWeap.ammo = ammo + AmmoToBuy;
		UpdateAmmo();
		UpdateBoltsCollected(true);
		GameController.instance.playerController.dontFire = true;
	}

	public void UpdateAmmo()
	{
		Color color = new Color(1f, 1f, 1f, 1f);
		Color color2 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		Weapon myWeap = GameController.instance.playerController.myWeap;
		uint ammo = myWeap.ammo;
		uint ammoMax = (uint)myWeap.ammoMax;
		txt_Ammo.text = ammo + "/" + ammoMax;
		uint num = (uint)(GameController.instance.playerController.GetBoltsTotal() / myWeap.ammoBoltCost[myWeap.GetWeaponUpgradeLevel() - 1]);
		uint num2 = ammoMax - ammo;
		AmmoToBuy = (uint)Mathf.Min(num, num2);
		AmmoBoltCost = myWeap.ammoBoltCost[myWeap.GetWeaponUpgradeLevel() - 1] * AmmoToBuy;
		AmmoCostLabel.text = AmmoBoltCost.ToString();
		if (num != 0)
		{
			BuyAmmoButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
			BuyAmmoButton.UpdateColor(true, true);
			BuyAmmoButton.transform.Find("Label").GetComponent<UILabel>().color = color;
		}
		else
		{
			AmmoCostLabel.text = myWeap.ammoBoltCost[myWeap.GetWeaponUpgradeLevel() - 1].ToString();
			BuyAmmoButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
			BuyAmmoButton.UpdateColor(false, true);
			BuyAmmoButton.transform.Find("Label").GetComponent<UILabel>().color = color2;
		}
		if (ammo == 0)
		{
			OutOfAmmoContainer.SetActive(true);
		}
		else
		{
			OutOfAmmoContainer.SetActive(false);
		}
		AmmoBar.sliderValue = (float)ammo / (float)ammoMax;
	}

	public void UpdateWeaponIcon()
	{
		WeaponIcon.SetIconSprite(GameController.instance.playerController.myWeap.spriteName, (IconScript.HexLevel)GameController.instance.playerController.myWeap.GetWeaponUpgradeLevel());
	}

	public void UpdateShotgunCrosshairs(Vector3 TouchPos, float CurAngle)
	{
		if (!ShotgunSpread.activeSelf)
		{
			ShotgunSpread.SetActive(true);
		}
		Transform transform = ShotgunSpread.transform;
		if (uiCamera != null)
		{
			TouchPos.x = Mathf.Clamp01(TouchPos.x / (float)Screen.width);
			TouchPos.y = Mathf.Clamp01(TouchPos.y / (float)Screen.height);
			transform.position = uiCamera.ViewportToWorldPoint(TouchPos);
			TouchPos = transform.localPosition;
			TouchPos.x = Mathf.Round(TouchPos.x);
			TouchPos.y = Mathf.Round(TouchPos.y);
			transform.localPosition = TouchPos;
		}
		float num = 4f;
		ShotgunSpreadLeft.transform.localPosition = new Vector3((0f - CurAngle) * num, ShotgunSpreadLeft.transform.localPosition.y, ShotgunSpreadLeft.transform.localPosition.z);
		ShotgunSpreadRight.transform.localPosition = new Vector3(CurAngle * num, ShotgunSpreadRight.transform.localPosition.y, ShotgunSpreadRight.transform.localPosition.z);
		float num2 = -20f;
		num2 += CurAngle * num * 2f;
		ShotgunSpreadMiddle.transform.localScale = new Vector3(ShotgunSpreadMiddle.transform.localScale.x, num2, ShotgunSpreadMiddle.transform.localScale.z);
	}

	public void HideShotgunCrosshairs()
	{
		ShotgunSpread.SetActive(false);
	}

	public void UpdateHP()
	{
		int num = 0;
		int hP = GameController.instance.playerController.GetHP();
		int reflectorHealth = GadgetManager.instance.GetReflectorHealth();
		for (int i = 0; i < HPGrid.transform.GetChildCount(); i++)
		{
			GameObject gameObject = HPGrid.transform.GetChild(i).gameObject;
			if (i < hP)
			{
				gameObject.SetActive(true);
				num++;
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < ReflectorHPGrid.transform.GetChildCount(); j++)
		{
			GameObject gameObject2 = ReflectorHPGrid.transform.GetChild(j).gameObject;
			if (j < reflectorHealth)
			{
				gameObject2.SetActive(true);
			}
			else
			{
				gameObject2.SetActive(false);
			}
		}
		ReflectorHPGrid.transform.localPosition = new Vector3(HPGrid.transform.localPosition.x + HPGrid.cellWidth * (float)num, ReflectorHPGrid.transform.localPosition.y, ReflectorHPGrid.transform.localPosition.z);
	}

	public void UpdateBoltMultiplier()
	{
		int boltMultiplier = GadgetManager.instance.GetBoltMultiplier();
		if (boltMultiplier > 1)
		{
			txt_BoltMultiplier.text = "x" + boltMultiplier;
		}
		else
		{
			txt_BoltMultiplier.text = string.Empty;
		}
		int gadgetLevel = GadgetManager.instance.GetGadgetLevel(GadgetManager.eGadgets.g_Multiplier);
		float duration = GadgetManager.instance.boltMultiplierDuration[gadgetLevel - 1];
		ShowBolts(duration);
	}

	public void NotifyKilled()
	{
		SkillPointPresentation.SetActive(false);
		HUD.SetActive(false);
	}

	public void NotifyRevived()
	{
		ShowHUD(true);
	}
}
