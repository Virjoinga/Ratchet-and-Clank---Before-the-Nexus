using UnityEngine;

public class Raritanium : Pickup
{
	private void OnTriggerEnter(Collider other)
	{
		PlayerController component = other.GetComponent<PlayerController>();
		if (component != null)
		{
			GameObjectPool.instance.SetFree(base.gameObject, true);
			component.addRaritanium(1);
			SFXManager.instance.PlaySound("UI_raritanium_pickup_1");
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalPickupsAcquired);
			if (TutorialUnlockManager.instance.tutorialLocks[12])
			{
				TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.RaritaniumTut);
			}
		}
	}
}
