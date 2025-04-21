using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;

namespace Sony.OTG.Discovery
{
	public class DiscoveryClient : MonoBehaviour
	{
		private class PlatformString
		{
			public const string PLATFORM = "ANDROID";
		}

		private const string DISCOVERY_PROD = "https://discovery-prod.online.scea.com";

		public static DiscoveryClient instance;

		public string discoveryServerUrl = "https://discovery-prod.online.scea.com";

		public string metricsServerUrl = "https://om-prod.online.scea.com";

		public string discoveryClientId;

		public string discoveryApiKey;

		public string discoverySecretKey;

		public AdSize desiredAdSize = AdSize.PHONE_LANDSCAPE_FULLSCREEN_850X480;

		public bool automaticallyFetchNextAd;

		private int numAdsToRemember = 3;

		private string baseRequestUrl;

		private List<string> lastSeen = new List<string>();

		private Creative currentAdCreative;

		private bool adIsDisplayedByDiscoveryClient;

		private float adDisplayStart;

		private float savedTimeScale;

		private float savedAudioVolume;

		private List<MonoBehaviour> pausedObjects = new List<MonoBehaviour>();

		private bool savedLockCursor;

		private Rect adImagePos;

		private Rect cancelButtonPos;

		private Rect adFramePos;

		private DateTime timeOfDiscoveryRequest;

		private DateTime timeOfImageRequest;

		private DateTime adDisplayStartTime;

		private AdDisplayLocation location;

		private DiscoveryMetrics metrics;

		public Texture2D AdFrameTexture { get; set; }

		public Texture2D CancelButtonTexture { get; set; }

		public string UserId { get; set; }

		public Action AdReceivedCallback { get; set; }

		public int NumAdsToRemember
		{
			get
			{
				return numAdsToRemember;
			}
			set
			{
				numAdsToRemember = value;
			}
		}

		public Texture2D AdImageTexture { get; private set; }

		public float AdPreTimeout { get; private set; }

		public float AdDisplayTimeout { get; private set; }

		public void FetchPublisherConfig(Action<Dictionary<string, string>> callback)
		{
			StartCoroutine(issueDiscoveryConfigRequest(callback));
		}

		public void FetchAdvertisement()
		{
			StartCoroutine(issueDiscoveryRequest());
		}

		public void DisplayAdvertisement(AdScreenLocation screenPos, AdDisplayLocation gameLocation)
		{
			if (adIsDisplayedByDiscoveryClient)
			{
				return;
			}
			if (currentAdCreative == null || AdImageTexture == null)
			{
				Debug.LogWarning("DISC: Ad is not yet available.");
				return;
			}
			if (!desiredAdSize.ToString().Contains("FULLSCREEN"))
			{
				base.enabled = false;
				throw new NotImplementedException("DISC: Need to generalize this function for non-fullscreen images.");
			}
			int num;
			int num2;
			int num3;
			if (AdImageTexture.width > AdImageTexture.height)
			{
				num = Screen.width * 3 / 4;
				num2 = (int)((float)num / (float)AdImageTexture.width * (float)AdImageTexture.height);
				num3 = num2 / 12;
			}
			else
			{
				num2 = Screen.height * 3 / 4;
				num = (int)((float)num2 / (float)AdImageTexture.height * (float)AdImageTexture.width);
				num3 = num / 12;
			}
			switch (screenPos)
			{
			case AdScreenLocation.TOP:
				throw new NotImplementedException("DISC: AdScreenLocation.TOP not yet implemented.");
			case AdScreenLocation.CENTER:
			{
				int num4 = Screen.width / 2 - (num / 2 + num3);
				int num5 = Screen.height / 2 - (num2 / 2 + num3);
				adFramePos = new Rect(num4, num5, num + 2 * num3, num2 + 2 * num3);
				adImagePos = new Rect(num4 + num3, num5 + num3, num, num2);
				cancelButtonPos = new Rect(num4, num5, num3 * 12 / 10, num3 * 12 / 10);
				OnAdDisplayed(gameLocation);
				adDisplayStart = Time.realtimeSinceStartup;
				savedAudioVolume = AudioListener.volume;
				AudioListener.volume = 0f;
				savedTimeScale = Time.timeScale;
				Time.timeScale = 0f;
				savedLockCursor = Screen.lockCursor;
				Screen.lockCursor = false;
				adIsDisplayedByDiscoveryClient = true;
				break;
			}
			case AdScreenLocation.BOTTOM:
				throw new NotImplementedException("DISC: AdScreenLocation.BOTTOM not yet implemented.");
			default:
				throw new NotImplementedException(screenPos.ToString() + " not yet implemented.");
			}
		}

