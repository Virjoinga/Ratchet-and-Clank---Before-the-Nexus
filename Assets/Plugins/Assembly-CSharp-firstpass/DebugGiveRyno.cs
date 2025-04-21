using UnityEngine;

public class DebugGiveRyno : MonoBehaviour
{
	private void OnClick()
	{
		WeaponsManager.instance.UpgradeWeapon(4u);
	}
}
