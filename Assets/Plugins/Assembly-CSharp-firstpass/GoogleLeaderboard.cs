using System.Collections;

public class GoogleLeaderboard : JSONObject
{
	public enum ScoreOrder
	{
		SMALLER_IS_BETTER = 0,
		LARGER_IS_BETTER = 1
	}

	private string m_LeaderboardId;

	private string m_DisplayName;

	private ScoreOrder m_ScoreOrder;

	public string GetLeaderboardId()
	{
		return m_LeaderboardId;
	}

	public string GetDisplayName()
	{
		return m_DisplayName;
	}

	public ScoreOrder GetScoreOrder()
	{
		return m_ScoreOrder;
	}

	public override void FromJSON(IDictionary a_JSONDict)
	{
		m_LeaderboardId = JSONObject.GetValueFromDict(a_JSONDict, "leaderboardId", string.Empty);
		m_DisplayName = JSONObject.GetValueFromDict(a_JSONDict, "displayName", string.Empty);
		int intValueFromDict = JSONObject.GetIntValueFromDict(a_JSONDict, "scoreOrder", 0);
		m_ScoreOrder = (ScoreOrder)intValueFromDict;
	}
}
