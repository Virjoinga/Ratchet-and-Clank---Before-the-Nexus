using System;
using System.Collections;
using System.Collections.Generic;
using Sony.OTG.Discovery;
using UnityEngine;

public class GameController : MonoBehaviour
{
	public enum eGameState
	{
		GS_Grinding = 0,
		GS_TransitToGnd = 1,
		GS_OnGround = 2,
		GS_TransitToRail = 3
	}

	private const int DEFAULT_NUM_DEATHS_BEFORE_FIRST_AD = 0;

	private const int DEFAULT_NUM_DEATHS_BETWEEN_ADS = 5;

	public GameObject mainCamera;

	public CameraFollow gameCamera;

	public eGameState gameState;

	public int numGrdSectionsBeforeHero;

	private int curNumGrdSections;

	private bool m_ShootingMode;

	public static GameController instance;

	public PlayerController playerController;

	public GameObject ratchet;

	public bool isPaused;

	public bool inMenu;

	public string PSNSessionID;

	protected GameObject currentSkybox;

	public Color axmFogColor = Color.black;

	public float axmFogNear = 450f;

	public float axmFogFar = 550f;

	public float axmCameraFarClip = 600f;

	public Color axmGndFogColor = Color.black;

	public float axmGndFogNear = 450f;

	public float axmGndFogFar = 550f;

	public float axmGndCameraFarClip = 600f;

	public Color axmHeroFogColor = Color.black;

	public float axmHeroFogNear = 450f;

	public float axmHeroFogFar = 550f;

	public float axmHeroCameraFarClip = 600f;

	public Color terFogColor = Color.black;

	public float terFogNear = 150f;

	public float terFogFar = 200f;

	public float terCameraFarClip = 250f;

	public Color terGndFogColor = Color.black;

	public float terGndFogNear = 150f;

	public float terGndFogFar = 200f;

	public float terGndCameraFarClip = 250f;

	public Color terHeroFogColor = Color.black;

	public float terHeroFogNear = 150f;

	public float terHeroFogFar = 200f;

	public float terHeroCameraFarClip = 250f;

	public Color polFogColor = Color.black;

	public float polFogNear = 150f;

	public float polFogFar = 200f;

	public float polCameraFarClip = 250f;

	public Color polGndFogColor = Color.black;

	public float polGndFogNear = 150f;

	public float polGndFogFar = 200f;

	public float polGndCameraFarClip = 250f;

	public Color polHeroFogColor = Color.black;

	public float polHeroFogNear = 150f;

	public float polHeroFogFar = 200f;

	public float polHeroCameraFarClip = 250f;

	protected bool bStartingGame;

	protected bool bNeedsInput;

	public bool bFirstStart = true;

	public List<LeviathanNodes> PathNodes;

	private bool showFlags;

	protected float roundStartTime;

	public int latestChangelist;

	private float countdownTimerStart;

	public float ShootingTutorialPopupDelay = 1f;

	public bool forceInstructions;

	private bool _MusicOn = true;

	private bool _SoundOn = true;

	public float totalGameplayTime;

	public float longestRunTime;

	private int numDeathsBeforeFirstAd;

	private int numDeathsBetweenAds = 5;

	private int numDeaths;

	public bool MusicOn
	{
		get
		{
			return _MusicOn;
		}
		set
		{
			_MusicOn = value;
			if (_MusicOn)
			{
				PlayerPrefs.SetInt("Music", 0);
			}
			else
			{
				PlayerPrefs.SetInt("Music", 1);
			}
			PlayerPrefs.Save();
		}
	}

