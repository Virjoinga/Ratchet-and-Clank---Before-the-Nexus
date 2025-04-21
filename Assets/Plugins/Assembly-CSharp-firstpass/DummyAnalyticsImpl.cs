public class DummyAnalyticsImpl : IAnalyticsInterface
{
	public void ForceSubmit()
	{
	}

	public void SetBuildName(string buildName)
	{
	}

	public void SetArea(string areaName)
	{
	}

	public void SendDesignEvent(string eventName, float eventValue)
	{
	}

	public void SendQualityEvent(string eventName, string message)
	{
	}

	public void SendBusinessEvent(string eventName, string currency, int amount)
	{
	}
}
