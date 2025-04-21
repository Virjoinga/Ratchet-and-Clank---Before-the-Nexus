using UnityEngine;

public class TutorialJumpPadScript : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (TutorialUnlockManager.instance.tutorialLocks[6])
		{
			TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.JumpPadTut);
		}
	}
}
