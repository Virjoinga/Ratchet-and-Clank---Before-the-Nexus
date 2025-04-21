using System.Collections;
using System.Collections.Generic;
using Uniject;

namespace Unibill.Impl
{
	public class AmazonAppStoreBillingService : IBillingService
	{
		private IBillingServiceCallback callback;

		private ProductIdRemapper remapper;

		private InventoryDatabase db;

		private ILogger logger;

		private IRawAmazonAppStoreBillingInterface amazon;

		private HashSet<string> unknownAmazonProducts = new HashSet<string>();

		private TransactionDatabase tDb;

		public AmazonAppStoreBillingService(IRawAmazonAppStoreBillingInterface amazon, ProductIdRemapper remapper, InventoryDatabase db, TransactionDatabase tDb, ILogger logger)
		{
			this.remapper = remapper;
			this.db = db;
			this.logger = logger;
			logger.prefix = "UnibillAmazonBillingService";
			this.amazon = amazon;
			this.tDb = tDb;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			callback = biller;
			amazon.initialise(this);
			amazon.initiateItemDataRequest(remapper.getAllPlatformSpecificProductIds());
		}

		public void purchase(string item)
		{
			if (unknownAmazonProducts.Contains(item))
			{
				callback.logError(UnibillError.AMAZONAPPSTORE_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_AMAZON, item);
				callback.onPurchaseFailedEvent(item);
			}
			else
			{
				amazon.initiatePurchaseRequest(item);
			}
		}

		public void restoreTransactions()
		{
			amazon.restoreTransactions();
		}

		public void onSDKAvailable(string isSandbox)
		{
			bool flag = bool.Parse(isSandbox);
			logger.Log("Running against {0} Amazon environment", (!flag) ? "PRODUCTION" : "SANDBOX");
		}

		public void onGetItemDataFailed()
		{
			callback.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_FAILED);
			callback.onSetupComplete(true);
		}

		public void onProductListReceived(string productListString)
		{
			Hashtable hashtable = (Hashtable)MiniJSON.jsonDecode(productListString);
			if (hashtable.Count == 0)
			{
				callback.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_NO_PRODUCTS_RETURNED);
				callback.onSetupComplete(false);
				return;
			}
			HashSet<PurchasableItem> hashSet = new HashSet<PurchasableItem>();
			foreach (object key in hashtable.Keys)
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(key.ToString());
				Hashtable hashtable2 = (Hashtable)hashtable[key];
				PurchasableItem.Writer.setLocalizedPrice(purchasableItemFromPlatformSpecificId, hashtable2["price"].ToString());
				PurchasableItem.Writer.setLocalizedTitle(purchasableItemFromPlatformSpecificId, (string)hashtable2["localizedTitle"]);
				PurchasableItem.Writer.setLocalizedDescription(purchasableItemFromPlatformSpecificId, (string)hashtable2["localizedDescription"]);
				hashSet.Add(purchasableItemFromPlatformSpecificId);
			}
			HashSet<PurchasableItem> hashSet2 = new HashSet<PurchasableItem>(db.AllPurchasableItems);
			hashSet2.ExceptWith(hashSet);
			if (hashSet2.Count > 0)
			{
				foreach (PurchasableItem item in hashSet2)
				{
					unknownAmazonProducts.Add(remapper.mapItemIdToPlatformSpecificId(item));
					callback.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_MISSING_PRODUCT, item.Id, remapper.mapItemIdToPlatformSpecificId(item));
				}
			}
			callback.onSetupComplete(true);
		}

		public void onUserIdRetrieved(string userId)
		{
			tDb.UserId = userId;
		}

		public void onTransactionsRestored(string successString)
		{
			if (bool.Parse(successString))
			{
				callback.onTransactionsRestoredSuccess();
			}
			else
			{
				callback.onTransactionsRestoredFail(string.Empty);
			}
		}

		public void onPurchaseFailed(string item)
		{
			callback.onPurchaseFailedEvent(item);
		}

		public void onPurchaseCancelled(string item)
		{
			callback.onPurchaseCancelledEvent(item);
		}

		public void onPurchaseSucceeded(string json)
		{
			Hashtable hashtable = (Hashtable)MiniJSON.jsonDecode(json);
			string platformSpecificId = (string)hashtable["productId"];
			string receipt = (string)hashtable["purchaseToken"];
			callback.onPurchaseSucceeded(platformSpecificId, receipt);
		}

		public void onPurchaseUpdateFailed()
		{
			logger.LogWarning("AmazonAppStoreBillingService: onPurchaseUpdate() failed.");
		}

		public void onPurchaseUpdateSuccess(string data)
		{
			List<string> revoked = new List<string>();
			List<string> purchased = new List<string>();
			parsePurchaseUpdates(revoked, purchased, data);
			onPurchaseUpdateSucceeded(revoked, purchased);
		}

		public void onPurchaseUpdateSucceeded(List<string> revoked, List<string> purchased)
		{
			foreach (string item in revoked)
			{
				callback.onPurchaseRefundedEvent(item);
			}
			foreach (string item2 in purchased)
			{
				callback.onPurchaseSucceeded(item2);
			}
		}

		public static void parsePurchaseUpdates(List<string> revoked, List<string> purchased, string data)
		{
			string[] array = data.Split('|');
			revoked.AddRange(array[0].Split(','));
			purchased.AddRange(array[1].Split(','));
			revoked.RemoveAll((string x) => x == string.Empty);
			purchased.RemoveAll((string x) => x == string.Empty);
		}
	}
}