	public bool SoundOn
	{
		get
		{
			return _SoundOn;
		}
		set
		{
			_SoundOn = value;
			if (_SoundOn)
			{
				PlayerPrefs.SetInt("Sound", 0);
			}
			else
			{
				PlayerPrefs.SetInt("Sound", 1);
			}
			PlayerPrefs.Save();
		}
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("GameController: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
		Application.targetFrameRate = 30;
		ratchet = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Characters/CHA_RatchetClank/Ratchet_PF"));
		playerController = ratchet.GetComponent<PlayerController>();
		if (playerController == null)
		{
			Debug.LogError("PlayerController not found");
		}
		UnityEngine.Object.Instantiate(Resources.Load("Misc/GoogleGamesManager"));
		instance = this;
		m_ShootingMode = false;
		SetLanguage();
		PathNodes = new List<LeviathanNodes>();
		mainCamera = base.gameObject.transform.parent.gameObject;
		gameCamera = Camera.main.GetComponent<CameraFollow>();
		TextAsset textAsset = (TextAsset)Resources.Load("Data/latestChangelist_" + Application.platform, typeof(TextAsset));
		if (textAsset == null)
		{
			textAsset = (TextAsset)Resources.Load("Data/latestChangelist_Android", typeof(TextAsset));
		}
		if (textAsset != null)
		{
			if (!int.TryParse(textAsset.text, out latestChangelist))
			{
				Analytics.Get().SetBuildName("UNKNOWN:" + textAsset.text);
				Debug.Log("Failed to get a proper changelist number!");
			}
			else
			{
				Analytics.Get().SetBuildName("CL#" + latestChangelist);
			}
		}
		if (PlayerPrefs.GetInt(forceInstructions.ToString()) == 1)
		{
			forceInstructions = false;
		}
	}

	private void Start()
	{
		ChangeState(gameState);
		inMenu = true;
		Time.timeScale = 0f;
		UIManager.instance.SetInput(false);
		bNeedsInput = true;
		bFirstStart = true;
		if (PlayerPrefs.GetInt("Music") == 1)
		{
			_MusicOn = false;
			MusicManager.instance.MuteMusic(true);
		}
		if (PlayerPrefs.GetInt("Sound") == 1)
		{
			_SoundOn = false;
			AudioListener.volume = 0f;
		}
		TextAsset textAsset = (TextAsset)Resources.Load("Data/GoogleAnalyticsInfo", typeof(TextAsset));
		if ((bool)textAsset)
		{
			EasyAnalytics.Instance.trackerWithTrackingId(textAsset.text.Trim());
			EasyAnalytics.Instance.setStartSession(true);
		}
		else
		{
			Debug.LogError("Cannot read GoogleAnalyticsInfo file. Google Analytics will not be enabled");
		}
		if (DiscoveryClient.instance == null)
		{
			throw new Exception("Discovery Client game object not found.");
		}
		DiscoveryClient.instance.FetchPublisherConfig(onPublisherConfigReceived);
	}

	private void onPublisherConfigReceived(Dictionary<string, string> config)
	{
		if (config.ContainsKey("NumDeathsBeforeFirstAd") && !int.TryParse(config["NumDeathsBeforeFirstAd"], out numDeathsBeforeFirstAd))
		{
			Debug.LogWarning("DISC: Failed to parse config NumDeathsBeforeFirstAd value of " + config["NumDeathsBeforeFirstAd"]);
			numDeathsBeforeFirstAd = 0;
		}
		if (config.ContainsKey("NumDeathsBetweenAds") && !int.TryParse(config["NumDeathsBetweenAds"], out numDeathsBetweenAds))
		{
			Debug.LogWarning("DISC: Failed to parse config NumDeathsBetweenAds value of " + config["NumDeathsBetweenAds"]);
			numDeathsBetweenAds = 5;
		}
		if (config.ContainsKey("NumAdsToRemember"))
		{
			int result = 0;
			if (int.TryParse(config["NumAdsToRemember"], out result))
			{
				DiscoveryClient.instance.NumAdsToRemember = result;
			}
			else
			{
				Debug.LogWarning("DISC: Failed to parse config NumAdsToRemember value of " + config["NumAdsToRemember"]);
			}
		}
	}

	public void Start321Countdown()
	{
		countdownTimerStart = Time.realtimeSinceStartup;
	}

	public int GetCountdownTime()
	{
		return (int)((3f - (Time.realtimeSinceStartup - countdownTimerStart)) * 1.33f);
	}

	public void OnApplicationFocus(bool wasPaused)
	{
		if ((!(playerController != null) || !playerController.IsDoingIntro()) && ((!isPaused && !inMenu && !playerController.IsPlayerDead()) || (UIManager.instance.GetHUD() != null && UIManager.instance.GetHUD().CountdownLabel.gameObject.activeSelf)) && !Debug.isDebugBuild)
		{
			Time.timeScale = 0f;
			isPaused = true;
			inMenu = true;
			UIManager.instance.GetHUD().CountdownLabel.gameObject.SetActive(false);
			UIManager.instance.OpenMenu(UIManager.MenuPanels.PauseMenu);
			SFXManager.instance.StopAllSounds();
			MusicManager.instance.Pause();
		}
	}

	public void OnApplicationPause(bool wasPaused)
	{
		Debug.Log("OnApplicationPaused() wasPaused=" + wasPaused);
		AnalyticsHandlePause(wasPaused);
		if (!wasPaused)
		{
		}
		if ((!(playerController != null) || !playerController.IsDoingIntro()) && ((!isPaused && !inMenu && !playerController.IsPlayerDead()) || (UIManager.instance.GetHUD() != null && UIManager.instance.GetHUD().CountdownLabel.gameObject.activeSelf)) && !Debug.isDebugBuild)
		{
			Time.timeScale = 0f;
			isPaused = true;
			inMenu = true;
			UIManager.instance.GetHUD().CountdownLabel.gameObject.SetActive(false);
			UIManager.instance.OpenMenu(UIManager.MenuPanels.PauseMenu);
			SFXManager.instance.StopAllSounds();
			MusicManager.instance.Pause();
		}
	}

	private void OnApplicationQuit()
	{
		StatsTracker.instance.SaveStatsAndReset();
		PlayerPrefs.Save();
	}

	private void Update()
	{
		if (isPaused)
		{
			UIManager.instance.GetHUD().UpdateCountdownTimer();
		}
		if (bStartingGame)
		{
			if ((bFirstStart || !GameObjectPool.instance.IsStreaming()) && !UIManager.instance.IsMoviePlaying())
			{
				StartGameInternal();
			}
			return;
		}
		if (bNeedsInput)
		{
			if (!GameObjectPool.instance.IsStreaming())
			{
				UIManager.instance.SetInput(true);
				bNeedsInput = false;
			}
			return;
		}
		if (Input.GetKeyDown(KeyCode.F8))
		{
			StatsTracker.instance.FullReset();
		}
		if (MyFPSCounter.instance != null)
		{
			MyFPSCounter.instance.FPSUpdate(Time.deltaTime);
		}
		ComputeScore();
		eGameState gameStateFromTile = TileSpawnManager.instance.GetGameStateFromTile(1);
		if (gameStateFromTile != gameState)
		{
			ChangeState(gameStateFromTile);
		}
	}

	public void InitialStartGame()
	{
		playerController.StartIntro();
		StartGame();
	}

	public void StartGame()
	{
		UIManager.instance.SetInput(false);
		UIManager.instance.ShowLoadingGear();
		bStartingGame = true;
		Time.timeScale = 1f;
		GameObjectPool.instance.FreeAllObjects();
		SFXManager.instance.StopAllSounds();
		WeaponsManager.instance.SetUpWeaponsSystem();
		TileSpawnManager.instance.PrepForRestart();
		if (!TileSpawnManager.instance.overwriteStartTile)
		{
			ChangeState(eGameState.GS_Grinding);
		}
		TileSpawnManager.ENVType nextEnvironmentType = TileSpawnManager.ENVType.ENV_Ter;
		switch (UnityEngine.Random.Range(0, 5))
		{
		case 0:
			nextEnvironmentType = TileSpawnManager.ENVType.ENV_Axm;
			break;
		case 1:
			if (!GameObjectPool.instance.IsMemoryRestricted())
			{
				nextEnvironmentType = TileSpawnManager.ENVType.ENV_Pol;
			}
			break;
		}
		TileSpawnManager.instance.SetNextEnvironmentType(nextEnvironmentType);
		SetSkybox(TileSpawnManager.instance.startingEnvironmentType, TileSpawnManager.TileSpawnState.Rails);
		TileSpawnManager.instance.StreamBiomes();
		ChallengeSystem.instance.ActiveChallengesDirty = true;
		if ((bool)UIManager.instance.GetHUD())
		{
			UIManager.instance.GetHUD().isHudHiding = false;
		}
	}

	public void StartGameInternal()
	{
		bStartingGame = false;
		bFirstStart = false;
		TileSpawnManager.instance.Restart();
		playerController.Revive();
		playerController.Restart();
		roundStartTime = Time.time;
		CameraFollow component = Camera.main.GetComponent<CameraFollow>();
		component.ResetCamera();
		UIManager.instance.OpenMenu(UIManager.MenuPanels.HUD);
		UIManager.instance.GetHUD().UpdateAll();
		UIManager.instance.SetInput(true);
		UIManager.instance.HideLoadingGear();
		inMenu = false;
	}

	public void HeroBoltRevive()
	{
		playerController.HeroBoltRevive();
		UIManager.instance.GetHUD().UpdateAll();
		UIManager.instance.GetHUD().NotifyRevived();
	}

	public void RoundEnded(bool showResults = true)
	{
		float num = Time.time - roundStartTime;
		Analytics.Get().SendDesignEvent("roundEnd:lifeTime", num);
		totalGameplayTime += num;
		Time.timeScale = 0f;
		inMenu = true;
		SFXManager.instance.StopAllLoopingSounds();
		if (showResults)
		{
			playerController.UploadLeaderboardScores();
			bool flag = numDeathsBeforeFirstAd == numDeaths || (numDeaths - numDeathsBeforeFirstAd) % numDeathsBetweenAds == 0;
			numDeaths++;
			if (flag)
			{
				UIManager.instance.OpenMenu(UIManager.MenuPanels.AdScreen);
				return;
			}
			if (playerController.GetHeroBoltsTotal() > 0)
			{
				UIManager.instance.ShowHeroBoltPopup();
				return;
			}
			SFXManager.instance.StopSound("Death_Music");
			UIManager.instance.ShowEndRoundScreen();
		}
	}

	public void SetSkybox(TileSpawnManager.ENVType envType, TileSpawnManager.TileSpawnState state)
	{
		if (currentSkybox != null)
		{
			GameObjectPool.instance.SetFree(currentSkybox, true);
			currentSkybox = null;
		}
		Color color = axmFogColor;
		float fogStartDistance = axmFogNear;
		float fogEndDistance = axmFogFar;
		float farClipPlane = axmCameraFarClip;
		switch (envType)
		{
		case TileSpawnManager.ENVType.ENV_Axm:
			switch (state)
			{
			case TileSpawnManager.TileSpawnState.Rails:
				currentSkybox = GameObjectPool.instance.GetNextFree("AXM_Skybox");
				color = axmFogColor;
				fogStartDistance = axmFogNear;
				fogEndDistance = axmFogFar;
				farClipPlane = axmCameraFarClip;
				break;
			case TileSpawnManager.TileSpawnState.Ground:
				currentSkybox = GameObjectPool.instance.GetNextFree("AXM_Skybox_Ground");
				color = axmGndFogColor;
				fogStartDistance = axmGndFogNear;
				fogEndDistance = axmGndFogFar;
				farClipPlane = axmGndCameraFarClip;
				break;
			case TileSpawnManager.TileSpawnState.Hero:
				currentSkybox = GameObjectPool.instance.GetNextFree("AXM_Skybox_Hero");
				color = axmHeroFogColor;
				fogStartDistance = axmHeroFogNear;
				fogEndDistance = axmHeroFogFar;
				farClipPlane = axmHeroCameraFarClip;
				break;
			}
			break;
		case TileSpawnManager.ENVType.ENV_Ter:
			switch (state)
			{
			case TileSpawnManager.TileSpawnState.Rails:
				currentSkybox = GameObjectPool.instance.GetNextFree("TER_Skybox");
				color = terFogColor;
				fogStartDistance = terFogNear;
				fogEndDistance = terFogFar;
				farClipPlane = terCameraFarClip;
				break;
			case TileSpawnManager.TileSpawnState.Ground:
				currentSkybox = GameObjectPool.instance.GetNextFree("TER_Skybox_Ground");
				color = terGndFogColor;
				fogStartDistance = terGndFogNear;
				fogEndDistance = terGndFogFar;
				farClipPlane = terGndCameraFarClip;
				break;
			case TileSpawnManager.TileSpawnState.Hero:
				currentSkybox = GameObjectPool.instance.GetNextFree("TER_Skybox_Hero");
				color = terHeroFogColor;
				fogStartDistance = terHeroFogNear;
				fogEndDistance = terHeroFogFar;
				farClipPlane = terHeroCameraFarClip;
				break;
			}
			break;
		default:
			currentSkybox = null;
			switch (state)
			{
			case TileSpawnManager.TileSpawnState.Rails:
				color = polFogColor;
				fogStartDistance = polFogNear;
				fogEndDistance = polFogFar;
				farClipPlane = polCameraFarClip;
				break;
			case TileSpawnManager.TileSpawnState.Ground:
				color = polGndFogColor;
				fogStartDistance = polGndFogNear;
				fogEndDistance = polGndFogFar;
				farClipPlane = polGndCameraFarClip;
				break;
			case TileSpawnManager.TileSpawnState.Hero:
				color = polHeroFogColor;
				fogStartDistance = polHeroFogNear;
				fogEndDistance = polHeroFogFar;
				farClipPlane = polHeroCameraFarClip;
				break;
			}
			break;
		}
		RenderSettings.fogColor = color;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.fogEndDistance = fogEndDistance;
		Camera.main.farClipPlane = farClipPlane;
		Camera.main.backgroundColor = color;
		Material skybox = null;
		if (currentSkybox != null)
		{
			Skybox component = currentSkybox.GetComponent<Skybox>();
			if (component != null)
			{
				skybox = component.material;
			}
		}
		RenderSettings.skybox = skybox;
	}

	public void ChangeBiome()
	{
		List<int> list = new List<int>();
		list.Add(0);
		list.Add(2);
		List<int> list2 = list;
		if (!GameObjectPool.instance.IsMemoryRestricted())
		{
			list2.Add(1);
		}
		int index = UnityEngine.Random.Range(0, list2.Count);
		TileSpawnManager.ENVType nextEnvironmentType = (TileSpawnManager.ENVType)list2[index];
		TileSpawnManager.instance.SetNextEnvironmentType(nextEnvironmentType);
	}

	private void LeaveState(eGameState state)
	{
		switch (gameState)
		{
		case eGameState.GS_Grinding:
			break;
		case eGameState.GS_TransitToGnd:
			break;
		case eGameState.GS_OnGround:
			break;
		case eGameState.GS_TransitToRail:
			break;
		}
	}

	private void EnterState(eGameState state)
	{
		if (UIManager.instance.GetHUD() != null)
		{
			UIManager.instance.GetHUD().EnableGadgets();
		}
		switch (state)
		{
		case eGameState.GS_Grinding:
			WeaponsManager.instance.HideWeapon(true);
			gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Rails);
			break;
		case eGameState.GS_TransitToGnd:
			if (playerController.CurrentPickupType == PlayerController.PickupTypes.Jetpack)
			{
				UIManager.instance.GetHUD().DisableGadgets();
			}
			if (UIManager.instance.GetHUD() != null)
			{
				UIManager.instance.GetHUD().AmmoContainer.SetActive(false);
			}
			WeaponsManager.instance.HideWeapon(true);
			gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Tunnel);
			break;
		case eGameState.GS_OnGround:
			if (UIManager.instance.GetHUD() != null)
			{
				UIManager.instance.GetHUD().AmmoContainer.SetActive(true);
			}
			WeaponsManager.instance.HideWeapon(false);
			if (UIManager.instance.GetHUD() != null)
			{
				UIManager.instance.GetHUD().GetWeaponTutorial();
			}
			if (TileSpawnManager.instance.floorTileList[1].GetComponent<TileInfo>().spawnedState == TileSpawnManager.TileSpawnState.Hero)
			{
				gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Boss);
				UIManager.instance.GetHUD().DisableGadgets();
			}
			else
			{
				gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Ground);
			}
			break;
		case eGameState.GS_TransitToRail:
			if (playerController.CurrentPickupType == PlayerController.PickupTypes.Jetpack)
			{
				UIManager.instance.GetHUD().DisableGadgets();
			}
			WeaponsManager.instance.HideWeapon(true);
			gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Tunnel);
			break;
		}
		MegaWeaponManager.instance.DeactivateMegaWeapon(true);
		playerController.ChangeAnimationState(state);
		gameState = state;
	}

	private IEnumerator ShootingTutorialPopup()
	{
		yield return new WaitForSeconds(ShootingTutorialPopupDelay);
		TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.ShootEnemy);
	}

	private void RandomlySpawnTestEnemy()
	{
		switch (UnityEngine.Random.Range(1, 6))
		{
		case 1:
			EnemyManager.instance.SpawnEnemy("Thermosplitter_PF", 1);
			break;
		case 2:
			EnemyManager.instance.SpawnEnemy("BreegusWasp_PF", 1);
			break;
		case 3:
			EnemyManager.instance.SpawnEnemy("Thugs4Less_PF", 1);
			break;
		case 4:
			EnemyManager.instance.SpawnEnemy("SecurityBot_PF", 1);
			break;
		case 5:
			EnemyManager.instance.SpawnEnemy("Protoguard_PF", 1);
			break;
		case 6:
			EnemyManager.instance.SpawnEnemy("CerulleanSwarmer_PF", 1);
			break;
		}
	}

	public void ChangeState(eGameState state)
	{
		LeaveState(state);
		EnterState(state);
	}

	public void ToggleShootingMode()
	{
		m_ShootingMode = !m_ShootingMode;
	}

	public bool GetShootingMode()
	{
		if (m_ShootingMode)
		{
			return true;
		}
		return false;
	}

	private void SetLanguage()
	{
		SystemLanguage systemLanguage = Application.systemLanguage;
		switch (systemLanguage)
		{
		case SystemLanguage.Japanese:
			UIManager.instance.ShouldSwapFonts = true;
			PlayerPrefs.SetString("Language", systemLanguage.ToString());
			Localization.instance.currentLanguage = systemLanguage.ToString();
			showFlags = false;
			break;
		case SystemLanguage.Danish:
		case SystemLanguage.Dutch:
		case SystemLanguage.Finnish:
		case SystemLanguage.French:
		case SystemLanguage.German:
		case SystemLanguage.Italian:
		case SystemLanguage.Polish:
		case SystemLanguage.Portuguese:
		case SystemLanguage.Russian:
		case SystemLanguage.Spanish:
		case SystemLanguage.Swedish:
			PlayerPrefs.SetString("Language", systemLanguage.ToString());
			Localization.instance.currentLanguage = systemLanguage.ToString();
			showFlags = false;
			break;
		default:
			PlayerPrefs.SetString("Language", "English");
			break;
		}
	}

	public void SetBritishEnglish()
	{
		PlayerPrefs.SetString("Language", "UKEnglish");
		Localization.instance.currentLanguage = "UKEnglish";
		showFlags = false;
	}

	public void SetAmericanEnglish()
	{
		showFlags = false;
	}

	public void SetNorwegian()
	{
		PlayerPrefs.SetString("Language", "Norwegian");
		Localization.instance.currentLanguage = "Norwegian";
		showFlags = false;
	}

	public bool ShouldShowFlags()
	{
		return showFlags;
	}

	private void ComputeScore()
	{
		int num = (int)StatsTracker.instance.GetStat(StatsTracker.Stats.distanceTraveled);
		int num2 = (int)StatsTracker.instance.GetStat(StatsTracker.Stats.boltzCollected);
		int num3 = (int)StatsTracker.instance.GetStat(StatsTracker.Stats.totalPickupsAcquired);
		int num4 = (int)StatsTracker.instance.GetStat(StatsTracker.Stats.totalEnemiesKilled);
		int num5 = (int)StatsTracker.instance.GetStat(StatsTracker.Stats.bossEnemiesKilled);
		float stat = StatsTracker.instance.GetStat(StatsTracker.Stats.totalShotsFired);
		float stat2 = StatsTracker.instance.GetStat(StatsTracker.Stats.totalShotsHit);
		int num6 = num * 5 + num2 * 20 + num3 * 10 + (int)(stat2 / stat * 5000f) * (num4 + num5 * 10);
		StatsTracker.instance.SetStat(StatsTracker.Stats.pointsScored, num6);
		if (UIManager.instance.GetHUD() != null)
		{
			UIManager.instance.GetHUD().UpdateScore();
		}
	}

	public int GetScore()
	{
		return (int)StatsTracker.instance.GetStat(StatsTracker.Stats.pointsScored);
	}

	public void AnalyticsHandlePause(bool wasPaused)
	{
		if (wasPaused)
		{
			float num = Time.time - roundStartTime;
			Analytics.Get().SendDesignEvent("roundEnd:lifeTime", num);
			totalGameplayTime += num;
			EasyAnalytics.Instance.sendTiming("Total Gameplay Time", (long)(totalGameplayTime * 1000f), "TotalGameplayTime", "Total Time In Gameplay", true);
			EasyAnalytics.Instance.setStartSession(false);
			Debug.Log("Google Anal Times: Total Gameplay-- " + totalGameplayTime);
			Debug.Log("Google Anal : Stop Session ");
			totalGameplayTime = 0f;
			longestRunTime = 0f;
			roundStartTime = Time.time;
		}
		else
		{
			Debug.Log("Google Anal : Start Session ");
			EasyAnalytics.Instance.setStartSession(true);
		}
	}
}
