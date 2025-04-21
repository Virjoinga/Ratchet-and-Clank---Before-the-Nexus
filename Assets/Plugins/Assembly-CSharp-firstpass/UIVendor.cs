using System;
using UnityEngine;

public class UIVendor : UIScreen
{
	public enum VendorItemType
	{
		ITEM_Weapon = 0,
		ITEM_Gadget = 1,
		ITEM_Armor = 2,
		ITEM_Raritanium = 3
	}

	public VendorListItemScript ListItemRenderer;

	public UISprite WeaponPreviewSprite;

	public UISprite GadgetPreviewSprite;

	public UISprite RaritaniumPreviewSprite;

	public UISprite ArmorPreviewSprite;

	public UILabel txt_WeaponPreviewDesc;

	public UILabel txt_ArmorPreviewDesc;

	public UILabel txt_GadgetPreviewDesc;

	public UILabel txt_RaritaniumPreviewDesc;

	public UILabel txt_TotalBolts;

	public UILabel txt_TotalRaritanium;

	public UISprite WeaponPreviewImage;

	public UISprite ArmorPreviewImage;

	public UISprite GadgetPreviewImage;

	public UIButton BuyWeaponButton;

	public UIButton BuyGadgetButton;

	public UIButton BuyArmorButton;

	public UIButton BuyBoltsButton;

	public UIButton BuyRaritaniumButton;

	public UIButton EquipButton;

	public UIButton WeaponInfoButton;

	public UIGrid WeaponList;

	public UIGrid ArmorList;

	public UIGrid GadgetList;

	public UIGrid RaritaniumList;

	public UIGrid CurrentList;

	public UIDraggablePanel WeaponDraggablePanel;

	public UIDraggablePanel ArmorDraggablePanel;

	public UIDraggablePanel GadgetsDraggablePanel;

	public UIDraggablePanel RaritaniumDraggablePanel;

	public PanelManager HorizPanels;

	private int SelectedWeaponIndex;

	private int SelectedArmorIndex;

	private int SelectedGadgetIndex;

	private int SelectedRaritaniumIndex;

	private int CurrentIndex;

	private int TotalBolts;

	private int LastBolts;

	private int TotalRaritanium;

	private int LastRaritanium;

	private VendorListItemScript SelectedWeapon;

	private VendorListItemScript SelectedArmor;

	private VendorListItemScript SelectedGadget;

	private VendorListItemScript SelectedRaritanium;

	private VendorListItemScript SelectedItem;

	private VendorItemType BrowseItemType;

	private bool isPopulated;

	public PopupBoxScript ConfirmPanel;

	public BoltCrateRendererScript BoltCrateRenderer;

	private UIPanel ConfirmPanelInstance;

	private UIGrid BoltCrateGrid;

	public WeaponInfoScript WeaponInfoRenderer;

