using System.IO;

namespace Uniject
{
	public interface IResourceLoader
	{
		TextReader openTextFile(string path);
	}
}
