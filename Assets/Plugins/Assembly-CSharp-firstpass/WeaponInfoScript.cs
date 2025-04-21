using UnityEngine;

public class WeaponInfoScript : MonoBehaviour
{
	public UILabel VersionLabel;

	public UILabel ShellTypeValueLabel;

	public UILabel SpreadValueLabel;

	public UILabel ExtraTextLabel;

	public UISlider DamageSlider;

	public UISlider RateOfFireSlider;

	public UISlider AreaOfEffectSlider;

	public IconScript Icon;

	private Weapon Weap;

	public void Init(Weapon W, Transform Parent, bool IsNextLevel, bool SkipTransforms = false)
	{
		Weap = W;
		if (!SkipTransforms)
		{
			base.transform.parent = Parent.transform;
			base.transform.localPosition = Vector3.zero;
			base.transform.localScale = new Vector3(32f, 32f, 1f);
		}
		DamageSlider.sliderValue = (float)Weap.damage / WeaponsManager.instance.WeaponStatsDamageMax;
		RateOfFireSlider.sliderValue = 1f - Weap.fireRate;
		AreaOfEffectSlider.sliderValue = (float)Weap.AOEDamage / WeaponsManager.instance.WeaponStatsDamageMax;
		UILocalize component = SpreadValueLabel.GetComponent<UILocalize>();
		if (Weap.spreadShot)
		{
			component.key = "UI_Menu_167";
			component.Localize();
			component.enabled = false;
		}
		else
		{
			component.key = "UI_Menu_168";
			component.Localize();
			component.enabled = false;
		}
		UILocalize component2 = ShellTypeValueLabel.GetComponent<UILocalize>();
		UILocalize component3 = ExtraTextLabel.GetComponent<UILocalize>();
		component2.key = Weap.LocDescShellType;
		component2.Localize();
		component2.enabled = false;
		component3.key = Weap.LocDescExtra;
		component3.Localize();
		component3.enabled = false;
		if (Weap.weaponName == "RynoM")
		{
			VersionLabel.text = string.Empty;
			Icon.SetIconSprite(Weap.spriteName, IconScript.HexLevel.HEX_V3, !IsNextLevel);
		}
		else
		{
			VersionLabel.text = "v" + Weap.GetWeaponUpgradeLevel();
			Icon.SetIconSprite(Weap.spriteName, (IconScript.HexLevel)Weap.GetWeaponUpgradeLevel(), !IsNextLevel);
		}
	}
}
