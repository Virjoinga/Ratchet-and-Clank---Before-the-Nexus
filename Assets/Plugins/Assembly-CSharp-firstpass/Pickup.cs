using UnityEngine;

public class Pickup : MonoBehaviour
{
	public float anglePerUpdate = 2f;

	public float initialAngle = 180f;

	public bool randomizeInitialAngle;

	private void Start()
	{
		if (randomizeInitialAngle)
		{
			initialAngle = Random.Range(0f, 360f);
		}
		base.transform.RotateAround(Vector3.up, initialAngle);
	}

	protected virtual void Update()
	{
		base.transform.RotateAround(Vector3.up, anglePerUpdate * Time.deltaTime);
	}

	public virtual void Activate()
	{
	}

	protected virtual void FixedUpdate()
	{
		if (!base.enabled || !base.gameObject.activeSelf || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!base.renderer.enabled)
		{
			base.renderer.enabled = true;
			base.transform.parent.gameObject.GetComponentInChildren<ParticleSystem>().Play(true);
		}
		if (base.transform.position.x < GameController.instance.playerController.transform.position.x - TileSpawnManager.instance.worldPieceSize)
		{
			PickupManager.instance.getPickups().Remove(base.gameObject);
			if (GetComponent<Boltz>() != null || GetComponent<Raritanium>() != null)
			{
				GameObjectPool.instance.SetFree(base.gameObject);
			}
			else
			{
				GameObjectPool.instance.SetFree(base.transform.parent.gameObject);
			}
		}
	}
}
