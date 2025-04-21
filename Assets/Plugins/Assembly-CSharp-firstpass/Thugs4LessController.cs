using System.Collections;
using UnityEngine;

public class Thugs4LessController : EnemyController
{
	private bool onGround = true;

	protected int IdleGroundState = Animator.StringToHash("Base Layer.IdleGround");

	private GameObject chargeUpParticles;

	public float chargeupPlaybackSpeed = 1f;

	public override void Initialize()
	{
		base.Initialize();
		onGround = true;
		anim.SetBool("Fly", false);
	}

	protected override void Start()
	{
		base.Start();
		chargeUpParticles = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("RShoulder")
			.Find("RForearm")
			.Find("FX_WeaponCharge")
			.gameObject;
		laserOriginObject = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("RShoulder")
			.gameObject;
		laserColor = Color.red;
	}

	protected override bool UpdateTargetPosition(EnemyType type)
	{
		if (InRangeOfPlayer() && currentPosDelay <= 0f)
		{
			targetPos.z = Random.Range(minZ, maxZ);
			targetPos.y = Random.Range(minHeight, maxHeight);
			currentPosDelay = minPosDelay + Random.value * (maxPosDelay - minPosDelay);
			beginMovePos = base.rigidbody.position;
			return true;
		}
		return false;
	}

	protected override void HandleAttackChance()
	{
		curFireTimer -= Time.deltaTime;
		if (InRangeOfPlayer() && onGround)
		{
			if (curMeleeChance <= meleeChance * 100f)
			{
				onGround = false;
				anim.SetBool("Fly", true);
			}
			curMeleeChance = Random.Range(1, 100);
		}
		if (!(curFireTimer <= 0f))
		{
			return;
		}
		if (currentBaseState.nameHash == IdleGroundState)
		{
			if (!anim.IsInTransition(0) && onGround)
			{
				StartCoroutine("BeginGroundAttack");
			}
		}
		else if (currentBaseState.nameHash == IdleState && !anim.IsInTransition(0) && !onGround)
		{
			StartCoroutine("BeginAttack");
		}
	}

	private IEnumerator BeginGroundAttack()
	{
		SetRailToFireAt();
		SetGroundFireTargetEffect();
		StartLaserSight();
		curFireTimer = fireRate;
		anim.SetBool("GroundAttack", true);
		yield return new WaitForSeconds(0.2f);
		FireProjectile();
		anim.SetBool("GroundAttack", false);
	}

	protected virtual void SetGroundFireTargetEffect()
	{
		Vector3 zero = Vector3.zero;
		float num = 0f + (base.rigidbody.position.x - GameController.instance.playerController.rigidbody.position.x) / projectileSpeed;
		float xOffset = GameController.instance.playerController.rigidbody.position.x + GameController.instance.playerController.rigidbody.velocity.x * num;
		zero = TileSpawnManager.instance.getRailNodePosition(railToAttack, xOffset, TileSpawnManager.instance.railTileList[0]);
		if (particleGroundTarget != null)
		{
			curParticleGroundTarget = GameObjectPool.instance.GetNextFree(particleGroundTarget.name, true);
			curParticleGroundTarget.transform.position = zero;
			curParticleGroundTarget.particleSystem.Play();
		}
	}

	protected override void MoveEnemy()
	{
		if (!InRangeOfPlayer())
		{
			return;
		}
		Vector3 position = base.rigidbody.position;
		Vector3 vector = targetPos;
		float num = GameController.instance.playerController.curVelocityX * 1.1f;
		if (num != 0f)
		{
			vector.x += GameController.instance.playerController.rigidbody.position.x;
			Vector3 railNodePosition = TileSpawnManager.instance.getRailNodePosition(0, base.rigidbody.position.x, TileSpawnManager.instance.railTileList[0]);
			vector.y += railNodePosition.y;
			vector.z += railNodePosition.z;
			float num2 = ((vector.y - base.rigidbody.position.y != 0f) ? (Mathf.Abs(Mathf.Abs(vector.y - beginMovePos.y) - Mathf.Abs(vector.y - base.rigidbody.position.y)) / Mathf.Abs(vector.y - base.rigidbody.position.y)) : ((vector.z - base.rigidbody.position.z != 0f) ? (Mathf.Abs(Mathf.Abs(vector.z - beginMovePos.z) - Mathf.Abs(vector.z - base.rigidbody.position.z)) / Mathf.Abs(vector.z - base.rigidbody.position.z)) : 1f));
			if (num2 < 0f || num2 > 1f)
			{
				num2 = 1f;
			}
			float num3 = Vector3.Distance(vector, base.rigidbody.position) / (EnemySpeedOverDistance.Evaluate(num2) * num);
			if (num3 <= Time.fixedDeltaTime)
			{
				position = vector;
				num3 = 1f;
			}
			if (!shouldDespawn && !onGround)
			{
				position.x = vector.x;
				position.y = Mathf.Lerp(position.y, vector.y, Time.fixedDeltaTime / num3);
				position.z = Mathf.Lerp(position.z, vector.z, Time.fixedDeltaTime / num3);
			}
			base.rigidbody.MovePosition(position);
		}
	}

	public override void Death()
	{
		SFXManager.instance.PlaySound("Cha_Thug_Death");
		base.Death();
		if (onGround)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.groundEnemiesKilled);
		}
		else
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.flyingEnemiesKilled);
		}
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.thugs4LessKilled);
		if (weaponHitBy == "BuzzBlades")
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.buzzbladesThugKills);
		}
	}

	protected override void PlayChargeParticle()
	{
		if (chargeUpParticles != null)
		{
			chargeUpParticles.particleSystem.playbackSpeed = chargeupPlaybackSpeed;
			chargeUpParticles.particleSystem.Play();
		}
	}
}
