using UnityEngine;

public class AdButtonScript : MonoBehaviour
{
	private void OnClick()
	{
		UIAdScreen.instance.CloseAdScreen(true);
	}
}
