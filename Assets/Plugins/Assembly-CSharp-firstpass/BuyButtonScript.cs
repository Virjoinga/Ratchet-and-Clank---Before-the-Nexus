using UnityEngine;

public class BuyButtonScript : MonoBehaviour
{
	public UIVendor ParentUIVendor;

	private void Start()
	{
	}

	private void OnClick()
	{
		ParentUIVendor.BuyButtonClicked();
	}
}
