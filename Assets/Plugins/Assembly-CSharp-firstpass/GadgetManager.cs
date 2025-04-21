using System.Collections.Generic;
using UnityEngine;

public class GadgetManager : MonoBehaviour
{
	public enum eGadgets
	{
		g_NONE = -1,
		g_Magnetizer = 0,
		g_Multiplier = 1,
		g_Reflector = 2,
		g_Jetpack = 3,
		g_MAX = 4
	}

	public enum eUnifiedGadget
	{
		u_NONE = -1,
		u_Magnetizer = 0,
		u_Groovitron = 1,
		u_Multiplier = 2,
		u_Reflector = 3,
		u_RiftInducer = 4,
		u_Jetpack = 5,
		u_Tornado = 6,
		u_MAX = 7
	}

	public const string magnetizerName = "PIK_Frame_Magnetizer";

	public const string multiplierName = "PIK_Frame_Multiplier";

	public const string reflectorName = "PIK_Frame_Reflector";

	public const string jetpackName = "PIK_Jetpack";

	public static GadgetManager instance;

	private int[] GadgetUpgradeLevel;

	public string[] GadgetLocKeys;

	public string[] GadgetLocKeyDescriptions;

	public string[] GadgetSpriteNames;

	public int minTilesBeforeGadget = 3;

	public int maxGadgetLevels = 4;

	public float magnetizerBaseRadius = 5f;

	public float[] magnetizerRadius;

	public float[] magnetizerDuration;

	public ParticleSystem magnetizerParticles;

	public Vector3 magnetizerOffset = Vector3.zero;

	public float MagnetizerStrength = 1.4f;

	public int[] reflectorHealthUpgrade;

	public float[] reflectorDuration;

	public ParticleSystem reflectorParticles;

	public Vector3 reflectorOffset = Vector3.zero;

	public float reflectorSpeedBoostMultiplier = 2f;

	public int[] boltMultiplier;

	public float[] boltMultiplierDuration;

	public ParticleSystem boltMultiplierParticles;

	public Vector3 boltMultiplierOffset = Vector3.zero;

	public int[] jetpackDuration;

	public float jetpackSpeedBoostMultiplier = 2f;

	public float maxJetpackAltitude = 6.5f;

	public float updownJetpackAltTime = 1f;

	public float jetpackInvincibleLandingTime = 3f;

	public Vector3 jetpackOffset = Vector3.zero;

	public ParticleSystem jetpackInvincibleParticles;

	public float maxDropPercentage = 25f;

	private float magnetizerDropPercentage;

	private float multiplierDropPercentage;

	private float reflectorDropPercentage;

	private float jetpackDropPercentage;

	private List<GameObject> obstaclesToConsiderForGadgetSpawning = new List<GameObject>();

	private float boltMultiplierTimer;

	private GameObject curMultiplierObject;

	private float magnetizerTimer;

	private GameObject curMagnetizerObject;

	private int reflectorHealth;

	public float reflectorTimer;

	private GameObject curReflectorObject;

	private float jetpackTimer;

	public float jetpackInvincibleTimer;

	private bool Jetpack;

	public bool invincibleLanding;

	private float jetpackAltitude;

	private int numTilesSinceLastGadget;

