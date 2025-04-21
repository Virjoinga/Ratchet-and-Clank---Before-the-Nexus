using System.Collections;

public class GoogleLeaderboardScore : JSONObject
{
	private long m_Rank;

	private string m_DisplayRank;

	private long m_RawScore;

	private string m_DisplayScore;

	private long m_TimestampMillis;

	private GooglePlayer m_ScoreHolder;

	public long GetRank()
	{
		return m_Rank;
	}

	public string GetDisplayRank()
	{
		return m_DisplayRank;
	}

	public long GetRawScore()
	{
		return m_RawScore;
	}

	public string GetDisplayScore()
	{
		return m_DisplayScore;
	}

	public long GetTimestampMillis()
	{
		return m_TimestampMillis;
	}

	public GooglePlayer GetScoreHolder()
	{
		return m_ScoreHolder;
	}

	public string GetScoreHolderDisplayName()
	{
		if (m_ScoreHolder != null)
		{
			return m_ScoreHolder.GetDisplayName();
		}
		return null;
	}

	public override void FromJSON(IDictionary a_JSONDict)
	{
		m_Rank = JSONObject.GetLongValueFromDict(a_JSONDict, "rank", -1L);
		m_DisplayRank = JSONObject.GetValueFromDict(a_JSONDict, "displayRank", string.Empty);
		m_RawScore = JSONObject.GetLongValueFromDict(a_JSONDict, "rawScore", -1L);
		m_DisplayScore = JSONObject.GetValueFromDict(a_JSONDict, "displayScore", string.Empty);
		m_TimestampMillis = JSONObject.GetLongValueFromDict(a_JSONDict, "timestampMillis", -1L);
		IDictionary valueFromDict = JSONObject.GetValueFromDict<IDictionary>(a_JSONDict, "scoreHolder", null, false);
		if (valueFromDict != null)
		{
			m_ScoreHolder = new GooglePlayer();
			m_ScoreHolder.FromJSON(valueFromDict);
		}
		else
		{
			m_ScoreHolder = null;
		}
	}
}
