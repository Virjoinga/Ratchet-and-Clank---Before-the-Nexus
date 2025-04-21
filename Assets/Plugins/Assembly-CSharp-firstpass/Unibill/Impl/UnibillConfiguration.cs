using System;
using Uniject;

namespace Unibill.Impl
{
	public class UnibillConfiguration
	{
		public string iOSSKU { get; private set; }

		public BillingPlatform CurrentPlatform { get; private set; }

		public string GooglePlayPublicKey { get; private set; }

		public bool AmazonSandboxEnabled { get; private set; }

		public UnibillConfiguration(IResourceLoader loader, UnibillXmlParser parser, ILogger logger)
		{
			UnibillXmlParser.UnibillXElement unibillXElement = parser.Parse("unibillInventory", "inventory")[0];
			iOSSKU = unibillXElement.TryGetFirstElement("iOSSKU");
			GooglePlayPublicKey = unibillXElement.TryGetFirstElement("GooglePlayPublicKey");
			AmazonSandboxEnabled = bool.Parse(unibillXElement.TryGetFirstElement("useAmazonSandbox"));
			CurrentPlatform = BillingPlatform.MacAppStore;
			CurrentPlatform = (BillingPlatform)(int)Enum.Parse(typeof(BillingPlatform), unibillXElement.TryGetFirstElement("androidBillingPlatform"));
		}
	}
}
