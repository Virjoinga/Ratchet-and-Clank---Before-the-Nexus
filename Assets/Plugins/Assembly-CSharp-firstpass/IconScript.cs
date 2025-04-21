using UnityEngine;

public class IconScript : MonoBehaviour
{
	public enum HexLevel
	{
		HEX_V0 = 0,
		HEX_V1 = 1,
		HEX_V2 = 2,
		HEX_V3 = 3
	}

	public UISprite IconSprite;

	public UISprite Hex;

	public string SpriteName = "icon_weapon_pistol";

	public HexLevel HexType = HexLevel.HEX_V1;

	public TweenScale ScaleTween;

	private void Start()
	{
		ScaleTween = GetComponentInChildren<TweenScale>();
		UpdateIcon();
	}

	public void SetIconSprite(string NewSpriteName, HexLevel HexLev, bool Active = true, bool AlwaysTween = false)
	{
		if (NewSpriteName != string.Empty)
		{
			SpriteName = NewSpriteName;
		}
		HexType = HexLev;
		UpdateIcon();
		SetActiveIcon(Active, AlwaysTween);
	}

	public void UpdateIcon()
	{
		IconSprite.spriteName = SpriteName;
		Hex.spriteName = "icon_hex";
		if (HexType > HexLevel.HEX_V1)
		{
			Hex.spriteName += (int)HexType;
		}
		IconSprite.MakePixelPerfect();
		Hex.MakePixelPerfect();
	}

	public void SetActiveIcon(bool active, bool alwaysTween = false)
	{
		Color color = new Color(0.25f, 0.25f, 0.25f, 1f);
		Color color2 = new Color(1f, 1f, 1f, 1f);
		if (!active)
		{
			IconSprite.alpha = 1f;
			Hex.color = color;
			IconSprite.color = color;
			if (ScaleTween != null && !alwaysTween)
			{
				ScaleTween.enabled = false;
			}
		}
		else
		{
			IconSprite.alpha = 1f;
			Hex.color = color2;
			IconSprite.color = color2;
			if (ScaleTween != null)
			{
				ScaleTween.enabled = true;
			}
		}
	}
}
