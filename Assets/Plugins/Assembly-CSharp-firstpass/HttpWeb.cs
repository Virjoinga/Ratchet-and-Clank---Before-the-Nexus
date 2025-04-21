using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HTTP;
using UnityEngine;

public class HttpWeb : MonoBehaviour
{
	protected static HttpWeb instance;

	public string url { get; private set; }

	public string text { get; private set; }

	public string error { get; private set; }

	private void Start()
	{
		if ((bool)instance)
		{
			Debug.LogError("Only one instance of HttpWeb is allowed");
		}
		else
		{
			instance = this;
		}
	}

	public void OnDisable()
	{
		instance = null;
	}

	public static void GET(string url, Dictionary<string, string> paramFields = null, OnHttpWebPassDelegate passCallback = null, OnHttpWebFailDelegate failCallback = null, object userData = null, Hashtable headers = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (paramFields != null)
		{
			bool flag = true;
			foreach (KeyValuePair<string, string> paramField in paramFields)
			{
				if (!flag)
				{
					stringBuilder.Append("&");
				}
				else
				{
					stringBuilder.Append("?");
					flag = false;
				}
				stringBuilder.Append(paramField.Key + "=" + paramField.Value);
			}
		}
		Request request = new Request("GET", url + stringBuilder.ToString());
		if (headers != null)
		{
			foreach (DictionaryEntry header in headers)
			{
				request.headers.Set(Convert.ToString(header.Key), Convert.ToString(header.Value));
			}
		}
		instance.url = url;
		instance.StartCoroutine(sendRequest(request, passCallback, failCallback, userData));
	}

	public static void POST(string url, Dictionary<string, string> textFields = null, Dictionary<string, byte[]> binaryFields = null, OnHttpWebPassDelegate passCallback = null, OnHttpWebFailDelegate failCallback = null, object userData = null, Hashtable headers = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (textFields != null)
		{
			bool flag = true;
			foreach (KeyValuePair<string, string> textField in textFields)
			{
				if (!flag)
				{
					stringBuilder.Append("&");
				}
				else
				{
					flag = false;
				}
				stringBuilder.Append(textField.Key + "=" + textField.Value);
			}
		}
		if (binaryFields != null)
		{
		}
		Request request = new Request("POST", url);
		if (headers != null)
		{
			foreach (DictionaryEntry header in headers)
			{
				request.headers.Set(Convert.ToString(header.Key), Convert.ToString(header.Value));
			}
		}
		instance.url = url;
		request.Bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		instance.StartCoroutine(sendRequest(request, passCallback, failCallback, userData));
	}

	private static IEnumerator sendRequest(Request request, OnHttpWebPassDelegate passCallback = null, OnHttpWebFailDelegate failCallback = null, object userData = null)
	{
		yield return request.Send();
		try
		{
			if (request.exception == null)
			{
				instance.text = request.response.Text;
				if (request.response.status == 200 || request.response.status == 201)
				{
					if (passCallback != null)
					{
						passCallback(instance, userData);
					}
				}
				else
				{
					instance.error = request.response.status + " " + request.response.message + " " + request.response.Text;
					if (failCallback != null)
					{
						failCallback(instance, userData);
					}
				}
				Debug.Log("HttpWeb Response: " + request.response.status + " " + request.response.message + " " + request.response.Text);
			}
			else
			{
				instance.error = request.exception.Message;
				if (failCallback != null)
				{
					failCallback(instance, userData);
				}
				Debug.LogError(string.Concat("HttpWeb Error: ", request.method, request.exception, request.exception.Message, request.exception.StackTrace));
			}
		}
		catch (Exception e)
		{
			if (failCallback != null)
			{
				failCallback(instance, userData);
			}
			Debug.LogError(e.Message + e.StackTrace);
		}
	}
}
