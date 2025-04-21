using System.Collections.Generic;

namespace Unibill.Impl
{
	public class ProductIdRemapper
	{
		private Dictionary<string, string> genericToPlatformSpecificIds;

		private Dictionary<string, string> platformSpecificToGenericIds;

		private UnibillXmlParser parser;

		public InventoryDatabase db { get; private set; }

		public ProductIdRemapper(InventoryDatabase db, UnibillXmlParser parser, UnibillConfiguration config)
		{
			this.db = db;
			this.parser = parser;
			initialiseForPlatform(config.CurrentPlatform);
		}

		public void initialiseForPlatform(BillingPlatform platform)
		{
			genericToPlatformSpecificIds = new Dictionary<string, string>();
			platformSpecificToGenericIds = new Dictionary<string, string>();
			string text = string.Format("{0}.Id", platform);
			foreach (UnibillXmlParser.UnibillXElement item in parser.Parse("unibillInventory", "item"))
			{
				string text2 = item.attributes["id"];
				string text3 = text2;
				if (item.kvps.ContainsKey(text))
				{
					text3 = item.TryGetFirstElement(text);
				}
				genericToPlatformSpecificIds[text2] = text3;
				platformSpecificToGenericIds[text3] = text2;
			}
		}

		public string[] getAllPlatformSpecificProductIds()
		{
			List<string> list = new List<string>();
			foreach (PurchasableItem allPurchasableItem in db.AllPurchasableItems)
			{
				list.Add(mapItemIdToPlatformSpecificId(allPurchasableItem));
			}
			return list.ToArray();
		}

		public string mapItemIdToPlatformSpecificId(PurchasableItem item)
		{
			return genericToPlatformSpecificIds[item.Id];
		}

		public PurchasableItem getPurchasableItemFromPlatformSpecificId(string platformSpecificId)
		{
			string id = platformSpecificToGenericIds[platformSpecificId];
			return db.getItemById(id);
		}

		public bool canMapProductSpecificId(string id)
		{
			if (platformSpecificToGenericIds.ContainsKey(id))
			{
				return true;
			}
			return false;
		}
	}
}
