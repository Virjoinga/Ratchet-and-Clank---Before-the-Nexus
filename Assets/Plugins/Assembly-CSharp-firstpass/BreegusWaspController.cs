using UnityEngine;

public class BreegusWaspController : EnemyController
{
	private GameObject chargeUpParticles;

	public float chargeupPlaybackSpeed = 1f;

	protected override void Start()
	{
		base.Start();
		chargeUpParticles = base.gameObject.transform.Find("FX_WeaponCharge").gameObject;
		laserOriginObject = chargeUpParticles;
		laserColor = Color.magenta;
	}

	protected override bool UpdateTargetPosition(EnemyType type)
	{
		if (currentPosDelay <= 0f)
		{
			targetPos.z = Random.Range(minZ, maxZ);
			targetPos.y = Random.Range(minHeight, maxHeight);
			currentPosDelay = minPosDelay + Random.value * (maxPosDelay - minPosDelay);
			beginMovePos = base.rigidbody.position;
			return true;
		}
		return false;
	}

	public override void Death()
	{
		base.Death();
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.breegusWaspsKilled);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.flyingEnemiesKilled);
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
