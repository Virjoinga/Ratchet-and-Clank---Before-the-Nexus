using UnityEngine;

public class FallZone : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnCollisionEnter(Collision other)
	{
		PlayerController component = other.gameObject.GetComponent<PlayerController>();
		if (!(component != null) || (component.activeSwingShot != null && (component.activeSwingShot.isSwinging || component.activeSwingShot.isActive)))
		{
			return;
		}
		if (component.GodMode)
		{
			if (component.currentRail == 2)
			{
				component.ForceNextJump(true);
			}
			else if (component.currentRail == 1)
			{
				component.ForceNextJump(false);
			}
			else
			{
				component.ForceNextJump(true);
			}
		}
		Debug.Log("Kill by fallzone");
		component.Kill(PlayerController.EDeathDealer.EDeath_Fall);
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerController component = other.GetComponent<PlayerController>();
		if (!(component != null) || (component.activeSwingShot != null && (component.activeSwingShot.isSwinging || component.activeSwingShot.isActive)) || component.wasSwinging)
		{
			return;
		}
		if (component.GodMode)
		{
			if (component.currentRail == 2)
			{
				component.ForceNextJump(true);
			}
			else if (component.currentRail == 1)
			{
				component.ForceNextJump(false);
			}
			else
			{
				component.ForceNextJump(true);
			}
		}
		Debug.Log("Kill by fallzone");
		component.Kill(PlayerController.EDeathDealer.EDeath_Fall);
	}
}
