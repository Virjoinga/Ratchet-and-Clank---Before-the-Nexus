namespace Uniject
{
	public interface ILogger
	{
		string prefix { get; set; }

		void Log(string message);

		void Log(string message, params object[] formatArgs);

		void LogWarning(string message, params object[] formatArgs);

		void LogError(string message, params object[] formatArgs);
	}
}
