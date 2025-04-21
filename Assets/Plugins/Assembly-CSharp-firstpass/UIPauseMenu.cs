using System;
using UnityEngine;

public class UIPauseMenu : UIScreen
{
	public ChallengeGridScript ChallengesListGrid;

	public UILabel CompletedChallengesLabel;

	public UILabel WeaponDescLabel;

	public UILabel WeaponTitleLabel;

	public UILabel WeaponLevelLabel;

	public UILabel WeaponAmmoLabel;

	public UILabel txt_BoltsCollected;

	public UILabel txt_DistanceTraveled;

	public UILabel txt_RaritaniumCollected;

	public UILabel txt_HeroBolts;

	public UILabel txt_Terachnoids;

	public UILabel txt_Score;

	private uint CurrentWeaponIndex;

	private Weapon CurrentWeapon;

	public IconScript IconPistol;

	public IconScript IconShotgun;

	public IconScript IconBuzz;

	public IconScript IconPredator;

	public IconScript IconRyno;

	public UIButton MenuButton;

	public UIButton ResumeButton;

	public UIButton BuyAmmoButton;

	public UISprite PreviewSprite;

	public PanelManager HorizPanels;

	public WeaponInfoScript WeaponInfo;

	public UISlider AmmoBar;

	private bool Started;

	private bool PopulatedChallenges;

	private uint AmmoToBuy;

