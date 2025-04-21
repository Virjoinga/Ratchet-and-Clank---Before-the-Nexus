namespace Unibill.Impl
{
	public interface IBillingService
	{
		void initialise(IBillingServiceCallback biller);

		void purchase(string item);

		void restoreTransactions();
	}
}
