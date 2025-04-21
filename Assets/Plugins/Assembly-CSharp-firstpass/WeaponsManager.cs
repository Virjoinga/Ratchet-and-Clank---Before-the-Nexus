using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
	public enum WeaponList
	{
		WEP_ConstructoPistol = 0,
		WEP_ConstructoShotgun = 1,
		WEP_BuzzBlades = 2,
		WEP_PredatorLauncher = 3,
		WEP_RynoM = 4
	}

	private class RaycastHitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit a, RaycastHit b)
		{
			if (a.distance < b.distance)
			{
				return -1;
			}
			if (a.distance > b.distance)
			{
				return 1;
			}
			return 0;
		}
	}

	public const uint MaxWeaps = 5u;

	public static WeaponsManager instance;

	public uint curWeapIndex;

	public GameObject[] WeapInventory;

	private bool weapHidden;

	public bool FireOnMissing;

	public int[] extraShotgunShots;

	public int extraRhynoShots = 6;

	public Vector3[] spreadRandOffset;

	public float[] shotgunDamageFalloffPerUnit;

	public int maxBuzzBounces = 1;

	public float delayTillBounce = 1f;

	public float WeaponStatsDamageMax;

	public float WeaponStatsDamageMin;

	public float WeaponStatsROFMax;

	public float WeaponStatsROFMin;

	private bool[] weaponUsedThisRun;

	public float buzzBladeTimeToHit = 0.4f;

	public float predatorTimeToHit = 0.4f;

	public float timeBetweenBuzzBlades = 0.03f;

	public float startAngle = 15f;

	public float maxAngle = 75f;

	public float deltaAngle = 25f;

	public float shotgunWallDistance = 45f;

	public float curAngle;

	private List<EnemyController> predatorLock;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("WeaponsManager: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		SetUpWeaponsSystem();
		weaponUsedThisRun = new bool[5];
		for (int i = 0; i < 5; i++)
		{
			weaponUsedThisRun[i] = false;
		}
	}

	private void Start()
	{
		predatorLock = new List<EnemyController>();
		curAngle = startAngle;
	}

	public void SetUpWeaponsSystem()
	{
		WeapInventory = new GameObject[5];
		WeapInventory[0] = GameObjectPool.instance.GetNextFree("WEP_ConstructoPistol");
		WeapInventory[1] = GameObjectPool.instance.GetNextFree("WEP_ConstructoShotgun");
		WeapInventory[2] = GameObjectPool.instance.GetNextFree("WEP_BuzzBlades");
		WeapInventory[3] = GameObjectPool.instance.GetNextFree("WEP_PredatorLauncher");
		WeapInventory[4] = GameObjectPool.instance.GetNextFree("WEP_RynoM");
		WeapInventory[0].GetComponent<Renderer>().enabled = false;
		WeapInventory[1].GetComponent<Renderer>().enabled = false;
		WeapInventory[2].GetComponent<Renderer>().enabled = false;
		WeapInventory[3].GetComponent<Renderer>().enabled = false;
		WeapInventory[4].GetComponent<Renderer>().enabled = false;
		WeapInventory[curWeapIndex].transform.parent = GameController.instance.playerController.rightHand;
		WeapInventory[curWeapIndex].transform.localPosition = Vector3.zero;
		WeapInventory[curWeapIndex].transform.localRotation = Quaternion.identity;
		WeapInventory[0].GetComponent<Weapon>().isOwned = true;
		GameController.instance.playerController.myWeap = WeapInventory[curWeapIndex].GetComponent<Weapon>();
		if (StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.terachnoidsCollected) >= (float)TileSpawnManager.instance.terachnoidsMax)
		{
			WeapInventory[4].GetComponent<Weapon>().isOwned = true;
		}
	}

	public void SwapWeapons(uint index, uint upgradeLevel)
	{
		if (index != curWeapIndex && index <= 4)
		{
			WeapInventory[curWeapIndex].transform.parent = null;
			WeapInventory[curWeapIndex].transform.localPosition = Vector3.zero;
			WeapInventory[curWeapIndex].GetComponent<Renderer>().enabled = false;
			if (!weapHidden)
			{
				WeapInventory[index].transform.parent = GameController.instance.playerController.rightHand;
				WeapInventory[index].transform.localPosition = Vector3.zero;
				WeapInventory[index].transform.localRotation = Quaternion.identity;
			}
			GameController.instance.playerController.myWeap = WeapInventory[index].GetComponent<Weapon>();
			GameController.instance.playerController.myWeap.LoadWeapData(GameController.instance.playerController.myWeap.weaponName, upgradeLevel);
			GameController.instance.playerController.myWeap.GetComponent<Renderer>().enabled = true;
			curWeapIndex = index;
			if (!weaponUsedThisRun[curWeapIndex])
			{
				weaponUsedThisRun[curWeapIndex] = true;
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.weaponsUsed);
			}
		}
	}

	public void OnRunStart()
	{
		StatsTracker.instance.SetStat(StatsTracker.Stats.weaponsUsed, 0f);
		for (int i = 0; i < 5; i++)
		{
			weaponUsedThisRun[i] = false;
		}
		weaponUsedThisRun[curWeapIndex] = true;
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.weaponsUsed);
	}

	public void HideWeapon(bool hide)
	{
		if (curWeapIndex < 5 && WeapInventory != null && WeapInventory[curWeapIndex] != null)
		{
			if (hide)
			{
				WeapInventory[curWeapIndex].transform.parent = null;
				WeapInventory[curWeapIndex].transform.localPosition = Vector3.zero;
			}
			else
			{
				WeapInventory[curWeapIndex].transform.parent = GameController.instance.playerController.rightHand;
				WeapInventory[curWeapIndex].transform.localPosition = Vector3.zero;
				WeapInventory[curWeapIndex].transform.localRotation = Quaternion.identity;
			}
			WeapInventory[curWeapIndex].GetComponent<Renderer>().enabled = !hide;
			weapHidden = hide;
		}
	}

	public bool HaveBoughtWeapon(WeaponList w_Weap)
	{
		return WeapInventory[(int)w_Weap].GetComponent<Weapon>().isOwned;
	}

	public void UpgradeWeapon(uint index)
	{
		WeapInventory[index].GetComponent<Weapon>().UpgradeWeapon();
	}

	public bool FireWeapon(Vector3 target)
	{
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			if (curWeapIndex == 4 && WeapInventory[curWeapIndex].GetComponent<Weapon>().ammo != 0)
			{
				EnemyManager.instance.DestroyAll(true);
			}
			Ray ray = Camera.main.ScreenPointToRay(target);
			RaycastHit[] array = Physics.RaycastAll(ray);
			Array.Sort(array, new RaycastHitComparer());
			MoveAim(target);
			if (array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].collider != null && InitialRayCheck(ray, array[i]))
					{
						ray.origin = GameController.instance.playerController.rightHand.position;
						ray.direction = array[i].point - ray.origin;
						array = Physics.RaycastAll(ray);
						Array.Sort(array, new RaycastHitComparer());
					}
				}
			}
			if (array.Length != 0)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].collider != null && FireAtCollider(ray, array[j]))
					{
						ExtraShotsFire(target);
						if (curWeapIndex == 1)
						{
							FireConeBlast(ray, array[j].transform.position);
						}
						return true;
					}
				}
			}
			if (FireOnMissing && FireDownDirection(ray.origin, ray.direction, 200f))
			{
				ExtraShotsFire(target);
				if (curWeapIndex == 1)
				{
					FireConeBlast(ray, Vector3.zero);
				}
				return true;
			}
		}
		return false;
	}

	private bool FireAtRigidBody(Ray ray, RaycastHit hit)
	{
		EnemyController component = hit.rigidbody.gameObject.GetComponent<EnemyController>();
		if (component != null)
		{
			if (GameController.instance.playerController.myWeap.TryFireWeapon())
			{
				component.EnemyTapped(GameController.instance.playerController.myWeap);
				return true;
			}
		}
		else if (FireOnMissing)
		{
			return FireDownDirection(ray.origin, ray.direction, hit.distance);
		}
		return false;
	}

	private bool InitialRayCheck(Ray ray, RaycastHit hit)
	{
		EnemyController enemyController = null;
		if (hit.collider.gameObject.transform.parent != null)
		{
			enemyController = hit.collider.gameObject.transform.parent.gameObject.GetComponent<EnemyController>();
		}
		Crate component = hit.collider.gameObject.GetComponent<Crate>();
		TileInfo component2 = hit.collider.gameObject.GetComponent<TileInfo>();
		if (component2 != null)
		{
			return true;
		}
		if (enemyController != null)
		{
			return true;
		}
		if (component != null)
		{
			return true;
		}
		return false;
	}

	private bool FireAtCollider(Ray ray, RaycastHit hit)
	{
		EnemyController enemyController = null;
		if (hit.collider.gameObject.transform.parent != null)
		{
			enemyController = hit.collider.gameObject.transform.parent.gameObject.GetComponent<EnemyController>();
		}
		Crate component = hit.collider.gameObject.GetComponent<Crate>();
		TileInfo component2 = hit.collider.gameObject.GetComponent<TileInfo>();
		if (component2 != null)
		{
			if (FireOnMissing)
			{
				return FireDownDirection(ray.origin, ray.direction, hit.distance);
			}
			return true;
		}
		if (enemyController != null)
		{
			if (GameController.instance.playerController.myWeap.TryFireWeapon())
			{
				if (curWeapIndex != 1)
				{
					enemyController.EnemyTapped(GameController.instance.playerController.myWeap);
				}
				return true;
			}
		}
		else if (component != null && GameController.instance.playerController.myWeap.TryFireWeapon())
		{
			component.CrateTapped(GameController.instance.playerController.myWeap);
			return true;
		}
		return false;
	}

	private bool FireDownDirection(Vector3 Origin, Vector3 Dir, float Dist)
	{
		Vector3 target = Origin + Dir * Dist;
		if (Dist >= 15f && GameController.instance.playerController.myWeap.TryFireWeapon())
		{
			GameController.instance.playerController.myWeap.FireProjectile(target);
			return true;
		}
		return false;
	}

	private void RandomSpreadFire(Vector3 target, int numExtraShots)
	{
		for (int i = 0; i < numExtraShots; i++)
		{
			Ray ray = Camera.main.ScreenPointToRay(target);
			Vector3 targetPos = ray.origin + ray.direction * 200f;
			targetPos = SpreadOffset(targetPos);
			GameController.instance.playerController.myWeap.FireFakeProjectile(targetPos);
		}
	}

	private Vector3 SpreadOffset(Vector3 targetPos)
	{
		targetPos.x += Random.Range(0f - spreadRandOffset[WeapInventory[curWeapIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() - 1].x, spreadRandOffset[WeapInventory[curWeapIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() - 1].x);
		targetPos.y += Random.Range(0f - spreadRandOffset[WeapInventory[curWeapIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() - 1].y, spreadRandOffset[WeapInventory[curWeapIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() - 1].y);
		targetPos.z += Random.Range(0f - spreadRandOffset[WeapInventory[curWeapIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() - 1].z, spreadRandOffset[WeapInventory[curWeapIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() - 1].z);
		return targetPos;
	}

	private void ExtraShotsFire(Vector3 targetPos)
	{
		if (curWeapIndex == 1)
		{
			if (extraShotgunShots != null && extraShotgunShots.Length > WeapInventory[curWeapIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() - 1)
			{
				RandomSpreadFire(targetPos, extraShotgunShots[WeapInventory[curWeapIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() - 1]);
			}
		}
		else if (curWeapIndex == 4)
		{
			RandomSpreadFire(targetPos, extraRhynoShots);
		}
	}

	public bool HoldSpecialWeapon(Vector3 target)
	{
		switch ((WeaponList)curWeapIndex)
		{
		case WeaponList.WEP_ConstructoPistol:
			if (FireWeapon(target))
			{
				GameController.instance.playerController.RatchetFireAnim();
				return true;
			}
			break;
		case WeaponList.WEP_ConstructoShotgun:
			if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
			{
				UpdateFireAngle();
				MoveAim(target);
				UIHUD hUD = UIManager.instance.GetHUD();
				if (hUD != null)
				{
					hUD.UpdateShotgunCrosshairs(GameController.instance.playerController.lastTouchPos, curAngle);
				}
			}
			break;
		case WeaponList.WEP_BuzzBlades:
			if (FireWeapon(target))
			{
				GameController.instance.playerController.RatchetFireAnim();
				return true;
			}
			break;
		case WeaponList.WEP_PredatorLauncher:
			MoveAim(target);
			if (predatorLock.Count < 4 && WeapInventory[curWeapIndex].GetComponent<Weapon>().ammo > predatorLock.Count)
			{
				return LockOnCheck(target);
			}
			break;
		case WeaponList.WEP_RynoM:
			if (FireWeapon(target))
			{
				GameController.instance.playerController.RatchetFireAnim();
				return true;
			}
			break;
		}
		return false;
	}

	private void MoveAim(Vector3 target)
	{
		Ray ray = Camera.main.ScreenPointToRay(target);
		GameController.instance.playerController.AdjustAim(ray.origin + ray.direction * 25f);
	}

	public bool EndHoldFire(Vector3 target)
	{
		switch ((WeaponList)curWeapIndex)
		{
		case WeaponList.WEP_ConstructoPistol:
			if (FireWeapon(target))
			{
				GameController.instance.playerController.RatchetFireAnim();
				return true;
			}
			break;
		case WeaponList.WEP_ConstructoShotgun:
		{
			UIHUD hUD = UIManager.instance.GetHUD();
			if (hUD != null)
			{
				hUD.HideShotgunCrosshairs();
			}
			if (FireWeapon(target))
			{
				GameController.instance.playerController.RatchetFireAnim();
				return true;
			}
			break;
		}
		case WeaponList.WEP_BuzzBlades:
			if (FireWeapon(target))
			{
				GameController.instance.playerController.RatchetFireAnim();
				return true;
			}
			break;
		case WeaponList.WEP_PredatorLauncher:
			if (predatorLock.Count > 0)
			{
				FireAllRockets();
				return true;
			}
			break;
		case WeaponList.WEP_RynoM:
			if (FireWeapon(target))
			{
				GameController.instance.playerController.RatchetFireAnim();
				return true;
			}
			break;
		}
		return false;
	}

	private bool LockOnCheck(Vector3 target)
	{
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			Ray ray = Camera.main.ScreenPointToRay(target);
			LayerMask layerMask = 1 << LayerMask.NameToLayer("Enemies");
			RaycastHit[] array = Physics.RaycastAll(ray, 300f, layerMask);
			Array.Sort(array, new RaycastHitComparer());
			MoveAim(target);
			if (array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].collider != null && CheckForEnemyLock(ray, array[i]))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool CheckForEnemyLock(Ray ray, RaycastHit hit)
	{
		EnemyController enemyController = null;
		if (hit.collider.gameObject.transform.parent != null)
		{
			enemyController = hit.collider.gameObject.transform.parent.gameObject.GetComponent<EnemyController>();
		}
		TileInfo component = hit.collider.gameObject.GetComponent<TileInfo>();
		if (component != null)
		{
			return false;
		}
		if (enemyController != null)
		{
			return AddLock(enemyController);
		}
		return false;
	}

	private bool AddLock(EnemyController enemy)
	{
		for (int i = 0; i < predatorLock.Count; i++)
		{
			EnemyController enemyController = predatorLock[i];
			if (enemyController.gameObject == enemy.gameObject)
			{
				return false;
			}
		}
		predatorLock.Add(enemy);
		enemy.LockOnStart();
		enemy.gameObject.SendMessage("FlashTint", this, SendMessageOptions.DontRequireReceiver);
		return true;
	}

	public void FireAllRockets()
	{
		for (int i = 0; i < predatorLock.Count; i++)
		{
			EnemyController enemyController = predatorLock[i];
			if (!enemyController.isDead && EnemyManager.instance.DoesEnemyStillExist(enemyController))
			{
				WeapInventory[curWeapIndex].GetComponent<Weapon>().ammo--;
				UIManager.instance.GetHUD().UpdateAmmo();
				enemyController.LockOnStop();
				enemyController.EnemyTapped(GameController.instance.playerController.myWeap);
			}
		}
		predatorLock.Clear();
	}

	private void UpdateFireAngle()
	{
		if (curAngle < maxAngle)
		{
			curAngle += deltaAngle * Time.deltaTime;
		}
		if (curAngle > maxAngle)
		{
			curAngle = maxAngle;
		}
	}

	private void FireConeBlast(Ray ray, Vector3 hitPoint)
	{
		List<GameObject> enemies = EnemyManager.instance.getEnemies();
		float num = curAngle / 2f;
		for (int i = 0; i < enemies.Count; i++)
		{
			GameObject gameObject = enemies[i];
			Vector3 vector = gameObject.GetComponent<Rigidbody>().position - GameController.instance.playerController.rightHand.position;
			Vector3 vector2 = ((!(hitPoint == Vector3.zero)) ? (hitPoint - GameController.instance.playerController.rightHand.position) : (ray.direction * shotgunWallDistance));
			float num2 = Vector3.Dot(vector, vector2);
			float num3 = Mathf.Acos(num2 / (Vector3.Magnitude(vector) * Vector3.Magnitude(vector2)));
			float num4 = 180f / (float)Math.PI * num3;
			if (num4 < num)
			{
				gameObject.GetComponent<EnemyController>().EnemyTapped(GameController.instance.playerController.myWeap);
			}
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("Crates");
		foreach (GameObject gameObject2 in array)
		{
			Vector3 vector3 = gameObject2.transform.position - GameController.instance.playerController.rightHand.position;
			Vector3 vector4 = ((!(hitPoint == Vector3.zero)) ? (hitPoint - GameController.instance.playerController.rightHand.position) : (ray.direction * shotgunWallDistance));
			float num5 = Vector3.Dot(vector3, vector4);
			float num6 = Mathf.Acos(num5 / (Vector3.Magnitude(vector3) * Vector3.Magnitude(vector4)));
			float num7 = 180f / (float)Math.PI * num6;
			if (num7 < num)
			{
				gameObject2.GetComponent<Crate>().CrateTapped(GameController.instance.playerController.myWeap);
			}
		}
		curAngle = startAngle;
	}
}
