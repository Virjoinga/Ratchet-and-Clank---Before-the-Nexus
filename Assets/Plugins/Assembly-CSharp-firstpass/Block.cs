using UnityEngine;

public class Block : Obstacle
{
	public ParticleSystem destroyParticles;

	protected virtual void OnCollisionEnter(Collision other)
	{
		PlayerController component = other.gameObject.GetComponent<PlayerController>();
		if (!(component != null))
		{
			return;
		}
		if (base.gameObject.tag == "Obstacle")
		{
			component.PlayOnHitParticle();
			if (GameController.instance.playerController.GodMode)
			{
				PlayDestroyParticle();
				GameObjectPool.instance.SetFree(base.gameObject, true);
				return;
			}
			int hP = GameController.instance.playerController.GetHP();
			if (GadgetManager.instance.IsJetpackInvincibleLanding())
			{
				PlayDestroyParticle();
				GameObjectPool.instance.SetFree(base.gameObject, true);
				return;
			}
			if (GadgetManager.instance.GetReflectorHealth() > 0)
			{
				GadgetManager.instance.ReflectorHit();
				PlayDestroyParticle();
				GameObjectPool.instance.SetFree(base.gameObject, true);
				return;
			}
			SFXManager.instance.PlaySound("cha_Ratchet_Gethit_VO");
			hP--;
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.damageTaken);
			StatsTracker.instance.SetStat(StatsTracker.Stats.noDamageDistanceBase, GameController.instance.playerController.travelDist);
			GameController.instance.playerController.SetHP(hP);
			UIManager.instance.GetHUD().UpdateHP();
			if (hP > 0)
			{
				if (hP == 2)
				{
					GameController.instance.playerController.SetArmorLevel(hP);
				}
				PlayDestroyParticle();
				GameObjectPool.instance.SetFree(base.gameObject, true);
				SFXManager.instance.PlaySound("cha_Ratchet_ArmorBreak");
				return;
			}
		}
		switch (objectType)
		{
		case 0:
			if (TileSpawnManager.instance.floorTileList[1].GetComponent<TileInfo>().spawnedState == TileSpawnManager.TileSpawnState.Hero || TileSpawnManager.instance.floorTileList[1].GetComponent<TileInfo>().spawnedState == TileSpawnManager.TileSpawnState.Ground)
			{
				component.Kill(PlayerController.EDeathDealer.EDeath_Proj);
			}
			else
			{
				component.Kill(PlayerController.EDeathDealer.EDeath_Obs);
			}
			break;
		case 1:
			component.Kill(PlayerController.EDeathDealer.EDeath_Explode);
			PlayDestroyParticle();
			GameObjectPool.instance.SetFree(base.gameObject, true);
			break;
		case 2:
			component.Kill(PlayerController.EDeathDealer.EDeath_Elem);
			break;
		case 3:
			component.Kill(PlayerController.EDeathDealer.EDeath_Fire);
			break;
		}
	}

	private void PlayDestroyParticle()
	{
		if (destroyParticles != null)
		{
			ParticleSystem particleSystem = (ParticleSystem)Object.Instantiate(destroyParticles, base.transform.position, base.transform.rotation);
			particleSystem.Play();
			Object.Destroy(particleSystem.gameObject, particleSystem.duration);
		}
	}
}
