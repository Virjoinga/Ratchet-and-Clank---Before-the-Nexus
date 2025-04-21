using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace HTTP
{
	public class HTTPProtocol
	{
		public static byte[] EOL = new byte[2] { 13, 10 };

		public static UTF8Encoding enc = new UTF8Encoding();

		public static string ReadLine(Stream stream)
		{
			List<byte> list = new List<byte>();
			while (true)
			{
				int num = -1;
				try
				{
					num = stream.ReadByte();
				}
				catch (IOException)
				{
					throw new HTTPException("Terminated Stream");
				}
				if (num == -1)
				{
					throw new HTTPException("Unterminated Stream");
				}
				byte b = (byte)num;
				if (b == EOL[1])
				{
					break;
				}
				list.Add(b);
			}
			byte[] array = list.ToArray();
			return enc.GetString(array, 0, array.Length).Trim();
		}

		public static string[] ReadKeyValue(Stream stream)
		{
			string text = ReadLine(stream);
			if (text == string.Empty)
			{
				return null;
			}
			int num = text.IndexOf(':');
			if (num == -1)
			{
				return null;
			}
			return new string[2]
			{
				text.Substring(0, num).Trim(),
				text.Substring(num + 1).Trim()
			};
		}

		public static void ReadChunks(Stream inputStream, Stream output, ref float progress)
		{
			byte[] array = new byte[8192];
			while (true)
			{
				string text = ReadLine(inputStream);
				if (text == "0")
				{
					break;
				}
				int num = int.Parse(text, NumberStyles.AllowHexSpecifier);
				progress = 0f;
				int num2 = num;
				while (num > 0)
				{
					int num3 = inputStream.Read(array, 0, Mathf.Min(array.Length, num));
					output.Write(array, 0, num3);
					progress = Mathf.Clamp01(1f - (float)num / (float)num2);
					num -= num3;
				}
				progress = 1f;
				inputStream.ReadByte();
				inputStream.ReadByte();
			}
		}

		public static void ReadBody(Stream inputStream, Stream output, Headers headers, bool strict, ref float progress)
		{
			byte[] array = new byte[8192];
			int result = 0;
			if (int.TryParse(headers.Get("Content-Length"), out result))
			{
				if (result <= 0)
				{
					return;
				}
				int num = result;
				while (num > 0)
				{
					int num2 = inputStream.Read(array, 0, array.Length);
					if (num2 == 0)
					{
						break;
					}
					num -= num2;
					output.Write(array, 0, num2);
					progress = Mathf.Clamp01(1f - (float)num / (float)result);
				}
				return;
			}
			if (!strict)
			{
				for (int num3 = inputStream.Read(array, 0, array.Length); num3 > 0; num3 = inputStream.Read(array, 0, array.Length))
				{
					output.Write(array, 0, num3);
				}
			}
			progress = 1f;
		}

		public static void CollectHeaders(Stream inputStream, Headers headers)
		{
			while (true)
			{
				string[] array = ReadKeyValue(inputStream);
				if (array == null)
				{
					break;
				}
				headers.Add(array[0], array[1]);
			}
		}

		public static void WriteResponse(Stream stream, int status, string message, Headers headers, byte[] bytes)
		{
			if (bytes == null)
			{
				bytes = new byte[0];
			}
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(enc.GetBytes(string.Format("HTTP/1.1 {0} {1}\r\n", status, message)));
			headers.Set("Content-Length", bytes.Length.ToString());
			headers.Write(binaryWriter);
			binaryWriter.Write(enc.GetBytes("\r\n"));
			binaryWriter.Write(bytes);
			binaryWriter.Flush();
		}
	}
}
