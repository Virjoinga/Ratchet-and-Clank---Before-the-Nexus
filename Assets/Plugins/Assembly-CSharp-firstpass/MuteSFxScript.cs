using UnityEngine;

public class MuteSFxScript : MonoBehaviour
{
	public bool muted;

	private void OnEnable()
	{
		UpdateButton();
	}

	public void UpdateButton()
	{
		if (GameController.instance != null)
		{
			muted = !GameController.instance.SoundOn;
			if (muted)
			{
				GetComponent<UICheckbox>().isChecked = true;
			}
			else
			{
				GetComponent<UICheckbox>().isChecked = false;
			}
		}
	}

	private void OnClick()
	{
		muted = !muted;
		GameController.instance.SoundOn = !muted;
		if (muted)
		{
			AudioListener.volume = 0f;
			return;
		}
		AudioListener.volume = 1f;
		SFXManager.instance.PlaySound("UI_confirm");
	}
}
