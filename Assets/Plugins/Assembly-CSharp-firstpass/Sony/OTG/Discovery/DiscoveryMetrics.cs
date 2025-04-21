using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using LitJson;
using UnityEngine;

namespace Sony.OTG.Discovery
{
	public class DiscoveryMetrics : MonoBehaviour
	{
		internal const string OM_PROD = "https://om-prod.online.scea.com";

		private const string OM_REQUEST_PATH = "v1/metric";

		private const string SERVICE_ID = "Discovery";

		private const string CONTENT_TYPE = "application/json";

		private bool loggedMessage;

		private static readonly MD5 md5 = MD5.Create();

		private static HMACSHA1 hmac;

		public string MetricsServerUrl { get; set; }

		public string ProductId { get; set; }

		public string ApiKey { get; set; }

		public string SecretKey { get; set; }

		public string UserId { get; set; }

		public string DeviceId { get; set; }

		public string SessionId { get; set; }

		public DiscoveryMetrics()
		{
			SessionId = Guid.NewGuid().ToString();
		}

		public void SendEvent(string eventType, string key)
		{
			SendEvent(eventType, key, null);
		}

		public void SendEvent(string eventType, string key, string value)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(1);
			dictionary.Add(key, value);
			SendEvent(eventType, dictionary);
		}

		public void SendEvent(string eventType, Dictionary<string, string> payload)
		{
			StartCoroutine(SendEventCoroutine(eventType, payload));
		}

		private IEnumerator SendEventCoroutine(string _eventType, Dictionary<string, string> payload)
		{
			Metric metric = new Metric
			{
				serviceId = "Discovery",
				productId = ProductId,
				sessionId = SessionId,
				userId = UserId,
				deviceId = DeviceId,
				eventType = _eventType,
				clientTime = DateTime.Now.ToString("s", CultureInfo.InvariantCulture),
				data = payload
			};
			if (ProductId == null || ProductId.Length == 0)
			{
				Debug.LogError("DISC: ProductId has not been set.");
			}
			if (MetricsServerUrl == null)
			{
				MetricsServerUrl = "https://om-prod.online.scea.com";
			}
			if (!loggedMessage)
			{
				Debug.Log("DISC: OM URL: " + MetricsServerUrl);
				loggedMessage = true;
			}
			string content = JsonMapper.ToJson(metric);
			string contentHash = getMd5Hash(content);
			string stringToSign = string.Format("POST\n{0}\n{1}", contentHash, "v1/metric");
			string signature = generateHmac(stringToSign);
			WWW www = new WWW(headers: new Hashtable
			{
				{
					"Authorization",
					string.Format("{0}:{1}", ApiKey, signature)
				},
				{ "Content-Type", "application/json" }
			}, url: MetricsServerUrl + "/v1/metric", postData: Encoding.UTF8.GetBytes(content));
			yield return www;
			while (!www.isDone)
			{
				yield return null;
			}
			if (www.error != null)
			{
				Debug.LogError("DISC: Error from OM API request: " + www.error);
			}
		}

		private string getMd5Hash(string input)
		{
			return toHex(md5.ComputeHash(Encoding.UTF8.GetBytes(input)));
		}

		private string generateHmac(string input)
		{
			if (hmac == null)
			{
				if (SecretKey == null)
				{
					Debug.LogError("DISC: Warning: SecretKey has not been set!");
					return "undefined";
				}
				hmac = new HMACSHA1(Encoding.ASCII.GetBytes(SecretKey));
			}
			return toHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(input)));
		}

		private static string toHex(byte[] data)
		{
			StringBuilder stringBuilder = new StringBuilder(data.Length * 2);
			foreach (byte b in data)
			{
				stringBuilder.Append(b.ToString("x2"));
			}
			return stringBuilder.ToString();
		}
	}
}
