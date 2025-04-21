public class Analytics
{
	protected static IAnalyticsInterface analyticsImplementation;

	public static IAnalyticsInterface Get()
	{
		if (analyticsImplementation == null)
		{
			analyticsImplementation = new DummyAnalyticsImpl();
		}
		return analyticsImplementation;
	}
}
