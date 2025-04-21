using System;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeSystem : MonoBehaviour
{
	public class ChallengeInfo
	{
		public int UID;

		public bool Active;

		public bool CurrentComplete;

		public int Category;

		public string Title;

		public string Description;

		public string LocKeyDesc;

		public string LocKeyName;

		public string Key;

		public int Duration;

		public float DurationScale;

		public int CurrentLevel;

		public int MaxLevel;

		public int CompleteLevel = -1;

		public StatsTracker.Stats PrimaryStat;

		public float PrimaryInit;

		public float PrimaryScale;

		public bool LessThan;

		public StatsTracker.Stats SecondaryStat;

		public float SecondaryInit;

		public float SecondaryScale;

		public int RequiredWeapon;

		public int RequiredGadget;

		public string SpriteName;
	}

	private const int NUM_CHALLENGE_CATEGORIES = 3;

	public static ChallengeSystem instance;

	public Dictionary<int, ChallengeInfo> challengeList;

	public Dictionary<int, ChallengeInfo> achievementList;

	private List<int>[] challengesByCategory = new List<int>[3];

	private List<int>[] eligibleChallengeIDs = new List<int>[3];

	private string[] activeChallengeStrings = new string[3] { "AC1", "AC2", "AC3" };

	private int[] activeChallengeIDs = new int[3];

	public string ChallengeFile;

	public bool ActiveChallengesDirty;

	private int numComplete;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("ChallengeSystem: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		challengeList = new Dictionary<int, ChallengeInfo>();
		achievementList = new Dictionary<int, ChallengeInfo>();
		challengesByCategory[0] = new List<int>();
		challengesByCategory[1] = new List<int>();
		challengesByCategory[2] = new List<int>();
		eligibleChallengeIDs[0] = new List<int>();
		eligibleChallengeIDs[1] = new List<int>();
		eligibleChallengeIDs[2] = new List<int>();
		LoadChallengeInfo("Achievements");
		LoadChallengeInfo("Challenges");
		RegisterAchievements();
	}

	private void Start()
	{
		SelectActiveChallenges();
	}

	public int[] GetChallengeIDsDebug()
	{
		int[] array = new int[challengeList.Count];
		int num = 0;
		foreach (KeyValuePair<int, ChallengeInfo> challenge in challengeList)
		{
			array[num++] = challenge.Value.UID;
		}
		return array;
	}

	public string ChallengeTextVariableReplacement(ChallengeInfo ci, string Text)
	{
		float num = ci.PrimaryInit + (float)ci.CurrentLevel * ci.PrimaryScale;
		float num2 = ci.SecondaryInit + (float)ci.CurrentLevel * ci.SecondaryScale;
		if (ci.CurrentComplete)
		{
			num = ci.PrimaryInit + (float)ci.CompleteLevel * ci.PrimaryScale;
			num2 = ci.SecondaryInit + (float)ci.CompleteLevel * ci.SecondaryScale;
		}
		return Text.Replace("{0}", num.ToString()).Replace("{1}", num2.ToString());
	}

	public List<ChallengeInfo> GetActiveChallengeList()
	{
		List<ChallengeInfo> list = new List<ChallengeInfo>();
		for (int i = 0; i < activeChallengeIDs.Length; i++)
		{
			list.Add(challengeList[activeChallengeIDs[i]]);
		}
		return list;
	}

	public bool CheckAchievementCompletion(int uid)
	{
		if (!achievementList.ContainsKey(uid))
		{
			Debug.LogError("ChallengeSystem: ID not found in Achievement List. Is this a Challenge ID?");
			return false;
		}
		ChallengeInfo challengeInfo = achievementList[uid];
		float lifetimeStat = StatsTracker.instance.GetLifetimeStat(challengeInfo.PrimaryStat);
		if (lifetimeStat >= challengeInfo.PrimaryInit + (float)challengeInfo.CurrentLevel * challengeInfo.PrimaryScale)
		{
			SocialManager.instance.CompleteAchievement(challengeInfo.Key);
			return true;
		}
		return false;
	}

	public bool CheckAchievementCompletionDelegate(int uid)
	{
		if (!achievementList.ContainsKey(uid))
		{
			Debug.LogError("ChallengeSystem: ID not found in Achievement List. Is this a Challenge ID?");
			return false;
		}
		ChallengeInfo challengeInfo = achievementList[uid];
		if (challengeInfo.CurrentComplete)
		{
			return true;
		}
		float lifetimeStat = StatsTracker.instance.GetLifetimeStat(challengeInfo.PrimaryStat);
		if (lifetimeStat >= challengeInfo.PrimaryInit + (float)challengeInfo.CurrentLevel * challengeInfo.PrimaryScale)
		{
			challengeInfo.CurrentComplete = true;
			challengeInfo.CurrentLevel++;
			PlayerPrefs.SetInt(challengeInfo.UID.ToString(), challengeInfo.CurrentLevel);
			if (challengeInfo.Duration > 0 && UIManager.instance != null && (UIManager.instance.GetCurrentMenu() == UIManager.MenuPanels.VendorMenu || UIManager.instance.GetCurrentMenu() == UIManager.MenuPanels.VendorFrontMenu))
			{
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.raritaniumPurchased);
				GameController.instance.playerController.addRaritanium(0);
			}
			else
			{
				GameController.instance.playerController.addRaritanium(1);
			}
			if (UIManager.instance != null && UIManager.instance.GetCurrentMenu() != UIManager.MenuPanels.Achievements && UIManager.instance.GetCurrentMenu() != UIManager.MenuPanels.Challenges)
			{
				SocialManager.instance.CompleteAchievement(challengeInfo.Key);
				UIManager.instance.PersistentUI.ShowAchievementCompleted(challengeInfo);
			}
			return true;
		}
		return false;
	}

	public bool CheckChallengeCompletion(int uid)
	{
		if (challengeList.ContainsKey(uid))
		{
			if (challengeList[uid].CurrentComplete)
			{
				return true;
			}
			if (IsGoalComplete(uid))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckChallengeCompletionDelegate(int uid)
	{
		if (GameController.instance.inMenu)
		{
			UIManager.MenuPanels currentMenu = UIManager.instance.GetCurrentMenu();
			if (currentMenu != UIManager.MenuPanels.PauseMenu && currentMenu != 0)
			{
				return false;
			}
		}
		bool flag = false;
		ChallengeInfo challengeInfo = challengeList[uid];
		if (challengeInfo.CurrentComplete || !challengeInfo.Active)
		{
			return true;
		}
		if (challengeInfo.Duration < 0)
		{
			Debug.LogError("ChallengeSystem: CheckChallengeCompletionDelegate called on Achievement");
			return false;
		}
		if (challengeInfo.Duration == 0)
		{
			flag = IsGoalComplete(uid);
		}
		if (flag)
		{
			challengeInfo.CurrentComplete = true;
			numComplete++;
			PlayerController playerController = GameController.instance.playerController;
			playerController.addBoltz(playerController.ChallengeReward, true);
			if (numComplete == activeChallengeIDs.Length)
			{
				playerController.addHeroBolt(1);
			}
			challengeInfo.CompleteLevel = challengeInfo.CurrentLevel;
			if (UIManager.instance != null && UIManager.instance.GetCurrentMenu() != UIManager.MenuPanels.Achievements && UIManager.instance.GetCurrentMenu() != UIManager.MenuPanels.Challenges)
			{
				UIManager.instance.PersistentUI.ShowChallengeCompleted(challengeInfo);
			}
			if (challengeInfo.CurrentLevel < challengeInfo.MaxLevel)
			{
				PlayerPrefs.SetInt(challengeInfo.UID.ToString(), challengeInfo.CurrentLevel + 1);
			}
			else
			{
				PlayerPrefs.SetInt(challengeInfo.UID.ToString(), 0);
			}
			PlayerPrefs.Save();
			return true;
		}
		return false;
	}

	private bool IsGoalComplete(int uid, bool isSecondary = false)
	{
		ChallengeInfo challengeInfo = challengeList[uid];
		float num = 0f;
		float num2 = 0f;
		if (isSecondary)
		{
			num = challengeInfo.SecondaryInit;
			num2 = challengeInfo.SecondaryScale;
		}
		else
		{
			num = challengeInfo.PrimaryInit;
			num2 = challengeInfo.PrimaryScale;
		}
		int currentLevel = challengeInfo.CurrentLevel;
		float stat = StatsTracker.instance.GetStat(challengeInfo.PrimaryStat);
		if (challengeInfo.LessThan)
		{
			if (stat <= num + (float)currentLevel * num2)
			{
				return true;
			}
		}
		else if (stat >= num + (float)currentLevel * num2)
		{
			return true;
		}
		return false;
	}

	private int GetActiveChallengeIndex(int uid)
	{
		for (int i = 0; i < activeChallengeIDs.Length; i++)
		{
			if (activeChallengeIDs[i] == uid)
			{
				return i;
			}
		}
		return -1;
	}

	private void SelectActiveChallenges()
	{
		DetermineEligibleChallenges();
		for (int i = 0; i < 3; i++)
		{
			int num = PlayerPrefs.GetInt(activeChallengeStrings[i]);
			if (!challengeList.ContainsKey(num))
			{
				num = eligibleChallengeIDs[i][Random.Range(0, eligibleChallengeIDs[i].Count)];
				PlayerPrefs.SetInt(activeChallengeStrings[i].ToString(), num);
			}
			eligibleChallengeIDs[i].Remove(num);
			activeChallengeIDs[i] = num;
			if (challengeList.ContainsKey(activeChallengeIDs[i]))
			{
				challengeList[activeChallengeIDs[i]].Active = true;
				challengeList[activeChallengeIDs[i]].CurrentComplete = false;
			}
		}
		foreach (KeyValuePair<int, ChallengeInfo> challenge in challengeList)
		{
			StatsTracker.instance.RegisterCallback(challenge.Value.PrimaryStat, challenge.Value.UID, CheckChallengeCompletionDelegate, challenge.Value.Active);
		}
		ActiveChallengesDirty = false;
	}

	public void SelectNewChallenges()
	{
		if (!ActiveChallengesDirty)
		{
			return;
		}
		DetermineEligibleChallenges();
		numComplete = 0;
		for (int i = 0; i < activeChallengeIDs.Length; i++)
		{
			ChallengeInfo challengeInfo = challengeList[activeChallengeIDs[i]];
			if (challengeInfo.CurrentComplete)
			{
				int num = eligibleChallengeIDs[i][Random.Range(0, eligibleChallengeIDs[i].Count)];
				challengeInfo.Active = false;
				StatsTracker.instance.SetNotActive(challengeInfo.PrimaryStat);
				eligibleChallengeIDs[i].Add(challengeInfo.UID);
				activeChallengeIDs[i] = num;
				eligibleChallengeIDs[i].Remove(num);
				ChallengeInfo challengeInfo2 = challengeList[activeChallengeIDs[i]];
				if (challengeInfo2.CurrentComplete && challengeInfo2.CurrentLevel < challengeInfo2.MaxLevel)
				{
					challengeInfo2.CurrentLevel = challengeInfo2.CompleteLevel + 1;
				}
				challengeInfo2.CurrentComplete = false;
				challengeInfo2.Active = true;
				StatsTracker.instance.SetActive(challengeInfo2.PrimaryStat);
				PlayerPrefs.SetInt(activeChallengeStrings[i].ToString(), challengeInfo2.UID);
				PlayerPrefs.Save();
			}
		}
		UIHUD hUD = UIManager.instance.GetHUD();
		if (hUD != null)
		{
			hUD.SkillPointChallengeGrid.UpdateActiveChallenges();
		}
		ActiveChallengesDirty = false;
	}

	private void RegisterAchievements()
	{
		foreach (KeyValuePair<int, ChallengeInfo> achievement in achievementList)
		{
			if (achievement.Value.CurrentLevel == 0)
			{
				achievement.Value.Active = true;
				StatsTracker.instance.RegisterCallback(achievement.Value.PrimaryStat, achievement.Key, CheckAchievementCompletionDelegate, achievement.Value.Active);
			}
		}
	}

	private void LoadChallengeInfo(string challengeFile)
	{
		Dictionary<string, StatsTracker.Stats> dictionary = new Dictionary<string, StatsTracker.Stats>();
		int num = Enum.GetNames(typeof(StatsTracker.Stats)).Length;
		for (int i = 0; i < num; i++)
		{
			dictionary.Add(((StatsTracker.Stats)i).ToString(), (StatsTracker.Stats)i);
		}
		if (challengeFile == string.Empty)
		{
			Debug.LogError("ChallengeSystem: Input file not specified");
			return;
		}
		cmlReader cmlReader2 = new cmlReader("Data/" + challengeFile);
		if (cmlReader2 == null)
		{
			return;
		}
		List<cmlData> list = cmlReader2.Children();
		foreach (cmlData item in list)
		{
			List<cmlData> list2 = cmlReader2.Children(item.ID);
			foreach (cmlData item2 in list2)
			{
				ChallengeInfo challengeInfo = new ChallengeInfo();
				challengeInfo.UID = int.Parse(item2["UID"]);
				challengeInfo.Title = item2["Title"];
				challengeInfo.Description = item2["Description"];
				challengeInfo.LocKeyDesc = item2["LocKeyDesc"];
				challengeInfo.LocKeyName = item2["LocKeyName"];
				challengeInfo.SpriteName = item2["SpriteName"];
				challengeInfo.Key = item2["Key"];
				challengeInfo.Duration = int.Parse(item2["Duration"]);
				challengeInfo.MaxLevel = int.Parse(item2["MaxLevel"]);
				string text = item2["PrimaryStat"];
				if (text != string.Empty)
				{
					challengeInfo.PrimaryStat = dictionary[text];
				}
				text = item2["PrimaryInit"];
				if (text != string.Empty)
				{
					challengeInfo.PrimaryInit = float.Parse(text);
				}
				text = item2["PrimaryScale"];
				if (text != string.Empty)
				{
					challengeInfo.PrimaryScale = float.Parse(text);
				}
				text = item2["LessThan"];
				if (text != string.Empty)
				{
					if (int.Parse(text) == 0)
					{
						challengeInfo.LessThan = false;
					}
					else
					{
						challengeInfo.LessThan = true;
						StatsTracker.instance.SetStat(challengeInfo.PrimaryStat, 5000000f);
					}
				}
				text = item2["Category"];
				if (text != string.Empty)
				{
					int category = int.Parse(text);
					challengeInfo.Category = category;
				}
				text = item2["SecondaryStat"];
				if (text != string.Empty)
				{
					challengeInfo.SecondaryStat = dictionary[text];
				}
				text = item2["SecondaryInit"];
				if (text != string.Empty)
				{
					challengeInfo.SecondaryInit = float.Parse(text);
				}
				text = item2["SecondaryScale"];
				if (text != string.Empty)
				{
					challengeInfo.SecondaryScale = float.Parse(text);
				}
				if (challengeInfo.UID == 0)
				{
					Debug.LogError("ChallengeSystem: 0 is not a valid challenge ID");
					continue;
				}
				challengeInfo.CurrentLevel = PlayerPrefs.GetInt(challengeInfo.UID.ToString(), 0);
				challengeInfo.CompleteLevel = challengeInfo.CurrentLevel - 1;
				challengeInfo.Active = false;
				challengeInfo.CurrentComplete = false;
				if (challengeInfo.Duration == -1)
				{
					if (achievementList.ContainsKey(challengeInfo.UID))
					{
						Debug.LogError("ChallengeSystem: Duplicate Achievement ID detected");
					}
					else
					{
						achievementList.Add(challengeInfo.UID, challengeInfo);
					}
					continue;
				}
				challengeInfo.RequiredWeapon = int.Parse(item2["WeaponReq"]);
				challengeInfo.RequiredGadget = int.Parse(item2["GadgetReq"]);
				if (challengeList.ContainsKey(challengeInfo.UID))
				{
					Debug.LogError("ChallengeSystem: Duplicate Challenge ID detected");
					continue;
				}
				challengesByCategory[challengeInfo.Category].Add(challengeInfo.UID);
				challengeList.Add(challengeInfo.UID, challengeInfo);
			}
		}
	}

	private void DetermineEligibleChallenges()
	{
		Debug.Log("Determine Eligible Challenges - --- --- ---- --- - - ----- ---- -- ----- - ---- --- ------- - ------- ");
		for (int i = 0; i < 3; i++)
		{
			eligibleChallengeIDs[i].Clear();
			int num = -1;
			int m = -1;
			int g = -1;
			for (int j = 0; j <= 4; j++)
			{
				if (WeaponsManager.instance.HaveBoughtWeapon((WeaponsManager.WeaponList)j))
				{
					num = j;
				}
			}
			if (num < 4)
			{
				num++;
			}
			for (int k = 0; k <= 2; k++)
			{
				if (MegaWeaponManager.instance.HaveBoughtMegaWeapon((MegaWeaponManager.eMegaWeapons)k))
				{
					m = k;
				}
			}
			for (int l = 0; l <= 3; l++)
			{
				if (GadgetManager.instance.HaveBoughtGadget((GadgetManager.eGadgets)l))
				{
					g = l;
				}
			}
			int num2 = Mathf.Max((int)GadgetManager.GetUnifiedFromGadget((GadgetManager.eGadgets)g), (int)GadgetManager.GetUnifiedFromMega((MegaWeaponManager.eMegaWeapons)m));
			if (num2 < 6)
			{
				num2++;
			}
			foreach (int item in challengesByCategory[i])
			{
				ChallengeInfo challengeInfo = challengeList[item];
				if (challengeInfo.RequiredWeapon <= num && challengeInfo.RequiredGadget <= num2)
				{
					eligibleChallengeIDs[i].Add(item);
				}
			}
		}
	}
}
