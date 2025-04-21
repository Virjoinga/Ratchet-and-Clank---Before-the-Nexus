using System;
using UnityEngine;

public class UIPSNLogin : UIScreen
{
	public UIInput UsernameInput;

	public UIInput PasswordInput;

	public UIButton LoginButton;

	public UIButton BackButton;

	public float Timeout = 20f;

	public float LoginTime;

	private bool LoggingIn;

	private void Start()
	{
		ServerConnection.onLoginFlowSuccess += OnLoginFlowSuccess;
		ServerConnection.onLoginFlowFail += OnLoginFlowFail;
		UIEventListener.Get(LoginButton.gameObject).onClick = OnLoginClick;
		UIEventListener.Get(LoginButton.gameObject).onPress = OnLoginPressed;
		UIEventListener.Get(BackButton.gameObject).onPress = OnBackButtonPressed;
	}

	public override void Show()
	{
		base.Show();
		EasyAnalytics.Instance.sendView("/PSNLogin");
		DisableButton();
	}

	public void InitServerConnection()
	{
		ServerConnection component = GameController.instance.GetComponent<ServerConnection>();
		if (component != null)
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
		PSNAuthorizationWindow component2 = GameController.instance.GetComponent<PSNAuthorizationWindow>();
		if (component2 != null)
		{
			UnityEngine.Object.DestroyImmediate(component2);
		}
		HttpWeb component3 = GameController.instance.GetComponent<HttpWeb>();
		if (component3 != null)
		{
			UnityEngine.Object.DestroyImmediate(component3);
		}
		GameController.instance.gameObject.AddComponent<ServerConnection>();
		GameController.instance.gameObject.AddComponent<PSNAuthorizationWindow>();
		GameController.instance.gameObject.AddComponent<HttpWeb>();
	}

	private void OnLoginPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			BackButton.GetComponent<BoxCollider>().collider.enabled = false;
		}
		else
		{
			BackButton.GetComponent<BoxCollider>().collider.enabled = true;
		}
	}

	private void OnBackButtonPressed(GameObject obj, bool Down)
	{
		if (Down)
		{
			LoginButton.GetComponent<BoxCollider>().collider.enabled = false;
		}
		else
		{
			LoginButton.GetComponent<BoxCollider>().collider.enabled = true;
		}
	}

	public void DisableButton()
	{
		LoginButton.GetComponent<BoxCollider>().collider.enabled = false;
		LoginButton.UpdateColor(false, true);
	}

	public void EnableButton()
	{
		LoginButton.GetComponent<BoxCollider>().collider.enabled = true;
		LoginButton.UpdateColor(true, true);
	}

	private void OnLoginClick(GameObject obj)
	{
		if (UIManager.instance.CheckWifi())
		{
			if (ServerConnection.IsAuthorized())
			{
				OnLoginFlowSuccess();
			}
			else if (UsernameInput.text.Length != 0 && PasswordInput.text.Length != 0)
			{
				UsernameInput.GetComponent<BoxCollider>().collider.enabled = false;
				PasswordInput.GetComponent<BoxCollider>().collider.enabled = false;
				DisableButton();
				ServerConnection.LoginWithCredentials(UsernameInput.text, PasswordInput.text);
				UIManager.instance.PersistentUI.ShowPopup("PopupLoggingIn", "UI_Menu_161", false);
				UIEventListener.Get(UIManager.instance.PersistentUI.Popup.Overlay.gameObject).onClick = LoginCancelClick;
				UIEventListener uIEventListener = UIEventListener.Get(UIManager.instance.PersistentUI.Popup.transform.Find("ConfirmationBox/CancelButton").gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(LoginCancelClick));
				LoginTime = Time.realtimeSinceStartup;
				LoggingIn = true;
			}
		}
		else
		{
			DisableButton();
		}
	}

	private void LoginCancelClick(GameObject Obj)
	{
		OnLoginFlowFail();
	}

	public void OnLoginFlowSuccess()
	{
		UsernameInput.GetComponent<BoxCollider>().collider.enabled = true;
		PasswordInput.GetComponent<BoxCollider>().collider.enabled = true;
		EnableButton();
		UIManager.instance.PersistentUI.HidePopup();
		UIManager.instance.PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_157", true);
		UIManager.instance.CloseMenu();
		LoggingIn = false;
	}

	public void OnLoginFlowFail()
	{
		UsernameInput.GetComponent<BoxCollider>().collider.enabled = true;
		PasswordInput.GetComponent<BoxCollider>().collider.enabled = true;
		LoginButton.GetComponent<BoxCollider>().collider.enabled = true;
		LoginButton.UpdateColor(true, true);
		UIManager.instance.PersistentUI.HidePopup();
		UIManager.instance.PersistentUI.ShowPopup("ConfirmationPanel", "UI_Menu_158", true);
		LoggingIn = false;
	}

	public void TimedOut()
	{
		OnLoginFlowFail();
	}

	public void Update()
	{
		HttpWeb component = GameController.instance.GetComponent<HttpWeb>();
		if (component != null)
		{
			EnableButton();
		}
		if (LoggingIn && Time.realtimeSinceStartup > LoginTime + Timeout)
		{
			TimedOut();
		}
	}
}
