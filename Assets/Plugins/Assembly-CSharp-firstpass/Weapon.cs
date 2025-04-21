using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public string weaponName;

	public uint damage;

	public float fireRate;

	public uint boltConversion;

	public uint AOE;

	public uint AOEDamage;

	public bool spreadShot;

	public uint dotDamagePerSecond;

	public float dotDuration;

	private float curCooldown;

	public uint ammo;

	public int ammoMax;

	private uint curUpgradeLevel;

	public GameObject upgrade0Projectile;

	public GameObject upgrade1Projectile;

	public GameObject upgrade2Projectile;

	public ParticleSystem projHit0;

	public ParticleSystem projHit1;

	public ParticleSystem projHit2;

	public ParticleSystem[] muzzleFlash;

	public ParticleSystem particleOnHit;

	public string spriteName;

	public string LocKeyName;

	public string LocKeyDesc;

	public string LocDescShellType;

	public string LocDescExtra;

	public AudioClip[] Fire;

	public AudioClip[] Hit;

	public AudioClip[] Equip;

	public AudioClip[] Empty;

	public AudioClip[] Reload;

	public string soundOnHit;

	public uint[] upgradeBoltCost = new uint[3];

	public uint[] ammoBoltCost = new uint[3];

	public bool isOwned;

	private void Start()
	{
		LoadWeapData(weaponName, 0u);
		ammo = (uint)ammoMax;
		int num = PlayerPrefs.GetInt(weaponName);
		if (weaponName == "ConstructoPistol")
		{
			num--;
		}
		for (int i = 0; i < num; i++)
		{
			UpgradeWeapon(true);
		}
	}

	public void Update()
	{
		if (curCooldown > 0f)
		{
			curCooldown -= Time.deltaTime;
		}
	}

	public bool TryFireWeapon()
	{
		if (curCooldown <= 0f)
		{
			if (ammo != 0)
			{
				if (Fire.Length > curUpgradeLevel)
				{
					if (weaponName != "RynoM")
					{
						SFXManager.instance.ModulatePitch(Fire[curUpgradeLevel].name, 0.8f, 1.2f, fireRate);
						SFXManager.instance.PlaySound(Fire[curUpgradeLevel].name);
					}
					else
					{
						SFXManager.instance.ModulatePitch("RYNO_fire2", 0.8f, 1.2f, fireRate);
						SFXManager.instance.PlaySound("RYNO_fire2");
						SFXManager.instance.ModulatePitch("RYNO_fire3", 0.8f, 1.2f, fireRate);
						SFXManager.instance.PlaySound("RYNO_fire3");
						SFXManager.instance.ModulatePitch("RYNO_projectile3", 0.8f, 1.2f, fireRate);
						SFXManager.instance.PlaySound("RYNO_projectile3");
						SFXManager.instance.ModulatePitch("RYNO_fire2", 0.8f, 1.2f, fireRate);
						SFXManager.instance.PlaySound("RYNO_fire2");
						SFXManager.instance.ModulatePitch("RYNO_fire3", 0.8f, 1.2f, fireRate);
						SFXManager.instance.PlaySound("RYNO_fire3");
					}
				}
				curCooldown = fireRate;
				DecrementAmmo();
				UIManager.instance.GetHUD().UpdateAmmo();
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalShotsFired);
				switch (weaponName)
				{
				case "ConstructoPistol":
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.pistolShotsFired);
					StatsTracker.instance.SetStat(StatsTracker.Stats.oneShotPistolKills, 0f);
					break;
				case "ConstructoShotgun":
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.shotgunShotsFired);
					StatsTracker.instance.SetStat(StatsTracker.Stats.oneShotShotgunKills, 0f);
					break;
				case "BuzzBlades":
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.buzzbladesShotsFired);
					StatsTracker.instance.SetStat(StatsTracker.Stats.oneShotBuzzbladeKills, 0f);
					break;
				case "PredatorLauncher":
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.predatorShotsFired);
					StatsTracker.instance.SetStat(StatsTracker.Stats.oneShotPredatorKills, 0f);
					break;
				case "RynoM":
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.rynoShotsFired);
					break;
				default:
					Debug.LogError("Weapon: Unknown weapon");
					break;
				}
				return true;
			}
			if (Fire.Length > curUpgradeLevel)
			{
				SFXManager.instance.PlaySound(Empty[curUpgradeLevel].name);
			}
		}
		return false;
	}

	public void FireProjectile(Vector3 target, EnemyController targetEnemy = null)
	{
		GameObject gameObject = null;
		switch (curUpgradeLevel)
		{
		case 0u:
			gameObject = GameObjectPool.instance.GetNextFree(upgrade0Projectile.name, true);
			break;
		case 1u:
			gameObject = GameObjectPool.instance.GetNextFree(upgrade1Projectile.name, true);
			break;
		case 2u:
			gameObject = GameObjectPool.instance.GetNextFree(upgrade2Projectile.name, true);
			break;
		}
		gameObject.GetComponent<RatchetProjectile>().weap = this;
		gameObject.GetComponent<RatchetProjectile>().ecTarget = targetEnemy;
		gameObject.GetComponent<RatchetProjectile>().SetProjectileData(target);
		if (weaponName == "BuzzBlades")
		{
			if (targetEnemy != null)
			{
				gameObject.GetComponent<RatchetProjectile>().bounces = WeaponsManager.instance.maxBuzzBounces;
				if (ammo > 2)
				{
					StartCoroutine("BurstFireEnemyDelay", targetEnemy);
				}
			}
			else if (ammo > 2)
			{
				StartCoroutine("BurstFireDistanceDelay", target);
			}
		}
		if (Reload.Length > curUpgradeLevel)
		{
			SFXManager.instance.PlaySound(Reload[curUpgradeLevel].name);
		}
		if (muzzleFlash.Length > curUpgradeLevel && muzzleFlash[curUpgradeLevel] != null)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(muzzleFlash[curUpgradeLevel].name, true);
			nextFree.transform.position = GameController.instance.playerController.rightHand.position;
			nextFree.transform.parent = GameController.instance.playerController.transform;
			nextFree.transform.rotation = GameController.instance.playerController.rightHand.rotation;
			nextFree.GetComponent<ParticleSystem>().Play();
		}
	}

	private IEnumerator BurstFireDistanceDelay(Vector3 target)
	{
		GameObject projectile = null;
		yield return new WaitForSeconds(WeaponsManager.instance.timeBetweenBuzzBlades);
		switch (curUpgradeLevel)
		{
		case 0u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade0Projectile.name, true);
			break;
		case 1u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade1Projectile.name, true);
			break;
		case 2u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade2Projectile.name, true);
			break;
		}
		projectile.GetComponent<RatchetProjectile>().weap = this;
		projectile.GetComponent<RatchetProjectile>().ecTarget = null;
		projectile.GetComponent<RatchetProjectile>().SetProjectileData(target);
		DecrementAmmo();
		UIManager.instance.GetHUD().UpdateAmmo();
		yield return new WaitForSeconds(WeaponsManager.instance.timeBetweenBuzzBlades);
		switch (curUpgradeLevel)
		{
		case 0u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade0Projectile.name, true);
			break;
		case 1u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade1Projectile.name, true);
			break;
		case 2u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade2Projectile.name, true);
			break;
		}
		projectile.GetComponent<RatchetProjectile>().weap = this;
		projectile.GetComponent<RatchetProjectile>().ecTarget = null;
		projectile.GetComponent<RatchetProjectile>().SetProjectileData(target);
		DecrementAmmo();
		UIManager.instance.GetHUD().UpdateAmmo();
	}

	private IEnumerator BurstFireEnemyDelay(EnemyController targetEnemy)
	{
		GameObject projectile = null;
		yield return new WaitForSeconds(WeaponsManager.instance.timeBetweenBuzzBlades);
		switch (curUpgradeLevel)
		{
		case 0u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade0Projectile.name, true);
			break;
		case 1u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade1Projectile.name, true);
			break;
		case 2u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade2Projectile.name, true);
			break;
		}
		projectile.GetComponent<RatchetProjectile>().weap = this;
		projectile.GetComponent<RatchetProjectile>().ecTarget = targetEnemy;
		projectile.GetComponent<RatchetProjectile>().SetProjectileData(targetEnemy.GetComponent<Rigidbody>().position);
		projectile.GetComponent<RatchetProjectile>().bounces = WeaponsManager.instance.maxBuzzBounces;
		DecrementAmmo();
		UIManager.instance.GetHUD().UpdateAmmo();
		yield return new WaitForSeconds(WeaponsManager.instance.timeBetweenBuzzBlades);
		switch (curUpgradeLevel)
		{
		case 0u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade0Projectile.name, true);
			break;
		case 1u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade1Projectile.name, true);
			break;
		case 2u:
			projectile = GameObjectPool.instance.GetNextFree(upgrade2Projectile.name, true);
			break;
		}
		projectile.GetComponent<RatchetProjectile>().weap = this;
		projectile.GetComponent<RatchetProjectile>().ecTarget = targetEnemy;
		projectile.GetComponent<RatchetProjectile>().SetProjectileData(targetEnemy.GetComponent<Rigidbody>().position);
		projectile.GetComponent<RatchetProjectile>().bounces = WeaponsManager.instance.maxBuzzBounces;
		DecrementAmmo();
		UIManager.instance.GetHUD().UpdateAmmo();
	}

	public void FireFakeProjectile(Vector3 target)
	{
		GameObject gameObject = null;
		switch (curUpgradeLevel)
		{
		case 0u:
			gameObject = GameObjectPool.instance.GetNextFree(upgrade0Projectile.name, true);
			break;
		case 1u:
			gameObject = GameObjectPool.instance.GetNextFree(upgrade1Projectile.name, true);
			break;
		case 2u:
			gameObject = GameObjectPool.instance.GetNextFree(upgrade2Projectile.name, true);
			break;
		}
		gameObject.GetComponent<RatchetProjectile>().weap = this;
		gameObject.GetComponent<RatchetProjectile>().ecTarget = null;
		gameObject.GetComponent<RatchetProjectile>().SetProjectileData(target);
	}

	public void UpgradeWeapon(bool init = false)
	{
		if (isOwned && weaponName.CompareTo("RynoM") != 0)
		{
			if (curUpgradeLevel < 2)
			{
				curUpgradeLevel++;
				LoadWeapData(weaponName, curUpgradeLevel);
				if (!init)
				{
					switch (weaponName)
					{
					case "ConstructoPistol":
						StatsTracker.instance.UpdateStat(StatsTracker.Stats.pistolUpgraded);
						break;
					case "ConstructoShotgun":
						StatsTracker.instance.UpdateStat(StatsTracker.Stats.shotgunUpgraded);
						break;
					case "BuzzBlades":
						StatsTracker.instance.UpdateStat(StatsTracker.Stats.buzzbladesUpgraded);
						break;
					case "PredatorLauncher":
						StatsTracker.instance.UpdateStat(StatsTracker.Stats.predatorUpgraded);
						break;
					default:
						Debug.LogError("Weapon: Unknown weapon");
						break;
					}
					PlayerPrefs.SetInt(weaponName, (int)(curUpgradeLevel + 1));
					if (curUpgradeLevel == 2)
					{
						StatsTracker.instance.UpdateStat(StatsTracker.Stats.weaponsUpgraded);
					}
				}
			}
		}
		else
		{
			isOwned = true;
			if (!init)
			{
				PlayerPrefs.SetInt(weaponName, 1);
			}
		}
		if (weaponName.CompareTo("RynoM") == 0)
		{
			UIHUD hUD = UIManager.instance.GetHUD();
			if (hUD == null)
			{
				StatsTracker.instance.SaveStatsAndReset();
			}
		}
		else
		{
			StatsTracker.instance.SaveStatsAndReset();
		}
	}

	public uint GetWeaponUpgradeLevel()
	{
		return curUpgradeLevel + 1;
	}

	public void FireTrail(Vector3 target, bool isHit)
	{
		Vector3 position = GameController.instance.playerController.transform.position;
		Vector3 vector = target - GameController.instance.playerController.transform.position;
		float num = vector.magnitude / 20f;
		vector.Normalize();
		Vector3 zero = Vector3.zero;
		Color32 color = ((!isHit) ? new Color32(0, 0, byte.MaxValue, byte.MaxValue) : new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
		for (int i = 0; i < 20; i++)
		{
			GameController.instance.playerController.effectParticles.Emit(position, zero, 1f, 2f, color);
			position += vector * num;
		}
	}

	private void SetParticleOnHit(uint upgradeLevel)
	{
		switch (upgradeLevel)
		{
		case 0u:
			particleOnHit = projHit0;
			break;
		case 1u:
			particleOnHit = projHit1;
			break;
		case 2u:
			particleOnHit = projHit2;
			break;
		}
		if (Hit != null && Hit.Length > upgradeLevel)
		{
			soundOnHit = Hit[upgradeLevel].name;
		}
	}

	public void LoadWeapData(string WeaponName, uint upgradeLevel = 0)
	{
		int num = 0;
		cmlReader cmlReader2 = new cmlReader("Data/WeaponsData");
		SetParticleOnHit(upgradeLevel);
		if (cmlReader2 == null)
		{
			return;
		}
		List<cmlData> list = cmlReader2.Children();
		foreach (cmlData item in list)
		{
			if (item["Name"] == WeaponName)
			{
				spriteName = item["SpriteName"];
				LocKeyName = item["LocKeyName"];
				LocKeyDesc = item["LocKeyDesc"];
				List<cmlData> list2 = cmlReader2.Children(item.ID);
				for (int i = 0; i < 3; i++)
				{
					upgradeBoltCost[i] = uint.Parse(item["BoltCost" + i]);
				}
				{
					foreach (cmlData item2 in list2)
					{
						if (uint.Parse(item2["UpgradeLevel"]) == upgradeLevel)
						{
							curUpgradeLevel = upgradeLevel;
							damage = uint.Parse(item2["damage"]);
							fireRate = float.Parse(item2["fireRate"]);
							boltConversion = uint.Parse(item2["boltConversion"]);
							AOE = uint.Parse(item2["AOE"]);
							AOEDamage = uint.Parse(item2["AOEDamage"]);
							spreadShot = bool.Parse(item2["spreadShot"]);
							dotDamagePerSecond = uint.Parse(item2["dotDamagePerSecond"]);
							dotDuration = float.Parse(item2["dotDuration"]);
							ammoBoltCost[curUpgradeLevel] = uint.Parse(item2["AmmoCost"]);
							LocDescShellType = item2["LocDescShellType"];
							LocDescExtra = item2["LocDescExtra"];
							WeaponsManager.instance.WeaponStatsDamageMax = Mathf.Max(WeaponsManager.instance.WeaponStatsDamageMax, damage);
							WeaponsManager.instance.WeaponStatsDamageMin = Mathf.Min(WeaponsManager.instance.WeaponStatsDamageMin, damage);
							WeaponsManager.instance.WeaponStatsROFMax = Mathf.Max(WeaponsManager.instance.WeaponStatsROFMax, fireRate);
							WeaponsManager.instance.WeaponStatsROFMin = Mathf.Min(WeaponsManager.instance.WeaponStatsROFMin, fireRate);
							break;
						}
					}
					break;
				}
			}
			num++;
		}
	}

	public void DecrementAmmo()
	{
		if (ammo != 0)
		{
			ammo--;
		}
	}
}
