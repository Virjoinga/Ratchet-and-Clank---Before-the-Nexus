using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : Obstacle
{
	public int crateLevel;

	public ParticleSystem breakParticles;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		if (MegaWeaponManager.instance.megaWeaponState == MegaWeaponManager.eMegaWeapons.mw_Tornado && MegaWeaponManager.instance.GetTornadoPickupRange() >= Vector3.Distance(base.transform.position, MegaWeaponManager.instance.GetMegaWeaponPosition()))
		{
			PickupManager.instance.SpawnExplosionOfBolts(base.transform.position);
			GameObjectPool.instance.SetFree(base.gameObject);
			if (breakParticles != null)
			{
				GameObject nextFree = GameObjectPool.instance.GetNextFree(breakParticles.name, true);
				nextFree.transform.position = base.transform.position;
				nextFree.particleSystem.Play();
			}
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.cratesBroken);
			StatsTracker.instance.SetStat(StatsTracker.Stats.noCratesDistanceTraveled, GameController.instance.playerController.travelDist);
		}
	}

	private IEnumerator CrateDestroy(float timeUntilHit)
	{
		yield return new WaitForSeconds(timeUntilHit);
		PickupManager.instance.SpawnExplosionOfBolts(base.transform.position);
		GameObjectPool.instance.SetFree(base.gameObject);
		if (breakParticles != null)
		{
			GameObject crateBreak = GameObjectPool.instance.GetNextFree(breakParticles.name, true);
			crateBreak.transform.position = base.transform.position;
			crateBreak.particleSystem.Play();
		}
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.cratesBroken);
		StatsTracker.instance.SetStat(StatsTracker.Stats.noCratesDistanceTraveled, GameController.instance.playerController.travelDist);
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerController component = other.GetComponent<PlayerController>();
		if (component != null)
		{
			StartCoroutine("CrateDestroy", 0f);
		}
	}

	public void CrateTapped(Weapon weap)
	{
		float num = 0.05f;
		num = GetBulletTime(weap.weaponName);
		if (weap.AOE != 0)
		{
			List<GameObject> list = EnemyManager.instance.WithinRadius(base.transform.position, weap.AOE);
			foreach (GameObject item in list)
			{
				item.GetComponent<EnemyController>().TakeDamage(weap.AOEDamage);
			}
		}
		GameController.instance.playerController.myWeap.FireProjectile(base.transform.position);
		StartCoroutine("CrateDestroy", num);
	}

	private float GetBulletTime(string name)
	{
		switch (name)
		{
		case "PredatorLauncher":
			return WeaponsManager.instance.predatorTimeToHit;
		case "RynoM":
			return WeaponsManager.instance.predatorTimeToHit;
		case "BuzzBlades":
			return WeaponsManager.instance.buzzBladeTimeToHit;
		default:
			return 0.05f;
		}
	}
}
