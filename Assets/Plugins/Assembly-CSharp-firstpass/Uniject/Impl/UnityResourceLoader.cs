using System.IO;
using UnityEngine;

namespace Uniject.Impl
{
	public class UnityResourceLoader : IResourceLoader
	{
		public TextReader openTextFile(string path)
		{
			return new StringReader(((TextAsset)Resources.Load(path)).text);
		}
	}
}
