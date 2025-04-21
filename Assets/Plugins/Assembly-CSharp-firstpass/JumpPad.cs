using UnityEngine;

public class JumpPad : MonoBehaviour
{
	public bool activated;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SpawnInit()
	{
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject == GameController.instance.playerController.gameObject)
		{
			GameController.instance.playerController.hitJumpPad(base.gameObject);
		}
	}
}
