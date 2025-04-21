using UnityEngine;

public class UISync : UIScreen
{
	public UILabel RaritaniumValueLabel;

	public UILabel ChronoVaultValueLabel;

	public UIButton SyncButton;

	public GameObject ErrorLabel;

	public Object UserData;

	private int PostRaritaniumAmount;

	private Color InactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	private Color WhiteColor = new Color(1f, 1f, 1f, 1f);

	private void Start()
	{
		UpdateSyncStatus();
		UIEventListener.Get(SyncButton.gameObject).onClick = SyncButtonClicked;
		ErrorLabel.SetActive(false);
	}

	public override void Show()
	{
		base.Show();
		DisableButton();
		UpdateSyncStatus();
	}

	private void DisableButton()
	{
		SyncButton.GetComponent<BoxCollider>().collider.enabled = false;
		SyncButton.transform.Find("Label").GetComponent<UILabel>().color = InactiveColor;
		SyncButton.UpdateColor(false, true);
	}

	private void EnableButton()
	{
		SyncButton.GetComponent<BoxCollider>().collider.enabled = true;
		SyncButton.transform.Find("Label").GetComponent<UILabel>().color = WhiteColor;
		SyncButton.UpdateColor(true, true);
	}

	public void UpdateSyncStatus()
	{
		if (UIManager.instance.CheckWifi())
		{
			RaritaniumValueLabel.text = GameController.instance.playerController.GetLifetimeRaritaniumTotal().ToString();
			ServerConnection.GetRaritanium(GetRaritaniumSuccess, GetRaritaniumFailed, UserData);
		}
		else
		{
			DisableButton();
		}
	}

	private void GetRaritaniumSuccess(HttpWeb target, object userData = null)
	{
		if (GameController.instance.playerController.GetLifetimeRaritaniumTotal() > 0)
		{
			EnableButton();
		}
		ChronoVaultValueLabel.text = target.text;
		ErrorLabel.SetActive(false);
	}

	private void GetRaritaniumFailed(HttpWeb target, object userData = null)
	{
		ChronoVaultValueLabel.text = "---";
		ErrorLabel.SetActive(true);
	}

	private void SyncButtonClicked(GameObject obj)
	{
		if (UIManager.instance.CheckWifi())
		{
			DisableButton();
			PostRaritaniumAmount = GameController.instance.playerController.GetLifetimeRaritaniumTotal();
			ServerConnection.PostRaritanium(PostRaritaniumAmount, SetRaritaniumSuccess, SetRaritaniumFailed, UserData);
			SFXManager.instance.PlaySound("UI_Send_Raritanium");
		}
		else
		{
			DisableButton();
		}
	}

	private void SetRaritaniumSuccess(HttpWeb target, object userData = null)
	{
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.raritaniumDeposited, PostRaritaniumAmount);
		StatsTracker.instance.SaveStatsAndReset();
		UIManager.instance.PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_139", true);
		UpdateSyncStatus();
		ErrorLabel.SetActive(false);
	}

	private void SetRaritaniumFailed(HttpWeb target, object userData = null)
	{
		SyncButton.GetComponent<BoxCollider>().collider.enabled = true;
		SyncButton.UpdateColor(true, true);
		ChronoVaultValueLabel.text = "---";
		UIManager.instance.PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_140", true);
		ErrorLabel.SetActive(true);
	}
}
