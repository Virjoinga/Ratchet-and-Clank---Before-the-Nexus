using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HTTP
{
	public class Headers
	{
		private static byte[] EOL = new byte[2] { 13, 10 };

		private Dictionary<string, List<string>> headers = new Dictionary<string, List<string>>();

		public List<string> Keys
		{
			get
			{
				return headers.Keys.ToList();
			}
		}

		public void Add(string name, string value)
		{
			GetAll(name).Add(value);
		}

		public string Get(string name)
		{
			List<string> all = GetAll(name);
			if (all.Count == 0)
			{
				return string.Empty;
			}
			return all[0];
		}

		public bool Contains(string name)
		{
			List<string> all = GetAll(name);
			if (all.Count == 0)
			{
				return false;
			}
			return true;
		}

		public List<string> GetAll(string name)
		{
			foreach (string key in headers.Keys)
			{
				if (name.ToLower() == key.ToLower())
				{
					return headers[key];
				}
			}
			List<string> list = new List<string>();
			headers.Add(name, list);
			return list;
		}

		public void Set(string name, string value)
		{
			List<string> all = GetAll(name);
			all.Clear();
			all.Add(value);
		}

		public void Pop(string name)
		{
			if (headers.ContainsKey(name))
			{
				headers.Remove(name);
			}
		}

		public void Write(BinaryWriter stream)
		{
			foreach (string key in headers.Keys)
			{
				foreach (string item in headers[key])
				{
					stream.Write(Encoding.UTF8.GetBytes(key + ": " + item));
					stream.Write(EOL);
				}
			}
		}

		public void Clear()
		{
			headers.Clear();
		}
	}
}
