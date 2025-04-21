using UnityEngine;

namespace Uniject.Impl
{
	public class UnityPlayerPrefsStorage : IStorage
	{
		public int GetInt(string key, int defaultValue)
		{
			return PlayerPrefs.GetInt(key, defaultValue);
		}

		public void SetInt(string key, int value)
		{
			PlayerPrefs.SetInt(key, value);
		}
	}
}
