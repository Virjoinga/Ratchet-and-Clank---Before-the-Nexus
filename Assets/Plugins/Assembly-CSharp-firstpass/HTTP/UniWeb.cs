using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HTTP
{
	public class UniWeb : MonoBehaviour
	{
		private static UniWeb _instance;

		private List<Action> onQuit = new List<Action>();

		public static UniWeb Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject("SimpleWWW", typeof(UniWeb)).GetComponent<UniWeb>();
					_instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
				}
				return _instance;
			}
		}

		private void Awake()
		{
			if (_instance != null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void Send(Request request, Action<Request> requestDelegate)
		{
			StartCoroutine(_Send(request, requestDelegate));
		}

		public void Send(Request request, Action<Response> responseDelegate)
		{
			StartCoroutine(_Send(request, responseDelegate));
		}

		private IEnumerator _Send(Request request, Action<Response> responseDelegate)
		{
			request.Send();
			while (!request.isDone)
			{
				yield return new WaitForEndOfFrame();
			}
			if (request.exception != null)
			{
				Debug.LogError(request.exception);
			}
			else
			{
				responseDelegate(request.response);
			}
		}

		private IEnumerator _Send(Request request, Action<Request> requestDelegate)
		{
			request.Send();
			while (!request.isDone)
			{
				yield return new WaitForEndOfFrame();
			}
			requestDelegate(request);
		}

		public void OnQuit(Action fn)
		{
			onQuit.Add(fn);
		}

		private void OnApplicationQuit()
		{
			if (this != _instance)
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
				return;
			}
			foreach (Action item in onQuit)
			{
				try
				{
					item();
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			_instance = null;
		}
	}
}
