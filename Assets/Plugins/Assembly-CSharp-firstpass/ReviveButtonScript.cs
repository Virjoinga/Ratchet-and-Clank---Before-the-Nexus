using UnityEngine;

public class ReviveButtonScript : MonoBehaviour
{
	private void OnClick()
	{
		if (StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.heroBoltsCollected) > 0f)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.heroBoltsUsed);
			GameController.instance.HeroBoltRevive();
			SFXManager.instance.ModulateVolume("Hero_Bolt", 1.25f, 1.25f, 0f);
			SFXManager.instance.PlaySound("Hero_Bolt");
			SFXManager.instance.ModulateVolume("bolt_4", 1.25f, 1.25f, 0f);
			SFXManager.instance.PlaySound("bolt_4");
		}
	}
}
