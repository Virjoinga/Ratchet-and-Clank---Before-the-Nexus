using System.Collections.Generic;
using UnityEngine;

public class UIVendorFrontMenu : UIScreen
{
	public List<UITweener> ShowTweens;

	public UIButton BackButton;

	public UIButton WeaponsButton;

	public UIButton GadgetsButton;

	public UIButton ArmorButton;

	public UIButton RaritaniumButton;

	public UIButton BuyBoltsButton;

	public GameObject AffordableWeaponIndicator;

	public GameObject AffordableGadgetIndicator;

	private void Start()
	{
		UIEventListener.Get(BackButton.gameObject).onClick = BackButtonClicked;
		UIEventListener.Get(WeaponsButton.gameObject).onClick = WeaponButtonClicked;
		UIEventListener.Get(GadgetsButton.gameObject).onClick = GadgetsButtonClicked;
		UIEventListener.Get(ArmorButton.gameObject).onClick = ArmorButtonClicked;
		UIEventListener.Get(RaritaniumButton.gameObject).onClick = RaritaniumButtonClicked;
		UIEventListener.Get(BuyBoltsButton.gameObject).onClick = BuyBoltsButtonClicked;
	}

	public void BackButtonClicked(GameObject obj)
	{
		UIManager.instance.HideHexBG();
	}

	public override void Hide()
	{
		base.Hide();
	}

	private void WeaponButtonClicked(GameObject obj)
	{
		UIManager.instance.OpenMenu(UIManager.MenuPanels.VendorMenu);
		UIVendor vendorMenu = UIManager.instance.GetVendorMenu();
		if (vendorMenu != null)
		{
			vendorMenu.HorizPanels.SwitchMenu(0);
		}
	}

	private void GadgetsButtonClicked(GameObject obj)
	{
		UIManager.instance.OpenMenu(UIManager.MenuPanels.VendorMenu);
		UIVendor vendorMenu = UIManager.instance.GetVendorMenu();
		if (vendorMenu != null)
		{
			vendorMenu.HorizPanels.SwitchMenu(1);
		}
	}

	private void ArmorButtonClicked(GameObject obj)
	{
		UIManager.instance.OpenMenu(UIManager.MenuPanels.VendorMenu);
		UIVendor vendorMenu = UIManager.instance.GetVendorMenu();
		if (vendorMenu != null)
		{
			vendorMenu.HorizPanels.SwitchMenu(2);
		}
	}

	private void RaritaniumButtonClicked(GameObject obj)
	{
		UIManager.instance.OpenMenu(UIManager.MenuPanels.VendorMenu);
		UIVendor vendorMenu = UIManager.instance.GetVendorMenu();
		if (vendorMenu != null)
		{
			vendorMenu.HorizPanels.SwitchMenu(3);
		}
	}

	private void BuyBoltsButtonClicked(GameObject obj)
	{
		UIManager.instance.OpenMenu(UIManager.MenuPanels.VendorMenu);
		UIVendor vendorMenu = UIManager.instance.GetVendorMenu();
		if (vendorMenu != null)
		{
			vendorMenu.ShowBoltsPopup();
		}
	}

	public void BuyBoltsButtonEnable(bool on)
	{
		if (on)
		{
			BuyBoltsButton.gameObject.SetActive(true);
		}
		else
		{
			BuyBoltsButton.gameObject.SetActive(false);
		}
	}

	public void UpdateAffordableItemIndicator()
	{
		int affordableWeapons = UIManager.instance.GetAffordableWeapons();
		if (affordableWeapons > 0)
		{
			AffordableWeaponIndicator.SetActive(true);
		}
		else
		{
			AffordableWeaponIndicator.SetActive(false);
		}
		affordableWeapons = UIManager.instance.GetAffordableGadgets();
		if (affordableWeapons > 0)
		{
			AffordableGadgetIndicator.SetActive(true);
		}
		else
		{
			AffordableGadgetIndicator.SetActive(false);
		}
	}

	public override void Show()
	{
		base.Show();
		EasyAnalytics.Instance.sendView("/ShopMenu");
		for (int i = 0; i < ShowTweens.Count; i++)
		{
			ShowTweens[i].Reset();
			ShowTweens[i].Play(true);
		}
		UIManager.instance.ShowBG(true);
		UIManager.instance.ShowHexBG();
		MusicManager.instance.Play(MusicManager.eMusicTrackType.Menu, false, 0f);
		SFXManager.instance.StopSound("UI_Tally");
		UpdateAffordableItemIndicator();
		BuyBoltsButtonEnable(InAppPurchaseManager.instance.isInitialized);
	}
}
