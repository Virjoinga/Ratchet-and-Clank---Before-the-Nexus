using UnityEngine;

public class Terachnoid : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerController>() != null)
		{
			other.GetComponent<PlayerController>().addTerachnoid(1);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalPickupsAcquired);
			SFXManager.instance.PlaySound("cha_terachnoid_pickup");
			GameObjectPool.instance.SetFree(base.gameObject, true);
			if (TutorialUnlockManager.instance.tutorialLocks[5])
			{
				TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.TerachnoidTut);
			}
		}
	}

	private void FixedUpdate()
	{
		GameObject mainCamera = GameController.instance.mainCamera;
		if (mainCamera != null && base.transform.position.x < mainCamera.transform.position.x)
		{
			GameObjectPool.instance.SetFree(base.gameObject, true);
		}
	}
}
