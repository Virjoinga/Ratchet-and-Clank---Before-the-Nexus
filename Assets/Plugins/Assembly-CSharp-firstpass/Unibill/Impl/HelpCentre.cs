using System;
using System.Collections.Generic;

namespace Unibill.Impl
{
	public class HelpCentre
	{
		private Dictionary<UnibillError, string> helpMap = new Dictionary<UnibillError, string>();

		public HelpCentre(UnibillXmlParser parser)
		{
			foreach (UnibillXmlParser.UnibillXElement item in parser.Parse("unibillStrings", "unibillError"))
			{
				UnibillError key = (UnibillError)(int)Enum.Parse(typeof(UnibillError), item.attributes["id"]);
				helpMap[key] = item.kvps["message"][0];
			}
		}

		public string getMessage(UnibillError error)
		{
			string arg = string.Format("http://www.outlinegames.com/unibillerrors#{0}", error);
			return string.Format("{0}.\nSee {1}", helpMap[error], arg);
		}
	}
}
