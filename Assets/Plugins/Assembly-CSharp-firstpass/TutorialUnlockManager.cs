using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUnlockManager : MonoBehaviour
{
	public enum TutorialLock
	{
		TapSwingshot = 0,
		ShootEnemy = 1,
		UseGadgets = 2,
		SwipeUpJump = 3,
		SwipeHoldHeli = 4,
		TerachnoidTut = 5,
		JumpPadTut = 6,
		PistolTut = 7,
		ShotgunTut = 8,
		BuzzBladesTut = 9,
		PredatorTut = 10,
		RynoTut = 11,
		RaritaniumTut = 12,
		HeroBoltTut = 13
	}

	public static TutorialUnlockManager instance;

	public bool[] tutorialLocks;

	private List<GameObject> m_Tutorials;

	private int numLocks;

	private int currentTutorialIndex;

	private bool AmmoContainerActive;

	private bool OutOfAmmoContainerActive;

	public bool TutorialShowing;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("TutorialUnlockManager: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
			numLocks = Enum.GetNames(typeof(TutorialLock)).Length;
			tutorialLocks = new bool[numLocks];
		}
	}

	private void Start()
	{
		LoadTutorialResources();
	}

	private void LoadTutorialResources()
	{
		m_Tutorials = new List<GameObject>();
		for (int i = 0; i < tutorialLocks.Length; i++)
		{
			if (PlayerPrefs.GetInt(((TutorialLock)i).ToString()) == 0)
			{
				tutorialLocks[i] = true;
				string text = ((TutorialLock)i).ToString();
				UnityEngine.Object @object = Resources.Load("UI/Tutorials/" + text);
				if (@object != null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
					if (gameObject != null)
					{
						m_Tutorials.Add(gameObject);
						UIManager.instance.SwapFont();
						m_Tutorials[i].SetActive(false);
						m_Tutorials[i].transform.parent = UIManager.instance.transform;
						m_Tutorials[i].transform.localPosition = Vector3.zero;
						m_Tutorials[i].transform.localScale = Vector3.one;
					}
					else
					{
						Debug.Log("TutorialManager: " + text + " not found.");
					}
				}
			}
			else
			{
				m_Tutorials.Add(null);
				tutorialLocks[i] = false;
			}
		}
	}

	public void OpenTutorial(TutorialLock tlock)
	{
		if ((int)tlock < m_Tutorials.Count && !(m_Tutorials[(int)tlock] == null))
		{
			TutorialShowing = true;
			m_Tutorials[(int)tlock].SetActive(true);
			currentTutorialIndex = (int)tlock;
			UIEventListener.Get(m_Tutorials[(int)tlock].transform.Find("OKButton").gameObject).onClick = TutorialClicked;
			UIEventListener.Get(m_Tutorials[(int)tlock].transform.Find("OKButton").gameObject).onHover = TutorialHover;
			SFXManager.instance.PlaySound("UI_Resume");
			UIHUD hUD = UIManager.instance.GetHUD();
			if (hUD != null)
			{
				hUD.HUD.SetActive(false);
				hUD.SkillPointDialogue.SetActive(false);
				hUD.isSkillDialogueShowing = false;
				SFXManager.instance.StopSound("UI_Transmission_Loop");
				hUD.isSkillChallengesShowing = false;
				hUD.SkillPointPresentation.SetActive(false);
				hUD.SkillPointChallenges.SetActive(false);
			}
			Time.timeScale = 0f;
			GameController.instance.isPaused = true;
			GameController.instance.inMenu = true;
			SFXManager.instance.StopAllSounds();
			if (tlock == TutorialLock.TerachnoidTut)
			{
				SFXManager.instance.PlayUniqueSound("cha_terachnoid_pickup");
			}
			MusicManager.instance.SetVolume(0.5f);
		}
	}

	private void TutorialClicked(GameObject obj)
	{
		TutorialComplete();
	}

	public void TutorialComplete()
	{
		TutorialShowing = false;
		TutorialLock tutorialLock = (TutorialLock)currentTutorialIndex;
		tutorialLocks[(int)tutorialLock] = false;
		PlayerPrefs.SetInt(tutorialLock.ToString(), 1);
		PlayerPrefs.Save();
		m_Tutorials[(int)tutorialLock].SetActive(false);
		UIHUD hUD = UIManager.instance.GetHUD();
		if (hUD != null)
		{
			if (OutOfAmmoContainerActive)
			{
				OutOfAmmoContainerActive = false;
				hUD.OutOfAmmoContainer.SetActive(true);
			}
			if (AmmoContainerActive)
			{
				AmmoContainerActive = false;
				hUD.AmmoContainer.SetActive(true);
			}
			hUD.PauseButton.SetActive(true);
		}
		GameObjectPool.instance.UnloadIndividualObject(m_Tutorials[(int)tutorialLock]);
		MusicManager.instance.SetVolume(1f);
		SFXManager.instance.PlaySound("UI_Resume");
		GameController.instance.Start321Countdown();
		UIManager.instance.GetHUD().Start321Countdown();
	}

	private void TutorialHover(GameObject obj, bool isOver)
	{
		GameController.instance.playerController.dontFire = isOver;
	}
}
