using UnityEngine;

public class ProfileItemRendererScript : MonoBehaviour
{
	public UILabel TitleLabel;

	public UILabel ValueLabel;

	public IconScript Icon;

	public int ProfileStatIndex;

	public bool isLifetimeStat;

	public int ActiveChallengeIndex;

	private void Start()
	{
	}

	public void Init(UIProfile.ProfileInfo Profile, bool LifetimeStat, int ProfileIndex, Transform Parent)
	{
		ProfileStatIndex = ProfileIndex;
		isLifetimeStat = LifetimeStat;
		base.transform.parent = Parent.transform;
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
		Icon.SetIconSprite(Profile.SpriteName, IconScript.HexLevel.HEX_V1);
		UILocalize component = TitleLabel.GetComponent<UILocalize>();
		component.key = Profile.LocKey;
		component.Localize();
	}
}
