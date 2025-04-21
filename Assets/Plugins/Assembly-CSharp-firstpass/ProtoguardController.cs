using UnityEngine;

public class ProtoguardController : EnemyController
{
	private GameObject chargeUpLeftParticles;

	private GameObject chargeUpRightParticles;

	public float chargeupPlaybackSpeed = 1f;

	protected override void Start()
	{
		base.Start();
		chargeUpLeftParticles = base.gameObject.transform.Find("Root").Find("Body").Find("LeftArm")
			.Find("LeftForearm")
			.Find("LeftHand")
			.Find("FX_WeaponCharge")
			.gameObject;
		chargeUpRightParticles = base.gameObject.transform.Find("Root").Find("Body").Find("RightArm")
			.Find("RightForearm")
			.Find("RightHand")
			.Find("FX_WeaponCharge")
			.gameObject;
		laserOriginObject = base.gameObject.transform.Find("Root").Find("Body").gameObject;
		laserColor = Color.yellow;
	}

	protected override bool UpdateTargetPosition(EnemyType type)
	{
		if (currentPosDelay <= 0f)
		{
			targetPos.z = Random.Range(minZ, maxZ);
			targetPos.y = Random.Range(minHeight, maxHeight);
			currentPosDelay = minPosDelay + Random.value * (maxPosDelay - minPosDelay);
			beginMovePos = base.GetComponent<Rigidbody>().position;
			return true;
		}
		return false;
	}

	public override void Death()
	{
		SFXManager.instance.PlaySound("pro_enemyShipSm_Explode01_32m_jcm");
		base.Death();
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.protoGuardKilled);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.flyingEnemiesKilled);
	}

	protected override void PlayChargeParticle()
	{
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
}
