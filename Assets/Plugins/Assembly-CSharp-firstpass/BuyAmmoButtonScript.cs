using UnityEngine;

public class BuyAmmoButtonScript : MonoBehaviour
{
	public UIPauseMenu ParentUIPauseMenu;

	private void Start()
	{
	}

	private void OnClick()
	{
		ParentUIPauseMenu.BuyAmmoButtonClicked();
	}
}
