using System.Collections;
using System.Collections.Generic;
using Uniject;

namespace Unibill.Impl
{
	public class GooglePlayBillingService : IBillingService
	{
		private string publicKey;

		private IRawGooglePlayInterface rawInterface;

		private IBillingServiceCallback callback;

		private ProductIdRemapper remapper;

		private InventoryDatabase db;

		private ILogger logger;

		private HashSet<string> unknownAmazonProducts = new HashSet<string>();

		public GooglePlayBillingService(IRawGooglePlayInterface rawInterface, UnibillConfiguration config, ProductIdRemapper remapper, InventoryDatabase db, ILogger logger)
		{
			this.rawInterface = rawInterface;
			publicKey = config.GooglePlayPublicKey;
			this.remapper = remapper;
			this.db = db;
			this.logger = logger;
		}

		public void initialise(IBillingServiceCallback callback)
		{
			this.callback = callback;
			if (publicKey == null || publicKey.Equals("[Your key]"))
			{
				callback.logError(UnibillError.GOOGLEPLAY_PUBLICKEY_NOTCONFIGURED, publicKey);
				callback.onSetupComplete(false);
				return;
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add("publicKey", publicKey);
			ArrayList arrayList = new ArrayList();
			foreach (PurchasableItem allPurchasableItem in db.AllPurchasableItems)
			{
				Hashtable hashtable2 = new Hashtable();
				hashtable2.Add("productId", remapper.mapItemIdToPlatformSpecificId(allPurchasableItem));
				hashtable2.Add("consumable", allPurchasableItem.PurchaseType == PurchaseType.Consumable);
				arrayList.Add(hashtable2);
			}
			hashtable.Add("products", arrayList);
			string text = hashtable.toJson();
			rawInterface.initialise(this, text);
		}

		public void restoreTransactions()
		{
			rawInterface.restoreTransactions();
		}

		public void purchase(string item)
		{
			if (unknownAmazonProducts.Contains(item))
			{
				callback.logError(UnibillError.GOOGLEPLAY_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_GOOGLEPLAY, item);
				callback.onPurchaseFailedEvent(item);
			}
			else
			{
				rawInterface.purchase(item);
			}
		}

		public void onBillingNotSupported()
		{
			callback.logError(UnibillError.GOOGLEPLAY_BILLING_UNAVAILABLE);
			callback.onSetupComplete(false);
		}

		public void onPurchaseSucceeded(string json)
		{
			Hashtable hashtable = (Hashtable)MiniJSON.jsonDecode(json);
			callback.onPurchaseSucceeded((string)hashtable["productId"], (string)hashtable["signature"]);
		}

		public void onPurchaseCancelled(string item)
		{
			callback.onPurchaseCancelledEvent(item);
		}

		public void onPurchaseRefunded(string item)
		{
			callback.onPurchaseRefundedEvent(item);
		}

		public void onPurchaseFailed(string item)
		{
			callback.onPurchaseFailedEvent(item);
		}

		public void onTransactionsRestored(string success)
		{
			if (bool.Parse(success))
			{
				callback.onTransactionsRestoredSuccess();
			}
			else
			{
				callback.onTransactionsRestoredFail(string.Empty);
			}
		}

		public void onInvalidPublicKey(string key)
		{
			callback.logError(UnibillError.GOOGLEPLAY_PUBLICKEY_INVALID, key);
			callback.onSetupComplete(false);
		}

		public void onProductListReceived(string productListString)
		{
			Hashtable hashtable = (Hashtable)MiniJSON.jsonDecode(productListString);
			if (hashtable.Count == 0)
			{
				callback.logError(UnibillError.GOOGLEPLAY_NO_PRODUCTS_RETURNED);
				callback.onSetupComplete(false);
				return;
			}
			HashSet<PurchasableItem> hashSet = new HashSet<PurchasableItem>();
			foreach (object key in hashtable.Keys)
			{
				if (remapper.canMapProductSpecificId(key.ToString()))
				{
					PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(key.ToString());
					Hashtable hashtable2 = (Hashtable)hashtable[key];
					PurchasableItem.Writer.setLocalizedPrice(purchasableItemFromPlatformSpecificId, hashtable2["price"].ToString());
					PurchasableItem.Writer.setLocalizedTitle(purchasableItemFromPlatformSpecificId, (string)hashtable2["localizedTitle"]);
					PurchasableItem.Writer.setLocalizedDescription(purchasableItemFromPlatformSpecificId, (string)hashtable2["localizedDescription"]);
					hashSet.Add(purchasableItemFromPlatformSpecificId);
				}
				else
				{
					logger.LogError("Warning: Unknown product identifier: {0}", key.ToString());
				}
			}
			HashSet<PurchasableItem> hashSet2 = new HashSet<PurchasableItem>(db.AllPurchasableItems);
			hashSet2.ExceptWith(hashSet);
			if (hashSet2.Count > 0)
			{
				foreach (PurchasableItem item in hashSet2)
				{
					unknownAmazonProducts.Add(remapper.mapItemIdToPlatformSpecificId(item));
					callback.logError(UnibillError.GOOGLEPLAY_MISSING_PRODUCT, item.Id, remapper.mapItemIdToPlatformSpecificId(item));
				}
			}
			callback.onSetupComplete(true);
		}
	}
}
