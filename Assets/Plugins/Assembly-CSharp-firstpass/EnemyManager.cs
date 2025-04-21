using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
	private const string overrideSentinelString = "Standard";

	public static EnemyManager instance;

	private List<GameObject> Enemies;

	public GameObject Leviathan;

	public Vector2 axiSafeZone;

	public Vector2 polySafeZone;

	public Vector2 terSafeZone;

	public bool shouldHavePrimaryEnemy;

	private bool primaryEnemySet;

	public string enemyOverride = "Standard";

	public List<GameObject> getEnemies()
	{
		return Enemies;
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("EnemyManager: Multiple instances spawned");
			Object.Destroy(base.gameObject);
		}
		else
		{
			Enemies = new List<GameObject>();
			instance = this;
		}
	}

	public void SpawnEnemy(string EnemyName, int NumberOfEnemies)
	{
		if (enemyOverride != "Standard")
		{
			EnemyName = enemyOverride;
		}
		for (int i = 0; i < NumberOfEnemies; i++)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(EnemyName);
			if (nextFree == null)
			{
				Debug.Log("EnemyManager: No free objects in ObjectPool of EnemyType - " + EnemyName);
				break;
			}
			EnemyController component = nextFree.GetComponent<EnemyController>();
			component.LoadEnemyData(EnemyName);
			LeviathanCheck(nextFree);
			component.SpawnLocation(Vector3.zero);
			component.SpawnMovement();
			component.Initialize();
			ScaleEnemy(component, nextFree);
			SetSafeZone(component);
			SetPrimaryEnemy(component);
			Enemies.Add(nextFree);
		}
	}

	public void SpawnEnemy(string EnemyName, int NumberOfEnemies, Vector3 startPos)
	{
		float num = 0f;
		if (enemyOverride != "Standard")
		{
			EnemyName = enemyOverride;
		}
		for (int i = 0; i < NumberOfEnemies; i++)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(EnemyName);
			if (nextFree == null)
			{
				Debug.LogWarning("EnemyManager: No free objects in ObjectPool of EnemyType - " + EnemyName);
				break;
			}
			EnemyController component = nextFree.GetComponent<EnemyController>();
			component.LoadEnemyData(EnemyName);
			LeviathanCheck(nextFree);
			if (component.onRail)
			{
				startPos.x += num;
				component.SpawnLocation(startPos);
			}
			else
			{
				nextFree.rigidbody.MovePosition(startPos);
				nextFree.transform.localPosition = startPos;
			}
			num += 10f;
			component.SpawnMovement();
			component.Initialize();
			ScaleEnemy(component, nextFree);
			SetSafeZone(component);
			SetPrimaryEnemy(component);
			switch (MegaWeaponManager.instance.megaWeaponState)
			{
			case MegaWeaponManager.eMegaWeapons.mw_Groovitron:
				component.Groove();
				break;
			case MegaWeaponManager.eMegaWeapons.mw_RiftInducer:
				component.Rift();
				break;
			case MegaWeaponManager.eMegaWeapons.mw_Tornado:
				component.Tornado();
				break;
			}
			Enemies.Add(nextFree);
		}
	}

	private void SetPrimaryEnemy(EnemyController EC)
	{
		if (shouldHavePrimaryEnemy && !primaryEnemySet)
		{
			primaryEnemySet = true;
			EC.atkRatchetRail = true;
		}
	}

	public LeviathanController SpawnIntroLeviathon()
	{
		GameObject nextFree = GameObjectPool.instance.GetNextFree("Leviathan_PF");
		if (nextFree == null)
		{
			Debug.LogWarning("EnemyManager: No free objects in ObjectPool of EnemyType - Leviathan_PF");
			return null;
		}
		LeviathanController component = nextFree.GetComponent<LeviathanController>();
		Leviathan = nextFree;
		component.Initialize();
		Enemies.Add(nextFree);
		return component;
	}

	private void LeviathanCheck(GameObject Enemy)
	{
		if (Leviathan == null && Enemy.GetComponent<EnemyController>().theBoss)
		{
			Leviathan = Enemy;
			LeviathanController component = Enemy.GetComponent<LeviathanController>();
			component.ResetFiringState();
		}
		if (Leviathan == Enemy)
		{
			UIHUD hUD = UIManager.instance.GetHUD();
			if (hUD != null)
			{
				EnemyController component2 = Enemy.GetComponent<EnemyController>();
				hUD.UpdateBossHealth(component2.HP, component2.MaxHP);
				hUD.ShowBossHealth();
			}
		}
	}

	public void DestroyAll(bool isRyno)
	{
		if (MegaWeaponManager.instance.groovitronOn)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.groovitronCurrentKills, Enemies.Count);
		}
		else if (MegaWeaponManager.instance.riftInducerOn)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.riftInducerCurrentKills, Enemies.Count);
		}
		else if (MegaWeaponManager.instance.tornadoOn)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.tornadoLauncherCurrentKills, Enemies.Count);
		}
		foreach (GameObject enemy in Enemies)
		{
			if (enemy.GetComponent<EnemyController>().isDead)
			{
				continue;
			}
			if (isRyno)
			{
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.rynoShotsHit);
			}
			if (enemy.GetComponent<LeviathanController>() == null)
			{
				if (isRyno)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.rynoKills);
				}
				enemy.GetComponent<EnemyController>().ClearWeaponHitBy();
				enemy.GetComponent<EnemyController>().Death();
			}
		}
	}

	public List<GameObject> WithinRadius(Vector3 target, uint radius)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject enemy in Enemies)
		{
			if ((float)radius >= Vector3.Distance(enemy.rigidbody.position, target))
			{
				list.Add(enemy);
			}
		}
		return list;
	}

	private void FixedUpdate()
	{
		if (Enemies.Count != 0)
		{
			foreach (GameObject enemy in Enemies)
			{
				if (TileSpawnManager.instance.GetGameStateFromTile(1) != GameController.eGameState.GS_OnGround)
				{
					enemy.GetComponent<EnemyController>().shouldDespawn = true;
				}
				if ((enemy.GetComponent<EnemyController>().isDead || enemy.rigidbody.position.x < GameController.instance.playerController.rigidbody.position.x - 50f) && (!enemy.GetComponent<EnemyController>().theBoss || enemy.GetComponent<EnemyController>().isDead || !(enemy.rigidbody.position.x > GameController.instance.playerController.rigidbody.position.x - 150f)))
				{
					if (enemy.GetComponent<EnemyController>().atkRatchetRail)
					{
						enemy.GetComponent<EnemyController>().atkRatchetRail = false;
						primaryEnemySet = false;
					}
					GameObjectPool.instance.SetFree(enemy);
					Enemies.Remove(enemy);
					AnimationEvents component = enemy.GetComponent<AnimationEvents>();
					if (component != null)
					{
						component.AE_StopAllLoopingSounds();
					}
					break;
				}
			}
			return;
		}
		primaryEnemySet = false;
	}

	private void ScaleEnemy(EnemyController EC, GameObject Enemy)
	{
		Vector3 localScale = default(Vector3);
		localScale.x = EC.enemyScale;
		localScale.y = EC.enemyScale;
		localScale.z = EC.enemyScale;
		Vector3 position = Enemy.rigidbody.position;
		Enemy.transform.localScale = localScale;
		Enemy.rigidbody.position = position;
	}

	private void SetSafeZone(EnemyController EC)
	{
		switch (TileSpawnManager.instance.currentEnvironmentType)
		{
		case TileSpawnManager.ENVType.ENV_Axm:
			EC.minZ = 0f - axiSafeZone.x;
			EC.maxZ = axiSafeZone.x;
			EC.maxHeight = axiSafeZone.y;
			EC.minHeight = 2f;
			break;
		case TileSpawnManager.ENVType.ENV_Pol:
			EC.minZ = 0f - polySafeZone.x;
			EC.maxZ = polySafeZone.x;
			EC.maxHeight = polySafeZone.y;
			EC.minHeight = 2f;
			break;
		case TileSpawnManager.ENVType.ENV_Ter:
			EC.minZ = 0f - terSafeZone.x;
			EC.maxZ = terSafeZone.x;
			EC.maxHeight = terSafeZone.y;
			EC.minHeight = 2f;
			break;
		}
	}

	public EnemyController GetNewBuzzBladeTarget(EnemyController oldEnemy)
	{
		foreach (GameObject enemy in Enemies)
		{
			if (enemy.gameObject == oldEnemy.gameObject)
			{
				continue;
			}
			return enemy.GetComponent<EnemyController>();
		}
		return null;
	}

	public bool DoesEnemyStillExist(EnemyController oldEnemy)
	{
		foreach (GameObject enemy in Enemies)
		{
			if (enemy.gameObject == oldEnemy.gameObject)
			{
				return true;
			}
		}
		return false;
	}
}
