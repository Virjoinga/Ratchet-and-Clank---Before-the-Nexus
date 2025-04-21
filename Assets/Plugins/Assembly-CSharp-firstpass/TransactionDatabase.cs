using System;
using Uniject;

public class TransactionDatabase
{
	private IStorage storage;

	private ILogger logger;

	public string UserId { get; set; }

	public TransactionDatabase(IStorage storage, ILogger logger)
	{
		this.storage = storage;
		this.logger = logger;
		UserId = "default";
	}

	public int getPurchaseHistory(PurchasableItem item)
	{
		return storage.GetInt(getKey(item.Id), 0);
	}

	public void onPurchase(PurchasableItem item)
	{
		int purchaseHistory = getPurchaseHistory(item);
		if (item.PurchaseType != 0 && purchaseHistory != 0)
		{
			logger.LogWarning("Apparently multi purchased a non consumable:{0}", item.Id);
		}
		else
		{
			storage.SetInt(getKey(item.Id), purchaseHistory + 1);
		}
	}

	public void clearPurchases(PurchasableItem item)
	{
		storage.SetInt(getKey(item.Id), 0);
	}

	public void onRefunded(PurchasableItem item)
	{
		int purchaseHistory = getPurchaseHistory(item);
		purchaseHistory = Math.Max(0, purchaseHistory - 1);
		storage.SetInt(getKey(item.Id), purchaseHistory);
	}

	private string getKey(string fragment)
	{
		return string.Format("{0}.{1}", UserId, fragment);
	}
}