		public void OnAdDisplayed(AdDisplayLocation location)
		{
			adDisplayStartTime = DateTime.Now;
			this.location = location;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["CreativeId"] = currentAdCreative.uuid;
			dictionary["CreativeName"] = currentAdCreative.name;
			dictionary["ProductName"] = currentAdCreative.productName;
			dictionary["AdvertisementName"] = currentAdCreative.advertisementName;
			dictionary["SystemLanguage"] = Application.systemLanguage.ToString();
			dictionary["LanguageCode"] = mapLanguageToLanguageCode(Application.systemLanguage);
			dictionary["DisplayLocation"] = location.ToString();
			dictionary["Platform"] = "ANDROID";
			dictionary["DeviceModel"] = SystemInfo.deviceModel;
			dictionary["DeviceName"] = SystemInfo.deviceName;
			metrics.SendEvent("AdDisplayed", dictionary);
			saveLastSeen();
		}

		public void OnAdClicked()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["CreativeId"] = currentAdCreative.uuid;
			dictionary["CreativeName"] = currentAdCreative.name;
			dictionary["ProductName"] = currentAdCreative.productName;
			dictionary["AdvertisementName"] = currentAdCreative.advertisementName;
			dictionary["SystemLanguage"] = Application.systemLanguage.ToString();
			dictionary["LanguageCode"] = mapLanguageToLanguageCode(Application.systemLanguage);
			dictionary["DisplayLocation"] = location.ToString();
			dictionary["Platform"] = "ANDROID";
			dictionary["DeviceModel"] = SystemInfo.deviceModel;
			dictionary["DeviceName"] = SystemInfo.deviceName;
			dictionary["AdDuration"] = (DateTime.Now - adDisplayStartTime).TotalMilliseconds.ToString();
			metrics.SendEvent("AdClicked", dictionary);
			Debug.Log("DISC: Launching the following action: " + currentAdCreative.actionUrl);
			Application.OpenURL(currentAdCreative.actionUrl);
			ResetAdvertisement();
		}

