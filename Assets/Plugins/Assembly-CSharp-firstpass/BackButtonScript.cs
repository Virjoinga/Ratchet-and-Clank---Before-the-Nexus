using UnityEngine;

public class BackButtonScript : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnClick()
	{
		UIManager.instance.CloseMenu();
		SFXManager.instance.PlaySound("UI_Back");
		MusicManager.instance.Play(MusicManager.eMusicTrackType.Menu, false, 0f);
	}
}
