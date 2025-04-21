using UnityEngine;

namespace Unibill.Impl
{
	public class RawGooglePlayInterface : IRawGooglePlayInterface
	{
		private AndroidJavaObject plugin;

		public void initialise(GooglePlayBillingService callback, string publicKey)
		{
			new GameObject().AddComponent<GooglePlayCallbackMonoBehaviour>().Initialise(callback);
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.outlinegames.unibill.UniBill"))
			{
				plugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
			plugin.Call("initialise", publicKey);
		}

		public void restoreTransactions()
		{
			plugin.Call("restoreTransactions");
		}

		public void purchase(string id)
		{
			plugin.Call("purchaseProduct", id);
		}
	}
}
