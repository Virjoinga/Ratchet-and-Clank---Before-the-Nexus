using UnityEngine;

public class PanelButtonScript : MonoBehaviour
{
	public enum ScrollDirection
	{
		DIR_Left = 0,
		DIR_Right = 1,
		DIR_Up = 2,
		DIR_Down = 3
	}

	public PanelManager mPanel;

	public ScrollDirection ScrollDir;

	private void Start()
	{
		if (mPanel == null)
		{
			Debug.LogError("PanelButtonScript: Cannot find parent panel");
		}
	}

	private void OnClick()
	{
		mPanel.SwitchMenu(ScrollDir);
	}
}
