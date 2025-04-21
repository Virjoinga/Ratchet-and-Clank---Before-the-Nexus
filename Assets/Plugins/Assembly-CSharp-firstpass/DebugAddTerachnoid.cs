using UnityEngine;

public class DebugAddTerachnoid : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnClick()
	{
		GameController.instance.playerController.addTerachnoid(1);
	}
}
