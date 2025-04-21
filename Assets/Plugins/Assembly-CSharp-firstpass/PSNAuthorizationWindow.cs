using System.Runtime.CompilerServices;
using UnityEngine;

public class PSNAuthorizationWindow : MonoBehaviour
{
	protected static PSNAuthorizationWindow instance;

	protected AndroidJavaObject window;

	protected bool didLogin;

	[method: MethodImpl(32)]
	public static event OnLoginSuccess onLoginSuccess;

	[method: MethodImpl(32)]
	public static event OnLoginFail onLoginFail;

	private void Start()
	{
		if ((bool)instance)
		{
			Debug.LogError("Only one instance of PSNAuthorizationWindow is allowed");
			return;
		}
		instance = this;
		window = new AndroidJavaObject("com.darkside.PSNAutorizationWindow");
		window.Call("Init", base.name);
		didLogin = false;
	}

	public void AutoLogin(string login, string password)
	{
		window.Call("StartLoginWithCredentials", login, password);
	}

	public static void LoginWithCredentials(string login, string password)
	{
		instance.AutoLogin(login, password);
	}

	public void OnSuccess(string code)
	{
		if (!didLogin)
		{
			Debug.Log("LOGIN SUCCESS: " + code);
			if (PSNAuthorizationWindow.onLoginSuccess != null)
			{
				PSNAuthorizationWindow.onLoginSuccess(code);
			}
			window.Call("SetVisible", false);
			didLogin = true;
		}
	}

	public void OnOptOut(string result)
	{
		Debug.Log("LOGIN SKIP: " + result);
		if (PSNAuthorizationWindow.onLoginFail != null)
		{
			PSNAuthorizationWindow.onLoginFail(result);
		}
		window.Call("SetVisible", false);
	}

	public void OnDisable()
	{
		instance = null;
		if (window != null)
		{
			window.Call("SetVisible", false);
			window.Call("Destroy", null);
		}
	}

	private void Update()
	{
	}
}
