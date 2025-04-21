using System.Runtime.InteropServices;
using UnityEngine;

namespace Unibill.Impl
{
	public class StoreKitPluginImpl : IStoreKitPlugin
	{
		[DllImport("__Internal")]
		private static extern bool _storeKitPaymentsAvailable();

		[DllImport("__Internal")]
		private static extern void _storeKitRequestProductData(string productIdentifiers);

		[DllImport("__Internal")]
		private static extern void _storeKitPurchaseProduct(string productId);

		[DllImport("__Internal")]
		private static extern void _storeKitRestoreTransactions();

		public void initialise(AppleAppStoreBillingService svc)
		{
			GameObject gameObject = new GameObject();
			gameObject.AddComponent<AppleAppStoreCallbackMonoBehaviour>().initialise(svc);
		}

		public bool storeKitPaymentsAvailable()
		{
			return _storeKitPaymentsAvailable();
		}

		public void storeKitRequestProductData(string productIdentifiers)
		{
			_storeKitRequestProductData(productIdentifiers);
		}

		public void storeKitPurchaseProduct(string productId)
		{
			_storeKitPurchaseProduct(productId);
		}

		public void storeKitRestoreTransactions()
		{
			_storeKitRestoreTransactions();
		}
	}
}
