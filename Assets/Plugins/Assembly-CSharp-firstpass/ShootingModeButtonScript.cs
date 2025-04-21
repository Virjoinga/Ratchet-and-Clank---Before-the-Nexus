using UnityEngine;

public class ShootingModeButtonScript : MonoBehaviour
{
	public UILabel m_ButtonLabel;

	private void Start()
	{
		m_ButtonLabel.text = ((!GameController.instance.GetShootingMode()) ? "Running" : "Shooting");
	}

	private void Update()
	{
	}

	private void OnClick()
	{
		GameController.instance.ToggleShootingMode();
		m_ButtonLabel.text = ((!GameController.instance.GetShootingMode()) ? "Running" : "Shooting");
	}
}
