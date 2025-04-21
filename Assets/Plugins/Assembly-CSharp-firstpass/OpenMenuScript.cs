using System;
using UnityEngine;

public class OpenMenuScript : MonoBehaviour
{
	private UIManager.MenuPanels MenuEnum = UIManager.MenuPanels.InvalidMenu;

	private void Start()
	{
		string text = base.name.Replace("Button", string.Empty);
		string[] names = Enum.GetNames(typeof(UIManager.MenuPanels));
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i] == text)
			{
				MenuEnum = (UIManager.MenuPanels)i;
				break;
			}
		}
		if (MenuEnum == UIManager.MenuPanels.InvalidMenu)
		{
			Debug.LogError("OpenMenuScript: Menu Enum for " + text + " not found.");
		}
	}

	private void Update()
	{
	}

	private void OnClick()
	{
		UIManager.instance.OpenMenu(MenuEnum);
	}
}
