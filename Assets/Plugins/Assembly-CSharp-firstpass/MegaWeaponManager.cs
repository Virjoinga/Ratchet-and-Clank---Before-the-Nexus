using System.Collections.Generic;
using UnityEngine;

public class MegaWeaponManager : MonoBehaviour
{
	public enum eMegaWeapons
	{
		mw_Groovitron = 0,
		mw_RiftInducer = 1,
		mw_Tornado = 2,
		mw_MAX = 3,
		mw_NONE = 4
	}

	public const string groovitronName = "PIK_Frame_Groovitron";

	public const string riftInducerName = "PIK_Frame_RiftInducer";

	public const string tornadoName = "PIK_Frame_TornadoLauncher";

	public static MegaWeaponManager instance;

	private List<GameObject> obstaclesToConsiderForMegaWeaponSpawning = new List<GameObject>();

	private int numTilesSinceLastMegaWeapon;

	public int numMegaWeapons = 3;

	public int minTilesBeforeMegaWeapon = 3;

	public int maxMegaWeaponLevels = 4;

	private int[] megaWeaponLevels;

	public eMegaWeapons megaWeaponState = eMegaWeapons.mw_NONE;

	public float maxDropPercentage = 30f;

	private float groovitronDropPercentage;

	private float riftDropPercentage;

	private float tornadoDropPercentage;

	public bool groovitronOn;

	public bool riftInducerOn;

	public bool tornadoOn;

	protected float curMegaDuration;

	protected GameObject currentMegaEffect;

	public GameObject curMegaObject;

	protected Vector3 effectPosition = Vector3.zero;

	protected Vector3 objectPosition = Vector3.zero;

	protected float effectRotateSpeed;

	public string[] MegaWeaponLocKeys;

	public string[] MegaWeaponSpriteNames;

	protected MusicManager.eMusicTrackType previousTrack = MusicManager.eMusicTrackType.None;

	public ParticleSystem groovitronSystem;

	public float[] groovitronDuration;

	public float groovitronRotateSpeed = 3f;

	public ParticleSystem riftClouds;

	public GameObject riftTentacles;

	public float[] riftDuration;

	public ParticleSystem tornadoSpin;

	public float[] tornadoDuration;

	public float[] tornadoBoltPickupRange;

	public float tornadoRotateSpeed = 3f;

	public void Validate()
	{
		float num = maxDropPercentage * 3f;
		if (num > 100f)
		{
			Debug.Log("ERROR: megaWeapons are using more than 100% drop percentage!");
		}
		if (groovitronDuration.Length != maxMegaWeaponLevels)
		{
			Debug.LogWarning("groovitronDuration array is not the size of the maxMegaWeaponLevels!");
		}
		if (riftDuration.Length != maxMegaWeaponLevels)
		{
			Debug.LogWarning("riftDuration array is not the size of the maxMegaWeaponLevels!");
		}
		if (tornadoDuration.Length != maxMegaWeaponLevels)
		{
			Debug.LogWarning("tornadoDuration array is not the size of the maxMegaWeaponLevels!");
		}
		if (tornadoBoltPickupRange.Length != maxMegaWeaponLevels)
		{
			Debug.LogWarning("tornadoBoltPickupRange array is not the size of the maxMegaWeaponLevels!");
		}
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("MegaWeaponManager: Multiple instances spawned");
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		megaWeaponLevels = new int[3];
		for (int i = 0; i < megaWeaponLevels.Length; i++)
		{
			megaWeaponLevels[i] = 0;
			int @int = PlayerPrefs.GetInt(((eMegaWeapons)i).ToString());
			for (int j = 0; j < @int; j++)
			{
				UpgradeMegaWeapon((eMegaWeapons)i, true);
			}
		}
	}

	private void Start()
	{
		Validate();
	}

	private void Update()
	{
		if (curMegaDuration > 0f)
		{
			curMegaDuration -= Time.deltaTime;
			AttemptFlicker(curMegaDuration);
		}
		else if (megaWeaponState != eMegaWeapons.mw_NONE)
		{
			DeactivateMegaWeapon(true);
		}
	}

