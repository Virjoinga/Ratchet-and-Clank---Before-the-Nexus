using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

namespace HTTP
{
	public class Request : BaseHTTP
	{
		public static Uri proxy = null;

		public bool isDone;

		public Exception exception;

		public int maximumRedirects = 8;

		public bool acceptGzip = true;

		public bool useCache;

		public readonly Headers headers = new Headers();

		public bool enableCookies = true;

		public float timeout;

		public static readonly CookieContainer cookies = new CookieContainer();

		private byte[] bytes;

		public string method;

		private string protocol = "HTTP/1.1";

		private static Dictionary<string, string> etags = new Dictionary<string, string>();

		private Action<Request> OnDone;

		public Response response { get; set; }

		public Uri uri { get; set; }

		public HttpConnection upgradedConnection { get; private set; }

		public float Progress
		{
			get
			{
				return (response != null) ? response.progress : 0f;
			}
		}

		public string Text
		{
			set
			{
				bytes = ((value != null) ? HTTPProtocol.enc.GetBytes(value) : null);
			}
		}

		public byte[] Bytes
		{
			set
			{
				bytes = value;
			}
		}

		public Request()
		{
			method = "GET";
		}

		public Request(string method, string uri)
		{
			this.method = method;
			this.uri = new Uri(uri);
		}

		public Request(string method, string uri, bool useCache)
		{
			this.method = method;
			this.uri = new Uri(uri);
			this.useCache = useCache;
		}

		public Request(string uri, WWWForm form)
		{
			method = "POST";
			this.uri = new Uri(uri);
			bytes = form.data;
			foreach (string key in form.headers.Keys)
			{
				headers.Set(key, (string)form.headers[key]);
			}
		}

		public Request(string method, string uri, byte[] bytes)
		{
			this.method = method;
			this.uri = new Uri(uri);
			this.bytes = bytes;
		}

		public Coroutine Send(Action<Request> OnDone)
		{
			this.OnDone = OnDone;
			return Send();
		}

		public Coroutine Send()
		{
			BeginSending();
			return UniWeb.Instance.StartCoroutine(_Wait());
		}

		public static Request BuildFromStream(string host, NetworkStream stream)
		{
			Request request = CreateFromTopLine(host, HTTPProtocol.ReadLine(stream));
			if (request == null)
			{
				return null;
			}
			HTTPProtocol.CollectHeaders(stream, request.headers);
			float progress = 0f;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				if (request.headers.Get("transfer-encoding").ToLower() == "chunked")
				{
					HTTPProtocol.ReadChunks(stream, memoryStream, ref progress);
					HTTPProtocol.CollectHeaders(stream, request.headers);
				}
				else
				{
					HTTPProtocol.ReadBody(stream, memoryStream, request.headers, true, ref progress);
				}
				request.Bytes = memoryStream.ToArray();
				return request;
			}
		}

		private static Request CreateFromTopLine(string host, string top)
		{
			string[] array = top.Split(' ');
			if (array.Length != 3)
			{
				return null;
			}
			if (array[2] != "HTTP/1.1")
			{
				return null;
			}
			Request request = new Request();
			request.method = array[0].ToUpper();
			request.uri = new Uri(host + array[1]);
			request.response = new Response(request);
			return request;
		}

		private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private HttpConnection CreateConnection(string host, int port, bool useSsl)
		{
			HttpConnection httpConnection = new HttpConnection();
			httpConnection.host = host;
			httpConnection.port = port;
			HttpConnection httpConnection2 = httpConnection;
			httpConnection2.Connect();
			if (useSsl)
			{
				httpConnection2.stream = new SslStream(httpConnection2.client.GetStream(), false, ValidateServerCertificate);
				SslStream sslStream = httpConnection2.stream as SslStream;
				sslStream.AuthenticateAsClient(uri.Host);
			}
			else
			{
				httpConnection2.stream = httpConnection2.client.GetStream();
			}
			return httpConnection2;
		}

		private IEnumerator Timeout()
		{
			yield return new WaitForSeconds(timeout);
			if (!isDone)
			{
				exception = new TimeoutException("Web request timed out");
				isDone = true;
			}
		}

