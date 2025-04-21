using System.IO;

namespace Unibill.Impl
{
	public class FakeRawAmazonAppStoreBillingInterface : IRawAmazonAppStoreBillingInterface
	{
		private AmazonAppStoreBillingService amazon;

		public void initialise(AmazonAppStoreBillingService amazon)
		{
			this.amazon = amazon;
		}

		public void initiateItemDataRequest(string[] productIds)
		{
			amazon.onProductListReceived(File.ReadAllText("../../../data/requestProductsResponse.json"));
		}

		public void initiatePurchaseRequest(string productId)
		{
			amazon.onPurchaseSucceeded(productId);
		}

		public void restoreTransactions()
		{
			amazon.onTransactionsRestored("true");
		}
	}
}
