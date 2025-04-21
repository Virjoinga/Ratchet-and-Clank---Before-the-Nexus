using UnityEngine;

namespace Uniject.Impl
{
	public class UnityLogger : ILogger
	{
		public string prefix { get; set; }

		public void LogWarning(string message, params object[] formatArgs)
		{
			Debug.LogWarning(string.Format(message, formatArgs));
		}

		public void Log(string message)
		{
		}

		public void Log(string message, object[] args)
		{
			Log(string.Format(message, args));
		}

		public void LogError(string message, params object[] formatArgs)
		{
			Debug.LogError(formatMessage(string.Format(message, formatArgs)));
		}

		private string formatMessage(string message)
		{
			if (prefix == null)
			{
				return message;
			}
			return string.Format("{0}: {1}", prefix, message);
		}
	}
}
