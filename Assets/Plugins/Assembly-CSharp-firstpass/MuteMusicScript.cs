using UnityEngine;

public class MuteMusicScript : MonoBehaviour
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
			muted = !GameController.instance.MusicOn;
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
		GameController.instance.MusicOn = !muted;
		MusicManager.instance.MuteMusic(muted);
		SFXManager.instance.PlaySound("UI_confirm");
	}
}
