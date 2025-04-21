using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace HTTP
{
	public class JsonSerializer
	{
		public const int TOKEN_NONE = 0;

		public const int TOKEN_CURLY_OPEN = 1;

		public const int TOKEN_CURLY_CLOSE = 2;

		public const int TOKEN_SQUARED_OPEN = 3;

		public const int TOKEN_SQUARED_CLOSE = 4;

		public const int TOKEN_COLON = 5;

		public const int TOKEN_COMMA = 6;

		public const int TOKEN_STRING = 7;

		public const int TOKEN_NUMBER = 8;

		public const int TOKEN_TRUE = 9;

		public const int TOKEN_FALSE = 10;

		public const int TOKEN_NULL = 11;

		public static object Decode(byte[] json)
		{
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			return Decode(uTF8Encoding.GetString(json, 0, json.Length));
		}

		public static object Decode(string json)
		{
			bool success = true;
			return Decode(json, ref success);
		}

		public static void Decode(object instance, string json)
		{
			object obj = Decode(json);
			PopulateObject(instance.GetType(), obj, instance);
		}

		public static object Decode(string json, ref bool success)
		{
			success = true;
			if (json != null)
			{
				char[] json2 = json.ToCharArray();
				int index = 0;
				return ParseValue(json2, ref index, ref success);
			}
			return null;
		}

		public static string Encode(object json)
		{
			StringBuilder stringBuilder = new StringBuilder();
			return (!SerializeValue(json, stringBuilder)) ? null : stringBuilder.ToString();
		}

		public static T Decode<T>(byte[] json) where T : class, new()
		{
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			return Decode<T>(uTF8Encoding.GetString(json, 0, json.Length));
		}

		public static T Decode<T>(string json) where T : class, new()
		{
			bool success = true;
			object obj = Decode(json, ref success);
			return PopulateObject(typeof(T), obj) as T;
		}

		private static object PopulateObject(Type T, object obj)
		{
			return PopulateObject(T, obj, null);
		}

		private static object PopulateObject(Type T, object obj, object instance)
		{
			if (obj == null)
			{
				return null;
			}
			if (T.IsAssignableFrom(obj.GetType()))
			{
				instance = obj;
			}
			else if (obj is Hashtable)
			{
				Hashtable hashtable = (Hashtable)obj;
				if (instance == null)
				{
					instance = Activator.CreateInstance(T);
				}
				FieldInfo[] fields = T.GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					if (hashtable.ContainsKey(fieldInfo.Name))
					{
						fieldInfo.SetValue(instance, PopulateObject(fieldInfo.FieldType, hashtable[fieldInfo.Name]));
					}
				}
			}
			else if (obj is IEnumerable)
			{
				if (instance == null)
				{
					instance = Activator.CreateInstance(T);
				}
				IList list = instance as IList;
				if (list != null)
				{
					Type t = typeof(object);
					Type type = instance.GetType();
					if (type.IsGenericType)
					{
						Type[] genericArguments = type.GetGenericArguments();
						if (genericArguments.Length != 1)
						{
							return null;
						}
						t = genericArguments[0];
					}
					foreach (object item in (IEnumerable)obj)
					{
						list.Add(PopulateObject(t, item));
					}
				}
			}
			return instance;
		}

		protected static Hashtable ParseObject(char[] json, ref int index, ref bool success)
		{
			Hashtable hashtable = new Hashtable();
			NextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				switch (LookAhead(json, index))
				{
				case 0:
					success = false;
					return null;
				case 6:
					NextToken(json, ref index);
					continue;
				case 2:
					NextToken(json, ref index);
					return hashtable;
				}
				string key = ParseString(json, ref index, ref success);
				if (!success)
				{
					success = false;
					return null;
				}
				int num = NextToken(json, ref index);
				if (num != 5)
				{
					success = false;
					return null;
				}
				object value = ParseValue(json, ref index, ref success);
				if (!success)
				{
					success = false;
					return null;
				}
				hashtable[key] = value;
			}
			return hashtable;
		}

		protected static ArrayList ParseArray(char[] json, ref int index, ref bool success)
		{
			ArrayList arrayList = new ArrayList();
			NextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				switch (LookAhead(json, index))
				{
				case 0:
					success = false;
					return null;
				case 6:
					NextToken(json, ref index);
					continue;
				case 4:
					break;
				default:
				{
					object value = ParseValue(json, ref index, ref success);
					if (!success)
					{
						return null;
					}
					arrayList.Add(value);
					continue;
				}
				}
				NextToken(json, ref index);
				break;
			}
			return arrayList;
		}

		protected static object ParseValue(char[] json, ref int index, ref bool success)
		{
			switch (LookAhead(json, index))
			{
			case 7:
				return ParseString(json, ref index, ref success);
			case 8:
				return ParseNumber(json, ref index, ref success);
			case 1:
				return ParseObject(json, ref index, ref success);
			case 3:
				return ParseArray(json, ref index, ref success);
			case 9:
				NextToken(json, ref index);
				return true;
			case 10:
				NextToken(json, ref index);
				return false;
			case 11:
				NextToken(json, ref index);
				return null;
			default:
				success = false;
				return null;
			}
		}

		protected static string ParseString(char[] json, ref int index, ref bool success)
		{
			StringBuilder stringBuilder = new StringBuilder();
			EatWhitespace(json, ref index);
			char c = json[index++];
			bool flag = false;
			while (!flag && index != json.Length)
			{
				c = json[index++];
				switch (c)
				{
				case '"':
					flag = true;
					break;
				case '\\':
				{
					if (index == json.Length)
					{
						break;
					}
					switch (json[index++])
					{
					case '"':
						stringBuilder.Append('"');
						continue;
					case '\\':
						stringBuilder.Append('\\');
						continue;
					case '/':
						stringBuilder.Append('/');
						continue;
					case 'b':
						stringBuilder.Append('\b');
						continue;
					case 'f':
						stringBuilder.Append('\f');
						continue;
					case 'n':
						stringBuilder.Append('\n');
						continue;
					case 'r':
						stringBuilder.Append('\r');
						continue;
					case 't':
						stringBuilder.Append('\t');
						continue;
					case 'u':
						break;
					default:
						continue;
					}
					int num = json.Length - index;
					if (num < 4)
					{
						break;
					}
					uint result;
					if (!(success = uint.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result)))
					{
						return string.Empty;
					}
					stringBuilder.Append(char.ConvertFromUtf32((int)result));
					index += 4;
					continue;
				}
				default:
					stringBuilder.Append(c);
					continue;
				}
				break;
			}
			if (!flag)
			{
				success = false;
				return null;
			}
			return stringBuilder.ToString();
		}

		protected static object ParseNumber(char[] json, ref int index, ref bool success)
		{
			EatWhitespace(json, ref index);
			int lastIndexOfNumber = GetLastIndexOfNumber(json, index);
			int length = lastIndexOfNumber - index + 1;
			string text = new string(json, index, length);
			index = lastIndexOfNumber + 1;
			if (text.Contains("."))
			{
				float result;
				if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
				{
					return result;
				}
				return text;
			}
			long result2;
			if (long.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
			{
				return result2;
			}
			return text;
		}

		protected static int GetLastIndexOfNumber(char[] json, int index)
		{
			int i;
			for (i = index; i < json.Length && "0123456789+-.eE".IndexOf(json[i]) != -1; i++)
			{
			}
			return i - 1;
		}

		protected static void EatWhitespace(char[] json, ref int index)
		{
			while (index < json.Length && " \t\n\r".IndexOf(json[index]) != -1)
			{
				index++;
			}
		}

		protected static int LookAhead(char[] json, int index)
		{
			int index2 = index;
			return NextToken(json, ref index2);
		}

		protected static int NextToken(char[] json, ref int index)
		{
			EatWhitespace(json, ref index);
			if (index == json.Length)
			{
				return 0;
			}
			char c = json[index];
			index++;
			switch (c)
			{
			case '{':
				return 1;
			case '}':
				return 2;
			case '[':
				return 3;
			case ']':
				return 4;
			case ',':
				return 6;
			case '"':
				return 7;
			case '-':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				return 8;
			case ':':
				return 5;
			default:
			{
				index--;
				int num = json.Length - index;
				if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
				{
					index += 5;
					return 10;
				}
				if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
				{
					index += 4;
					return 9;
				}
				if (num >= 4 && json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
				{
					index += 4;
					return 11;
				}
				return 0;
			}
			}
		}

		protected static bool SerializeValue(object value, StringBuilder builder)
		{
			bool result = true;
			if (value is string)
			{
				result = SerializeString((string)value, builder);
			}
			else if (value is Hashtable)
			{
				result = SerializeObject((Hashtable)value, builder);
			}
			else if (value is IEnumerable)
			{
				result = SerializeArray((IEnumerable)value, builder);
			}
			else if (value is float)
			{
				result = SerializeNumber(Convert.ToSingle(value), builder);
			}
			else if (value is int || value is long || value is uint)
			{
				result = SerializeNumber(Convert.ToInt64(value), builder);
			}
			else if (value is double)
			{
				result = SerializeNumber(Convert.ToDouble(value), builder);
			}
			else if (value is bool && (bool)value)
			{
				builder.Append("true");
			}
			else if (value is bool && !(bool)value)
			{
				builder.Append("false");
			}
			else if (value == null)
			{
				builder.Append("null");
			}
			else if (value is DateTime)
			{
				builder.Append(((DateTime)value).ToString("o"));
			}
			else
			{
				Hashtable hashtable = new Hashtable();
				FieldInfo[] fields = value.GetType().GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					if (!fieldInfo.IsNotSerialized)
					{
						hashtable[fieldInfo.Name] = fieldInfo.GetValue(value);
					}
				}
				PropertyInfo[] properties = value.GetType().GetProperties();
				foreach (PropertyInfo propertyInfo in properties)
				{
					hashtable[propertyInfo.Name] = propertyInfo.GetValue(value, null);
				}
				SerializeObject(hashtable, builder);
			}
			return result;
		}

		protected static bool SerializeObject(Hashtable anObject, StringBuilder builder)
		{
			builder.Append("{");
			IDictionaryEnumerator enumerator = anObject.GetEnumerator();
			bool flag = true;
			while (enumerator.MoveNext())
			{
				string aString = enumerator.Key.ToString();
				object value = enumerator.Value;
				if (!flag)
				{
					builder.Append(", ");
				}
				SerializeString(aString, builder);
				builder.Append(":");
				if (!SerializeValue(value, builder))
				{
					return false;
				}
				flag = false;
			}
			builder.Append("}");
			return true;
		}

		protected static bool SerializeArray(IEnumerable anArray, StringBuilder builder)
		{
			builder.Append("[");
			bool flag = true;
			foreach (object item in anArray)
			{
				if (!flag)
				{
					builder.Append(", ");
				}
				if (!SerializeValue(item, builder))
				{
					return false;
				}
				flag = false;
			}
			builder.Append("]");
			return true;
		}

		protected static bool SerializeString(string aString, StringBuilder builder)
		{
			builder.Append("\"");
			char[] array = aString.ToCharArray();
			foreach (char c in array)
			{
				switch (c)
				{
				case '"':
					builder.Append("\\\"");
					continue;
				case '\\':
					builder.Append("\\\\");
					continue;
				case '\b':
					builder.Append("\\b");
					continue;
				case '\f':
					builder.Append("\\f");
					continue;
				case '\n':
					builder.Append("\\n");
					continue;
				case '\r':
					builder.Append("\\r");
					continue;
				case '\t':
					builder.Append("\\t");
					continue;
				}
				int num = Convert.ToInt32(c);
				if (num >= 32 && num <= 126)
				{
					builder.Append(c);
				}
				else
				{
					builder.Append("\\u" + Convert.ToString(num, 16).PadLeft(4, '0'));
				}
			}
			builder.Append("\"");
			return true;
		}

		protected static bool SerializeNumber(int number, StringBuilder builder)
		{
			builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
			return true;
		}

		protected static bool SerializeNumber(float number, StringBuilder builder)
		{
			builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
			return true;
		}

		protected static bool SerializeNumber(long number, StringBuilder builder)
		{
			builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
			return true;
		}

		protected static bool SerializeNumber(double number, StringBuilder builder)
		{
			builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
			return true;
		}

		protected static bool IsNumeric(object o)
		{
			float result;
			return o != null && float.TryParse(o.ToString(), out result);
		}
	}
}
