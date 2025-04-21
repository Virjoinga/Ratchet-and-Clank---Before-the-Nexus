namespace Unibill.Impl
{
	public interface IRawGooglePlayInterface
	{
		void initialise(GooglePlayBillingService callback, string publicKey);

		void purchase(string product);

		void restoreTransactions();
	}
}
