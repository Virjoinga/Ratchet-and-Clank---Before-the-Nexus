using System.Collections;

public class GooglePlayer : JSONObject
{
	private string m_PlayerId;

	private string m_DisplayName;

	private long m_RetrievedTimestamp;

	public string GetPlayerId()
	{
		return m_PlayerId;
	}

	public string GetDisplayName()
	{
		return m_DisplayName;
	}

	public long GetRetrievedTimestamp()
	{
		return m_RetrievedTimestamp;
	}

	public override void FromJSON(IDictionary a_JSONDict)
	{
		m_PlayerId = JSONObject.GetValueFromDict(a_JSONDict, "playerId", string.Empty);
		m_DisplayName = JSONObject.GetValueFromDict(a_JSONDict, "displayName", string.Empty);
		m_RetrievedTimestamp = JSONObject.GetLongValueFromDict(a_JSONDict, "timestamp", -1L);
	}
}
