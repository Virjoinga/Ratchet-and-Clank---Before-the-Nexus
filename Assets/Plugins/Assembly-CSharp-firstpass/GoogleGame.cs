using System.Collections;

public class GoogleGame : JSONObject
{
	private int m_AchievementTotalCount;

	private string m_ApplicationId;

	private string m_Description;

	private string m_DeveloperName;

	private string m_DisplayName;

	private int m_LeaderboardsCount;

	private string m_PrimaryCategory;

	private string m_SecondaryCategory;

	private bool m_IsPlayEnabledGame;

	public int GetAchievementTotalCount()
	{
		return m_AchievementTotalCount;
	}

	public string GetApplicationId()
	{
		return m_ApplicationId;
	}

	public string GetDescription()
	{
		return m_Description;
	}

	public string GetDeveloperName()
	{
		return m_DeveloperName;
	}

	public string GetDisplayName()
	{
		return m_DisplayName;
	}

	public int GetLeaderboardsCount()
	{
		return m_LeaderboardsCount;
	}

	public string GetPrimaryCategory()
	{
		return m_PrimaryCategory;
	}

	public string GetSecondaryCategory()
	{
		return m_SecondaryCategory;
	}

	public bool IsPlayEnabledGame()
	{
		return m_IsPlayEnabledGame;
	}

	public override void FromJSON(IDictionary a_JSONDict)
	{
		m_AchievementTotalCount = JSONObject.GetIntValueFromDict(a_JSONDict, "achievementCount", -1);
		m_ApplicationId = JSONObject.GetValueFromDict(a_JSONDict, "applicationId", string.Empty);
		m_Description = JSONObject.GetValueFromDict(a_JSONDict, "description", string.Empty);
		m_DeveloperName = JSONObject.GetValueFromDict(a_JSONDict, "developerName", string.Empty);
		m_DisplayName = JSONObject.GetValueFromDict(a_JSONDict, "displayName", string.Empty);
		m_LeaderboardsCount = JSONObject.GetIntValueFromDict(a_JSONDict, "leaderboardsCount", -1);
		m_PrimaryCategory = JSONObject.GetValueFromDict(a_JSONDict, "primaryCategory", string.Empty);
		m_SecondaryCategory = JSONObject.GetValueFromDict(a_JSONDict, "secondaryCategory", string.Empty);
		m_IsPlayEnabledGame = JSONObject.GetValueFromDict(a_JSONDict, "isPlayEnabled", false);
	}
}
