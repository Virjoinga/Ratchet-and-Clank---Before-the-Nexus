using UnityEngine;

public class DebugAddRaritanium : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnClick()
	{
		GameController.instance.playerController.addRaritanium(100);
	}
}
