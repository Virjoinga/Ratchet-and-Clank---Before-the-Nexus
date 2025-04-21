using UnityEngine;

public class InAppPurchaseManager : MonoBehaviour
{
	public static InAppPurchaseManager instance;

	public bool isInitialized;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("InAppPurchaseManager: Multiple instances spawned");
			Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	public bool Available()
	{
		return isInitialized;
	}

	private void Start()
	{
		Unibiller.onBillerReady += onInitialised;
		Unibiller.onPurchaseComplete += purchaseComplete;
		Unibiller.onPurchaseCancelled += purchaseCancelled;
		Unibiller.onPurchaseFailed += purchaseFailed;
		Unibiller.Initialise();
	}

	private void onInitialised(UnibillState result)
	{
		if (result != UnibillState.CRITICAL_ERROR)
		{
			Debug.Log("isInitialized YES");
			isInitialized = true;
		}
		else
		{
			Debug.Log("onInitialized result=" + result);
		}
		UIVendor vendorMenu = UIManager.instance.GetVendorMenu();
		if (vendorMenu != null)
		{
			vendorMenu.BuyBoltsButtonEnable(isInitialized);
		}
		UIVendorFrontMenu vendorFrontMenu = UIManager.instance.GetVendorFrontMenu();
		if (vendorFrontMenu != null)
		{
			vendorFrontMenu.BuyBoltsButtonEnable(isInitialized);
		}
	}

	private void purchaseComplete(PurchasableItem purchased)
	{
		Debug.Log("purchase complete");
		GameController.instance.playerController.addItem(purchased);
		UIManager.instance.PersistentUI.HidePopup();
	}

	private void purchaseCancelled(PurchasableItem purchased)
	{
		Debug.Log("purchase Cancelled");
		UIManager.instance.PersistentUI.HidePopup();
	}

	private void purchaseFailed(PurchasableItem purchased)
	{
		Debug.Log("purchase Failed");
		UIManager.instance.PersistentUI.HidePopup();
	}
}
