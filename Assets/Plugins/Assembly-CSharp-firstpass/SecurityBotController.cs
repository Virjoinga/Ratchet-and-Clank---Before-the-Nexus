using UnityEngine;

public class SecurityBotController : EnemyController
{
	private GameObject chargeUpParticles;

	public float chargeupPlaybackSpeed = 1f;

	protected override void Start()
	{
		base.Start();
		chargeUpParticles = base.gameObject.transform.Find("FX_WeaponCharge").gameObject;
	}

	protected override bool UpdateTargetPosition(EnemyType type)
	{
		if (currentPosDelay <= 0f)
		{
			targetPos.z = Random.Range(minZ, maxZ);
			currentPosDelay = minPosDelay + Random.value * (maxPosDelay - minPosDelay);
			beginMovePos = base.rigidbody.position;
			return true;
		}
		return false;
	}

	public override void Death()
	{
		SFXManager.instance.PlaySound("pro_enemyShipSm_Explode01_32m_jcm");
		base.Death();
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.securityBotKilled);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.groundEnemiesKilled);
	}

	protected override void PlayChargeParticle()
	{
		if (chargeUpParticles != null)
		{
			chargeUpParticles.particleSystem.playbackSpeed = chargeupPlaybackSpeed;
			chargeUpParticles.particleSystem.Play();
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject == GameController.instance.playerController.gameObject)
		{
			GameController.instance.playerController.TakeDamage(meleeDamage);
		}
	}
}