	public bool HaveBoughtMegaWeapon(eMegaWeapons mw_MegaWeapon)
	{
		switch (mw_MegaWeapon)
		{
		case eMegaWeapons.mw_Groovitron:
			if (groovitronDropPercentage > 0f)
			{
				return true;
			}
			break;
		case eMegaWeapons.mw_RiftInducer:
			if (riftDropPercentage > 0f)
			{
				return true;
			}
			break;
		case eMegaWeapons.mw_Tornado:
			if (tornadoDropPercentage > 0f)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public int GetMegaWeaponLevel(eMegaWeapons mw_MegaWeapon)
	{
		switch (mw_MegaWeapon)
		{
		case eMegaWeapons.mw_Groovitron:
			return megaWeaponLevels[0] + 1;
		case eMegaWeapons.mw_RiftInducer:
			return megaWeaponLevels[1] + 1;
		case eMegaWeapons.mw_Tornado:
			return megaWeaponLevels[2] + 1;
		default:
			Debug.LogWarning("GetMegaWeaponLevel: MegaWeapon Not Found");
			return 0;
		}
	}

	public void UpgradeMegaWeapon(eMegaWeapons mw_MegaWeapon, bool init = false)
	{
		bool flag = false;
		int value = 0;
		switch (mw_MegaWeapon)
		{
		case eMegaWeapons.mw_Groovitron:
			if (groovitronDropPercentage == 0f)
			{
				groovitronDropPercentage = maxDropPercentage;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsBought);
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.groovitronUpgraded);
				}
				flag = true;
			}
			else if (megaWeaponLevels[0] < maxMegaWeaponLevels - 1)
			{
				megaWeaponLevels[0]++;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.groovitronUpgraded);
				}
				flag = true;
			}
			value = (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.groovitronUpgraded);
			break;
		case eMegaWeapons.mw_RiftInducer:
			if (riftDropPercentage == 0f)
			{
				riftDropPercentage = maxDropPercentage;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsBought);
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.riftInducerUpgraded);
				}
				flag = true;
			}
			else if (megaWeaponLevels[1] < maxMegaWeaponLevels - 1)
			{
				megaWeaponLevels[1]++;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.riftInducerUpgraded);
				}
				flag = true;
			}
			value = (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.riftInducerUpgraded);
			break;
		case eMegaWeapons.mw_Tornado:
			if (tornadoDropPercentage == 0f)
			{
				tornadoDropPercentage = maxDropPercentage;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsBought);
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.tornadoLauncherUpgraded);
				}
				flag = true;
			}
			else if (megaWeaponLevels[2] < maxMegaWeaponLevels - 1)
			{
				megaWeaponLevels[2]++;
				if (!init)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.tornadoLauncherUpgraded);
				}
				flag = true;
			}
			value = (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.tornadoLauncherUpgraded);
			break;
		default:
			Debug.LogWarning("UpgradeGadget: Gadget Not Found");
			break;
		}
		if (flag && !init)
		{
			PlayerPrefs.SetInt(mw_MegaWeapon.ToString(), value);
			if (megaWeaponLevels[(int)mw_MegaWeapon] >= maxMegaWeaponLevels - 1)
			{
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsUpgraded);
			}
		}
	}

	public void HandleMegaWeaponSpawning()
	{
		numTilesSinceLastMegaWeapon++;
		if (GameController.instance.gameState != GameController.eGameState.GS_OnGround || numTilesSinceLastMegaWeapon <= minTilesBeforeMegaWeapon)
		{
			return;
		}
		numTilesSinceLastMegaWeapon = 0;
		int num = Random.Range(0, 100);
		float num2 = groovitronDropPercentage / (float)maxMegaWeaponLevels * (float)(megaWeaponLevels[0] + 1);
		float num3 = tornadoDropPercentage / (float)maxMegaWeaponLevels * (float)(megaWeaponLevels[2] + 1);
		float num4 = riftDropPercentage / (float)maxMegaWeaponLevels * (float)(megaWeaponLevels[1] + 1);
		switch (Random.Range(0, 3))
		{
		case 0:
			if ((float)num < num2)
			{
				spawnMegaWeapon("PIK_Frame_Groovitron");
			}
			break;
		case 1:
			if ((float)num < num3)
			{
				spawnMegaWeapon("PIK_Frame_TornadoLauncher");
			}
			break;
		default:
			if ((float)num < num4)
			{
				spawnMegaWeapon("PIK_Frame_RiftInducer");
			}
			break;
		}
	}

	private void spawnMegaWeapon(string name)
	{
		obstaclesToConsiderForMegaWeaponSpawning.Clear();
		bool flag = false;
		float num = TileSpawnManager.instance.worldPieceSize / (float)ObstacleManager.instance.tileSpawnSegments;
		float lastRileTileOffset = TileSpawnManager.instance.getLastRileTileOffset();
		lastRileTileOffset += (0f - TileSpawnManager.instance.worldPieceSize) / 2f;
		int num2 = 0;
		float num3 = lastRileTileOffset;
		Vector3 newPosition = Vector3.zero;
		Vector3 newDirection = Vector3.zero;
		while (!flag && num2 < 10)
		{
			int num4 = Random.Range(0, 10) % 3;
			int num5 = Random.Range(0, ObstacleManager.instance.getSpawnSegmentsByDist());
			num3 = lastRileTileOffset + num * (float)num5;
			if (ObstacleManager.instance.segmentOccupied[num4, num5])
			{
				continue;
			}
			TileSpawnManager.instance.getSpawnPositionAndDirection(num4, num3, ref newPosition, ref newDirection);
			if (newPosition != Vector3.zero)
			{
				GameObject nextFree = GameObjectPool.instance.GetNextFree(name, true);
				if (nextFree != null)
				{
					nextFree.transform.position = newPosition;
					nextFree.transform.forward = newDirection;
					ObstacleManager.instance.segmentOccupied[num4, num5] = true;
					flag = true;
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
		obstaclesToConsiderForMegaWeaponSpawning.Clear();
	}

	public void SetMegaWeaponEffects(string effectName, Vector3 offset, float duration, float rotateSpeed)
	{
		effectRotateSpeed = rotateSpeed;
		effectPosition = offset;
		if (currentMegaEffect == null)
		{
			currentMegaEffect = GameObjectPool.instance.GetNextFree(effectName, true);
			currentMegaEffect.transform.position = GameController.instance.mainCamera.transform.position + offset;
			currentMegaEffect.particleSystem.Play();
		}
		if (curMegaObject != null)
		{
			curMegaObject.transform.position = GameController.instance.mainCamera.transform.position + objectPosition;
		}
		curMegaDuration = duration;
	}

	public Vector3 GetEffectPosition()
	{
		return effectPosition;
	}

	public void ActivateMegaWeapon(eMegaWeapons newState)
	{
		if (megaWeaponState != eMegaWeapons.mw_NONE)
		{
			DeactivateMegaWeapon(newState != megaWeaponState);
		}
		megaWeaponState = newState;
		string empty = string.Empty;
		Vector3 zero = Vector3.zero;
		float duration = 0f;
		float rotateSpeed = 0f;
		switch (megaWeaponState)
		{
		case eMegaWeapons.mw_Groovitron:
			empty = groovitronSystem.name;
			zero.x = 18f;
			zero.y = 2f;
			duration = groovitronDuration[megaWeaponLevels[0]];
			previousTrack = MusicManager.instance.CurrentTrackType;
			MusicManager.instance.Play(MusicManager.eMusicTrackType.Groove, true, 1f);
			rotateSpeed = groovitronRotateSpeed;
			groovitronOn = true;
			StatsTracker.instance.SetStat(StatsTracker.Stats.groovitronCurrentKills, 0f);
			break;
		case eMegaWeapons.mw_RiftInducer:
			empty = riftClouds.name;
			if (curMegaObject == null)
			{
				curMegaObject = GameObjectPool.instance.GetNextFree(riftTentacles.name, true);
			}
			zero.x = 16.5f;
			zero.y = 4f;
			objectPosition.x = 18f;
			objectPosition.y = 4f;
			duration = riftDuration[megaWeaponLevels[1]];
			riftInducerOn = true;
			StatsTracker.instance.SetStat(StatsTracker.Stats.riftInducerCurrentKills, 0f);
			break;
		case eMegaWeapons.mw_Tornado:
			empty = tornadoSpin.name;
			zero.x = 22.5f;
			zero.y = -4.5f;
			objectPosition.x = 22f;
			objectPosition.y = 2f;
			rotateSpeed = tornadoRotateSpeed;
			duration = tornadoDuration[megaWeaponLevels[2]];
			tornadoOn = true;
			SFXManager.instance.PlayUniqueSound("wep_Tornado");
			StatsTracker.instance.SetStat(StatsTracker.Stats.tornadoLauncherCurrentKills, 0f);
			break;
		}
		SetMegaWeaponEffects(empty, zero, duration, rotateSpeed);
	}

	public void DeactivateMegaWeapon(bool destroyObject)
	{
		if (megaWeaponState != eMegaWeapons.mw_NONE)
		{
			EnemyManager.instance.DestroyAll(false);
			curMegaDuration = 0f;
			if (destroyObject)
			{
				currentMegaEffect.particleSystem.Stop();
				GameObjectPool.instance.SetFree(currentMegaEffect, true);
				currentMegaEffect = null;
				if (curMegaObject != null)
				{
					GameObjectPool.instance.SetFree(curMegaObject, true);
					curMegaObject = null;
				}
			}
			if (previousTrack != MusicManager.eMusicTrackType.None)
			{
				MusicManager.instance.Play(previousTrack, false, 1f);
				previousTrack = MusicManager.eMusicTrackType.None;
			}
			groovitronOn = false;
			riftInducerOn = false;
			tornadoOn = false;
			SFXManager.instance.StopSound("wep_Tornado");
		}
		megaWeaponState = eMegaWeapons.mw_NONE;
	}

	public float GetTornadoPickupRange()
	{
		if (megaWeaponState == eMegaWeapons.mw_Tornado)
		{
			return tornadoBoltPickupRange[megaWeaponLevels[2]];
		}
		return 0f;
	}

	public Vector3 GetMegaWeaponPosition()
	{
		if (megaWeaponState != eMegaWeapons.mw_NONE)
		{
			return currentMegaEffect.transform.position;
		}
		return Vector3.zero;
	}

	public void AttemptFlicker(float timeRemaining)
	{
		if (timeRemaining > 1f)
		{
			AttemptPlay();
		}
		else if (timeRemaining > 0.9f)
		{
			AttemptStop();
		}
		else if (timeRemaining > 0.8f)
		{
			AttemptPlay();
		}
		else if (timeRemaining > 0.7f)
		{
			AttemptStop();
		}
		else if (timeRemaining > 0.6f)
		{
			AttemptPlay();
		}
		else if (timeRemaining > 0.5f)
		{
			AttemptStop();
		}
		else if (timeRemaining > 0.4f)
		{
			AttemptPlay();
		}
		else if (timeRemaining > 0.3f)
		{
			AttemptStop();
		}
		else if (timeRemaining > 0.2f)
		{
			AttemptPlay();
		}
		else if (timeRemaining > 0.1f)
		{
			AttemptStop();
		}
		else
		{
			AttemptPlay();
		}
	}

	private void AttemptStop()
	{
		if (curMegaObject != null)
		{
			Vector3 position = GameController.instance.mainCamera.transform.position + objectPosition;
			position.y = -100f;
			curMegaObject.transform.position = position;
		}
		if (currentMegaEffect != null)
		{
			Vector3 position2 = GameController.instance.mainCamera.transform.position + effectPosition;
			position2.y = -100f;
			currentMegaEffect.transform.position = position2;
		}
	}

	private void AttemptPlay()
	{
		if (curMegaObject != null && curMegaObject.transform.position.y <= -90f)
		{
			curMegaObject.transform.position = GameController.instance.mainCamera.transform.position + objectPosition;
		}
		if (currentMegaEffect != null && currentMegaEffect.transform.position.y <= -90f)
		{
			currentMegaEffect.transform.position = GameController.instance.mainCamera.transform.position + effectPosition;
		}
		if (curMegaObject != null)
		{
			curMegaObject.transform.position = Vector3.Lerp(curMegaObject.transform.position, GameController.instance.mainCamera.transform.position + objectPosition, Time.deltaTime * 30f);
		}
		if (currentMegaEffect != null)
		{
			if (megaWeaponState == eMegaWeapons.mw_Tornado)
			{
				currentMegaEffect.transform.position = TileSpawnManager.instance.getRailNodePosition(0, GameController.instance.mainCamera.transform.position.x + effectPosition.x, TileSpawnManager.instance.railTileList[0]);
			}
			else
			{
				currentMegaEffect.transform.position = Vector3.Lerp(currentMegaEffect.transform.position, GameController.instance.mainCamera.transform.position + effectPosition, Time.deltaTime * 30f);
			}
			currentMegaEffect.transform.RotateAround(Vector3.up, effectRotateSpeed * Time.deltaTime);
		}
	}
}