		public void OnAdCancelled()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["CreativeId"] = currentAdCreative.uuid;
			dictionary["CreativeName"] = currentAdCreative.name;
			dictionary["ProductName"] = currentAdCreative.productName;
			dictionary["AdvertisementName"] = currentAdCreative.advertisementName;
			dictionary["SystemLanguage"] = Application.systemLanguage.ToString();
			dictionary["LanguageCode"] = mapLanguageToLanguageCode(Application.systemLanguage);
			dictionary["DisplayLocation"] = location.ToString();
			dictionary["Platform"] = "ANDROID";
			dictionary["DeviceModel"] = SystemInfo.deviceModel;
			dictionary["DeviceName"] = SystemInfo.deviceName;
			dictionary["AdDuration"] = (DateTime.Now - adDisplayStartTime).TotalMilliseconds.ToString();
			metrics.SendEvent("AdCancelled", dictionary);
			ResetAdvertisement();
		}

		public void OnAdTimedOut()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["CreativeId"] = currentAdCreative.uuid;
			dictionary["CreativeName"] = currentAdCreative.name;
			dictionary["ProductName"] = currentAdCreative.productName;
			dictionary["AdvertisementName"] = currentAdCreative.advertisementName;
			dictionary["SystemLanguage"] = Application.systemLanguage.ToString();
			dictionary["LanguageCode"] = mapLanguageToLanguageCode(Application.systemLanguage);
			dictionary["DisplayLocation"] = location.ToString();
			dictionary["Platform"] = "ANDROID";
			dictionary["DeviceModel"] = SystemInfo.deviceModel;
			dictionary["DeviceName"] = SystemInfo.deviceName;
			dictionary["AdDuration"] = (DateTime.Now - adDisplayStartTime).TotalMilliseconds.ToString();
			metrics.SendEvent("AdTimedOut", dictionary);
			ResetAdvertisement();
		}

		private Texture2D loadTextureFromBytes(string resourceName)
		{
			TextAsset textAsset = Resources.Load(resourceName, typeof(TextAsset)) as TextAsset;
			if (textAsset == null)
			{
				Debug.LogError("DISC: Failed to load texture resource " + resourceName);
				base.enabled = false;
				throw new FileNotFoundException();
			}
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(textAsset.bytes);
			return texture2D;
		}

		private IEnumerator issueDiscoveryRequest()
		{
			if (baseRequestUrl == null)
			{
				baseRequestUrl = string.Format("{0}/client/v1/creative/{1}?size={2}&platform={3}&language={4}&id={5}", discoveryServerUrl, WWW.EscapeURL(discoveryClientId), desiredAdSize, "ANDROID", mapLanguageToLanguageCode(Application.systemLanguage), SystemInfo.deviceUniqueIdentifier);
				metrics = base.gameObject.AddComponent<DiscoveryMetrics>();
				metrics.MetricsServerUrl = metricsServerUrl;
				metrics.ProductId = discoveryClientId;
				metrics.ApiKey = discoveryApiKey;
				metrics.SecretKey = discoverySecretKey;
				metrics.UserId = UserId;
				metrics.DeviceId = SystemInfo.deviceUniqueIdentifier;
				metrics.SessionId = Guid.NewGuid().ToString();
			}
			string requestUrl = baseRequestUrl;
			if (lastSeen.Count != 0)
			{
				foreach (string id in lastSeen)
				{
					requestUrl = requestUrl + "&lastseen=" + id;
				}
			}
			Debug.Log("DISC: Discovery URL: " + requestUrl);
			timeOfDiscoveryRequest = DateTime.Now;
			WWW www = new WWW(requestUrl);
			yield return www;
			while (!www.isDone)
			{
				yield return null;
			}
			string statusCode = "Unknown";
			if (www.responseHeaders.ContainsKey("STATUS"))
			{
				statusCode = www.responseHeaders["STATUS"];
			}
			Dictionary<string, string> payload = new Dictionary<string, string>
			{
				{ "ResponseCode", statusCode },
				{
					"ResponseTime",
					(DateTime.Now - timeOfDiscoveryRequest).TotalMilliseconds.ToString()
				}
			};
			metrics.SendEvent("DiscoveryAPIResponse", payload);
			if (www.error != null)
			{
				Debug.LogError("DISC: Error from Discovery API request: " + www.error);
			}
			else
			{
				StartCoroutine(downloadImage(www.text));
			}
		}

		private IEnumerator issueDiscoveryConfigRequest(Action<Dictionary<string, string>> callback)
		{
			string requestUrl = string.Format("{0}/client/v1/config/{1}", discoveryServerUrl, WWW.EscapeURL(discoveryClientId));
			WWW www = new WWW(requestUrl);
			yield return www;
			while (!www.isDone)
			{
				yield return null;
			}
			if (www.error != null)
			{
				Debug.LogError("DISC: Error from Discovery Config API request: " + www.error);
				yield break;
			}
			Dictionary<string, string> config = JsonMapper.ToObject<Dictionary<string, string>>(www.text);
			if (config != null)
			{
				callback(config);
			}
		}

		private IEnumerator downloadImage(string responseText)
		{
			if (string.IsNullOrEmpty(responseText))
			{
				Debug.LogWarning("DISC: Empty response from Discovery API. No Ads to display.");
				yield return null;
				yield break;
			}
			currentAdCreative = JsonMapper.ToObject<Creative>(responseText);
			if (currentAdCreative == null)
			{
				Debug.LogError("DISC: Failed to parse response from Discovery API.");
				Debug.LogError("DISC: Response: <<" + responseText + ">>");
				yield return null;
				yield break;
			}
			AdPreTimeout = currentAdCreative.timeout;
			AdDisplayTimeout = 0f;
			timeOfImageRequest = DateTime.Now;
			WWW www = new WWW(currentAdCreative.url);
			yield return www;
			while (!www.isDone)
			{
				yield return null;
			}
			string statusCode = "Unknown";
			try
			{
				if (www.responseHeaders.ContainsKey("STATUS"))
				{
					statusCode = www.responseHeaders["STATUS"];
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			Dictionary<string, string> payload = new Dictionary<string, string>
			{
				{ "ResponseCode", statusCode },
				{
					"ResponseTime",
					(DateTime.Now - timeOfImageRequest).TotalMilliseconds.ToString()
				}
			};
			metrics.SendEvent("ImageDownloadResponse", payload);
			if (www.error != null)
			{
				Debug.LogError("DISC: Error from Image download request: " + www.error);
				yield break;
			}
			AdImageTexture = www.texture;
			if (AdReceivedCallback != null)
			{
				AdReceivedCallback();
			}
		}

		private void ResetAdvertisement()
		{
			currentAdCreative = null;
			AdImageTexture = null;
			if (adIsDisplayedByDiscoveryClient)
			{
				adIsDisplayedByDiscoveryClient = false;
				Time.timeScale = savedTimeScale;
				savedTimeScale = 0f;
				AudioListener.volume = savedAudioVolume;
				savedAudioVolume = 0f;
				Screen.lockCursor = savedLockCursor;
				foreach (MonoBehaviour pausedObject in pausedObjects)
				{
					pausedObject.enabled = true;
				}
				pausedObjects.Clear();
			}
			if (automaticallyFetchNextAd)
			{
				FetchAdvertisement();
			}
		}

		private void readLastSeen()
		{
			try
			{
				using (TextReader reader = File.OpenText(Application.persistentDataPath + "/lastSeenAds.json"))
				{
					lastSeen = JsonMapper.ToObject<List<string>>(reader);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private void saveLastSeen()
		{
			try
			{
				using (TextWriter writer = File.CreateText(Application.persistentDataPath + "/lastSeenAds.json"))
				{
					if (lastSeen.Count >= NumAdsToRemember)
					{
						lastSeen.RemoveRange(NumAdsToRemember - 1, lastSeen.Count - NumAdsToRemember + 1);
					}
					lastSeen.Add(currentAdCreative.uuid);
					JsonWriter writer2 = new JsonWriter(writer);
					JsonMapper.ToJson(lastSeen, writer2);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private string mapLanguageToLanguageCode(SystemLanguage systemLanguage)
		{
			switch (systemLanguage)
			{
			case SystemLanguage.Chinese:
				return "zh_CN";
			case SystemLanguage.Danish:
				return "da";
			case SystemLanguage.Dutch:
				return "nl";
			case SystemLanguage.English:
				return "en";
			case SystemLanguage.Finnish:
				return "fi";
			case SystemLanguage.French:
				return "fr";
			case SystemLanguage.German:
				return "de";
			case SystemLanguage.Italian:
				return "it";
			case SystemLanguage.Japanese:
				return "jp";
			case SystemLanguage.Korean:
				return "ko";
			case SystemLanguage.Norwegian:
				return "no";
			case SystemLanguage.Polish:
				return "pl";
			case SystemLanguage.Portuguese:
				return "pt";
			case SystemLanguage.Russian:
				return "ru";
			case SystemLanguage.Spanish:
				return "en";
			case SystemLanguage.Swedish:
				return "sv";
			case SystemLanguage.Turkish:
				return "tr";
			default:
				return "en";
			}
		}

		private void Awake()
		{
			if ((bool)instance)
			{
				Debug.LogError("DISC: Multiple instances spawned");
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				instance = this;
			}
		}

		private void Start()
		{
			Debug.Log("DISC: Screen size: " + Screen.width + " x " + Screen.height);
			readLastSeen();
			if (automaticallyFetchNextAd)
			{
				FetchAdvertisement();
			}
		}

		private void OnGUI()
		{
			if (CancelButtonTexture == null)
			{
				CancelButtonTexture = loadTextureFromBytes("Discovery/CancelTexture");
			}
			if (AdFrameTexture == null)
			{
				AdFrameTexture = loadTextureFromBytes("Discovery/AdFrameTexture");
			}
			if (adIsDisplayedByDiscoveryClient)
			{
				GUI.DrawTexture(adFramePos, AdFrameTexture, ScaleMode.StretchToFill);
				GUI.DrawTexture(adImagePos, AdImageTexture, ScaleMode.StretchToFill);
				bool flag = GUI.Button(adImagePos, string.Empty, GUIStyle.none);
				bool flag2 = false;
				if (Time.realtimeSinceStartup >= AdPreTimeout + adDisplayStart)
				{
					GUI.DrawTexture(cancelButtonPos, CancelButtonTexture);
					flag2 = GUI.Button(cancelButtonPos, string.Empty, GUIStyle.none);
				}
				if (flag2)
				{
					OnAdCancelled();
				}
				else if (flag)
				{
					OnAdClicked();
				}
			}
		}

		private void OnApplicationQuit()
		{
			if (adIsDisplayedByDiscoveryClient)
			{
				OnAdCancelled();
			}
		}
	}
}
