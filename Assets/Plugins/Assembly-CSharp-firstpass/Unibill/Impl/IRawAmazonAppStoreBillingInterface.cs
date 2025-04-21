namespace Unibill.Impl
{
	public interface IRawAmazonAppStoreBillingInterface
	{
		void initialise(AmazonAppStoreBillingService amazon);

		void initiateItemDataRequest(string[] productIds);

		void initiatePurchaseRequest(string productId);

		void restoreTransactions();
	}
}
