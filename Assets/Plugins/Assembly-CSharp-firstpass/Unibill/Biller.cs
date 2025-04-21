using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Xml;
using Tests;
using Unibill.Impl;
using Uniject;
using Uniject.Impl;
using UnityEngine;

namespace Unibill
{
	public class Biller : IBillingServiceCallback
	{
		private TransactionDatabase transactionDatabase;

		private Uniject.ILogger logger;

		private HelpCentre help;

		private ProductIdRemapper remapper;

		private Dictionary<PurchasableItem, List<string>> receiptMap = new Dictionary<PurchasableItem, List<string>>();

		private CurrencyManager currencyManager;

		private static CurrencyManager _currencyManager;

		private static TransactionDatabase _tDb;

		private static IStorage _storage;

		private static HelpCentre _helpCentre;

		private static InventoryDatabase _inventory;

		private static ProductIdRemapper _remapper;

		private static UnibillConfiguration _config;

		private static IResourceLoader _resourceLoader;

		public InventoryDatabase InventoryDatabase { get; private set; }

		public IBillingService billingSubsystem { get; private set; }

		public BillerState State { get; private set; }

		public List<UnibillError> Errors { get; private set; }

		public bool Ready
		{
			get
			{
				return State == BillerState.INITIALISED || State == BillerState.INITIALISED_WITH_ERROR;
			}
		}

		public string[] CurrencyIdentifiers
		{
			get
			{
				return getCurrencyManager().Currencies;
			}
		}

		[method: MethodImpl(32)]
		public event Action<bool> onBillerReady;

		[method: MethodImpl(32)]
		public event Action<PurchasableItem> onPurchaseComplete;

		[method: MethodImpl(32)]
		public event Action<bool> onTransactionsRestored;

		[method: MethodImpl(32)]
		public event Action<PurchasableItem> onPurchaseCancelled;

		[method: MethodImpl(32)]
		public event Action<PurchasableItem> onPurchaseRefunded;

		[method: MethodImpl(32)]
		public event Action<PurchasableItem> onPurchaseFailed;

		public Biller(InventoryDatabase db, TransactionDatabase tDb, IBillingService billingSubsystem, Uniject.ILogger logger, HelpCentre help, ProductIdRemapper remapper, CurrencyManager currencyManager)
		{
			InventoryDatabase = db;
			transactionDatabase = tDb;
			this.billingSubsystem = billingSubsystem;
			this.logger = logger;
			logger.prefix = "UnibillBiller";
			this.help = help;
			Errors = new List<UnibillError>();
			this.remapper = remapper;
			this.currencyManager = currencyManager;
		}

		public void Initialise()
		{
			if (InventoryDatabase.AllPurchasableItems.Count == 0)
			{
				logError(UnibillError.UNIBILL_NO_PRODUCTS_DEFINED);
				onSetupComplete(false);
			}
			else
			{
				billingSubsystem.initialise(this);
			}
		}

		public int getPurchaseHistory(PurchasableItem item)
		{
			return transactionDatabase.getPurchaseHistory(item);
		}

		public int getPurchaseHistory(string purchasableId)
		{
			return getPurchaseHistory(InventoryDatabase.getItemById(purchasableId));
		}

		public decimal getCurrencyBalance(string identifier)
		{
			return getCurrencyManager().GetCurrencyBalance(identifier);
		}

		public void creditCurrencyBalance(string identifier, decimal amount)
		{
			getCurrencyManager().CreditBalance(identifier, amount);
		}

		public bool debitCurrencyBalance(string identifier, decimal amount)
		{
			return getCurrencyManager().DebitBalance(identifier, amount);
		}

		public string[] getReceiptsForPurchasable(PurchasableItem item)
		{
			if (receiptMap.ContainsKey(item))
			{
				return receiptMap[item].ToArray();
			}
			return new string[0];
		}

