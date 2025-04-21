using UnityEngine;

public class Jetpack : Pickup
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
			playerController.SetCurrentPickupType(PlayerController.PickupTypes.Jetpack);
			base.transform.parent.gameObject.GetComponentInChildren<ParticleSystem>().Stop(true);
			SFXManager.instance.PlaySound("cha_ratchet_Pickup");
			base.renderer.enabled = false;
			base.enabled = false;
			PickupManager.instance.getPickups().Remove(base.transform.parent.gameObject);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalPickupsAcquired);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.gadgetsCollected);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.jetpackPickedup);
			if (TutorialUnlockManager.instance.tutorialLocks[2])
			{
				TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.UseGadgets);
			}
		}
	}

	public override void Activate()
	{
		GadgetManager.instance.ActivateJetpack();
		SFXManager.instance.PlaySound("cha_ratchet_Jet_Start");
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.jetpackActive);
		GameObjectPool.instance.SetFree(base.transform.parent.gameObject, true);
	}
}
