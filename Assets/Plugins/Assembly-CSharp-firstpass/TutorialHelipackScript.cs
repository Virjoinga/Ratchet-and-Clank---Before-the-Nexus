using UnityEngine;

public class TutorialHelipackScript : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (TutorialUnlockManager.instance.tutorialLocks[4])
		{
			TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.SwipeHoldHeli);
		}
	}
}
