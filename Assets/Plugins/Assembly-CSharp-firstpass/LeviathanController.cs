using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeviathanController : EnemyController
{
	private class LevNodeSort : IComparer<LeviathanNodes>
	{
		public int Compare(LeviathanNodes a, LeviathanNodes b)
		{
			if (a.transform.position.x < b.transform.position.x)
			{
				return -1;
			}
			if (a.transform.position.x > b.transform.position.x)
			{
				return 1;
			}
			return 0;
		}
	}

	private bool arrivedAtLocation = true;

	private float nodeAttackChance;

	private bool firing;

	private GameObject chargeUpLeftParticles;

	private GameObject chargeUpRightParticles;

	public float chargeupPlaybackSpeed = 1f;

	private GameObject engineFireParticles1;

	private GameObject engineFireParticles2;

	private GameObject engineFireParticles3;

	public ParticleSystem projectileImpactEffect;

	private GameObject impactEffect;

	private bool flyAway;

	public override void Initialize()
	{
		flyAway = false;
		firing = false;
		arrivedAtLocation = true;
		base.Initialize();
		engineFireParticles1 = base.gameObject.transform.Find("Root").Find("ShipBody").Find("L_Wing")
			.Find("FX_EngineFire1")
			.gameObject;
		engineFireParticles2 = base.gameObject.transform.Find("Root").Find("ShipBody").Find("R_Wing")
			.Find("FX_EngineFire2")
			.gameObject;
		engineFireParticles3 = base.gameObject.transform.Find("Root").Find("ShipBody").Find("R_Wing")
			.Find("FX_EngineFire3")
			.gameObject;
		engineFireParticles1.GetComponent<ParticleSystem>().Stop();
		engineFireParticles2.GetComponent<ParticleSystem>().Stop();
		engineFireParticles3.GetComponent<ParticleSystem>().Stop();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public void ResetFiringState()
	{
		firing = false;
	}

	protected override void Start()
	{
		base.Start();
		IdleState = Animator.StringToHash("Base Layer.MoveForward");
		chargeUpLeftParticles = base.gameObject.transform.Find("Root").Find("ShipBody").Find("FX_WeaponChargeL")
			.gameObject;
		chargeUpRightParticles = base.gameObject.transform.Find("Root").Find("ShipBody").Find("FX_WeaponChargeR")
			.gameObject;
		flyAway = false;
		laserOriginObject = chargeUpLeftParticles;
	}

	public void StartIntroAnim()
	{
		Vector3 position = GameController.instance.playerController.GetComponent<Rigidbody>().position;
		base.transform.rotation = Quaternion.identity;
		anim.enabled = true;
		anim.SetBool("Start", true);
		base.GetComponent<Rigidbody>().MovePosition(position);
		base.gameObject.GetComponent<BoxCollider>().enabled = false;
	}

	public void IntroOver()
	{
		base.gameObject.GetComponent<BoxCollider>().enabled = true;
		anim.SetBool("Start", false);
		shouldDespawn = true;
		Quaternion identity = Quaternion.identity;
		identity.y = 180f;
		base.transform.rotation = identity;
		Vector3 zero = Vector3.zero;
		zero.y = 20f;
		targetPos = zero;
		base.transform.position = zero;
		flyAway = false;
	}

	public override void Groove()
	{
	}

	public override void Rift()
	{
	}

	public override void Tornado()
	{
	}

	protected override bool UpdateTargetPosition(EnemyType type)
	{
		if (targetPos.y != 70f)
		{
			flyAway = false;
		}
		if (arrivedAtLocation && !flyAway)
		{
			if (GameController.instance.PathNodes.Count > 0)
			{
				if (GameController.instance.PathNodes.Count > 1)
				{
					GameController.instance.PathNodes.Sort(new LevNodeSort());
				}
				targetPos = GameController.instance.PathNodes[0].transform.position;
				nodeAttackChance = GameController.instance.PathNodes[0].chanceToAttack;
				GameController.instance.PathNodes.RemoveAt(0);
				arrivedAtLocation = false;
				shouldDespawn = false;
				MusicManager.instance.Resume();
				MusicManager.instance.Play(MusicManager.eMusicTrackType.Boss, false, 2f);
				SFXManager.instance.PlaySound("cha_Leviathan_engine_loop");
				return true;
			}
			MusicManager.instance.Play(MusicManager.eMusicTrackType.InGame1, false, 5f);
			SFXManager.instance.StopSound("cha_Leviathan_engine_loop");
			shouldDespawn = true;
			UIHUD hUD = UIManager.instance.GetHUD();
			if (hUD != null)
			{
				hUD.HideBossHealth();
			}
		}
		return false;
	}

	protected override void MoveEnemy()
	{
		if (flyAway && HP != 0)
		{
			flyAway = false;
		}
		if (targetPos.x >= base.GetComponent<Rigidbody>().position.x || flyAway)
		{
			if (flyAway && base.GetComponent<Rigidbody>().position.y >= 69f)
			{
				isDead = true;
				flyAway = false;
				targetPos.y = 20f;
			}
			if (GameController.instance.playerController.IsPlayerDead())
			{
				return;
			}
			Vector3 position = base.GetComponent<Rigidbody>().position;
			float num = GameController.instance.playerController.curVelocityX;
			if (base.GetComponent<Rigidbody>().position.x < GameController.instance.playerController.GetComponent<Rigidbody>().position.x + 15f || base.GetComponent<Rigidbody>().position.x > GameController.instance.playerController.GetComponent<Rigidbody>().position.x + 35f)
			{
				num *= 2f;
			}
			if (num != 0f)
			{
				float num2 = (targetPos.x - base.GetComponent<Rigidbody>().position.x) / num;
				if (num2 <= Time.fixedDeltaTime)
				{
					position = targetPos;
					arrivedAtLocation = true;
				}
				else
				{
					position.y = Mathf.Lerp(position.y, targetPos.y, Time.fixedDeltaTime / num2);
					position.z = Mathf.Lerp(position.z, targetPos.z, Time.fixedDeltaTime / num2);
				}
				if (!shouldDespawn)
				{
					position.x = GameController.instance.playerController.GetComponent<Rigidbody>().position.x + (float)keepAtDistance;
				}
				else
				{
					position.y += 2f;
				}
				base.GetComponent<Rigidbody>().MovePosition(position);
			}
		}
		else
		{
			arrivedAtLocation = true;
		}
	}

	protected override void MegaWeaponMovement()
	{
		MoveEnemy();
	}

	protected override void HandleAttackChance()
	{
	}

	protected override void FixedUpdate()
	{
		if (GameController.instance.playerController.IsDoingIntro())
		{
			return;
		}
		UIHUD hUD = UIManager.instance.GetHUD();
		if (hUD != null)
		{
			if (!hUD.BossHealth.gameObject.activeSelf && !shouldDespawn && !flyAway)
			{
				hUD.ShowBossHealth();
			}
			if (flyAway)
			{
				hUD.HideBossHealth();
			}
		}
		DamageEffectUpdate();
		base.FixedUpdate();
		if (!firing)
		{
			anim.SetBool("Attack", false);
			anim.SetBool("Move_F", true);
		}
		if (!anim.GetBool("Attack") && !anim.GetBool("ChargeUp") && !firing && !flyAway && arrivedAtLocation && !shouldDespawn && (float)Random.Range(1, 100) < nodeAttackChance * 100f)
		{
			FireProjectile();
		}
	}

	protected override void FireProjectile()
	{
		if (!firing)
		{
			bool flag;
			if (Random.Range(1, 100) < 50)
			{
				flag = true;
				railToAttack = 2;
			}
			else
			{
				flag = false;
				railToAttack = 1;
			}
			StartCoroutine("DelayFire", flag);
			firing = true;
		}
	}

	private IEnumerator DelayFire(bool leftRail)
	{
		SetGroundTargetEffect();
		PlayChargeParticle();
		StartLaserSight();
		yield return new WaitForSeconds(fireBuildUpSpeed);
		if (!flyAway)
		{
			GameObject projectileL = GameObjectPool.instance.GetNextFree("BulletThing");
			if (projectileL != null)
			{
				projectileL.GetComponent<EnemyProjectiles>().particles = projectileParticles;
				projectileL.GetComponent<EnemyProjectiles>().SetProjectileDataForBoss(fireDamage, projectileSpeed, laserOriginObject.transform.position, leftRail, curParticleGroundTarget.transform.position, false);
				projectileL.GetComponent<EnemyProjectiles>().ecOwner = this;
			}
			GameObject projectileR = GameObjectPool.instance.GetNextFree("BulletThing");
			if (projectileR != null)
			{
				projectileR.GetComponent<EnemyProjectiles>().particles = projectileParticles;
				projectileR.GetComponent<EnemyProjectiles>().SetProjectileDataForBoss(fireDamage, projectileSpeed, laserOriginObject.transform.position, leftRail, curParticleGroundTarget.transform.position, true);
				projectileR.GetComponent<EnemyProjectiles>().ecOwner = this;
			}
			anim.SetBool("Attack", true);
			anim.SetBool("Move_F", false);
		}
	}

	protected override void SetGroundTargetEffect()
	{
		Vector3 zero = Vector3.zero;
		float num = 0f + fireBuildUpSpeed + (laserOriginObject.transform.position.x - GameController.instance.playerController.GetComponent<Rigidbody>().position.x) / projectileSpeed;
		float xOffset = GameController.instance.playerController.GetComponent<Rigidbody>().position.x + GameController.instance.playerController.GetComponent<Rigidbody>().velocity.x * num;
		zero = TileSpawnManager.instance.getRailNodePosition(railToAttack, xOffset, TileSpawnManager.instance.railTileList[0]);
		if (railToAttack == 2)
		{
			zero.z -= 3.25f;
		}
		else
		{
			zero.z += 3.25f;
		}
		if (particleGroundTarget != null)
		{
			curParticleGroundTarget = GameObjectPool.instance.GetNextFree(particleGroundTarget.name, true);
			curParticleGroundTarget.transform.position = zero;
			curParticleGroundTarget.GetComponent<ParticleSystem>().Play();
		}
		StartCoroutine("DelayedGroundExplosionEffect", num);
	}

	private IEnumerator DelayedGroundExplosionEffect(float timeTillRatchet)
	{
		yield return new WaitForSeconds(timeTillRatchet);
		if (projectileImpactEffect != null && curParticleGroundTarget != null)
		{
			Vector3 impactPos = curParticleGroundTarget.transform.position;
			impactPos.x += 5f;
			impactEffect = GameObjectPool.instance.GetNextFree(projectileImpactEffect.name, true);
			impactEffect.transform.position = impactPos;
			impactEffect.GetComponent<ParticleSystem>().Play();
		}
		yield return new WaitForSeconds(0.1f);
		firing = false;
	}

	public override void SpawnLocation(Vector3 nodePos)
	{
		Vector3 zero = Vector3.zero;
		zero.x = Random.Range(240, 250);
		if (etEnemyState == EnemyType.air)
		{
			zero.y = Random.Range(1, 7);
		}
		zero.z = Random.Range(0, 10) - 5;
		base.transform.position = GameController.instance.playerController.GetComponent<Rigidbody>().position + zero;
	}

	public override void Death()
	{
		UIHUD hUD = UIManager.instance.GetHUD();
		if (flyAway)
		{
			if (hUD != null)
			{
				hUD.HideBossHealth();
			}
			return;
		}
		if (curParticleGroundTarget != null)
		{
			curParticleGroundTarget.transform.position = Vector3.zero;
			curParticleGroundTarget.GetComponent<ParticleSystem>().Stop();
			curParticleGroundTarget = null;
		}
		if (hUD != null)
		{
			hUD.HideBossHealth();
		}
		flyAway = true;
		targetPos.y = 70f;
		StopLaserSight();
		LockOnStop();
		PickupManager.instance.SpawnRaritanium(base.transform.position);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalEnemiesKilled);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.flyingEnemiesKilled);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.leviathanKilled);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.bossEnemiesKilled);
		switch (weaponHitBy)
		{
		case "ConstructoShotgun":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.shotgunBossKills);
			break;
		case "BuzzBlades":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.buzzbladesBossKills);
			break;
		case "PredatorLauncher":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.predatorBossKills);
			break;
		case "RynoM":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.rynoBossKills);
			break;
		}
		SFXManager.instance.StopSound("cha_Leviathan_engine_loop");
		if (!MusicManager.instance.isMuted())
		{
			SFXManager.instance.PlaySound("Endgame_Stinger");
		}
		MusicManager.instance.Play(MusicManager.eMusicTrackType.InGame1, false, 5f);
		TileSpawnManager.instance.spawnBoltsForHeroTiles();
	}

	public void AddPathNode(LeviathanNodes LevNode)
	{
		if (LevNode == null)
		{
			Debug.LogError("LeviathanController: AddPathNode(): Node is not a LeviathanNodes");
		}
		else
		{
			GameController.instance.PathNodes.Add(LevNode);
		}
	}

	protected override void PlayChargeParticle()
	{
		SFXManager.instance.PlaySound("cha_Leviathan_Charge");
		if (chargeUpLeftParticles != null)
		{
			chargeUpLeftParticles.GetComponent<ParticleSystem>().playbackSpeed = chargeupPlaybackSpeed;
			chargeUpLeftParticles.GetComponent<ParticleSystem>().Play();
		}
		if (chargeUpRightParticles != null)
		{
			chargeUpRightParticles.GetComponent<ParticleSystem>().playbackSpeed = chargeupPlaybackSpeed;
			chargeUpRightParticles.GetComponent<ParticleSystem>().Play();
		}
	}

	private void DamageEffectUpdate()
	{
		if ((float)HP < (float)MaxHP * 0.25f)
		{
			if (!engineFireParticles3.GetComponent<ParticleSystem>().isPlaying)
			{
				engineFireParticles3.GetComponent<ParticleSystem>().Play();
			}
		}
		else if ((float)HP < (float)MaxHP * 0.5f)
		{
			if (!engineFireParticles2.GetComponent<ParticleSystem>().isPlaying)
			{
				engineFireParticles2.GetComponent<ParticleSystem>().Play();
			}
		}
		else if ((float)HP < (float)MaxHP * 0.75f && !engineFireParticles1.GetComponent<ParticleSystem>().isPlaying)
		{
			engineFireParticles1.GetComponent<ParticleSystem>().Play();
		}
	}
}