	public void Validate()
	{
		float num = maxDropPercentage * 4f;
		if (num > 100f)
		{
			Debug.LogWarning("ERROR: Gadgets are using more than 100% drop percentage!");
		}
		if (magnetizerRadius.Length != maxGadgetLevels)
		{
			Debug.LogWarning("magnetizerRadius array is not the size of the maxGadgetLevels!");
		}
		if (magnetizerDuration.Length != maxGadgetLevels)
		{
			Debug.LogWarning("magnetizerDuration array is not the size of the maxGadgetLevels!");
		}
		if (reflectorHealthUpgrade.Length != maxGadgetLevels)
		{
			Debug.LogWarning("reflectorHealthUpgrade array is not the size of the maxGadgetLevels!");
		}
		if (reflectorDuration.Length != maxGadgetLevels)
		{
			Debug.LogWarning("reflectorDuration array is not the size of the maxGadgetLevels!");
		}
		if (boltMultiplier.Length != maxGadgetLevels)
		{
			Debug.LogWarning("boltMultiplier array is not the size of the maxGadgetLevels!");
		}
		if (boltMultiplierDuration.Length != maxGadgetLevels)
		{
			Debug.LogWarning("boltMultiplierDuration array is not the size of the maxGadgetLevels!");
		}
		if (jetpackDuration.Length != maxGadgetLevels)
		{
			Debug.LogWarning("jetpackDuration array is not the size of the maxGadgetLevels!");
		}
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("GadgetManager: Multiple instances spawned");
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		GadgetUpgradeLevel = new int[4];
		for (int i = 0; i < GadgetUpgradeLevel.Length; i++)
		{
			GadgetUpgradeLevel[i] = 0;
			int @int = PlayerPrefs.GetInt(((eGadgets)i).ToString());
			if (i == 0 && @int == 0)
			{
				UpgradeGadget(eGadgets.g_Magnetizer, true);
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsBought);
				PlayerPrefs.SetInt(eGadgets.g_Magnetizer.ToString(), 1);
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.magnetizerUpgraded);
				StatsTracker.instance.SaveStatsAndReset();
			}
			for (int j = 0; j < @int; j++)
			{
				UpgradeGadget((eGadgets)i, true);
			}
		}
	}

	private void Start()
	{
		Validate();
	}

	private void FixedUpdate()
	{
		List<Boltz> bolts = GameController.instance.playerController.Bolts;
		for (int i = 0; i < bolts.Count; i++)
		{
			Boltz boltz = bolts[i];
			if (Time.timeScale != 0f && boltz.gameObject.activeSelf && (GetMagnetizerRadius() >= Vector3.Distance(boltz.transform.position, GameController.instance.playerController.rigidbody.position) || MegaWeaponManager.instance.GetTornadoPickupRange() >= Vector3.Distance(boltz.transform.position, MegaWeaponManager.instance.GetMegaWeaponPosition())))
			{
				boltz.transform.position -= Vector3.Normalize(boltz.transform.position - GameController.instance.playerController.rigidbody.position) * MagnetizerStrength;
			}
		}
	}

	private void Update()
	{
		if (boltMultiplierTimer > 0f)
		{
			boltMultiplierTimer -= Time.deltaTime;
			if (curMultiplierObject != null)
			{
				AttemptFlicker(curMultiplierObject, boltMultiplierTimer);
			}
			if (boltMultiplierTimer <= 0f)
			{
				DeactivateBoltMultiplier();
			}
		}
		if (magnetizerTimer > 0f)
		{
			magnetizerTimer -= Time.deltaTime;
			if (curMagnetizerObject != null)
			{
				AttemptFlicker(curMagnetizerObject, magnetizerTimer);
			}
			if (magnetizerTimer <= 0f)
			{
				DeactivateMagnetizer();
			}
		}
		if (reflectorTimer > 0f)
		{
			reflectorTimer -= Time.deltaTime;
			if (curReflectorObject != null)
			{
				AttemptFlicker(curReflectorObject, reflectorTimer);
			}
			if (reflectorTimer <= 0f)
			{
				DeactivateReflector();
			}
		}
		if (jetpackTimer > 0f)
		{
			jetpackTimer -= Time.deltaTime;
			if (jetpackTimer <= 0f)
			{
				DeactivateJetpack();
			}
			else
			{
				UpdateJetpackAltitude();
			}
		}
		if (!(jetpackInvincibleTimer > 0f))
		{
			return;
		}
		jetpackInvincibleTimer -= Time.deltaTime;
		if (jetpackInvincibleTimer <= 0f)
		{
			invincibleLanding = false;
			if (GameController.instance.playerController.flashEffect != null)
			{
				GameController.instance.playerController.gameObject.SendMessage("StartFlashTint", this, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public bool HaveBoughtGadget(eGadgets g_Gadget)
	{
		switch (g_Gadget)
		{
		case eGadgets.g_Magnetizer:
			if (magnetizerDropPercentage > 0f)
			{
				return true;
			}
			break;
		case eGadgets.g_Reflector:
			if (reflectorDropPercentage > 0f)
			{
				return true;
			}
			break;
		case eGadgets.g_Multiplier:
			if (multiplierDropPercentage > 0f)
			{
				return true;
			}
			break;
		case eGadgets.g_Jetpack:
			if (jetpackDropPercentage > 0f)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public int GetGadgetLevel(eGadgets g_Gadget)
	{
		switch (g_Gadget)
		{
		case eGadgets.g_Magnetizer:
			return GadgetUpgradeLevel[0] + 1;
		case eGadgets.g_Reflector:
			return GadgetUpgradeLevel[2] + 1;
		case eGadgets.g_Multiplier:
			return GadgetUpgradeLevel[1] + 1;
		case eGadgets.g_Jetpack:
			return GadgetUpgradeLevel[3] + 1;
		default:
			Debug.LogWarning("GetGadgetLevel: Gadget Not Found");
			return 0;
		}
	}

	public void UpgradeGadget(eGadgets g_Gadget, bool init = false)
	{
		bool flag = false;
		int value = 0;
		switch (g_Gadget)
		{
		case eGadgets.g_Magnetizer:
			if (magnetizerDropPercentage == 0f)
			{
				magnetizerDropPercentage = maxDropPercentage;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsBought);
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.magnetizerUpgraded);
				}
				flag = true;
			}
			else if (GadgetUpgradeLevel[0] < maxGadgetLevels - 1)
			{
				GadgetUpgradeLevel[0]++;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.magnetizerUpgraded);
				}
				flag = true;
			}
			value = (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.magnetizerUpgraded);
			break;
		case eGadgets.g_Reflector:
			if (reflectorDropPercentage == 0f)
			{
				reflectorDropPercentage = maxDropPercentage;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsBought);
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.reflectorUpgraded);
				}
				flag = true;
			}
			else if (GadgetUpgradeLevel[2] < maxGadgetLevels - 1)
			{
				GadgetUpgradeLevel[2]++;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.reflectorUpgraded);
				}
				flag = true;
			}
			value = (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.reflectorUpgraded);
			break;
		case eGadgets.g_Multiplier:
			if (multiplierDropPercentage == 0f)
			{
				multiplierDropPercentage = maxDropPercentage;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsBought);
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.multiplierUpgraded);
				}
				flag = true;
			}
			else if (GadgetUpgradeLevel[1] < maxGadgetLevels - 1)
			{
				GadgetUpgradeLevel[1]++;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.multiplierUpgraded);
				}
				flag = true;
			}
			value = (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.multiplierUpgraded);
			break;
		case eGadgets.g_Jetpack:
			if (jetpackDropPercentage == 0f)
			{
				jetpackDropPercentage = maxDropPercentage;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsBought);
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.jetpackUpgraded);
				}
				flag = true;
			}
			else if (GadgetUpgradeLevel[3] < maxGadgetLevels - 1)
			{
				GadgetUpgradeLevel[3]++;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.jetpackUpgraded);
				}
				flag = true;
			}
			value = (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.jetpackUpgraded);
			break;
		default:
			Debug.LogWarning("UpgradeGadget: Gadget Not Found");
			break;
		}
		if (flag && !init)
		{
			PlayerPrefs.SetInt(g_Gadget.ToString(), value);
			if (GadgetUpgradeLevel[(int)g_Gadget] >= maxGadgetLevels - 1)
			{
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsUpgraded);
			}
		}
	}

	public void HandleGadgetSpawning()
	{
		numTilesSinceLastGadget++;
		if (numTilesSinceLastGadget > minTilesBeforeGadget)
		{
			numTilesSinceLastGadget = 0;
			float num = Random.Range(0, 100);
			float num2 = multiplierDropPercentage / (float)maxGadgetLevels * (float)(GadgetUpgradeLevel[1] + 1);
			float num3 = num2 + magnetizerDropPercentage / (float)maxGadgetLevels * (float)(GadgetUpgradeLevel[0] + 1);
			float num4 = num3 + reflectorDropPercentage / (float)maxGadgetLevels * (float)(GadgetUpgradeLevel[2] + 1);
			float num5 = num4 + jetpackDropPercentage / (float)maxGadgetLevels * (float)(GadgetUpgradeLevel[3] + 1);
			if (num < num2)
			{
				spawnGadget("PIK_Frame_Multiplier");
			}
			else if (num < num3)
			{
				spawnGadget("PIK_Frame_Magnetizer");
			}
			else if (num < num4)
			{
				spawnGadget("PIK_Frame_Reflector");
			}
			else if (num < num5)
			{
				spawnGadget("PIK_Jetpack");
			}
		}
	}

	private void spawnGadget(string name)
	{
		obstaclesToConsiderForGadgetSpawning.Clear();
		bool flag = false;
		float num = TileSpawnManager.instance.worldPieceSize / (float)ObstacleManager.instance.tileSpawnSegments;
		float lastRileTileOffset = TileSpawnManager.instance.getLastRileTileOffset();
		lastRileTileOffset += (0f - TileSpawnManager.instance.worldPieceSize) / 2f;
		int num2 = 0;
		float num3 = lastRileTileOffset;
		Vector3 zero = Vector3.zero;
		while (!flag && num2 < 10)
		{
			int num4 = Random.Range(0, 10) % 3;
			int num5 = Random.Range(0, ObstacleManager.instance.getSpawnSegmentsByDist());
			num3 = lastRileTileOffset + num * (float)num5;
			if (ObstacleManager.instance.segmentOccupied[num4, num5])
			{
				num2++;
				continue;
			}
			zero = TileSpawnManager.instance.getSpawnPosition(num4, num3);
			if (zero != Vector3.zero)
			{
				GameObject nextFree = GameObjectPool.instance.GetNextFree(name, true);
				if (nextFree != null)
				{
					nextFree.transform.position = zero;
					ObstacleManager.instance.segmentOccupied[num4, num5] = true;
					flag = true;
					nextFree.GetComponentInChildren<Pickup>().renderer.enabled = true;
					nextFree.GetComponentInChildren<Pickup>().enabled = true;
				}
				else
				{
					num2++;
				}
			}
			else
			{
				num2++;
			}
		}
		obstaclesToConsiderForGadgetSpawning.Clear();
	}

	public float GetMagnetizerRadius()
	{
		if (IsMagnetizerActive())
		{
			return magnetizerRadius[GadgetUpgradeLevel[0]];
		}
		return magnetizerBaseRadius;
	}

	public bool IsMagnetizerActive()
	{
		if (magnetizerTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public void ActivateMagnetizer()
	{
		magnetizerTimer = magnetizerDuration[GadgetUpgradeLevel[0]];
		if (curMagnetizerObject == null)
		{
			PlayMagnetizerParticle();
		}
		StatsTracker.instance.SetStat(StatsTracker.Stats.magnetizerBoltzCollected, 0f);
	}

	private void PlayMagnetizerParticle()
	{
		if (magnetizerParticles != null)
		{
			curMagnetizerObject = GameObjectPool.instance.GetNextFree(magnetizerParticles.name, true);
			curMagnetizerObject.transform.position = GameController.instance.playerController.rigidbody.position + magnetizerOffset;
			curMagnetizerObject.particleSystem.Play();
		}
	}

	public void DeactivateMagnetizer()
	{
		magnetizerTimer = 0f;
		if (curMagnetizerObject != null)
		{
			GameObjectPool.instance.SetFree(curMagnetizerObject, true);
			curMagnetizerObject = null;
		}
	}

	public int GetBoltMultiplier()
	{
		if (boltMultiplierTimer > 0f)
		{
			return boltMultiplier[GadgetUpgradeLevel[1]];
		}
		return 1;
	}

	public bool IsBoltMultiplierActive()
	{
		if (boltMultiplierTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public void ActivateBoltMultiplier()
	{
		boltMultiplierTimer = boltMultiplierDuration[GadgetUpgradeLevel[1]];
		UIManager.instance.GetHUD().UpdateBoltMultiplier();
		if (curMultiplierObject == null)
		{
			PlayBoltMultiplierParticle();
		}
	}

	private void PlayBoltMultiplierParticle()
	{
		if (boltMultiplierParticles != null)
		{
			curMultiplierObject = GameObjectPool.instance.GetNextFree(boltMultiplierParticles.name, true);
			curMultiplierObject.transform.position = GameController.instance.playerController.rigidbody.position + boltMultiplierOffset;
			curMultiplierObject.particleSystem.Play();
		}
	}

	private void DeactivateBoltMultiplier()
	{
		boltMultiplierTimer = 0f;
		UIManager.instance.GetHUD().UpdateBoltMultiplier();
	}

	public int GetReflectorHealth()
	{
		return reflectorHealth;
	}

	public void ActivateReflector()
	{
		if (reflectorHealthUpgrade.Length <= GadgetUpgradeLevel[2])
		{
			Debug.LogError("ActivateReflector : reflectorHealthUpgrade out of bounds of array");
		}
		reflectorHealth = reflectorHealthUpgrade[GadgetUpgradeLevel[2]];
		reflectorTimer = reflectorDuration[GadgetUpgradeLevel[2]];
		StatsTracker.instance.SetStat(StatsTracker.Stats.reflectorObstaclesSmashed, 0f);
		StatsTracker.instance.SetStat(StatsTracker.Stats.reflectorShotsAbsorbed, 0f);
		StatsTracker.instance.SetStat(StatsTracker.Stats.reflectorNoHits, 0f);
		PlayReflectorParticle();
		UIManager.instance.GetHUD().UpdateHP();
	}

	private void DeactivateReflector()
	{
		reflectorHealth = 0;
		reflectorTimer = 0f;
		if (curReflectorObject != null)
		{
			GameObjectPool.instance.SetFree(curReflectorObject, true);
			curReflectorObject = null;
		}
		if (!GameController.instance.playerController.GodMode)
		{
			int num = (int)StatsTracker.instance.GetStat(StatsTracker.Stats.reflectorShotsAbsorbed);
			int num2 = (int)StatsTracker.instance.GetStat(StatsTracker.Stats.reflectorObstaclesSmashed);
			if (num + num2 == 0)
			{
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.reflectorNoHits);
			}
		}
		UIManager.instance.GetHUD().UpdateHP();
	}

	public void PlayReflectorParticle()
	{
		if (reflectorParticles != null)
		{
			curReflectorObject = GameObjectPool.instance.GetNextFree(reflectorParticles.name, true);
			curReflectorObject.transform.position = GameController.instance.playerController.rigidbody.position + reflectorOffset;
			curReflectorObject.particleSystem.Play();
		}
	}

	public void ReflectorHit()
	{
		reflectorHealth--;
		SFXManager.instance.GetAudioSource("Wep_Gen_Bullet_Hit").pitch = Random.Range(0.8f, 1.2f);
		SFXManager.instance.PlaySound("Wep_Gen_Bullet_Hit");
		if (reflectorHealth <= 0)
		{
			DeactivateReflector();
		}
		UIHUD hUD = UIManager.instance.GetHUD();
		if (hUD != null)
		{
			hUD.UpdateHP();
		}
		if (TileSpawnManager.instance.floorTileList[1].GetComponent<TileInfo>().spawnedState == TileSpawnManager.TileSpawnState.Ground)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.reflectorShotsAbsorbed);
		}
		else
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.reflectorObstaclesSmashed);
		}
	}

	public bool GetJetpackStatus()
	{
		return Jetpack;
	}

	public float GetJetpackAltitude()
	{
		return jetpackAltitude;
	}

	public float GetMaxJetpackAltitude()
	{
		return maxJetpackAltitude;
	}

	private void UpdateJetpackAltitude()
	{
		if (jetpackTimer > (float)jetpackDuration[GadgetUpgradeLevel[3]] - updownJetpackAltTime)
		{
			jetpackAltitude = maxJetpackAltitude * (((float)jetpackDuration[GadgetUpgradeLevel[3]] - jetpackTimer) / updownJetpackAltTime);
		}
		else if (jetpackTimer < updownJetpackAltTime)
		{
			jetpackAltitude = maxJetpackAltitude * ((updownJetpackAltTime - (updownJetpackAltTime - jetpackTimer)) / updownJetpackAltTime);
		}
	}

	public void ActivateJetpack()
	{
		if (!Jetpack)
		{
			Jetpack = true;
			jetpackTimer = jetpackDuration[GadgetUpgradeLevel[3]];
			GameController.instance.playerController.SetJetpack(true);
			PickupManager.instance.SpawnBoltsForJetpack(jetpackTimer);
			SFXManager.instance.PlaySound("cha_Ratchet_Jet_Idle");
			StatsTracker.instance.SetStat(StatsTracker.Stats.jetPackCurrentKills, 0f);
			StatsTracker.instance.SetStat(StatsTracker.Stats.jetPackLanesSwitched, 0f);
			invincibleLanding = true;
			PlayJetpackParticle();
		}
		else
		{
			jetpackTimer = jetpackDuration[GadgetUpgradeLevel[3]];
			PickupManager.instance.SpawnBoltsForJetpack(jetpackTimer);
		}
	}

	public void DeactivateJetpack()
	{
		Jetpack = false;
		GameController.instance.playerController.SetJetpack(false);
		invincibleLanding = true;
		jetpackInvincibleTimer = jetpackInvincibleLandingTime;
		PlayJetpackParticle();
		SFXManager.instance.StopSound("cha_Ratchet_Jet_Idle");
	}

	public bool IsJetpackInvincibleLanding()
	{
		return invincibleLanding;
	}

	private void PlayJetpackParticle()
	{
		if (GameController.instance.playerController.flashEffect != null)
		{
			GameController.instance.playerController.flashEffect.duration = jetpackInvincibleLandingTime;
			GameController.instance.playerController.gameObject.SendMessage("FlashTint", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void DeactivateAllOnRestart()
	{
		if (Jetpack)
		{
			DeactivateJetpack();
		}
		if (IsMagnetizerActive())
		{
			DeactivateMagnetizer();
		}
		if (IsBoltMultiplierActive())
		{
			DeactivateBoltMultiplier();
		}
	}

	public void DeactivateAllOnDeath()
	{
		if (Jetpack)
		{
			DeactivateJetpack();
		}
		if (IsMagnetizerActive())
		{
			DeactivateMagnetizer();
		}
		if (IsBoltMultiplierActive())
		{
			DeactivateBoltMultiplier();
		}
		if (GetReflectorHealth() > 0)
		{
			DeactivateReflector();
		}
	}

	public void AttemptFlicker(GameObject particles, float timeRemaining)
	{
		if (timeRemaining > 1f)
		{
			AttemptPlay(particles);
		}
		else if (timeRemaining > 0.9f)
		{
			AttemptStop(particles);
		}
		else if (timeRemaining > 0.8f)
		{
			AttemptPlay(particles);
		}
		else if (timeRemaining > 0.7f)
		{
			AttemptStop(particles);
		}
		else if (timeRemaining > 0.6f)
		{
			AttemptPlay(particles);
		}
		else if (timeRemaining > 0.5f)
		{
			AttemptStop(particles);
		}
		else if (timeRemaining > 0.4f)
		{
			AttemptPlay(particles);
		}
		else if (timeRemaining > 0.3f)
		{
			AttemptStop(particles);
		}
		else if (timeRemaining > 0.2f)
		{
			AttemptPlay(particles);
		}
		else if (timeRemaining > 0.1f)
		{
			AttemptStop(particles);
		}
		else
		{
			AttemptPlay(particles);
		}
	}

	private void AttemptStop(GameObject particles)
	{
		Vector3 position = GameController.instance.playerController.RatchetHips.position;
		position.y = -100f;
		particles.transform.position = position;
	}

	private void AttemptPlay(GameObject particles)
	{
		if (particles.transform.position.y == -100f)
		{
			Vector3 position = particles.transform.position;
			position.y = GameController.instance.playerController.RatchetHips.position.y;
			particles.transform.position = position;
		}
		particles.transform.position = Vector3.Lerp(particles.transform.position, GameController.instance.playerController.RatchetHips.position, Time.deltaTime * 35f);
	}

	public static eUnifiedGadget GetUnifiedFromGadget(eGadgets g)
	{
		eUnifiedGadget result = eUnifiedGadget.u_NONE;
		switch (g)
		{
		case eGadgets.g_Magnetizer:
			result = eUnifiedGadget.u_Magnetizer;
			break;
		case eGadgets.g_Multiplier:
			result = eUnifiedGadget.u_Multiplier;
			break;
		case eGadgets.g_Reflector:
			result = eUnifiedGadget.u_Reflector;
			break;
		case eGadgets.g_Jetpack:
			result = eUnifiedGadget.u_Jetpack;
			break;
		}
		return result;
	}

	public static eUnifiedGadget GetUnifiedFromMega(MegaWeaponManager.eMegaWeapons m)
	{
		eUnifiedGadget result = eUnifiedGadget.u_NONE;
		switch (m)
		{
		case MegaWeaponManager.eMegaWeapons.mw_Groovitron:
			result = eUnifiedGadget.u_Groovitron;
			break;
		case MegaWeaponManager.eMegaWeapons.mw_RiftInducer:
			result = eUnifiedGadget.u_RiftInducer;
			break;
		case MegaWeaponManager.eMegaWeapons.mw_Tornado:
			result = eUnifiedGadget.u_Tornado;
			break;
		}
		return result;
	}

	public static eGadgets GetGadgetFromUnified(eUnifiedGadget u)
	{
		eGadgets result = eGadgets.g_NONE;
		switch (u)
		{
		case eUnifiedGadget.u_Magnetizer:
			result = eGadgets.g_Magnetizer;
			break;
		case eUnifiedGadget.u_Multiplier:
			result = eGadgets.g_Multiplier;
			break;
		case eUnifiedGadget.u_Reflector:
			result = eGadgets.g_Reflector;
			break;
		case eUnifiedGadget.u_Jetpack:
			result = eGadgets.g_Jetpack;
			break;
		}
		return result;
	}

	public static MegaWeaponManager.eMegaWeapons GetMegaFromUnified(eUnifiedGadget u)
	{
		MegaWeaponManager.eMegaWeapons result = MegaWeaponManager.eMegaWeapons.mw_NONE;
		switch (u)
		{
		case eUnifiedGadget.u_Groovitron:
			result = MegaWeaponManager.eMegaWeapons.mw_Groovitron;
			break;
		case eUnifiedGadget.u_RiftInducer:
			result = MegaWeaponManager.eMegaWeapons.mw_RiftInducer;
			break;
		case eUnifiedGadget.u_Tornado:
			result = MegaWeaponManager.eMegaWeapons.mw_Tornado;
			break;
		}
		return result;
	}
}
