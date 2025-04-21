using UnityEngine;

public class Obstacle : MonoBehaviour
{
	protected int objectType;

	public int rail { get; set; }

	private void Start()
	{
		if (base.gameObject.name.Contains("OBS_Mine"))
		{
			objectType = 1;
		}
		else if (base.gameObject.name.Contains("OBS_ElectricBaracade"))
		{
			objectType = 2;
		}
		else if (base.gameObject.name.Contains("OBS_Burners"))
		{
			objectType = 3;
		}
	}

	private void Update()
	{
		if (objectType == 1)
		{
			Vector3 eulerAngles = base.gameObject.transform.rotation.eulerAngles;
			eulerAngles.y += Time.deltaTime * 180f;
			base.gameObject.transform.rotation = Quaternion.Euler(eulerAngles);
		}
		else if (objectType != 2)
		{
		}
	}

	private void FixedUpdate()
	{
		if (base.enabled && base.gameObject.activeSelf && base.gameObject.activeInHierarchy && base.transform.position.x < GameController.instance.playerController.transform.position.x - TileSpawnManager.instance.worldPieceSize)
		{
			ObstacleManager.instance.getObstacles().Remove(base.gameObject);
			GameObjectPool.instance.SetFree(base.gameObject, true);
			base.enabled = false;
		}
	}
}
