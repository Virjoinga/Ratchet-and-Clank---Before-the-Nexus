using System;

[Serializable]
public class EventConditionEntry
{
	public string conditionParam;

	public EventConditionParamTypes conditionParamType;

	public EventConditionModes conditionMode;

	public float floatValue;

	public int intValue;

	public bool boolValue;
}
