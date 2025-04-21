using System.IO;
using UnityEngine;

namespace HTTP
{
	public class WebCache
	{
		public string root;

		public WebCache()
		{
			root = Application.persistentDataPath;
		}

		public void Delete(string filename)
		{
			string path = Path.Combine(root, filename);
			File.Delete(path);
		}

		public void Download(string filename, string url)
		{
			Request request = new Request("HEAD", url);
			request.Send(delegate(Request headreq)
			{
				if (headreq.exception == null)
				{
					if (headreq.response.status == 200)
					{
						Debug.Log(headreq.response.headers.Get("Content-Length"));
					}
				}
				else
				{
					Debug.Log(headreq.exception);
				}
			});
		}

		private void RecvHead(Request req)
		{
		}
	}
}
