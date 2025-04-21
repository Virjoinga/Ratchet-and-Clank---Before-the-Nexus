namespace Unibill.Impl
{
	public interface IStoreKitPlugin
	{
		void initialise(AppleAppStoreBillingService callback);

		bool storeKitPaymentsAvailable();

		void storeKitRequestProductData(string productIdentifiers);

		void storeKitPurchaseProduct(string productId);

		void storeKitRestoreTransactions();
	}
}
