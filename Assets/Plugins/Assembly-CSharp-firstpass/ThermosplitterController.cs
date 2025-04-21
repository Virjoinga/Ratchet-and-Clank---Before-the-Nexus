using UnityEngine;

public class ThermosplitterController : EnemyController
{
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
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.thermoSplitterKilled);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.groundEnemiesKilled);
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject == GameController.instance.playerController.gameObject)
		{
			GameController.instance.playerController.TakeDamage(meleeDamage);
		}
	}
}
