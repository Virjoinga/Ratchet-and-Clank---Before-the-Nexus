using System.IO;
using UnityEngine;

namespace HTTP
{
	public class Response : BaseHTTP
	{
		public string protocol;

		public float progress;

		public readonly Headers headers = new Headers();

		private Request request;

		private byte[] bytes;

		public int status { get; set; }

		public string message { get; set; }

		public string Text
		{
			get
			{
				return HTTPProtocol.enc.GetString(Bytes, 0, Bytes.Length);
			}
			set
			{
				Bytes = HTTPProtocol.enc.GetBytes(value);
			}
		}

		public byte[] Bytes
		{
			get
			{
				return bytes;
			}
			set
			{
				bytes = value;
			}
		}

		public Response(Request request)
		{
			this.request = request;
		}

		public AssetBundleCreateRequest AssetBundleCreateRequest()
		{
			return AssetBundle.CreateFromMemory(Bytes);
		}

		public void ReadFromStream(Stream inputStream)
		{
			progress = 0f;
			if (inputStream == null)
			{
				throw new HTTPException("Cannot read from server, server probably dropped the connection.");
			}
			string[] array = HTTPProtocol.ReadLine(inputStream).Split(' ');
			status = -1;
			int result = -1;
			if (array.Length <= 0 || !int.TryParse(array[1], out result))
			{
				throw new HTTPException("Bad Status Code, server probably dropped the connection.");
			}
			status = result;
			message = string.Join(" ", array, 2, array.Length - 2);
			protocol = array[0];
			HTTPProtocol.CollectHeaders(inputStream, headers);
			if (status == 101)
			{
				progress = 1f;
				return;
			}
			if (status == 204)
			{
				progress = 1f;
				return;
			}
			bool flag = headers.Get("Transfer-Encoding").ToLower() == "chunked";
			if (request.method.ToLower() == "head")
			{
				progress = 1f;
				return;
			}
			using (MemoryStream output = new MemoryStream())
			{
				if (flag)
				{
					HTTPProtocol.ReadChunks(inputStream, output, ref progress);
					HTTPProtocol.CollectHeaders(inputStream, headers);
				}
				else
				{
					HTTPProtocol.ReadBody(inputStream, output, headers, false, ref progress);
				}
				ProcessReceivedBytes(output);
			}
		}

		private void ProcessReceivedBytes(MemoryStream output)
		{
			lock (output)
			{
				bytes = output.ToArray();
			}
		}
	}
}
