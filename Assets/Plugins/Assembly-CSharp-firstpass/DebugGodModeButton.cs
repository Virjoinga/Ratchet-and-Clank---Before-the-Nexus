using UnityEngine;

public class DebugGodModeButton : MonoBehaviour
{
	public UIDebugMenu ParentUIDebugMenu;

	private void Start()
	{
	}

	private void OnClick()
	{
		bool flag = !GameController.instance.playerController.GodMode;
		GameController.instance.playerController.GodMode = flag;
		if (flag)
		{
			UIManager.instance.GetHUD().txt_GodMode.text = "GOD";
		}
		else
		{
			UIManager.instance.GetHUD().txt_GodMode.text = string.Empty;
		}
	}
}
