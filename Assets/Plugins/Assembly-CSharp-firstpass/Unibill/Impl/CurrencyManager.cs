using System.Collections.Generic;
using Uniject;

namespace Unibill.Impl
{
	public class CurrencyManager
	{
		private IStorage storage;

		public Dictionary<string, decimal> exchangeRates;

		public Dictionary<string, string> itemCurrencyMap;

		public string[] Currencies { get; private set; }

		public CurrencyManager(UnibillXmlParser parser, IStorage storage, ILogger logger)
		{
			this.storage = storage;
			List<string> list = new List<string>();
			exchangeRates = new Dictionary<string, decimal>();
			itemCurrencyMap = new Dictionary<string, string>();
			foreach (UnibillXmlParser.UnibillXElement item in parser.Parse("unibillInventory", "currency"))
			{
				string text = item.TryGetFirstElement("currencyId");
				list.Add(text);
				List<string> list2 = item.kvps["id"];
				List<string> list3 = item.kvps["amount"];
				for (int i = 0; i < list2.Count; i++)
				{
					string key = list2[i];
					string s = list3[i];
					itemCurrencyMap[key] = text;
					exchangeRates[key] = decimal.Parse(s);
				}
			}
			Currencies = list.ToArray();
		}

		public void OnPurchased(string id)
		{
			if (itemCurrencyMap.ContainsKey(id))
			{
				string id2 = itemCurrencyMap[id];
				decimal amount = exchangeRates[id];
				CreditBalance(id2, amount);
			}
		}

		public decimal GetCurrencyBalance(string id)
		{
			return storage.GetInt(getKey(id), 0);
		}

		public void CreditBalance(string id, decimal amount)
		{
			storage.SetInt(getKey(id), (int)(GetCurrencyBalance(id) + amount));
		}

		public bool DebitBalance(string id, decimal amount)
		{
			decimal currencyBalance = GetCurrencyBalance(id);
			if (currencyBalance - amount >= 0m)
			{
				storage.SetInt(getKey(id), (int)(currencyBalance - amount));
				return true;
			}
			return false;
		}

		private string getKey(string id)
		{
			return string.Format("com.outlinegames.unibill.currencies.{0}.balance", id);
		}
	}
}
