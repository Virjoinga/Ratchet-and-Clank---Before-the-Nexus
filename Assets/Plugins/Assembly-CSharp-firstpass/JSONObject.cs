using System.Collections;
using UnityEngine;

public class JSONObject
{
	public virtual void FromJSON(IDictionary a_JSONDict)
	{
	}

	public static int GetIntValueFromDict(IDictionary a_Dict, object a_Key, int a_DefaultValue)
	{
		int valueFromDict = GetValueFromDict(a_Dict, a_Key, int.MinValue, false);
		if (valueFromDict == int.MinValue)
		{
			return (int)GetFloatValueFromDict(a_Dict, a_Key, a_DefaultValue);
		}
		return valueFromDict;
	}

	public static long GetLongValueFromDict(IDictionary a_Dict, object a_Key, long a_DefaultValue)
	{
		long valueFromDict = GetValueFromDict(a_Dict, a_Key, long.MinValue, false);
		if (valueFromDict == long.MinValue)
		{
			return (long)GetFloatValueFromDict(a_Dict, a_Key, a_DefaultValue);
		}
		return valueFromDict;
	}

	public static float GetFloatValueFromDict(IDictionary a_Dict, object a_Key, float a_DefaultValue)
	{
		double valueFromDict = GetValueFromDict(a_Dict, a_Key, double.NaN, false);
		if (double.IsNaN(valueFromDict))
		{
			float valueFromDict2 = GetValueFromDict(a_Dict, a_Key, float.NaN, false);
			if (float.IsNaN(valueFromDict2))
			{
				int valueFromDict3 = GetValueFromDict(a_Dict, a_Key, int.MinValue, false);
				if (valueFromDict3 == int.MinValue)
				{
					Debug.LogWarning(string.Concat("Dictionary does not have key [", a_Key, "], returning default float value of [", a_DefaultValue, "]..."));
					return a_DefaultValue;
				}
				return valueFromDict3;
			}
			return valueFromDict2;
		}
		return (float)valueFromDict;
	}

	public static T GetValueFromDict<T>(IDictionary a_Dict, object a_Key, T a_DefaultValue, bool a_DoDebug = true)
	{
		if (a_Dict.Contains(a_Key))
		{
			object obj = a_Dict[a_Key];
			if (obj is T)
			{
				return (T)obj;
			}
		}
		if (a_DoDebug)
		{
			Debug.LogWarning(string.Concat("Dictionary does not have key [", a_Key, "], returning default value of [", a_DefaultValue, "]..."));
		}
		return a_DefaultValue;
	}
}
