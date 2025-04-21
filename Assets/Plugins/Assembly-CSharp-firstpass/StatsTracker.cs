using System;
using System.Collections.Generic;
using UnityEngine;

public class StatsTracker : MonoBehaviour
{
	public enum Stats
	{
		none = 0,
		onRightRail = 1,
		onLeftRail = 2,
		onMiddleRail = 3,
		weaponsUsed = 4,
		withinDistanceLanesSwitched = 5,
		groovitronActive = 6,
		groovitronCurrentKills = 7,
		groovitronCurrentJumps = 8,
		tornadoActive = 9,
		tornadoLauncherCurrentKills = 10,
		tornadoLauncherCurrentJumps = 11,
		tornadoBoltzCollected = 12,
		riftInducerActive = 13,
		riftInducerCurrentKills = 14,
		riftInducerCurrentJumps = 15,
		jetpackActive = 16,
		jetPackCurrentKills = 17,
		jetPackLanesSwitched = 18,
		reflectorObstaclesSmashed = 19,
		reflectorShotsAbsorbed = 20,
		reflectorNoHits = 21,
		noCratesDistanceTraveled = 22,
		noCratesDistanceBase = 23,
		noDamageDistanceTraveled = 24,
		noDamageDistanceBase = 25,
		noBoltzDistanceTraveled = 26,
		noBoltzDistanceBase = 27,
		pistolReloads = 28,
		oneShotPistolKills = 29,
		oneShotShotgunKills = 30,
		oneShotBuzzbladeKills = 31,
		oneShotPredatorKills = 32,
		buzzbladesThugKills = 33,
		multiplierActive = 34,
		reflectorActive = 35,
		distanceTraveled = 36,
		distanceFlown = 37,
		lanesSwitched = 38,
		swingShotUsed = 39,
		timesJumped = 40,
		deathDistance = 41,
		damageTaken = 42,
		deathTotal = 43,
		deathByCrash = 44,
		deathByFall = 45,
		deathByEnemy = 46,
		pointsScored = 47,
		boltzCollected = 48,
		boltzSpent = 49,
		heroBoltsCollected = 50,
		heroBoltsUsed = 51,
		raritaniumCollected = 52,
		raritaniumDeposited = 53,
		terachnoidsCollected = 54,
		creditsWatched = 55,
		weaponsUpgraded = 56,
		pistolUpgraded = 57,
		shotgunUpgraded = 58,
		buzzbladesUpgraded = 59,
		predatorUpgraded = 60,
		gadgetsUpgraded = 61,
		magnetizerUpgraded = 62,
		reflectorUpgraded = 63,
		jetpackUpgraded = 64,
		multiplierUpgraded = 65,
		armorUpgraded = 66,
		skinsBought = 67,
		gadgetsBought = 68,
		gadgetsCollected = 69,
		lightArmorPurchased = 70,
		heavyArmorPurchased = 71,
		nexusArmorPurchased = 72,
		totalPickupsAcquired = 73,
		cratesBroken = 74,
		reflectorPickedup = 75,
		jetpackPickedup = 76,
		multiplierPickedup = 77,
		magnetizerPickedup = 78,
		magnetizerActive = 79,
		magnetizerBoltzCollected = 80,
		megaWeaponsUsed = 81,
		groovitronUsed = 82,
		groovitronUpgraded = 83,
		groovitronKills = 84,
		tornadoLauncherUsed = 85,
		tornadoLauncherUpgraded = 86,
		tornadoLauncherKills = 87,
		riftInducerUsed = 88,
		riftInducerUpgraded = 89,
		riftInducerKills = 90,
		totalShotsFired = 91,
		totalShotsHit = 92,
		pistolShotsFired = 93,
		pistolShotsHit = 94,
		pistolKills = 95,
		shotgunShotsFired = 96,
		shotgunShotsHit = 97,
		shotgunKills = 98,
		shotgunBossKills = 99,
		buzzbladesShotsFired = 100,
		buzzbladesShotsHit = 101,
		buzzbladesKills = 102,
		buzzbladesBossKills = 103,
		predatorShotsFired = 104,
		predatorShotsHit = 105,
		predatorKills = 106,
		predatorBossKills = 107,
		rynoShotsFired = 108,
		rynoShotsHit = 109,
		rynoKills = 110,
		rynoBossKills = 111,
		totalEnemiesKilled = 112,
		flyingEnemiesKilled = 113,
		groundEnemiesKilled = 114,
		dancingEnemiesKilled = 115,
		bossEnemiesKilled = 116,
		breegusWaspsKilled = 117,
		thugs4LessKilled = 118,
		ceruleanSwarmerKilled = 119,
		securityBotKilled = 120,
		thermoSplitterKilled = 121,
		protoGuardKilled = 122,
		leviathanKilled = 123,
		raritaniumPurchased = 124
	}

