public class HeroBolt : Boltz
{
	protected override void ReportCollection(PlayerController controller)
	{
		controller.addHeroBolt(1);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalPickupsAcquired);
		SFXManager.instance.ModulateVolume("Hero_Bolt", 1.25f, 1.25f, 0f);
		SFXManager.instance.PlaySound("Hero_Bolt");
		SFXManager.instance.ModulateVolume("bolt_4", 1.25f, 1.25f, 0f);
		SFXManager.instance.PlaySound("bolt_4");
		if (TutorialUnlockManager.instance.tutorialLocks[13])
		{
			TutorialUnlockManager.instance.OpenTutorial(TutorialUnlockManager.TutorialLock.HeroBoltTut);
		}
	}
}
