using System.Collections;
using UnityEngine;

public class EasyAnalytics : MonoBehaviour
{
	private static EasyAnalytics instance;

	private string _trackinId = string.Empty;

	public static EasyAnalytics Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("EasyAnalytics");
				instance = gameObject.AddComponent<EasyAnalytics>();
				Object.DontDestroyOnLoad(gameObject);
			}
			return instance;
		}
	}

	private void Awake()
	{
		Debug.Log("Awake");
	}

	private void Start()
	{
		Debug.Log("Start");
	}

	public void trackerWithTrackingId(string trackingId)
	{
		StartCoroutine(trackerWithTrackingIdAsync((trackingId != null) ? trackingId : string.Empty));
	}

	private IEnumerator trackerWithTrackingIdAsync(string trackingId)
	{
		yield return new WaitForEndOfFrame();
		_trackinId = trackingId;
		AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("trackerWithTrackingId", ajo, _trackinId);
	}

	public void setCustomDimension(int index, string dimension)
	{
		StartCoroutine(setCustomDimensionAsync(index, (dimension != null) ? dimension : string.Empty));
	}

	private IEnumerator setCustomDimensionAsync(int index, string dimension)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("setCustomDimension", index, dimension);
	}

	public void setCustomMetric(int index, long metric)
	{
		StartCoroutine(setCustomMetricAsync(index, metric));
	}

	private IEnumerator setCustomMetricAsync(int index, long metric)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("setCustomMetric", index, metric);
	}

	public void sendEventWith(string category, string action, string buttonName, long aValue)
	{
		StartCoroutine(sendEventWithAsync((category != null) ? category : string.Empty, (action != null) ? action : string.Empty, (buttonName != null) ? buttonName : string.Empty, aValue));
	}

	private IEnumerator sendEventWithAsync(string category, string action, string buttonName, long aValue)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("sendEventWith", category, action, buttonName, aValue);
	}

	public void sendView(string page)
	{
		StartCoroutine(sendViewAsync((page != null) ? page : string.Empty));
	}

	private IEnumerator sendViewAsync(string page)
	{
		yield return new WaitForEndOfFrame();
		Debug.Log("EasyAnalytics.SendView:" + page);
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("sendView", page);
	}

	public void setStartSession(bool startSession)
	{
		StartCoroutine(setStartSessionAsync(startSession));
	}

	private IEnumerator setStartSessionAsync(bool startSession)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("setStartSession", startSession);
	}

	public void sendException(string description, bool fatal)
	{
		StartCoroutine(sendExceptionAsync((description != null) ? description : string.Empty, fatal));
	}

	private IEnumerator sendExceptionAsync(string description, bool fatal)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("sendException", description, fatal);
	}

	public void sendSocial(string network, string action, string target)
	{
		StartCoroutine(sendSocialAsync((network != null) ? network : string.Empty, (action != null) ? action : string.Empty, (target != null) ? target : string.Empty));
	}

	private IEnumerator sendSocialAsync(string network, string action, string target)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("sendSocial", network, action, target);
	}

	public void sendTiming(string category, long intervalInMilliseconds, string nameStr, string label, bool immed = false)
	{
		if (immed)
		{
			sendTimingImmediate((category != null) ? category : string.Empty, intervalInMilliseconds, (nameStr != null) ? nameStr : string.Empty, (label != null) ? label : string.Empty);
		}
		else
		{
			StartCoroutine(sendTimingAsync((category != null) ? category : string.Empty, intervalInMilliseconds, (nameStr != null) ? nameStr : string.Empty, (label != null) ? label : string.Empty));
		}
	}

	private IEnumerator sendTimingAsync(string category, long intervalInMilliseconds, string nameStr, string label)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("sendTiming", category, intervalInMilliseconds, nameStr, label);
	}

	private void sendTimingImmediate(string category, long intervalInMilliseconds, string nameStr, string label)
	{
		Debug.Log("EasyAnalytics::sendTimingImmed");
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		androidJavaClass.CallStatic("sendTiming", category, intervalInMilliseconds, nameStr, label);
	}

	public void sendTransaction(string transactionId, long orderTotal, string affiliation, long totalTax, long totalShipping, string currencyCode, string productSKU, string productName, long productPrice, int quantity, string productCategory)
	{
		StartCoroutine(sendTransactionAsync(transactionId, orderTotal, (affiliation != null) ? affiliation : string.Empty, totalTax, totalShipping, currencyCode, (productSKU != null) ? productSKU : string.Empty, (productName != null) ? productName : string.Empty, productPrice, quantity, (productCategory != null) ? productCategory : string.Empty));
	}

	private IEnumerator sendTransactionAsync(string transactionId, long orderTotal, string affiliation, long totalTax, long totalShipping, string currencyCode, string productSKU, string productName, long productPrice, int quantity, string productCategory)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("sendTransaction", transactionId, orderTotal, affiliation, totalTax, totalShipping, currencyCode, productSKU, productName, productPrice, quantity, productCategory);
	}

	public void setCampaignUrl(string campaignUrl)
	{
		StartCoroutine(setCampaignUrlAsync((campaignUrl != null) ? campaignUrl : string.Empty));
	}

	private IEnumerator setCampaignUrlAsync(string campaignUrl)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("setCampaignUrl", campaignUrl);
	}

	public void setReferrer(string url)
	{
		StartCoroutine(setReferrerAsync((url != null) ? url : string.Empty));
	}

	private IEnumerator setReferrerAsync(string url)
	{
		yield return new WaitForEndOfFrame();
		AndroidJavaClass jc = new AndroidJavaClass("com.c4m.gawrapper.GAITrackerWrapper");
		jc.CallStatic("setReferrer", url);
	}
}
