using UnityEngine;

public class DebugMenuButtonScript : MonoBehaviour
{
	public UIDebugMenu ParentUIDebugMenu;

	private void Start()
	{
	}

	private void OnClick()
	{
		ParentUIDebugMenu.Toggle();
	}
}
