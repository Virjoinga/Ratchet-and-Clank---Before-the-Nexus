using UnityEngine;

public class VendorListItemScript : MonoBehaviour
{
	private bool isSelected;

	public UILabel CostLabel;

	public UILabel LevelLabel;

	public UILabel NameLabel;

	public UISprite SelectedHighlight;

	public UISprite Background;

	public UISprite BoltIcon;

	public IconScript ItemIcon;

	public int ItemIndex;

	private UIVendor.VendorItemType ItemType;

	private UIVendor ParentUIVendor;

	public Color AffordableItemColor = new Color(0.9f, 0.9f, 0.9f, 1f);

	public Color UnAffordableItemColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private void Start()
	{
	}

	public void SetItemType(UIVendor.VendorItemType Type)
	{
		ItemType = Type;
	}

	public UIVendor.VendorItemType GetItemType()
	{
		return ItemType;
	}

	public void SetParentUI(UIVendor Vendor)
	{
		ParentUIVendor = Vendor;
	}

	public void Select()
	{
		isSelected = true;
		SelectedHighlight.enabled = true;
	}

	public void Unselect()
	{
		isSelected = false;
		SelectedHighlight.enabled = false;
	}

	private void OnClick()
	{
		if (!isSelected)
		{
			ParentUIVendor.SelectItem(ItemIndex);
		}
	}
}
