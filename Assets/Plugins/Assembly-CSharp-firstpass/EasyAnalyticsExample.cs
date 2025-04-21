using UnityEngine;

public class EasyAnalyticsExample : MonoBehaviour
{
	private void Start()
	{
		EasyAnalytics.Instance.trackerWithTrackingId("UA-45277750-1");
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 125f, 200f, 70f), "sendView hit"))
		{
			EasyAnalytics.Instance.sendView("/gaexample");
		}
		if (GUI.Button(new Rect(230f, 125f, 200f, 70f), "setCustomDimension"))
		{
			EasyAnalytics.Instance.setCustomDimension(1, "SOME_DIMENSION_VALUE_1");
			EasyAnalytics.Instance.setCustomDimension(2, "SOME_DIMENSION_VALUE_2");
		}
		if (GUI.Button(new Rect(10f, 225f, 200f, 70f), "sendEventWith"))
		{
			EasyAnalytics.Instance.sendEventWith("EVENT_CATEGORIE", "EVENT_ACTION", "EVENT_BUTTON", 123L);
		}
		if (GUI.Button(new Rect(230f, 225f, 200f, 70f), "setCustomMetric"))
		{
			EasyAnalytics.Instance.setCustomMetric(1, 12345L);
		}
		if (GUI.Button(new Rect(10f, 325f, 200f, 70f), "setStartSession"))
		{
			EasyAnalytics.Instance.setStartSession(true);
		}
		if (GUI.Button(new Rect(230f, 325f, 200f, 70f), "sendException"))
		{
			EasyAnalytics.Instance.sendException("AN_EXCEPTION_1", true);
			EasyAnalytics.Instance.sendException("AN_EXCEPTION_2", false);
		}
		if (GUI.Button(new Rect(10f, 425f, 200f, 70f), "sendSocial"))
		{
			EasyAnalytics.Instance.sendSocial("Twitter", "Tweet", "https://developers.google.com/analytics");
		}
		if (GUI.Button(new Rect(230f, 425f, 200f, 70f), "sendTiming"))
		{
			EasyAnalytics.Instance.sendTiming("A_CATEGORY", 12345L, "CAT_NAME", "CAT_LABEL");
		}
		if (GUI.Button(new Rect(10f, 525f, 200f, 70f), "sendTransaction"))
		{
			EasyAnalytics.Instance.sendTransaction("TRANSACTION_ID_12345", 2160000L, "In-App Store", 170000L, 0L, null, "PRODUCT_SKU_1", "PRODUCT_NAME", 1990000L, 1, "PRODUCT_CAT");
		}
		if (GUI.Button(new Rect(230f, 525f, 200f, 70f), "setCampaign"))
		{
			EasyAnalytics.Instance.setCampaignUrl("http://www.anyname.com?utm_campaign=my_campaign&utm_source=google_test&utm_medium=cpc&utm_term=my_keyword&utm_content=ad_variation1");
		}
		if (GUI.Button(new Rect(10f, 625f, 200f, 70f), "setReferrer"))
		{
			EasyAnalytics.Instance.setReferrer("http://www.anyname.com");
		}
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
