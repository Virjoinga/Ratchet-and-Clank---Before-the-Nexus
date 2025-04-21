using System.Collections.Generic;
using System.IO;
using Mono.Xml;
using Uniject;

namespace Unibill.Impl
{
	public class UnibillXmlParser : SmallXmlParser.IContentHandler
	{
		public class UnibillXElement
		{
			public Dictionary<string, string> attributes { get; private set; }

			public Dictionary<string, List<string>> kvps { get; private set; }

			public UnibillXElement(Dictionary<string, string> attributes, Dictionary<string, List<string>> kvps)
			{
				this.attributes = attributes;
				this.kvps = kvps;
			}

			public string TryGetFirstElement(string id)
			{
				if (kvps.ContainsKey(id))
				{
					List<string> list = kvps[id];
					if (list != null && list.Count > 0)
					{
						return list[0];
					}
				}
				return string.Empty;
			}
		}

		private SmallXmlParser parser;

		private IResourceLoader loader;

		private List<UnibillXElement> result;

		private string seeking;

		private bool reading;

		private Dictionary<string, string> currentAttributes;

		private Dictionary<string, List<string>> currentKvps;

		private string currentName;

		public UnibillXmlParser(SmallXmlParser parser, IResourceLoader loader)
		{
			this.loader = loader;
			this.parser = parser;
		}

		public List<UnibillXElement> Parse(string resourceFile, string forElements)
		{
			result = new List<UnibillXElement>();
			seeking = forElements;
			using (TextReader input = loader.openTextFile(resourceFile))
			{
				parser.Parse(input, this);
			}
			return result;
		}

		public void OnStartParsing(SmallXmlParser parser)
		{
		}

		public void OnEndParsing(SmallXmlParser parser)
		{
		}

		public void OnStartElement(string name, SmallXmlParser.IAttrList attrs)
		{
			if (reading)
			{
				currentName = name;
			}
			else if (name == seeking)
			{
				currentAttributes = new Dictionary<string, string>();
				for (int i = 0; i < attrs.Length; i++)
				{
					currentAttributes[attrs.Names[i]] = attrs.Values[i];
				}
				currentKvps = new Dictionary<string, List<string>>();
				reading = true;
			}
		}

		public void OnEndElement(string name)
		{
			if (name.Equals(seeking))
			{
				reading = false;
				result.Add(new UnibillXElement(currentAttributes, currentKvps));
			}
		}

		public void OnChars(string s)
		{
			if (reading)
			{
				if (!currentKvps.ContainsKey(currentName))
				{
					currentKvps[currentName] = new List<string>();
				}
				currentKvps[currentName].Add(s);
			}
		}

		public void OnIgnorableWhitespace(string s)
		{
		}

		public void OnProcessingInstruction(string name, string text)
		{
		}
	}
}
