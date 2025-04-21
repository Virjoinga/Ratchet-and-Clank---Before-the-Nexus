using Unibill.Impl;
using UnityEngine;

[AddComponentMenu("")]
public class AppleAppStoreCallbackMonoBehaviour : MonoBehaviour
{
	private AppleAppStoreBillingService callback;

	public void Awake()
	{
		base.gameObject.name = GetType().ToString();
		Object.DontDestroyOnLoad(this);
	}

	public void initialise(AppleAppStoreBillingService callback)
	{
		this.callback = callback;
	}

	public void onProductListReceived(string productList)
	{
		callback.onProductListReceived(productList);
	}

	public void onProductPurchaseSuccess(string productId)
	{
		callback.onPurchaseSucceeded(productId);
	}

	public void onProductPurchaseCancelled(string productId)
	{
		callback.onPurchaseCancelled(productId);
	}

	public void onProductPurchaseFailed(string productId)
	{
		callback.onPurchaseFailed(productId);
	}

	public void onTransactionsRestoredSuccess(string empty)
	{
		callback.onTransactionsRestoredSuccess();
	}

	public void onTransactionsRestoredFail(string error)
	{
		callback.onTransactionsRestoredFail(error);
	}

	public void onFailedToRetrieveProductList(string nop)
	{
		callback.onFailedToRetrieveProductList();
	}
}