	private class StatCheckInfo
	{
		public StatCheckDelegate callback;

		public bool active;
	}

	public delegate bool StatCheckDelegate(int uid);

	private const string bestRunPrefix = "BestRun";

	public static StatsTracker instance;

	private int numStats;

	private float[] statsArray;

	private Dictionary<Stats, Dictionary<int, StatCheckInfo>> statsCallbacks;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("StatsTracker: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		numStats = Enum.GetNames(typeof(Stats)).Length;
		statsArray = new float[numStats];
		statsCallbacks = new Dictionary<Stats, Dictionary<int, StatCheckInfo>>();
	}

	public void FullReset()
	{
		for (int i = 0; i < numStats; i++)
		{
			statsArray[i] = 0f;
		}
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	public void SaveStatsAndReset()
	{
		for (int i = 0; i < numStats; i++)
		{
			string text = ((Stats)i).ToString();
			string key = "BestRun" + text;
			if (statsArray[i] > PlayerPrefs.GetFloat(key))
			{
				PlayerPrefs.SetFloat(key, statsArray[i]);
			}
			PlayerPrefs.SetFloat(text, PlayerPrefs.GetFloat(text) + statsArray[i]);
			statsArray[i] = 0f;
		}
	}

	public void UpdateStat(Stats stat, float amount = 1f)
	{
		statsArray[(int)stat] += amount;
		CallCallbacks(stat);
	}

	public void SetStat(Stats stat, float newValue)
	{
		statsArray[(int)stat] = newValue;
		CallCallbacks(stat);
	}

	public float GetStat(Stats stat)
	{
		return statsArray[(int)stat];
	}

	public float GetSingleRunBestStat(Stats stat)
	{
		return PlayerPrefs.GetFloat("BestRun" + stat);
	}

	private void SetSingleRunBestStat(Stats stat, float amount)
	{
		PlayerPrefs.SetFloat("BestRun" + stat, amount);
	}

	public float GetLifetimeStat(Stats stat)
	{
		return statsArray[(int)stat] + PlayerPrefs.GetFloat(stat.ToString());
	}

	public void RegisterCallback(Stats stat, int uid, StatCheckDelegate statDelegate, bool active)
	{
		if (!statsCallbacks.ContainsKey(stat))
		{
			statsCallbacks[stat] = new Dictionary<int, StatCheckInfo>();
		}
		if (!statsCallbacks[stat].ContainsKey(uid))
		{
			StatCheckInfo statCheckInfo = new StatCheckInfo();
			statCheckInfo.callback = statDelegate;
			statCheckInfo.active = active;
			statsCallbacks[stat][uid] = statCheckInfo;
		}
		else
		{
			Debug.Log("StatsTracker: Multiple callbacks being registered for challenge");
			Debug.Log("Stat: " + stat.ToString() + " ID: " + uid);
		}
	}

	public void SetActive(Stats stat)
	{
		if (!statsCallbacks.ContainsKey(stat))
		{
			return;
		}
		foreach (KeyValuePair<int, StatCheckInfo> item in statsCallbacks[stat])
		{
			item.Value.active = true;
		}
	}

	public void SetNotActive(Stats stat)
	{
		if (!statsCallbacks.ContainsKey(stat))
		{
			return;
		}
		foreach (KeyValuePair<int, StatCheckInfo> item in statsCallbacks[stat])
		{
			item.Value.active = false;
		}
	}

	private void CallCallbacks(Stats stat)
	{
		if (!statsCallbacks.ContainsKey(stat))
		{
			return;
		}
		foreach (KeyValuePair<int, StatCheckInfo> item in statsCallbacks[stat])
		{
			if (item.Value.active)
			{
				item.Value.active = !item.Value.callback(item.Key);
			}
		}
	}
}
