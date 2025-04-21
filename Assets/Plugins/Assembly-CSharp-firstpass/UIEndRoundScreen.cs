using UnityEngine;

public class UIEndRoundScreen : UIScreen
{
	public UILabel txt_DistanceTraveled;

	public UILabel txt_BoltsCollected;

	public UILabel txt_RaritaniumCollected;

	public UILabel txt_HeroBolts;

	public UILabel txt_Terachnoids;

	public UILabel txt_Score;

	public UILabel txt_TotalBolts;

	public UILabel txt_TotalRaritanium;

	public UILabel txt_Kills;

	public UILabel txt_Accuracy;

	public GameObject AffordableItemIndicator;

	public GameObject ShopHighlight;

	public ChallengeGridScript ListGrid;

	public ChallengeItemRendererScript ListItemRenderer;

	private bool ChallengeGridPopulated;

	private void Start()
	{
	}

	public override void Show()
	{
		EasyAnalytics.Instance.sendView("/End Round");
	}

	public void UpdateStats(int BoltsCollected, int DistanceTraveled)
	{
		if (!ChallengeGridPopulated)
		{
			ListGrid.PopulateList();
			ChallengeGridPopulated = true;
		}
		MusicManager.instance.Stop();
		if (!MusicManager.instance.isMuted())
		{
			SFXManager.instance.PlaySound("Endgame_Stinger");
		}
		Analytics.Get().SendDesignEvent("roundEnd:travelledDistance", DistanceTraveled);
		txt_DistanceTraveled.GetComponent<CounterScript>().Init(0f);
		txt_DistanceTraveled.GetComponent<CounterScript>().StartCounter(0f, DistanceTraveled);
		BoltsCollected = (int)StatsTracker.instance.GetStat(StatsTracker.Stats.boltzCollected) - (int)StatsTracker.instance.GetStat(StatsTracker.Stats.boltzSpent);
		txt_BoltsCollected.GetComponent<CounterScript>().Init(0f);
		txt_BoltsCollected.GetComponent<CounterScript>().StartCounter(0f, BoltsCollected);
		Analytics.Get().SendDesignEvent("roundEnd:collected:bolts", BoltsCollected);
		txt_RaritaniumCollected.GetComponent<CounterScript>().StartCounter(0f, GameController.instance.playerController.GetRaritanium());
		Analytics.Get().SendDesignEvent("roundEnd:collected:raritanium", StatsTracker.instance.GetStat(StatsTracker.Stats.raritaniumCollected));
		txt_HeroBolts.text = GameController.instance.playerController.GetHeroBoltsTotal().ToString();
		Analytics.Get().SendDesignEvent("roundEnd:collected:heroBolts", StatsTracker.instance.GetStat(StatsTracker.Stats.heroBoltsCollected));
		txt_Terachnoids.text = StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.terachnoidsCollected).ToString();
		Analytics.Get().SendDesignEvent("roundEnd:collected:terachnoids", StatsTracker.instance.GetStat(StatsTracker.Stats.terachnoidsCollected));
		txt_Score.GetComponent<CounterScript>().Init(0f);
		txt_Score.GetComponent<CounterScript>().StartCounter(0f, GameController.instance.GetScore());
		Analytics.Get().SendDesignEvent("roundEnd:collected:score", GameController.instance.GetScore());
		txt_TotalBolts.text = GameController.instance.playerController.GetBoltsTotal().ToString();
		txt_TotalRaritanium.text = GameController.instance.playerController.GetRaritaniumTotal().ToString();
		txt_Kills.GetComponent<CounterScript>().Init(0f);
		txt_Kills.GetComponent<CounterScript>().StartCounter(0f, StatsTracker.instance.GetStat(StatsTracker.Stats.totalEnemiesKilled));
		Analytics.Get().SendDesignEvent("roundEnd:collected:kills", StatsTracker.instance.GetStat(StatsTracker.Stats.totalEnemiesKilled));
		Analytics.Get().ForceSubmit();
		float stat = StatsTracker.instance.GetStat(StatsTracker.Stats.totalShotsFired);
		float stat2 = StatsTracker.instance.GetStat(StatsTracker.Stats.totalShotsHit);
		int a = ((stat != 0f) ? ((int)(stat2 / stat * 100f)) : 100);
		a = Mathf.Min(a, 100);
		txt_Accuracy.GetComponent<CounterScript>().Init(0f);
		txt_Accuracy.GetComponent<CounterScript>().StartCounter(0f, a);
		UpdateActiveChallenges();
		if (UIManager.instance.GetAffordableVendorItems() > 0)
		{
			AffordableItemIndicator.SetActive(true);
			ShopHighlight.SetActive(true);
		}
		else
		{
			AffordableItemIndicator.SetActive(false);
			ShopHighlight.SetActive(false);
		}
	}

	public void UpdateActiveChallenges()
	{
		ListGrid.UpdateActiveChallenges();
	}
}
