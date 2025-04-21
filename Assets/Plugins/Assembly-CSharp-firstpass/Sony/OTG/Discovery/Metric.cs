using System.Collections.Generic;

namespace Sony.OTG.Discovery
{
	public class Metric
	{
		public string serviceId { get; set; }

		public string productId { get; set; }

		public string userId { get; set; }

		public string deviceId { get; set; }

		public string sessionId { get; set; }

		public string eventType { get; set; }

		public string clientTime { get; set; }

		public Dictionary<string, string> data { get; set; }
	}
}
