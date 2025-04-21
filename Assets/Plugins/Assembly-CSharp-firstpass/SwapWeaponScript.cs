using UnityEngine;

public class SwapWeaponScript : MonoBehaviour
{
	public WeaponsManager.WeaponList WeaponIndex;

	public UIPauseMenu ParentUIPauseMenu;

	private void Start()
	{
	}

	private void OnClick()
	{
		ParentUIPauseMenu.SwapWeapons((uint)WeaponIndex, true);
	}
}
