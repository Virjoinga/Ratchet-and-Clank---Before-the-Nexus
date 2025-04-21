using UnityEngine;

public class TutorialJumpScript : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (TutorialUnlockManager.instance.tutorialLocks[3])
		{
			TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.SwipeUpJump);
		}
	}
}
