using System;
using System.Runtime.CompilerServices;
using Unibill;
using Unibill.Impl;

public class Unibiller
{
	private static Biller biller;

	public static UnibillError[] Errors
	{
		get
		{
			if (biller != null)
			{
				return biller.Errors.ToArray();
			}
			return new UnibillError[0];
		}
	}

	public static PurchasableItem[] AllPurchasableItems
	{
		get
		{
			return biller.InventoryDatabase.AllPurchasableItems.ToArray();
		}
	}

	public static PurchasableItem[] AllNonConsumablePurchasableItems
	{
		get
		{
			return biller.InventoryDatabase.AllNonConsumablePurchasableItems.ToArray();
		}
	}

	public static PurchasableItem[] AllConsumablePurchasableItems
	{
		get
		{
			return biller.InventoryDatabase.AllConsumablePurchasableItems.ToArray();
		}
	}

	public static PurchasableItem[] AllSubscriptions
	{
		get
		{
			return biller.InventoryDatabase.AllSubscriptions.ToArray();
		}
	}

	public static string[] AllCurrencies
	{
		get
		{
			return biller.CurrencyIdentifiers;
		}
	}

	[method: MethodImpl(32)]
	public static event Action<UnibillState> onBillerReady;

	[method: MethodImpl(32)]
	public static event Action<PurchasableItem> onPurchaseCancelled;

	[method: MethodImpl(32)]
	public static event Action<PurchasableItem> onPurchaseComplete;

	[method: MethodImpl(32)]
	public static event Action<PurchasableItem> onPurchaseFailed;

	[method: MethodImpl(32)]
	public static event Action<PurchasableItem> onPurchaseRefunded;

	[method: MethodImpl(32)]
	public static event Action<bool> onTransactionsRestored;

	public static void Initialise()
	{
		if (biller == null)
		{
			_internal_doInitialise(Biller.instantiate());
		}
	}

	public static PurchasableItem GetPurchasableItemById(string unibillPurchasableId)
	{
		if (biller != null)
		{
			return biller.InventoryDatabase.getItemById(unibillPurchasableId);
		}
		return null;
	}

	public static string[] GetAllPurchaseReceipts(PurchasableItem forItem)
	{
		if (biller != null)
		{
			return biller.getReceiptsForPurchasable(forItem);
		}
		return new string[0];
	}

	public static void initiatePurchase(PurchasableItem purchasable)
	{
		if (biller != null)
		{
			biller.purchase(purchasable);
		}
	}

	public static void initiatePurchase(string purchasableId)
	{
		if (biller != null)
		{
			biller.purchase(purchasableId);
		}
	}

	public static int GetPurchaseCount(PurchasableItem item)
	{
		if (biller != null)
		{
			return biller.getPurchaseHistory(item);
		}
		return 0;
	}

	public static int GetPurchaseCount(string purchasableId)
	{
		if (biller != null)
		{
			return biller.getPurchaseHistory(purchasableId);
		}
		return 0;
	}

	public static decimal GetCurrencyBalance(string currencyIdentifier)
	{
		return biller.getCurrencyBalance(currencyIdentifier);
	}

	public static void CreditBalance(string currencyIdentifier, decimal amount)
	{
		biller.creditCurrencyBalance(currencyIdentifier, amount);
	}

	public static bool DebitBalance(string currencyIdentifier, decimal amount)
	{
		return biller.debitCurrencyBalance(currencyIdentifier, amount);
	}

	public static void restoreTransactions()
	{
		if (biller != null)
		{
			biller.restoreTransactions();
		}
	}

	public static void clearTransactions()
	{
		if (biller != null)
		{
			biller.ClearPurchases();
		}
	}

	public static void _internal_doInitialise(Biller biller)
	{
		Unibiller.biller = biller;
		biller.onBillerReady += delegate(bool success)
		{
			if (Unibiller.onBillerReady != null)
			{
				if (success)
				{
					Unibiller.onBillerReady((biller.State != BillerState.INITIALISED) ? UnibillState.SUCCESS_WITH_ERRORS : UnibillState.SUCCESS);
				}
				else
				{
					Unibiller.onBillerReady(UnibillState.CRITICAL_ERROR);
				}
			}
		};
		biller.onPurchaseCancelled += _onPurchaseCancelled;
		biller.onPurchaseComplete += _onPurchaseComplete;
		biller.onPurchaseFailed += _onPurchaseFailed;
		biller.onPurchaseRefunded += _onPurchaseRefunded;
		biller.onTransactionsRestored += _onTransactionsRestored;
		biller.Initialise();
	}

	private static void _onPurchaseCancelled(PurchasableItem item)
	{
		if (Unibiller.onPurchaseCancelled != null)
		{
			Unibiller.onPurchaseCancelled(item);
		}
	}

	private static void _onPurchaseComplete(PurchasableItem item)
	{
		if (Unibiller.onPurchaseComplete != null)
		{
			Unibiller.onPurchaseComplete(item);
		}
	}

	private static void _onPurchaseFailed(PurchasableItem item)
	{
		if (Unibiller.onPurchaseFailed != null)
		{
			Unibiller.onPurchaseFailed(item);
		}
	}

	private static void _onPurchaseRefunded(PurchasableItem item)
	{
		if (Unibiller.onPurchaseRefunded != null)
		{
			Unibiller.onPurchaseRefunded(item);
		}
	}

	private static void _onTransactionsRestored(bool success)
	{
		if (Unibiller.onTransactionsRestored != null)
		{
			Unibiller.onTransactionsRestored(success);
		}
	}
}
