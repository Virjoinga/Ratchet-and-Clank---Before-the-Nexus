public interface IAnalyticsInterface
{
	void ForceSubmit();

	void SetBuildName(string buildName);

	void SetArea(string areaName);

	void SendDesignEvent(string eventName, float eventValue);

	void SendQualityEvent(string eventName, string message);

	void SendBusinessEvent(string eventName, string currency, int amount);
}
