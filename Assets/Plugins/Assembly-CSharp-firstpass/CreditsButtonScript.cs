using UnityEngine;

public class CreditsButtonScript : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnClick()
	{
		UIManager.instance.OpenMenu(UIManager.MenuPanels.Credits);
	}
}
