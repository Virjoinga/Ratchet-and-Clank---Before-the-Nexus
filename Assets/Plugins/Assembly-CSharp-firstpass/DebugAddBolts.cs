using UnityEngine;

public class DebugAddBolts : MonoBehaviour
{
	public int Num = 1000;

	private void Start()
	{
	}

	private void OnClick()
	{
		GameController.instance.playerController.addBoltz(Num);
	}
}
