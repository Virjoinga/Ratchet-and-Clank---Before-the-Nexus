using System.Collections;

public class GoogleAchievement : JSONObject
{
	public const int STATE_UNLOCKED = 0;

	public const int STATE_REVEALED = 1;

	public const int STATE_HIDDEN = 2;

	public const int TYPE_STANDARD = 0;

	public const int TYPE_INCREMENTAL = 1;

	private string m_AchievementId;

	private int m_CurrentSteps;

	private string m_Description;

	private string m_FormattedCurrentSteps;

	private string m_FormattedTotalSteps;

	private long m_LastUpdatedTimestamp;

	private string m_Name;

	private GooglePlayer m_Player;

	private int m_State;

	private int m_TotalSteps;

	private int m_Type;

	public string GetAchievementId()
	{
		return m_AchievementId;
	}

	public int GetCurrentSteps()
	{
		return m_CurrentSteps;
	}

	public string GetDescription()
	{
		return m_Description;
	}

	public string GetFormattedCurrentSteps()
	{
		return m_FormattedCurrentSteps;
	}

	public string GetFormattedTotalSteps()
	{
		return m_FormattedTotalSteps;
	}

	public long GetLastUpdatedTimestamp()
	{
		return m_LastUpdatedTimestamp;
	}

	public string GetName()
	{
		return m_Name;
	}

	public GooglePlayer GetPlayer()
	{
		return m_Player;
	}

	public int GetState()
	{
		return m_State;
	}

	public int GetTotalSteps()
	{
		return m_TotalSteps;
	}

	public int GetAchievementType()
	{
		return m_Type;
	}

	public bool IsHidden()
	{
		return GetState() == 2;
	}

	public bool IsRevealed()
	{
		return GetState() == 1;
	}

	public bool IsUnlocked()
	{
		return GetState() == 0;
	}

	public bool IsIncrementalAchievement()
	{
		return GetAchievementType() == 1;
	}

	public bool IsStandardAchievement()
	{
		return GetAchievementType() == 0;
	}

	public override void FromJSON(IDictionary a_JSONDict)
	{
		m_AchievementId = JSONObject.GetValueFromDict(a_JSONDict, "achievementId", string.Empty);
		m_CurrentSteps = JSONObject.GetIntValueFromDict(a_JSONDict, "currentSteps", -1);
		m_Description = JSONObject.GetValueFromDict(a_JSONDict, "description", string.Empty);
		m_FormattedCurrentSteps = JSONObject.GetValueFromDict(a_JSONDict, "formattedCurrentSteps", string.Empty);
		m_FormattedTotalSteps = JSONObject.GetValueFromDict(a_JSONDict, "formattedTotalSteps", string.Empty);
		m_LastUpdatedTimestamp = JSONObject.GetLongValueFromDict(a_JSONDict, "lastUpdatedTimestamp", -1L);
		m_Name = JSONObject.GetValueFromDict(a_JSONDict, "name", string.Empty);
		m_State = JSONObject.GetIntValueFromDict(a_JSONDict, "state", 2);
		m_TotalSteps = JSONObject.GetIntValueFromDict(a_JSONDict, "totalSteps", -1);
		m_Type = JSONObject.GetIntValueFromDict(a_JSONDict, "type", 0);
		IDictionary valueFromDict = JSONObject.GetValueFromDict<IDictionary>(a_JSONDict, "player", null);
		if (valueFromDict != null)
		{
			m_Player = new GooglePlayer();
			m_Player.FromJSON(valueFromDict);
		}
	}
}
