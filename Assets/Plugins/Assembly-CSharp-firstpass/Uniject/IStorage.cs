namespace Uniject
{
	public interface IStorage
	{
		int GetInt(string key, int defaultValue);

		void SetInt(string key, int value);
	}
}