	private uint AmmoBoltCost;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(MenuButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnMenuButtonClicked));
		UIEventListener uIEventListener2 = UIEventListener.Get(MenuButton.gameObject);
		uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnMenuButtonPressed));
		UIEventListener uIEventListener3 = UIEventListener.Get(ResumeButton.gameObject);
		uIEventListener3.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onPress, new UIEventListener.BoolDelegate(OnResumeButtonPressed));
		UIEventListener uIEventListener4 = UIEventListener.Get(BuyAmmoButton.gameObject);
		uIEventListener4.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener4.onPress, new UIEventListener.BoolDelegate(OnAmmoButtonPressed));
		UpdateAll();
		Started = true;
		HorizPanels.CycleCallback = HorizScroll;
	}

	private void OnAmmoButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			ResumeButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
			MenuButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
		}
		else
		{
			ResumeButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
			MenuButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
		}
	}

	private void OnMenuButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			ResumeButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
			BuyAmmoButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
		}
		else
		{
			ResumeButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
			BuyAmmoButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
			UpdateAmmo();
		}
	}

	private void OnResumeButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			MenuButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
			BuyAmmoButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
		}
		else
		{
			MenuButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
			BuyAmmoButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
			UpdateAmmo();
		}
	}

	public override void Show()
	{
		SFXManager.instance.PlaySound("UI_Pause");
		if (Started)
		{
			base.Show();
			EasyAnalytics.Instance.sendView("/PauseMenu");
			UpdateAll();
		}
	}

	private void UpdateAll()
	{
		SwapWeapons(WeaponsManager.instance.curWeapIndex);
		UpdateBoltsCollected();
		UpdateDistanceTraveled();
		UpdateScore();
	}

	public void HorizScroll()
	{
		if (HorizPanels.GetCurrentPanelIndex() == 1)
		{
			if (!PopulatedChallenges)
			{
				ChallengesListGrid.PopulateList();
				PopulatedChallenges = true;
			}
			UpdateActiveChallenges();
		}
		else
		{
			UpdateSelectedWeapon();
		}
	}

	private void UpdateWeaponIcons()
	{
		Weapon component = WeaponsManager.instance.WeapInventory[0].GetComponent<Weapon>();
		Weapon component2 = WeaponsManager.instance.WeapInventory[1].GetComponent<Weapon>();
		Weapon component3 = WeaponsManager.instance.WeapInventory[2].GetComponent<Weapon>();
		Weapon component4 = WeaponsManager.instance.WeapInventory[3].GetComponent<Weapon>();
		Weapon component5 = WeaponsManager.instance.WeapInventory[4].GetComponent<Weapon>();
		IconPistol.SetIconSprite("icon_weapon_pistol", (IconScript.HexLevel)component.GetWeaponUpgradeLevel(), component.isOwned);
		if (component.isOwned)
		{
			IconPistol.GetComponent<BoxCollider>().enabled = true;
			IconPistol.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(true);
		}
		else
		{
			IconPistol.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(false);
			IconPistol.GetComponent<BoxCollider>().enabled = false;
		}
		if (WeaponsManager.instance.curWeapIndex == 0)
		{
			IconPistol.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(true);
			IconPistol.IconSprite.alpha = 1f;
			IconPistol.Hex.alpha = 1f;
		}
		else
		{
			IconPistol.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(false);
			IconPistol.IconSprite.alpha = 0.6f;
			IconPistol.Hex.alpha = 0.6f;
		}
		IconBuzz.SetIconSprite("icon_weapon_buzzblades", (IconScript.HexLevel)component3.GetWeaponUpgradeLevel(), component3.isOwned);
		if (component3.isOwned)
		{
			IconBuzz.GetComponent<BoxCollider>().enabled = true;
			IconBuzz.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(true);
		}
		else
		{
			IconBuzz.GetComponent<BoxCollider>().enabled = false;
			IconBuzz.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(false);
		}
		if (WeaponsManager.instance.curWeapIndex == 2)
		{
			IconBuzz.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(true);
			IconBuzz.IconSprite.alpha = 1f;
			IconBuzz.Hex.alpha = 1f;
		}
		else
		{
			IconBuzz.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(false);
			IconBuzz.IconSprite.alpha = 0.6f;
			IconBuzz.Hex.alpha = 0.6f;
		}
		IconShotgun.SetIconSprite("icon_weapon_shotgun", (IconScript.HexLevel)component2.GetWeaponUpgradeLevel(), component2.isOwned);
		if (component2.isOwned)
		{
			IconShotgun.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(true);
			IconShotgun.GetComponent<BoxCollider>().enabled = true;
		}
		else
		{
			IconShotgun.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(false);
			IconShotgun.GetComponent<BoxCollider>().enabled = false;
		}
		if (WeaponsManager.instance.curWeapIndex == 1)
		{
			IconShotgun.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(true);
			IconShotgun.IconSprite.alpha = 1f;
			IconShotgun.Hex.alpha = 1f;
		}
		else
		{
			IconShotgun.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(false);
			IconShotgun.IconSprite.alpha = 0.6f;
			IconShotgun.Hex.alpha = 0.6f;
		}
		IconPredator.SetIconSprite("icon_weapon_predator", (IconScript.HexLevel)component4.GetWeaponUpgradeLevel(), component4.isOwned);
		if (component4.isOwned)
		{
			IconPredator.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(true);
			IconPredator.GetComponent<BoxCollider>().enabled = true;
		}
		else
		{
			IconPredator.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(false);
			IconPredator.GetComponent<BoxCollider>().enabled = false;
		}
		if (WeaponsManager.instance.curWeapIndex == 3)
		{
			IconPredator.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(true);
			IconPredator.IconSprite.alpha = 1f;
			IconPredator.Hex.alpha = 1f;
		}
		else
		{
			IconPredator.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(false);
			IconPredator.IconSprite.alpha = 0.6f;
			IconPredator.Hex.alpha = 0.6f;
		}
		if (component5.isOwned)
		{
			IconRyno.SetIconSprite("icon_weapon_ryno", IconScript.HexLevel.HEX_V3, component5.isOwned);
			IconRyno.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(true);
			IconRyno.GetComponent<BoxCollider>().enabled = true;
		}
		else
		{
			IconRyno.SetIconSprite("icon_weapon_ryno", IconScript.HexLevel.HEX_V1, component5.isOwned);
			IconRyno.transform.Find("BackgroundContainer/IconSprite").gameObject.SetActive(false);
			IconRyno.GetComponent<BoxCollider>().enabled = false;
		}
		if (WeaponsManager.instance.curWeapIndex == 4)
		{
			IconRyno.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(true);
			IconRyno.IconSprite.alpha = 1f;
			IconRyno.Hex.alpha = 1f;
		}
		else
		{
			IconRyno.transform.Find("BackgroundContainer/Highlight").gameObject.SetActive(false);
			IconRyno.IconSprite.alpha = 0.6f;
			IconRyno.Hex.alpha = 0.6f;
		}
	}

	public void UpdateActiveChallenges()
	{
		ChallengesListGrid.UpdateActiveChallenges();
	}

	public void BuyAmmoButtonClicked()
	{
		UIManager.instance.PersistentUI.ShowPopup("PopupBuyAmmo", "UI_Menu_48", false);
		UIManager.instance.PersistentUI.SetPopupOKButtonCallback(RefillAmmo);
		uint ammo = CurrentWeapon.ammo;
		uint ammoMax = (uint)CurrentWeapon.ammoMax;
		UILabel component = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/AmmoLabelOffset/AmmoLabel").GetComponent<UILabel>();
		component.text = ammo + "/" + ammoMax;
		UILabel component2 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/BoltsLabel").GetComponent<UILabel>();
		UILabel component3 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/YourBoltsLabel").GetComponent<UILabel>();
		component2.text = AmmoBoltCost.ToString();
		component3.text = GameController.instance.playerController.GetBoltsTotal().ToString();
		Transform transform = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/YourBoltIcon");
		component3.GetComponent<LabelFit>().DoUpdate();
		transform.localPosition = new Vector3(component3.transform.localPosition.x + component3.relativeSize.x * component3.cachedTransform.localScale.x + 78f, component3.transform.localPosition.y, component3.transform.localPosition.z);
		UISlider component4 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/AmmoLabelOffset/Progress Bar").GetComponent<UISlider>();
		component4.sliderValue = (float)ammo / (float)ammoMax;
		UISlider component5 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/AmmoLabelOffset/NewAmmoBar").GetComponent<UISlider>();
		component5.sliderValue = (float)(ammo + AmmoToBuy) / (float)ammoMax;
	}

	public void RefillAmmo(GameObject obj)
	{
		uint ammo = CurrentWeapon.ammo;
		SFXManager.instance.PlaySound("UI_Purchase");
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.boltzSpent, AmmoBoltCost);
		if (CurrentWeapon.weaponName == "ConstructoPistol")
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.pistolReloads);
		}
		CurrentWeapon.ammo = ammo + AmmoToBuy;
		UIManager.instance.GetHUD().UpdateAmmo();
		UIManager.instance.GetHUD().UpdateBoltsCollected(true);
		UpdateSelectedWeapon();
	}

	public void UpdateAmmo()
	{
		Color color = new Color(1f, 1f, 1f, 1f);
		Color color2 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		uint ammo = CurrentWeapon.ammo;
		uint ammoMax = (uint)CurrentWeapon.ammoMax;
		WeaponAmmoLabel.text = ammo + "/" + ammoMax;
		AmmoBar.sliderValue = (float)ammo / (float)ammoMax;
		uint num = (uint)(GameController.instance.playerController.GetBoltsTotal() / CurrentWeapon.ammoBoltCost[CurrentWeapon.GetWeaponUpgradeLevel() - 1]);
		uint num2 = ammoMax - ammo;
		AmmoToBuy = (uint)Mathf.Min(num, num2);
		AmmoBoltCost = CurrentWeapon.ammoBoltCost[CurrentWeapon.GetWeaponUpgradeLevel() - 1] * AmmoToBuy;
		if (num != 0 && ammo != ammoMax)
		{
			BuyAmmoButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = true;
			BuyAmmoButton.UpdateColor(true, true);
			BuyAmmoButton.transform.Find("Label").GetComponent<UILabel>().color = color;
		}
		else
		{
			BuyAmmoButton.GetComponent<BoxCollider>().GetComponent<Collider>().enabled = false;
			BuyAmmoButton.UpdateColor(false, true);
			BuyAmmoButton.transform.Find("Label").GetComponent<UILabel>().color = color2;
		}
	}

	public void SwapWeapons(uint WeaponIndex, bool PlaySound = false)
	{
		CurrentWeapon = WeaponsManager.instance.WeapInventory[WeaponIndex].GetComponent<Weapon>();
		CurrentWeaponIndex = WeaponIndex;
		UpdateSelectedWeapon();
		UIManager.instance.GetHUD().UpdateAmmo();
		if (PlaySound)
		{
			if (CurrentWeapon.name != "WEP_RynoM")
			{
				SFXManager.instance.PlaySound(CurrentWeapon.Fire[CurrentWeapon.GetWeaponUpgradeLevel() - 1].name);
				return;
			}
			SFXManager.instance.PlaySound("RYNO_fire2");
			SFXManager.instance.PlaySound("RYNO_fire3");
			SFXManager.instance.PlaySound("RYNO_projectile3");
			SFXManager.instance.PlaySound("RYNO_fire2");
			SFXManager.instance.PlaySound("RYNO_fire3");
		}
	}

	private void UpdateSelectedWeapon()
	{
		if (CurrentWeapon == null)
		{
			CurrentWeapon = WeaponsManager.instance.WeapInventory[0].GetComponent<Weapon>();
		}
		WeaponsManager.instance.SwapWeapons(CurrentWeaponIndex, CurrentWeapon.GetWeaponUpgradeLevel() - 1);
		WeaponTitleLabel.GetComponent<UILocalize>().key = CurrentWeapon.LocKeyName;
		WeaponTitleLabel.GetComponent<UILocalize>().Localize();
		WeaponDescLabel.text = "Description for " + WeaponTitleLabel.text;
		WeaponLevelLabel.text = "v" + CurrentWeapon.GetWeaponUpgradeLevel();
		PreviewSprite.spriteName = CurrentWeapon.weaponName;
		PreviewSprite.MakePixelPerfect();
		UpdateAmmo();
		UIManager.instance.GetHUD().UpdateWeaponIcon();
		UpdateWeaponIcons();
		WeaponInfo.Init(CurrentWeapon, WeaponInfo.transform, false, true);
		UIManager.instance.GetHUD().GetWeaponTutorial();
	}

	public void UpdateScore()
	{
		txt_Score.text = GameController.instance.GetScore().ToString();
	}

	public void UpdateBoltsCollected()
	{
		int bolts = GameController.instance.playerController.GetBolts();
		txt_BoltsCollected.text = bolts.ToString();
		txt_RaritaniumCollected.text = GameController.instance.playerController.GetRaritanium().ToString();
		txt_HeroBolts.text = GameController.instance.playerController.GetHeroBoltsTotal().ToString();
		int terachnoidsTotal = GameController.instance.playerController.GetTerachnoidsTotal();
		if (terachnoidsTotal > 10)
		{
			txt_Terachnoids.text = terachnoidsTotal.ToString();
		}
		else
		{
			txt_Terachnoids.text = terachnoidsTotal + "/10";
		}
	}

	public void UpdateDistanceTraveled()
	{
		int num = (int)GameController.instance.playerController.GetTravelDist();
		txt_DistanceTraveled.text = num + "m";
	}

	public void OnMenuButtonClicked(GameObject obj)
	{
		UIManager.instance.PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_130", false);
		UIManager.instance.PersistentUI.SetPopupOKButtonCallback(MenuButtonOKButtonClicked);
	}

	private void MenuButtonOKButtonClicked(GameObject obj)
	{
		Time.timeScale = 1f;
		GameController.instance.playerController.Kill(PlayerController.EDeathDealer.EDeath_Quit);
		UIManager.instance.OpenMenu(UIManager.MenuPanels.StartMenu);
	}
}
