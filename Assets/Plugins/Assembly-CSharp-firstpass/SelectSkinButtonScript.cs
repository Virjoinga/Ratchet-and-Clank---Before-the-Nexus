using UnityEngine;

public class SelectSkinButtonScript : MonoBehaviour
{
	public UIVendor ParentUIVendor;

	public string SkinString;

	private void OnClick()
	{
		GameController.instance.playerController.SetSkin(SkinString);
		PlayerPrefs.SetString("Skin", SkinString);
		PlayerPrefs.Save();
	}
}
