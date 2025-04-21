using UnityEngine;

public class AdBackButtonScript : MonoBehaviour
{
	private void OnClick()
	{
		UIAdScreen.instance.CloseAdScreen(false);
	}
}
