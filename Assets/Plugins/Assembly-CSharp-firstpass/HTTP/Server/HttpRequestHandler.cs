using UnityEngine;

namespace HTTP.Server
{
	public class HttpRequestHandler : MonoBehaviour
	{
		public string path = "/";

		public virtual void GET(Request request)
		{
		}

		public virtual void PUT(Request request)
		{
		}

		public virtual void POST(Request request)
		{
		}

		public virtual void DELETE(Request request)
		{
		}

		public void Dispatch(Request request)
		{
			request.response.status = 200;
			request.response.message = "OK";
			string text = request.method.ToUpper();
			if (text == "GET")
			{
				GET(request);
			}
			if (text == "PUT")
			{
				PUT(request);
			}
			if (text == "POST")
			{
				POST(request);
			}
			if (text == "DELETE")
			{
				DELETE(request);
			}
		}
	}
}
