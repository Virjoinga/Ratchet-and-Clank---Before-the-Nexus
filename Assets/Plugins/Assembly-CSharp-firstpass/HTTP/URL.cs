using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HTTP
{
	public class URL
	{
		private static string safeChars = "-_.~abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		public static string Encode(string value)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in value)
			{
				if (safeChars.IndexOf(c) != -1)
				{
					stringBuilder.Append(c);
				}
				else
				{
					stringBuilder.Append('%' + string.Format("{0:X2}", (int)c));
				}
			}
			return stringBuilder.ToString();
		}

		public static string Decode(string s)
		{
			return WWW.UnEscapeURL(s);
		}

		public static Dictionary<string, string> KeyValue(string queryString)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (queryString.Length == 0)
			{
				return dictionary;
			}
			string[] array = queryString.Split('&');
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split('=');
				if (array3.Length >= 2)
				{
					dictionary[Decode(array3[0])] = Decode(array3[1]);
				}
			}
			return dictionary;
		}
	}
}
