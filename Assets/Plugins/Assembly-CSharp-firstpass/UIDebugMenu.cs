using UnityEngine;

public class UIDebugMenu : MonoBehaviour
{
	public Transform DebugMenu;

	private bool isActive;

	private void Start()
	{
		SetActive(false);
	}

	public void SetActive(bool Active)
	{
		DebugMenu.gameObject.SetActive(Active);
		isActive = Active;
		if (isActive)
		{
			Time.timeScale = 0f;
		}
		else
		{
			Time.timeScale = 1f;
		}
	}

	public void Toggle()
	{
		SetActive(!isActive);
	}
}
