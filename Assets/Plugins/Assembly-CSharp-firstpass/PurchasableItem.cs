using System;

public class PurchasableItem : IEquatable<PurchasableItem>
{
	internal class Writer
	{
		public static void setLocalizedPrice(PurchasableItem item, decimal price)
		{
			item.localizedPrice = price;
			item.localizedPriceString = price.ToString();
		}

		public static void setLocalizedPrice(PurchasableItem item, string price)
		{
			item.localizedPriceString = price;
		}

		public static void setLocalizedTitle(PurchasableItem item, string title)
		{
			item.localizedTitle = title;
		}

		public static void setLocalizedDescription(PurchasableItem item, string description)
		{
			item.localizedDescription = description;
		}
	}

	public string Id { get; private set; }

	public PurchaseType PurchaseType { get; private set; }

	public string name { get; private set; }

	public string description { get; private set; }

	public decimal localizedPrice { get; private set; }

	public string localizedPriceString { get; private set; }

	public string localizedTitle { get; private set; }

	public string localizedDescription { get; private set; }

	internal PurchasableItem(string id, PurchaseType purchaseType, string name, string description)
	{
		Id = id;
		PurchaseType = purchaseType;
		this.name = name;
		this.description = description;
	}

	public bool Equals(PurchasableItem other)
	{
		return other.Id == Id;
	}
}