		public void purchase(PurchasableItem item)
		{
			if (State == BillerState.INITIALISING)
			{
				logError(UnibillError.BILLER_NOT_READY);
				return;
			}
			if (State == BillerState.INITIALISED_WITH_CRITICAL_ERROR)
			{
				logError(UnibillError.UNIBILL_INITIALISE_FAILED_WITH_CRITICAL_ERROR);
				return;
			}
			if (item == null)
			{
				logger.LogError("Trying to purchase null PurchasableItem");
				return;
			}
			if (item.PurchaseType == PurchaseType.NonConsumable && transactionDatabase.getPurchaseHistory(item) > 0)
			{
				logError(UnibillError.UNIBILL_ATTEMPTING_TO_PURCHASE_ALREADY_OWNED_NON_CONSUMABLE);
				return;
			}
			billingSubsystem.purchase(remapper.mapItemIdToPlatformSpecificId(item));
			logger.Log("purchase({0})", item.Id);
		}

		public void purchase(string purchasableId)
		{
			PurchasableItem itemById = InventoryDatabase.getItemById(purchasableId);
			if (itemById == null)
			{
				logger.LogWarning("Unable to purchase unknown item with id: {0}", purchasableId);
			}
			purchase(itemById);
		}

		public void restoreTransactions()
		{
			logger.Log("restoreTransactions()");
			if (!Ready)
			{
				logError(UnibillError.BILLER_NOT_READY);
			}
			else
			{
				billingSubsystem.restoreTransactions();
			}
		}

