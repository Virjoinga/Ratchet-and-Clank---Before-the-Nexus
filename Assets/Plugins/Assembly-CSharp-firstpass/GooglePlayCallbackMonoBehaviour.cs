using Unibill.Impl;
using UnityEngine;

[AddComponentMenu("")]
public class GooglePlayCallbackMonoBehaviour : MonoBehaviour
{
	private GooglePlayBillingService callback;

	public void Awake()
	{
		base.gameObject.name = GetType().ToString();
		Object.DontDestroyOnLoad(this);
	}

	public void Initialise(GooglePlayBillingService callback)
	{
		this.callback = callback;
	}

	public void onProductListReceived(string json)
	{
		callback.onProductListReceived(json);
	}

	public void onBillingNotSupported()
	{
		callback.onBillingNotSupported();
	}

	public void onPurchaseSucceeded(string productId)
	{
		callback.onPurchaseSucceeded(productId);
	}

	public void onPurchaseCancelled(string productId)
	{
		callback.onPurchaseCancelled(productId);
	}

	public void onPurchaseRefunded(string productId)
	{
		callback.onPurchaseRefunded(productId);
	}

	public void onPurchaseFailed(string productId)
	{
		callback.onPurchaseFailed(productId);
	}

	public void onTransactionsRestored(string successString)
	{
		callback.onTransactionsRestored(successString);
	}

	public void onInvalidPublicKey(string publicKey)
	{
		callback.onInvalidPublicKey(publicKey);
	}
}
