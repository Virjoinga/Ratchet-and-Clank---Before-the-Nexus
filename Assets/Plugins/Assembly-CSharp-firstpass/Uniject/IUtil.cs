using System;
using UnityEngine;

namespace Uniject
{
	public interface IUtil
	{
		RuntimePlatform Platform { get; }

		string persistentDataPath { get; }

		DateTime currentTime { get; }

		T[] getAnyComponentsOfType<T>() where T : class;

		string loadedLevelName();
	}
}
