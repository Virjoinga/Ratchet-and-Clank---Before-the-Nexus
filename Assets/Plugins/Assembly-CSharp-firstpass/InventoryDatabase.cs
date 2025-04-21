using System;
using System.Collections.Generic;
using Unibill.Impl;
using Uniject;

public class InventoryDatabase
{
	private List<PurchasableItem> items;

	private ILogger logger;

	public List<PurchasableItem> AllPurchasableItems
	{
		get
		{
			return new List<PurchasableItem>(items);
		}
	}

	public List<PurchasableItem> AllNonConsumablePurchasableItems
	{
		get
		{
			return items.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.NonConsumable);
		}
	}

	public List<PurchasableItem> AllConsumablePurchasableItems
	{
		get
		{
			return items.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.Consumable);
		}
	}

	public List<PurchasableItem> AllSubscriptions
	{
		get
		{
			return items.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.Subscription);
		}
	}

	public List<PurchasableItem> AllNonSubscriptionPurchasableItems
	{
		get
		{
			return items.FindAll((PurchasableItem x) => x.PurchaseType != PurchaseType.Subscription);
		}
	}

	public InventoryDatabase(UnibillXmlParser parser, ILogger logger)
	{
		this.logger = logger;
		items = new List<PurchasableItem>();
		foreach (UnibillXmlParser.UnibillXElement item in parser.Parse("unibillInventory", "item"))
		{
			string value;
			item.attributes.TryGetValue("id", out value);
			PurchaseType purchaseType = (PurchaseType)(int)Enum.Parse(typeof(PurchaseType), item.attributes["purchaseType"]);
			string name = item.TryGetFirstElement("name");
			string description = item.TryGetFirstElement("description");
			items.Add(new PurchasableItem(value, purchaseType, name, description));
		}
	}

	public PurchasableItem getItemById(string id)
	{
		PurchasableItem purchasableItem = items.Find((PurchasableItem x) => x.Id == id);
		if (purchasableItem == null)
		{
			logger.LogWarning("Unknown purchasable item:{0}. Check your Unibill inventory configuration.", id);
		}
		return purchasableItem;
	}
}
