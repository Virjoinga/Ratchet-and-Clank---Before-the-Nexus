using System;
using System.IO;
using UnityEngine;

namespace Unibill.Impl
{
	public class RawAmazonAppStoreBillingInterface : IRawAmazonAppStoreBillingInterface
	{
		private AndroidJavaObject amazon;

		public RawAmazonAppStoreBillingInterface(UnibillConfiguration config)
		{
			if (config.CurrentPlatform == BillingPlatform.AmazonAppstore && config.AmazonSandboxEnabled)
			{
				string text = ((TextAsset)Resources.Load("amazon.sdktester.json")).text;
				File.WriteAllText("/sdcard/amazon.sdktester.json", text);
			}
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.outlinegames.unibillAmazon.Unibill"))
			{
				amazon = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}

		public void initialise(AmazonAppStoreBillingService amazon)
		{
			new GameObject().AddComponent<AmazonAppStoreCallbackMonoBehaviour>().initialise(amazon);
		}

		public void initiateItemDataRequest(string[] productIds)
		{
			IntPtr methodID = AndroidJNI.GetMethodID(amazon.GetRawClass(), "initiateItemDataRequest", "([Ljava/lang/String;)V");
			AndroidJNI.CallVoidMethod(amazon.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(new object[1] { productIds }));
		}

		public void initiatePurchaseRequest(string productId)
		{
			amazon.Call("initiatePurchaseRequest", productId);
		}

		public void restoreTransactions()
		{
			amazon.Call("restoreTransactions");
		}
	}
}
