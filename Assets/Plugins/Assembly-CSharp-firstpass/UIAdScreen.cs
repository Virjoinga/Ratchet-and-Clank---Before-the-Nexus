using System.IO;
using Sony.OTG.Discovery;
using UnityEngine;

public class UIAdScreen : UIScreen
{
	public static UIAdScreen instance;

	private Rect adImagePos;

	private Texture2D shadowTexture;

	private Rect shadowPos;

	private void Awake()
	{
		instance = this;
	}

	public override void Show()
	{
		if (DiscoveryClient.instance.AdImageTexture == null)
		{
			Debug.LogError("DISC: No Ad image to display.");
			closeAdScreen();
			return;
		}
		int num = Screen.width * 3 / 4;
		int num2 = (int)((float)num / (float)DiscoveryClient.instance.AdImageTexture.width * (float)DiscoveryClient.instance.AdImageTexture.height);
		adImagePos = new Rect(Screen.width / 2 - num / 2, Screen.height / 2 - num2 / 2, num, num2);
		shadowPos = adImagePos;
		shadowPos.x += shadowPos.height * 5f / 100f;
		shadowPos.y += shadowPos.height * 5f / 100f;
		shadowTexture = loadTextureFromBytes("Discovery/AdShadow");
		DiscoveryClient.instance.OnAdDisplayed(AdDisplayLocation.TURN_COMPLETE);
	}

	private Texture2D loadTextureFromBytes(string resourceName)
	{
		TextAsset textAsset = Resources.Load(resourceName, typeof(TextAsset)) as TextAsset;
		if (textAsset == null)
		{
			Debug.LogError("DISC: Failed to load texture resource " + resourceName);
			base.enabled = false;
			throw new FileNotFoundException();
		}
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.LoadImage(textAsset.bytes);
		return texture2D;
	}

	public void OnGUI()
	{
		if (DiscoveryClient.instance.AdImageTexture != null)
		{
			GUI.DrawTexture(shadowPos, shadowTexture, ScaleMode.StretchToFill);
			GUI.DrawTexture(adImagePos, DiscoveryClient.instance.AdImageTexture, ScaleMode.StretchToFill);
			if (GUI.Button(adImagePos, string.Empty, GUIStyle.none))
			{
				CloseAdScreen(true);
			}
		}
	}

	public void CloseAdScreen(bool adWasClicked)
	{
		if (adWasClicked)
		{
			DiscoveryClient.instance.OnAdClicked();
		}
		else
		{
			DiscoveryClient.instance.OnAdCancelled();
		}
		closeAdScreen();
	}

	private void closeAdScreen()
	{
		if (GameController.instance.playerController.GetHeroBoltsTotal() > 0)
		{
			UIManager.instance.CloseMenu();
			UIManager.instance.ShowHeroBoltPopup();
		}
		else
		{
			SFXManager.instance.PlaySound("UI_Back");
			UIManager.instance.ShowEndRoundScreen();
		}
	}
}
