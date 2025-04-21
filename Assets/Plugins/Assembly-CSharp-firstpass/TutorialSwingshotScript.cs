using UnityEngine;

public class TutorialSwingshotScript : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (TutorialUnlockManager.instance.tutorialLocks[0])
		{
			TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.TapSwingshot);
		}
	}
}