	public string[] RaritaniumDescLocs;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(BuyBoltsButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(BuyBoltsButtonClicked));
		UIEventListener uIEventListener2 = UIEventListener.Get(EquipButton.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(EquipButtonClicked));
		UIEventListener uIEventListener3 = UIEventListener.Get(WeaponInfoButton.gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(WeaponInfoButtonClicked));
	}

	private void SetupBoltCrates()
	{
		if (!(UIManager.instance.PersistentUI.Popup != ConfirmPanel))
		{
			return;
		}
		ConfirmPanel = UIManager.instance.PersistentUI.Popup;
		BoltCrateGrid = ConfirmPanel.transform.Find("ConfirmationBox/BoltCrateGridOffset/BoltCrateGrid").GetComponent<UIGrid>();
		GameObject go = ConfirmPanel.transform.Find("ConfirmationBox/CloseButtonOffset/CloseButton").gameObject;
		UIEventListener uIEventListener = UIEventListener.Get(go);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(ConfirmPanel.OverlayClicked));
		foreach (Transform item in BoltCrateGrid.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (InAppPurchaseManager.instance.isInitialized)
		{
			for (int i = 0; i < Unibiller.AllPurchasableItems.Length; i++)
			{
				BoltCrateRendererScript boltCrateRendererScript = (BoltCrateRendererScript)UnityEngine.Object.Instantiate(BoltCrateRenderer);
				boltCrateRendererScript.Init(Unibiller.AllPurchasableItems[i], BoltCrateGrid.transform);
				boltCrateRendererScript.BoltLabel.text = Unibiller.AllPurchasableItems[i].description.Split()[0];
				boltCrateRendererScript.CostLabel.text = Unibiller.AllPurchasableItems[i].localizedPriceString;
				boltCrateRendererScript.Icon.SetIconSprite("icon_bolt_crate", (IconScript.HexLevel)(i + 1), true, true);
				UIEventListener uIEventListener2 = UIEventListener.Get(boltCrateRendererScript.BuyButton.gameObject);
				uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(BuyBoltCrateButtonClicked));
			}
			BoltCrateGrid.Reposition();
			BoltCrateGrid.repositionNow = true;
		}
		UIManager.instance.SwapFont();
	}

	private void BuyBoltCrateButtonClicked(GameObject obj)
	{
		if (UIManager.instance.CheckWifi() && InAppPurchaseManager.instance.isInitialized)
		{
			BoltCrateRendererScript component = obj.transform.parent.GetComponent<BoltCrateRendererScript>();
			Debug.Log("Trying to buy product " + component.Item.Id);
			Unibiller.initiatePurchase(component.Item.Id);
		}
	}

	private void BuyBoltsButtonClicked(GameObject obj)
	{
		ShowBoltsPopup();
	}

	public void ShowBoltsPopup()
	{
		if (InAppPurchaseManager.instance.Available())
		{
			if (UIManager.instance.CheckWifi())
			{
				UIManager.instance.PersistentUI.ShowPopup("PopupBuyBolts", "UI_Menu_49", true);
				SetupBoltCrates();
			}
		}
		else
		{
			UIManager.instance.PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_188", true);
		}
	}

	private void BuyBoltsConfirmOKClicked(GameObject obj)
	{
		ConfirmPanel.Hide();
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

	public override void Show()
	{
		base.Show();
		LastBolts = 0;
		UpdateTotalBolts();
		HorizPanels.CycleCallback = ButtonCycleCallback;
		CurrentList = WeaponList;
		BrowseItemType = VendorItemType.ITEM_Weapon;
		PopulateAll();
		UpdateAll();
		SelectItem(0);
		BuyBoltsButtonEnable(InAppPurchaseManager.instance.isInitialized);
	}

	public void UpdateAll()
	{
		UpdateTotalBolts(true);
		UpdateWeapons();
		UpdateArmor();
		UpdateRaritanium();
		UpdateTotalRaritanium(true);
		UpdateGadgets();
	}

	private void PopulateAll()
	{
		if (!isPopulated)
		{
			PopulateArmor();
			PopulateWeapons();
			PopulateGadgets();
			PopulateRaritanium();
			UIManager.instance.SwapFont();
			isPopulated = true;
		}
	}

	private void UpdateTotalBolts(bool Instant = false)
	{
		TotalBolts = GameController.instance.playerController.GetBoltsTotal();
		txt_TotalBolts.text = TotalBolts.ToString();
		txt_TotalBolts.GetComponent<CounterScript>().StartCounter(LastBolts, TotalBolts);
		if (Instant)
		{
			txt_TotalBolts.GetComponent<CounterScript>().FinishCounter();
			txt_TotalBolts.text = TotalBolts.ToString();
		}
		LastBolts = TotalBolts;
	}

	private void UpdateTotalRaritanium(bool Instant = false)
	{
		TotalRaritanium = GameController.instance.playerController.GetLifetimeRaritaniumTotal();
		txt_TotalRaritanium.text = TotalRaritanium.ToString();
		txt_TotalRaritanium.GetComponent<CounterScript>().StartCounter(LastRaritanium, TotalRaritanium);
		if (Instant)
		{
			txt_TotalRaritanium.GetComponent<CounterScript>().FinishCounter();
		}
		LastRaritanium = TotalRaritanium;
	}

	private void InitListItem(VendorListItemScript Item, UIGrid ParentGrid, UIDraggablePanel DragPanel, int index, VendorItemType ItemType)
	{
		Item.transform.parent = ParentGrid.transform;
		Item.transform.localPosition = Vector3.zero;
		Item.transform.localScale = Vector3.one;
		Item.SetParentUI(this);
		Item.ItemIndex = index;
		Item.SetItemType(ItemType);
		Item.GetComponent<UIDragPanelContents>().draggablePanel = DragPanel;
	}

	private void PopulateArmor()
	{
		int num = 3;
		for (int i = 0; i < num; i++)
		{
			VendorListItemScript item = (VendorListItemScript)UnityEngine.Object.Instantiate(ListItemRenderer);
			InitListItem(item, ArmorList, ArmorDraggablePanel, i, VendorItemType.ITEM_Armor);
		}
		ArmorList.Reposition();
		UpdateArmor();
	}

	private void UpdateArmor()
	{
		Color color = new Color(1f, 0.1f, 0.1f, 1f);
		Color color2 = new Color(1f, 1f, 1f, 1f);
		VendorListItemScript component = ArmorList.transform.GetChild(0).GetComponent<VendorListItemScript>();
		component.NameLabel.GetComponent<UILocalize>().key = "UI_Menu_126";
		component.NameLabel.GetComponent<UILocalize>().Localize();
		component.NameLabel.GetComponent<UILocalize>().enabled = false;
		component.CostLabel.text = UIManager.instance.ArmorCosts[0].ToString();
		if (TotalBolts >= UIManager.instance.ArmorCosts[0])
		{
			component.CostLabel.color = color2;
			component.Background.color = component.AffordableItemColor;
		}
		else
		{
			component.CostLabel.color = color;
			component.Background.color = component.UnAffordableItemColor;
		}
		if (GameController.instance.playerController.GetInitialHP() == 3)
		{
			component.LevelLabel.GetComponent<UILocalize>().key = "UI_Menu_125";
			component.LevelLabel.GetComponent<UILocalize>().Localize();
			component.NameLabel.text += "\n";
		}
		else
		{
			component.LevelLabel.GetComponent<UILocalize>().key = "BLANK";
			component.LevelLabel.GetComponent<UILocalize>().Localize();
		}
		if (GameController.instance.playerController.GetInitialHP() >= 3)
		{
			component.CostLabel.enabled = false;
			component.Background.color = component.UnAffordableItemColor;
		}
		component.ItemIcon.SetIconSprite("icon_armor_holoflux", IconScript.HexLevel.HEX_V1, true, true);
		component = ArmorList.transform.GetChild(1).GetComponent<VendorListItemScript>();
		component.NameLabel.GetComponent<UILocalize>().key = "UI_Menu_127";
		component.NameLabel.GetComponent<UILocalize>().Localize();
		component.NameLabel.GetComponent<UILocalize>().enabled = false;
		component.CostLabel.text = UIManager.instance.ArmorCosts[1].ToString();
		if (TotalBolts >= UIManager.instance.ArmorCosts[1])
		{
			component.CostLabel.color = color2;
			component.Background.color = component.AffordableItemColor;
		}
		else
		{
			component.CostLabel.color = color;
			component.Background.color = component.UnAffordableItemColor;
		}
		if (GameController.instance.playerController.GetInitialHP() == 4)
		{
			component.LevelLabel.GetComponent<UILocalize>().key = "UI_Menu_125";
			component.LevelLabel.GetComponent<UILocalize>().Localize();
			component.NameLabel.text += "\n";
		}
		else
		{
			component.LevelLabel.GetComponent<UILocalize>().key = "BLANK";
			component.LevelLabel.GetComponent<UILocalize>().Localize();
		}
		if (GameController.instance.playerController.GetInitialHP() >= 4)
		{
			component.CostLabel.enabled = false;
			component.Background.color = component.UnAffordableItemColor;
		}
		component.ItemIcon.SetIconSprite("icon_armor_ectoflux", IconScript.HexLevel.HEX_V2, true, true);
		component = ArmorList.transform.GetChild(2).GetComponent<VendorListItemScript>();
		component.NameLabel.GetComponent<UILocalize>().key = "UI_Menu_128";
		component.NameLabel.GetComponent<UILocalize>().Localize();
		component.NameLabel.GetComponent<UILocalize>().enabled = false;
		component.CostLabel.text = UIManager.instance.ArmorCosts[2].ToString();
		if (TotalBolts >= UIManager.instance.ArmorCosts[2])
		{
			component.CostLabel.color = color2;
			component.Background.color = component.AffordableItemColor;
		}
		else
		{
			component.CostLabel.color = color;
			component.Background.color = component.UnAffordableItemColor;
		}
		if (GameController.instance.playerController.GetInitialHP() == 5)
		{
			component.LevelLabel.GetComponent<UILocalize>().key = "UI_Menu_125";
			component.LevelLabel.GetComponent<UILocalize>().Localize();
			component.NameLabel.text += "\n";
		}
		else
		{
			component.LevelLabel.GetComponent<UILocalize>().key = "BLANK";
			component.LevelLabel.GetComponent<UILocalize>().Localize();
		}
		if (GameController.instance.playerController.GetInitialHP() >= 5)
		{
			component.CostLabel.enabled = false;
			component.Background.color = component.UnAffordableItemColor;
		}
		component.ItemIcon.SetIconSprite("icon_armor_thermaflux", IconScript.HexLevel.HEX_V3, true, true);
		Transform transform = base.transform.FindChild("HorizScrollPanel/VendorSubMenuArmor/PreviewPane/SkinsContainer");
		if (!transform)
		{
			return;
		}
		for (int i = 0; i < transform.GetChildCount(); i++)
		{
			SelectSkinButtonScript component2 = transform.GetChild(i).GetComponent<SelectSkinButtonScript>();
			component2.transform.GetComponent<UICheckbox>().isChecked = false;
			if (component2.SkinString == GameController.instance.playerController.armorSkinString)
			{
				component2.transform.GetComponent<UICheckbox>().isChecked = true;
			}
		}
	}

	private void PopulateWeapons()
	{
		for (int i = 0; i < WeaponsManager.instance.WeapInventory.Length; i++)
		{
			VendorListItemScript item = (VendorListItemScript)UnityEngine.Object.Instantiate(ListItemRenderer);
			InitListItem(item, WeaponList, WeaponDraggablePanel, i, VendorItemType.ITEM_Weapon);
		}
		WeaponList.Reposition();
		UpdateWeapons();
	}

	private void UpdateWeapons()
	{
		Color color = new Color(1f, 0.1f, 0.1f, 1f);
		Color color2 = new Color(1f, 1f, 1f, 1f);
		for (int i = 0; i < WeaponList.transform.GetChildCount(); i++)
		{
			VendorListItemScript component = WeaponList.transform.GetChild(i).GetComponent<VendorListItemScript>();
			Weapon component2 = WeaponsManager.instance.WeapInventory[i].GetComponent<Weapon>();
			int num = (int)component2.GetWeaponUpgradeLevel();
			component.NameLabel.GetComponent<UILocalize>().key = component2.LocKeyName;
			component.NameLabel.GetComponent<UILocalize>().Localize();
			component.NameLabel.GetComponent<UILocalize>().enabled = false;
			if (WeaponsManager.instance.HaveBoughtWeapon((WeaponsManager.WeaponList)i) && i != 4)
			{
				component.NameLabel.text += "\n";
			}
			else
			{
				num = 0;
			}
			if (i != 4)
			{
				UpdateItemLevelStatus(component, num);
			}
			else
			{
				if (WeaponsManager.instance.HaveBoughtWeapon(WeaponsManager.WeaponList.WEP_RynoM))
				{
					num = 3;
				}
				UpdateItemLevelStatus(component, num, true);
			}
			component.ItemIcon.SetIconSprite(component2.spriteName, (IconScript.HexLevel)num, component2.isOwned, true);
			if (component2.GetWeaponUpgradeLevel() == 3 || i == 4)
			{
				component.CostLabel.gameObject.SetActive(false);
				component.BoltIcon.gameObject.SetActive(false);
				continue;
			}
			component.CostLabel.text = component2.upgradeBoltCost[num].ToString();
			if (TotalBolts >= component2.upgradeBoltCost[num])
			{
				component.CostLabel.color = color2;
				component.Background.color = component.AffordableItemColor;
			}
			else
			{
				component.CostLabel.color = color;
				component.Background.color = component.UnAffordableItemColor;
			}
		}
	}

	private void PopulateRaritanium()
	{
		for (int i = 0; i < 3; i++)
		{
			VendorListItemScript item = (VendorListItemScript)UnityEngine.Object.Instantiate(ListItemRenderer);
			InitListItem(item, RaritaniumList, RaritaniumDraggablePanel, i, VendorItemType.ITEM_Raritanium);
		}
		RaritaniumList.Reposition();
		UpdateRaritanium();
	}

	private void UpdateRaritanium()
	{
		Color color = new Color(1f, 0.1f, 0.1f, 1f);
		Color color2 = new Color(1f, 1f, 1f, 1f);
		UpdateTotalRaritanium();
		for (int i = 0; i < RaritaniumList.transform.GetChildCount(); i++)
		{
			VendorListItemScript component = RaritaniumList.transform.GetChild(i).GetComponent<VendorListItemScript>();
			component.NameLabel.GetComponent<UILocalize>().key = "UI_Menu_13";
			component.NameLabel.GetComponent<UILocalize>().Localize();
			component.NameLabel.GetComponent<UILocalize>().enabled = false;
			UILabel nameLabel = component.NameLabel;
			string text = nameLabel.text;
			nameLabel.text = text + " (" + UIManager.instance.RaritaniumValues[i] + ")";
			component.ItemIcon.SetIconSprite("icon_raritanium_crate", (IconScript.HexLevel)(i + 1), true, true);
			component.LevelLabel.GetComponent<UILocalize>().key = "BLANK";
			component.LevelLabel.GetComponent<UILocalize>().Localize();
			component.CostLabel.text = UIManager.instance.RaritaniumCosts[i].ToString();
			if (TotalBolts >= UIManager.instance.RaritaniumCosts[i])
			{
				component.CostLabel.color = color2;
				component.Background.color = component.AffordableItemColor;
			}
			else
			{
				component.CostLabel.color = color;
				component.Background.color = component.UnAffordableItemColor;
			}
		}
	}

	private void PopulateGadgets()
	{
		for (int i = 0; i < 7; i++)
		{
			VendorListItemScript item = (VendorListItemScript)UnityEngine.Object.Instantiate(ListItemRenderer);
			InitListItem(item, GadgetList, GadgetsDraggablePanel, i, VendorItemType.ITEM_Gadget);
		}
		UpdateGadgets();
	}

	private void UpdateGadgets()
	{
		for (int i = 0; i < 7; i++)
		{
			GadgetManager.eGadgets gadgetFromUnified = GadgetManager.GetGadgetFromUnified((GadgetManager.eUnifiedGadget)i);
			if (gadgetFromUnified != GadgetManager.eGadgets.g_NONE)
			{
				UpdateGadget(i, gadgetFromUnified);
			}
			else
			{
				UpdateMegaWeapon(i, GadgetManager.GetMegaFromUnified((GadgetManager.eUnifiedGadget)i));
			}
		}
	}

	private void UpdateGadget(int VendorItemIndex, GadgetManager.eGadgets Gadget)
	{
		Color color = new Color(1f, 0.1f, 0.1f, 1f);
		Color color2 = new Color(1f, 1f, 1f, 1f);
		VendorListItemScript component = GadgetList.transform.GetChild(VendorItemIndex).GetComponent<VendorListItemScript>();
		int num = GadgetManager.instance.GetGadgetLevel(Gadget);
		component.NameLabel.GetComponent<UILocalize>().key = GadgetManager.instance.GadgetLocKeys[(int)Gadget];
		component.NameLabel.GetComponent<UILocalize>().Localize();
		component.NameLabel.GetComponent<UILocalize>().enabled = false;
		if (GadgetManager.instance.HaveBoughtGadget(Gadget))
		{
			component.NameLabel.text += "\n";
		}
		else
		{
			num = 0;
		}
		if (num == 3)
		{
			component.CostLabel.gameObject.SetActive(false);
			component.BoltIcon.gameObject.SetActive(false);
		}
		else
		{
			int num2 = UIManager.instance.GadgetCosts[(int)Gadget, num];
			component.CostLabel.text = num2.ToString();
			if (TotalBolts >= UIManager.instance.GadgetCosts[(int)Gadget, num])
			{
				component.CostLabel.color = color2;
				component.Background.color = component.AffordableItemColor;
			}
			else
			{
				component.CostLabel.color = color;
				component.Background.color = component.UnAffordableItemColor;
			}
		}
		component.ItemIcon.SetIconSprite(GadgetManager.instance.GadgetSpriteNames[(int)Gadget], (IconScript.HexLevel)num, GadgetManager.instance.HaveBoughtGadget(Gadget), true);
		UpdateItemLevelStatus(component, num);
	}

	private void UpdateMegaWeapon(int VendorItemIndex, MegaWeaponManager.eMegaWeapons MegaWeap)
	{
		Color color = new Color(1f, 0.1f, 0.1f, 1f);
		Color color2 = new Color(1f, 1f, 1f, 1f);
		VendorListItemScript component = GadgetList.transform.GetChild(VendorItemIndex).GetComponent<VendorListItemScript>();
		int num = MegaWeaponManager.instance.GetMegaWeaponLevel(MegaWeap);
		component.NameLabel.GetComponent<UILocalize>().key = MegaWeaponManager.instance.MegaWeaponLocKeys[(int)MegaWeap];
		component.NameLabel.GetComponent<UILocalize>().Localize();
		component.NameLabel.GetComponent<UILocalize>().enabled = false;
		if (MegaWeaponManager.instance.HaveBoughtMegaWeapon(MegaWeap))
		{
			component.NameLabel.text += "\n";
		}
		else
		{
			num = 0;
		}
		if (num == 3)
		{
			component.CostLabel.gameObject.SetActive(false);
			component.BoltIcon.gameObject.SetActive(false);
		}
		else
		{
			int num2 = UIManager.instance.MegaWeaponCosts[(int)MegaWeap, num];
			component.CostLabel.text = num2.ToString();
			if (TotalBolts >= num2)
			{
				component.CostLabel.color = color2;
				component.Background.color = component.AffordableItemColor;
			}
			else
			{
				component.CostLabel.color = color;
				component.Background.color = component.UnAffordableItemColor;
			}
		}
		component.ItemIcon.SetIconSprite(MegaWeaponManager.instance.MegaWeaponSpriteNames[(int)MegaWeap], (IconScript.HexLevel)num, MegaWeaponManager.instance.HaveBoughtMegaWeapon(MegaWeap), true);
		UpdateItemLevelStatus(component, num);
	}

	private void UpdateItemLevelStatus(VendorListItemScript Item, int ItemLevel, bool BlankLevel = false)
	{
		if (ItemLevel <= 0 || BlankLevel)
		{
			Item.LevelLabel.GetComponent<UILocalize>().key = "BLANK";
			Item.LevelLabel.GetComponent<UILocalize>().Localize();
		}
		else
		{
			Item.LevelLabel.GetComponent<UILocalize>().enabled = false;
			Item.LevelLabel.text = "v" + ItemLevel;
		}
	}

	private void UpdateBuyButton(UIButton BuyButton, int ItemCost, bool OwnedItem, bool ItemMaxed)
	{
		string key = "UI_Menu_54";
		string key2 = "UI_Menu_56";
		Color color = new Color(1f, 1f, 1f, 1f);
		Color color2 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		UILabel component = BuyButton.transform.Find("Label").GetComponent<UILabel>();
		if (TotalBolts >= ItemCost && !ItemMaxed)
		{
			BuyButton.GetComponent<BoxCollider>().collider.enabled = true;
			component.color = color;
			BuyButton.UpdateColor(true, true);
		}
		else
		{
			BuyButton.GetComponent<BoxCollider>().collider.enabled = false;
			component.color = color2;
			BuyButton.UpdateColor(false, true);
		}
		if (OwnedItem)
		{
			component.GetComponent<UILocalize>().key = key2;
		}
		else
		{
			component.GetComponent<UILocalize>().key = key;
		}
		component.GetComponent<UILocalize>().Localize();
	}

	private void UpdateEquipButton()
	{
		Color color = new Color(1f, 1f, 1f, 1f);
		Color color2 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		bool flag = false;
		UILocalize component = EquipButton.transform.Find("Label").GetComponent<UILocalize>();
		uint curWeapIndex = WeaponsManager.instance.curWeapIndex;
		UILabel component2 = EquipButton.transform.Find("Label").GetComponent<UILabel>();
		if (SelectedWeaponIndex == curWeapIndex)
		{
			component.key = "UI_Menu_125";
		}
		else
		{
			component.key = "UI_Menu_156";
		}
		if (WeaponsManager.instance.WeapInventory[SelectedWeaponIndex].GetComponent<Weapon>().isOwned && SelectedWeaponIndex != curWeapIndex)
		{
			flag = true;
		}
		if (flag)
		{
			EquipButton.collider.enabled = true;
			EquipButton.UpdateColor(true, true);
			component2.color = color;
		}
		else
		{
			component2.color = color2;
			EquipButton.collider.enabled = false;
			EquipButton.UpdateColor(false, true);
		}
		component.Localize();
	}

	private void EquipButtonClicked(GameObject obj)
	{
		Weapon component = WeaponsManager.instance.WeapInventory[SelectedWeaponIndex].GetComponent<Weapon>();
		WeaponsManager.instance.SwapWeapons((uint)SelectedWeaponIndex, component.GetWeaponUpgradeLevel() - 1);
		if (component.Fire.Length > component.GetWeaponUpgradeLevel() - 1)
		{
			if (component.name != "WEP_RynoM")
			{
				SFXManager.instance.PlaySound(component.Fire[component.GetWeaponUpgradeLevel() - 1].name);
			}
			else
			{
				SFXManager.instance.PlaySound("RYNO_fire2");
				SFXManager.instance.PlaySound("RYNO_fire3");
				SFXManager.instance.PlaySound("RYNO_projectile3");
				SFXManager.instance.PlaySound("RYNO_fire2");
				SFXManager.instance.PlaySound("RYNO_fire3");
			}
		}
		UpdateEquipButton();
	}

	public void SelectItem(int ItemIndex)
	{
		int num = 0;
		bool itemMaxed = false;
		bool ownedItem = false;
		switch (BrowseItemType)
		{
		case VendorItemType.ITEM_Armor:
		{
			SelectedItem = ArmorList.transform.GetChild(ItemIndex).GetComponent<VendorListItemScript>();
			num = int.Parse(SelectedItem.CostLabel.text);
			SelectedArmor = SelectedItem;
			SelectedArmorIndex = SelectedArmor.ItemIndex;
			if (SelectedItem.LevelLabel.text == string.Empty)
			{
				if (GameController.instance.playerController.GetInitialHP() > SelectedArmorIndex + 3)
				{
					UpdateBuyButton(BuyArmorButton, num, false, true);
				}
				else
				{
					UpdateBuyButton(BuyArmorButton, num, false, false);
				}
			}
			else
			{
				UpdateBuyButton(BuyArmorButton, num, false, true);
			}
			CurrentList = ArmorList;
			CurrentIndex = SelectedArmorIndex;
			ArmorPreviewSprite.spriteName = SelectedItem.ItemIcon.SpriteName;
			ArmorPreviewSprite.MakePixelPerfect();
			UILocalize component2 = txt_ArmorPreviewDesc.GetComponent<UILocalize>();
			if (SelectedArmorIndex == 0)
			{
				component2.key = "UI_Menu_99";
			}
			else
			{
				component2.key = "UI_Menu_100";
			}
			component2.Localize();
			component2.enabled = false;
			txt_ArmorPreviewDesc.text = txt_ArmorPreviewDesc.text.Replace("{0}", (SelectedArmorIndex + 1).ToString());
			break;
		}
		case VendorItemType.ITEM_Weapon:
			SelectedItem = WeaponList.transform.GetChild(ItemIndex).GetComponent<VendorListItemScript>();
			num = int.Parse(SelectedItem.CostLabel.text);
			SelectedWeapon = SelectedItem;
			SelectedWeaponIndex = SelectedWeapon.ItemIndex;
			ownedItem = WeaponsManager.instance.HaveBoughtWeapon((WeaponsManager.WeaponList)SelectedWeaponIndex);
			itemMaxed = WeaponsManager.instance.WeapInventory[SelectedWeaponIndex].GetComponent<Weapon>().GetWeaponUpgradeLevel() >= 3;
			if (SelectedWeaponIndex == 4)
			{
				itemMaxed = true;
				if (!ownedItem)
				{
					UIManager.instance.PersistentUI.ShowPopup("PopupRYNOinfo", "UI_Tutorial_19", true);
					UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/Quantity").GetComponent<UILabel>().text = StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.terachnoidsCollected) + "/10";
				}
			}
			UpdateBuyButton(BuyWeaponButton, num, ownedItem, itemMaxed);
			CurrentList = WeaponList;
			CurrentIndex = SelectedWeaponIndex;
			txt_WeaponPreviewDesc.GetComponent<UILocalize>().key = WeaponsManager.instance.WeapInventory[SelectedWeaponIndex].GetComponent<Weapon>().LocKeyDesc;
			txt_WeaponPreviewDesc.GetComponent<UILocalize>().Localize();
			WeaponPreviewSprite.spriteName = WeaponsManager.instance.WeapInventory[SelectedWeaponIndex].GetComponent<Weapon>().weaponName;
			WeaponPreviewSprite.MakePixelPerfect();
			break;
		case VendorItemType.ITEM_Gadget:
		{
			SelectedItem = GadgetList.transform.GetChild(ItemIndex).GetComponent<VendorListItemScript>();
			num = int.Parse(SelectedItem.CostLabel.text);
			SelectedGadget = SelectedItem;
			SelectedGadgetIndex = SelectedGadget.ItemIndex;
			GadgetManager.eGadgets gadgetFromUnified = GadgetManager.GetGadgetFromUnified((GadgetManager.eUnifiedGadget)SelectedGadgetIndex);
			if (gadgetFromUnified != GadgetManager.eGadgets.g_NONE)
			{
				ownedItem = GadgetManager.instance.HaveBoughtGadget(gadgetFromUnified);
				itemMaxed = GadgetManager.instance.GetGadgetLevel(gadgetFromUnified) >= 3;
			}
			else
			{
				MegaWeaponManager.eMegaWeapons megaFromUnified = GadgetManager.GetMegaFromUnified((GadgetManager.eUnifiedGadget)SelectedGadgetIndex);
				if (megaFromUnified != MegaWeaponManager.eMegaWeapons.mw_NONE)
				{
					ownedItem = MegaWeaponManager.instance.HaveBoughtMegaWeapon(megaFromUnified);
					itemMaxed = MegaWeaponManager.instance.GetMegaWeaponLevel(megaFromUnified) >= 3;
				}
				else
				{
					Debug.LogError("UIVendor error. Selected Item is not a valid mega weapon");
				}
			}
			UpdateBuyButton(BuyGadgetButton, num, ownedItem, itemMaxed);
			CurrentList = GadgetList;
			CurrentIndex = SelectedGadgetIndex;
			txt_GadgetPreviewDesc.GetComponent<UILocalize>().key = GadgetManager.instance.GadgetLocKeyDescriptions[SelectedGadgetIndex];
			txt_GadgetPreviewDesc.GetComponent<UILocalize>().Localize();
			txt_GadgetPreviewDesc.GetComponent<UILocalize>().enabled = false;
			GadgetPreviewSprite.spriteName = SelectedItem.ItemIcon.SpriteName;
			GadgetPreviewSprite.MakePixelPerfect();
			break;
		}
		case VendorItemType.ITEM_Raritanium:
		{
			SelectedItem = RaritaniumList.transform.GetChild(ItemIndex).GetComponent<VendorListItemScript>();
			num = int.Parse(SelectedItem.CostLabel.text);
			UpdateBuyButton(BuyRaritaniumButton, num, false, false);
			SelectedRaritanium = SelectedItem;
			SelectedRaritaniumIndex = SelectedRaritanium.ItemIndex;
			CurrentList = RaritaniumList;
			CurrentIndex = SelectedRaritaniumIndex;
			RaritaniumPreviewSprite.spriteName = SelectedItem.ItemIcon.SpriteName;
			RaritaniumPreviewSprite.MakePixelPerfect();
			UILocalize component = txt_RaritaniumPreviewDesc.GetComponent<UILocalize>();
			component.key = RaritaniumDescLocs[SelectedRaritaniumIndex];
			component.Localize();
			component.enabled = false;
			txt_RaritaniumPreviewDesc.text = txt_RaritaniumPreviewDesc.text.Replace("{0}", UIManager.instance.RaritaniumValues[SelectedRaritaniumIndex].ToString());
			break;
		}
		default:
			Debug.LogError("UIVendor: Invalid item type for selected item!");
			break;
		}
		UpdateSelectedItem();
	}

	private void UpdateSelectedItem()
	{
		for (int i = 0; i < CurrentList.transform.GetChildCount(); i++)
		{
			VendorListItemScript component = CurrentList.transform.GetChild(i).GetComponent<VendorListItemScript>();
			if (i == CurrentIndex)
			{
				component.Select();
			}
			else
			{
				component.Unselect();
			}
		}
		UpdateEquipButton();
	}

	public void BuyButtonClicked()
	{
		if (SelectedItem.LevelLabel.text != string.Empty)
		{
			UIManager.instance.PersistentUI.ShowPopup("PopupBuyItem", "UI_Menu_132", false);
		}
		else
		{
			UIManager.instance.PersistentUI.ShowPopup("PopupBuyItem", "UI_Menu_131", false);
		}
		UISprite component = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/PreviewSprite").GetComponent<UISprite>();
		UISprite component2 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/PreviewSpriteHUD").GetComponent<UISprite>();
		if (HorizPanels.GetCurrentPanelIndex() == 0)
		{
			component.gameObject.SetActive(true);
			component2.gameObject.SetActive(false);
			component.spriteName = WeaponPreviewSprite.spriteName;
			component.MakePixelPerfect();
		}
		else
		{
			component.gameObject.SetActive(false);
			component2.gameObject.SetActive(true);
			component2.spriteName = SelectedItem.ItemIcon.SpriteName;
			component2.MakePixelPerfect();
		}
		IconScript component3 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/Icon").GetComponent<IconScript>();
		if (HorizPanels.GetCurrentPanelIndex() == 0 || HorizPanels.GetCurrentPanelIndex() == 1)
		{
			component3.SetIconSprite(SelectedItem.ItemIcon.SpriteName, SelectedItem.ItemIcon.HexType + 1, true, true);
		}
		else
		{
			component3.SetIconSprite(SelectedItem.ItemIcon.SpriteName, SelectedItem.ItemIcon.HexType, true, true);
		}
		UILabel component4 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/BoltsLabel").GetComponent<UILabel>();
		component4.text = SelectedItem.CostLabel.text;
		Transform transform = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/BoltIcon");
		transform.localPosition = new Vector3(component4.transform.localPosition.x + component4.relativeSize.x * component4.cachedTransform.localScale.x + 32f, component4.transform.localPosition.y, component4.transform.localPosition.z);
		UIManager.instance.PersistentUI.Popup.ConfirmText.text = UIManager.instance.PersistentUI.Popup.ConfirmText.text.Replace("{0}", "[FFCC66]" + SelectedItem.NameLabel.text.Replace("\n", string.Empty) + "[-]");
		UIManager.instance.PersistentUI.SetPopupOKButtonCallback(BuyOKButtonClicked);
	}

	public void BuyOKButtonClicked(GameObject obj)
	{
		VendorListItemScript vendorListItemScript = SelectedWeapon;
		switch (BrowseItemType)
		{
		case VendorItemType.ITEM_Armor:
		{
			vendorListItemScript = SelectedArmor;
			uint purchaseCost = uint.Parse(SelectedItem.CostLabel.text);
			if (TryPurchaseItem(purchaseCost))
			{
				int num = vendorListItemScript.ItemIndex + 1;
				PlayerPrefs.SetInt("ArmorLevel", num);
				PlayerPrefs.Save();
				GameController.instance.playerController.SetHP(2 + num);
				UpdateAll();
				SFXManager.instance.PlaySound("UI_purchase");
			}
			switch (vendorListItemScript.ItemIndex)
			{
			case 0:
				if (StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.lightArmorPurchased) == 0f)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.armorUpgraded);
				}
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.lightArmorPurchased);
				break;
			case 1:
				if (StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.heavyArmorPurchased) == 0f)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.armorUpgraded, 2f);
				}
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.heavyArmorPurchased);
				break;
			case 2:
				if (StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.nexusArmorPurchased) == 0f)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.armorUpgraded, 3f);
				}
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.nexusArmorPurchased);
				break;
			}
			break;
		}
		case VendorItemType.ITEM_Weapon:
		{
			vendorListItemScript = SelectedWeapon;
			Weapon component = WeaponsManager.instance.WeapInventory[SelectedWeaponIndex].GetComponent<Weapon>();
			uint num2 = component.GetWeaponUpgradeLevel();
			if (!WeaponsManager.instance.HaveBoughtWeapon((WeaponsManager.WeaponList)SelectedWeaponIndex))
			{
				num2 = 0u;
			}
			uint purchaseCost = component.upgradeBoltCost[num2];
			if (TryPurchaseItem(purchaseCost))
			{
				component.UpgradeWeapon();
				if (component.Fire.Length > component.GetWeaponUpgradeLevel() - 1)
				{
					SFXManager.instance.PlaySound(component.Fire[component.GetWeaponUpgradeLevel() - 1].name);
				}
				UpdateAll();
			}
			break;
		}
		case VendorItemType.ITEM_Gadget:
		{
			vendorListItemScript = SelectedGadget;
			uint purchaseCost = uint.Parse(SelectedItem.CostLabel.text);
			if (!TryPurchaseItem(purchaseCost))
			{
				break;
			}
			GadgetManager.eGadgets gadgetFromUnified = GadgetManager.GetGadgetFromUnified((GadgetManager.eUnifiedGadget)CurrentIndex);
			if (gadgetFromUnified != GadgetManager.eGadgets.g_NONE)
			{
				if (!GadgetManager.instance.HaveBoughtGadget(gadgetFromUnified))
				{
					SFXManager.instance.PlaySound("UI_purchase");
				}
				GadgetManager.instance.UpgradeGadget(gadgetFromUnified);
			}
			else
			{
				MegaWeaponManager.eMegaWeapons megaFromUnified = GadgetManager.GetMegaFromUnified((GadgetManager.eUnifiedGadget)CurrentIndex);
				if (megaFromUnified != MegaWeaponManager.eMegaWeapons.mw_NONE)
				{
					MegaWeaponManager.instance.UpgradeMegaWeapon(megaFromUnified);
				}
			}
			UpdateAll();
			break;
		}
		case VendorItemType.ITEM_Raritanium:
			vendorListItemScript = SelectedRaritanium;
			if (TryPurchaseItem((uint)UIManager.instance.RaritaniumCosts[CurrentIndex]))
			{
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.raritaniumPurchased, UIManager.instance.RaritaniumValues[CurrentIndex]);
				UpdateAll();
			}
			break;
		}
		Debug.Log(string.Concat("Trying to purchase ", BrowseItemType, " index: ", vendorListItemScript.ItemIndex));
		SelectItem(CurrentIndex);
		StatsTracker.instance.SaveStatsAndReset();
	}

	private bool TryPurchaseItem(uint PurchaseCost)
	{
		if (TotalBolts >= PurchaseCost)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.boltzSpent, PurchaseCost);
			switch (BrowseItemType)
			{
			case VendorItemType.ITEM_Weapon:
				return true;
			case VendorItemType.ITEM_Raritanium:
				if (Random.Range(0, 10) < 5)
				{
					SFXManager.instance.PlaySound("UI_raritanium_pickup_1");
				}
				else
				{
					SFXManager.instance.PlaySound("UI_raritanium_pickup_2");
				}
				return true;
			default:
				SFXManager.instance.PlaySound("UI_purchase");
				return true;
			}
		}
		return false;
	}

	private void ButtonCycleCallback()
	{
		int currentPanelIndex = HorizPanels.GetCurrentPanelIndex();
		BrowseItemType = (VendorItemType)currentPanelIndex;
		SelectItem(0);
		switch (BrowseItemType)
		{
		case VendorItemType.ITEM_Armor:
			EasyAnalytics.Instance.sendView("/ShopArmor");
			break;
		case VendorItemType.ITEM_Gadget:
			EasyAnalytics.Instance.sendView("/ShopGadgets");
			break;
		case VendorItemType.ITEM_Raritanium:
			EasyAnalytics.Instance.sendView("/ShopRaritanium");
			break;
		case VendorItemType.ITEM_Weapon:
			EasyAnalytics.Instance.sendView("/ShopWeapons");
			break;
		}
	}

	private void WeaponInfoButtonClicked(GameObject Obj)
	{
		Weapon component = WeaponsManager.instance.WeapInventory[SelectedWeaponIndex].GetComponent<Weapon>();
		UIGrid component2;
		if (component.GetWeaponUpgradeLevel() != 3 && SelectedWeaponIndex != 4)
		{
			UIManager.instance.PersistentUI.ShowPopup("PopupItemInfo", component.LocKeyName, true);
			component2 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/DescGrid").GetComponent<UIGrid>();
			foreach (Transform item in component2.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			WeaponInfoScript weaponInfoScript = (WeaponInfoScript)UnityEngine.Object.Instantiate(WeaponInfoRenderer);
			weaponInfoScript.Init(component, component2.transform, false);
			Weapon weapon = base.gameObject.AddComponent<Weapon>();
			weapon.LoadWeapData(component.weaponName, component.GetWeaponUpgradeLevel());
			WeaponInfoScript weaponInfoScript2 = (WeaponInfoScript)UnityEngine.Object.Instantiate(WeaponInfoRenderer);
			weaponInfoScript2.Init(weapon, component2.transform, true);
			UnityEngine.Object.Destroy(weapon);
		}
		else
		{
			UIManager.instance.PersistentUI.ShowPopup("PopupItemInfoMini", component.LocKeyName, true);
			component2 = UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/DescGrid").GetComponent<UIGrid>();
			foreach (Transform item2 in component2.transform)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
			WeaponInfoScript weaponInfoScript3 = (WeaponInfoScript)UnityEngine.Object.Instantiate(WeaponInfoRenderer);
			weaponInfoScript3.Init(component, component2.transform, false);
		}
		UIManager.instance.SwapFont();
		component2.Reposition();
		component2.repositionNow = true;
	}
}