		public void onPurchaseSucceeded(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseSucceeded({0})", purchasableItemFromPlatformSpecificId.Id);
				transactionDatabase.onPurchase(purchasableItemFromPlatformSpecificId);
				currencyManager.OnPurchased(purchasableItemFromPlatformSpecificId.Id);
				if (this.onPurchaseComplete != null)
				{
					this.onPurchaseComplete(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onPurchaseSucceeded(string platformSpecificId, string receipt)
		{
			if (receipt != null && receipt.Length > 0)
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(platformSpecificId);
				if (!receiptMap.ContainsKey(purchasableItemFromPlatformSpecificId))
				{
					receiptMap.Add(purchasableItemFromPlatformSpecificId, new List<string>());
				}
				receiptMap[purchasableItemFromPlatformSpecificId].Add(receipt);
			}
			onPurchaseSucceeded(platformSpecificId);
		}

		public void onSetupComplete(bool available)
		{
			logger.Log("onSetupComplete({0})", available);
			State = ((!available) ? BillerState.INITIALISED_WITH_CRITICAL_ERROR : ((Errors.Count <= 0) ? BillerState.INITIALISED : BillerState.INITIALISED_WITH_ERROR));
			if (this.onBillerReady != null)
			{
				this.onBillerReady(Ready);
			}
		}

		public void onPurchaseCancelledEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseCancelledEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				if (this.onPurchaseCancelled != null)
				{
					this.onPurchaseCancelled(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onPurchaseRefundedEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseRefundedEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				transactionDatabase.onRefunded(purchasableItemFromPlatformSpecificId);
				if (this.onPurchaseRefunded != null)
				{
					this.onPurchaseRefunded(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onPurchaseFailedEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseFailedEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				if (this.onPurchaseFailed != null)
				{
					this.onPurchaseFailed(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onTransactionsRestoredSuccess()
		{
			logger.Log("onTransactionsRestoredSuccess()");
			if (this.onTransactionsRestored != null)
			{
				this.onTransactionsRestored(true);
			}
		}

		public void ClearPurchases()
		{
			foreach (PurchasableItem allPurchasableItem in InventoryDatabase.AllPurchasableItems)
			{
				transactionDatabase.clearPurchases(allPurchasableItem);
			}
		}

		public void onTransactionsRestoredFail(string error)
		{
			logger.Log("onTransactionsRestoredFail({0})", error);
			this.onTransactionsRestored(false);
		}

		public void logError(UnibillError error)
		{
			logError(error, new object[0]);
		}

		public void logError(UnibillError error, params object[] args)
		{
			Errors.Add(error);
			logger.LogError(help.getMessage(error), args);
		}

		public static Biller instantiate()
		{
			IBillingService billingService = instantiateBillingSubsystem();
			return new Biller(getInventory(), getTransactionDatabase(), billingService, getLogger(), getHelp(), getMapper(), getCurrencyManager());
		}

		private static CurrencyManager getCurrencyManager()
		{
			if (_currencyManager == null)
			{
				_currencyManager = new CurrencyManager(getParser(), getStorage(), getLogger());
			}
			return _currencyManager;
		}

		private static TransactionDatabase getTransactionDatabase()
		{
			if (_tDb == null)
			{
				_tDb = new TransactionDatabase(getStorage(), getLogger());
			}
			return _tDb;
		}

		private static IStorage getStorage()
		{
			if (_storage == null)
			{
				_storage = new UnityPlayerPrefsStorage();
			}
			return _storage;
		}

		private bool verifyPlatformId(string platformId)
		{
			if (!remapper.canMapProductSpecificId(platformId))
			{
				logError(UnibillError.UNIBILL_UNKNOWN_PRODUCTID, platformId);
				return false;
			}
			return true;
		}

		private static IBillingService instantiateBillingSubsystem()
		{
			if (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor)
			{
				return new FakeBillingService(getMapper());
			}
			switch (getConfig().CurrentPlatform)
			{
			case BillingPlatform.AppleAppStore:
				return new AppleAppStoreBillingService(getInventory(), getMapper(), getStorekit());
			case BillingPlatform.AmazonAppstore:
				return new AmazonAppStoreBillingService(getAmazon(), getMapper(), getInventory(), getTransactionDatabase(), getLogger());
			case BillingPlatform.GooglePlay:
				return new GooglePlayBillingService(getGooglePlay(), getConfig(), getMapper(), getInventory(), getLogger());
			case BillingPlatform.MacAppStore:
				return new AppleAppStoreBillingService(getInventory(), getMapper(), getStorekit());
			default:
				throw new ArgumentException(getConfig().CurrentPlatform.ToString());
			}
		}

		private static IRawGooglePlayInterface getGooglePlay()
		{
			return new RawGooglePlayInterface();
		}

		private static IRawAmazonAppStoreBillingInterface getAmazon()
		{
			return new RawAmazonAppStoreBillingInterface(getConfig());
		}

		private static IStoreKitPlugin getStorekit()
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				return new StoreKitPluginImpl();
			}
			return new OSXStoreKitPluginImpl();
		}

		private static HelpCentre getHelp()
		{
			if (_helpCentre == null)
			{
				_helpCentre = new HelpCentre(getParser());
			}
			return _helpCentre;
		}

		private static InventoryDatabase getInventory()
		{
			if (_inventory == null)
			{
				_inventory = new InventoryDatabase(getParser(), getLogger());
			}
			return _inventory;
		}

		private static ProductIdRemapper getMapper()
		{
			if (_remapper == null)
			{
				_remapper = new ProductIdRemapper(getInventory(), getParser(), getConfig());
			}
			return _remapper;
		}

		private static Uniject.ILogger getLogger()
		{
			return new UnityLogger();
		}

		private static UnibillXmlParser getParser()
		{
			return new UnibillXmlParser(new SmallXmlParser(), getResourceLoader());
		}

		private static UnibillConfiguration getConfig()
		{
			if (_config == null)
			{
				_config = new UnibillConfiguration(getResourceLoader(), getParser(), getLogger());
			}
			return _config;
		}

		private static IUtil getUtil()
		{
			return new UnityUtil();
		}

		private static IResourceLoader getResourceLoader()
		{
			if (_resourceLoader == null)
			{
				_resourceLoader = new UnityResourceLoader();
			}
			return _resourceLoader;
		}
	}
}
