using System.Collections;
using System.Collections.Generic;

public class GoogleSubmitScoreResult : JSONObject
{
	public class Result : JSONObject
	{
		private string m_FormattedScore;

		private bool m_NewBest;

		private long m_RawScore;

		public string GetFormattedScore()
		{
			return m_FormattedScore;
		}

		public bool GetNewBest()
		{
			return m_NewBest;
		}

		public long GetRawScore()
		{
			return m_RawScore;
		}

		public override void FromJSON(IDictionary a_JSONDict)
		{
			m_FormattedScore = JSONObject.GetValueFromDict(a_JSONDict, "formattedScore", string.Empty);
			m_NewBest = JSONObject.GetValueFromDict(a_JSONDict, "newBest", false);
			m_RawScore = JSONObject.GetLongValueFromDict(a_JSONDict, "rawScore", -1L);
		}
	}

	private string m_LeaderboardId;

	private string m_PlayerId;

	private int m_StatusCode;

	private Dictionary<int, Result> m_ScoreResults;

	public string GetLeaderboardId()
	{
		return m_LeaderboardId;
	}

	public string GetPlayerId()
	{
		return m_PlayerId;
	}

	public int GetStatusCode()
	{
		return m_StatusCode;
	}

	public Result GetScoreResult(GoogleGamesBinding.TimeSpan a_TimeSpan)
	{
		return GetScoreResult((int)a_TimeSpan);
	}

	public Result GetScoreResult(int a_TimeSpan)
	{
		if (m_ScoreResults != null && m_ScoreResults.ContainsKey(a_TimeSpan))
		{
			return m_ScoreResults[a_TimeSpan];
		}
		return null;
	}

	public override void FromJSON(IDictionary a_JSONDict)
	{
		m_LeaderboardId = JSONObject.GetValueFromDict(a_JSONDict, "leaderboardId", string.Empty);
		m_PlayerId = JSONObject.GetValueFromDict(a_JSONDict, "playerId", string.Empty);
		m_StatusCode = JSONObject.GetIntValueFromDict(a_JSONDict, "statusCode", -1);
		ICollection valueFromDict = JSONObject.GetValueFromDict<ICollection>(a_JSONDict, "scoreResultsCollection", null);
		if (valueFromDict == null)
		{
			return;
		}
		m_ScoreResults = new Dictionary<int, Result>();
		foreach (object item in valueFromDict)
		{
			if (item is IDictionary)
			{
				IDictionary a_Dict = item as IDictionary;
				int intValueFromDict = JSONObject.GetIntValueFromDict(a_Dict, "timeSpan", -1);
				Result result = new Result();
				IDictionary valueFromDict2 = JSONObject.GetValueFromDict<IDictionary>(a_Dict, "result", null);
				if (valueFromDict2 != null)
				{
					result.FromJSON(valueFromDict2);
					m_ScoreResults[intValueFromDict] = result;
				}
			}
		}
	}
}
