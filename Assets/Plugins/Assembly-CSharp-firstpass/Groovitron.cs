using System.Collections.Generic;
using UnityEngine;

public class Groovitron : Pickup
{
	private void OnTriggerEnter(Collider other)
	{
		if (!base.enabled || !base.gameObject.activeSelf || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		PlayerController playerController = null;
		if (other.tag.Equals("Player"))
		{
			playerController = GameController.instance.playerController;
		}
		if (playerController != null)
		{
			playerController.SetCurrentPickup(this);
			playerController.SetCurrentPickupType(PlayerController.PickupTypes.Groove);
			base.transform.parent.gameObject.GetComponentInChildren<ParticleSystem>().Stop(true);
			base.GetComponent<Renderer>().enabled = false;
			PickupManager.instance.getPickups().Remove(base.transform.parent.gameObject);
			SFXManager.instance.PlaySound("cha_ratchet_Pickup");
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalPickupsAcquired);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsCollected);
			if (TutorialUnlockManager.instance.tutorialLocks[2])
			{
				TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.UseGadgets);
			}
		}
	}

	public override void Activate()
	{
		MegaWeaponManager.instance.ActivateMegaWeapon(MegaWeaponManager.eMegaWeapons.mw_Groovitron);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.megaWeaponsUsed);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.groovitronUsed);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.groovitronActive);
		GameObjectPool.instance.SetFree(base.transform.parent.gameObject, true);
		List<GameObject> enemies = EnemyManager.instance.getEnemies();
		foreach (GameObject item in enemies)
		{
			EnemyController component = item.GetComponent<EnemyController>();
			component.Groove();
		}
	}
}
