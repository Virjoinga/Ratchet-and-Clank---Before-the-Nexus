using UnityEngine;

public class DebugAddHeroBolt : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnClick()
	{
		GameController.instance.playerController.addHeroBolt(1);
	}
}
