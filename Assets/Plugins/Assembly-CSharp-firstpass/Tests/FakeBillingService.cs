using System.Collections.Generic;
using Unibill.Impl;

namespace Tests
{
	public class FakeBillingService : IBillingService
	{
		private IBillingServiceCallback biller;

		private List<string> purchasedItems = new List<string>();

		private ProductIdRemapper remapper;

		public bool reportError;

		public bool reportCriticalError;

		public bool purchaseCalled;

		public bool restoreCalled;

		public FakeBillingService(ProductIdRemapper remapper)
		{
			this.remapper = remapper;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			this.biller = biller;
			if (reportError)
			{
				biller.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_FAILED);
			}
			biller.onSetupComplete(!reportCriticalError);
		}

		public void purchase(string item)
		{
			purchaseCalled = true;
			if (remapper.getPurchasableItemFromPlatformSpecificId(item).PurchaseType == PurchaseType.NonConsumable)
			{
				purchasedItems.Add(item);
			}
			biller.onPurchaseSucceeded(item);
		}

		public void restoreTransactions()
		{
			restoreCalled = true;
			foreach (string purchasedItem in purchasedItems)
			{
				biller.onPurchaseSucceeded(purchasedItem);
			}
			biller.onTransactionsRestoredSuccess();
		}
	}
}