		private void AddHeadersToRequest()
		{
			if (useCache)
			{
				string value = string.Empty;
				if (etags.TryGetValue(uri.AbsoluteUri, out value))
				{
					headers.Set("If-None-Match", value);
				}
			}
			string text = uri.Host;
			if (uri.Port != 80 && uri.Port != 443)
			{
				text = text + ":" + uri.Port;
			}
			headers.Set("Host", text);
			if (!enableCookies || !(uri != null))
			{
				return;
			}
			try
			{
				string cookieHeader = cookies.GetCookieHeader(uri);
				if (cookieHeader != null && cookieHeader.Length > 0)
				{
					headers.Set("Cookie", cookieHeader);
				}
			}
			catch (NullReferenceException)
			{
			}
			catch (IndexOutOfRangeException)
			{
			}
		}

		private void BeginSending()
		{
			isDone = false;
			if (timeout > 0f)
			{
				UniWeb.Instance.StartCoroutine(Timeout());
			}
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					int num = 0;
					HttpConnection httpConnection = null;
					while (num < maximumRedirects)
					{
						AddHeadersToRequest();
						Uri uri = ((proxy != null) ? proxy : ((WebRequest.DefaultWebProxy == null) ? this.uri : WebRequest.DefaultWebProxy.GetProxy(this.uri)));
						httpConnection = CreateConnection(uri.Host, uri.Port, uri.Scheme.ToLower() == "https");
						WriteToStream(httpConnection.stream);
						response = new Response(this);
						try
						{
							response.ReadFromStream(httpConnection.stream);
						}
						catch (HTTPException)
						{
							num++;
							continue;
						}
						if (enableCookies)
						{
							foreach (string item in response.headers.GetAll("Set-Cookie"))
							{
								try
								{
									cookies.SetCookies(this.uri, item);
								}
								catch (CookieException)
								{
								}
							}
						}
						switch (response.status)
						{
						case 101:
							upgradedConnection = httpConnection;
							break;
						case 307:
							this.uri = new Uri(response.headers.Get("Location"));
							num++;
							continue;
						case 301:
						case 302:
							method = "GET";
							this.uri = new Uri(response.headers.Get("Location"));
							num++;
							continue;
						}
						break;
					}
					if (upgradedConnection == null)
					{
						httpConnection.Dispose();
					}
					if (useCache && response != null)
					{
						string text = response.headers.Get("etag");
						if (text.Length > 0)
						{
							etags[this.uri.AbsoluteUri] = text;
						}
					}
				}
				catch (Exception ex3)
				{
					exception = ex3;
					response = null;
				}
				isDone = true;
			});
		}

		private void WriteToStream(Stream outputStream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(outputStream);
			bool flag = false;
			string text = ((!(proxy == null)) ? uri.AbsoluteUri : uri.PathAndQuery);
			binaryWriter.Write(HTTPProtocol.enc.GetBytes(method.ToUpper() + " " + text + " " + protocol));
			binaryWriter.Write(HTTPProtocol.EOL);
			if (uri.UserInfo != null && uri.UserInfo != string.Empty && !headers.Contains("Authorization"))
			{
				headers.Set("Authorization", "Basic " + Convert.ToBase64String(HTTPProtocol.enc.GetBytes(uri.UserInfo)));
			}
			if (!headers.Contains("Accept"))
			{
				headers.Add("Accept", "*/*");
			}
			if (bytes != null && bytes.Length > 0)
			{
				headers.Set("Content-Length", bytes.Length.ToString());
				headers.Set("Content-Type", "application/x-www-form-urlencoded");
				flag = true;
			}
			else
			{
				headers.Pop("Content-Length");
			}
			headers.Write(binaryWriter);
			binaryWriter.Write(HTTPProtocol.EOL);
			if (flag)
			{
				binaryWriter.Write(bytes);
				binaryWriter.Write(HTTPProtocol.EOL);
			}
		}

		private IEnumerator _Wait()
		{
			while (!isDone)
			{
				yield return null;
			}
			if (OnDone != null)
			{
				OnDone(this);
			}
		}

		private static void AOTStrippingReferences()
		{
			new RijndaelManaged();
		}
	}
}
