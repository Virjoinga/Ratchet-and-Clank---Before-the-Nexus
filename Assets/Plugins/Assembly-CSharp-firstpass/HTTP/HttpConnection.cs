using System;
using System.IO;
using System.Net.Sockets;

namespace HTTP
{
	public class HttpConnection : IDisposable
	{
		public string host;

		public int port;

		public TcpClient client;

		public Stream stream;

		public void Connect()
		{
			client = new TcpClient();
			client.Connect(host, port);
		}

		public void Dispose()
		{
			stream.Dispose();
		}
	}
}
