using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
	public class ObstacleSpawnData
	{
		public string obstacleName;

		public int weight;

		public int count;
	}

	public class ObstacleSpawnGroup
	{
		public float distance;

		public int weightTotal;

		public List<ObstacleSpawnData> spawnDatas = new List<ObstacleSpawnData>();
	}

	[Serializable]
	public class ObstacleSpawnPct
	{
		public float distance;

		public float percentageChance;

		public int segmentsPerTile;
	}

	public GameObject[] allowedAXMObstacles;

	public GameObject[] allowedPOLObstacles;

	public GameObject[] allowedTERObstacles;

	public GameObject[] allowedAnywhere;

	public static ObstacleManager instance;

	private List<ObstacleSpawnGroup> obstacleSpawnGroups = new List<ObstacleSpawnGroup>();

	private List<GameObject> Obstacles = new List<GameObject>();

	public ObstacleSpawnPct[] obstacleSpawnPct;

	public int tileSpawnSegments = 16;

	private static int maxTileSpawnSegments = 32;

	public bool[,] segmentOccupied;

	public List<GameObject> getObstacles()
	{
		return Obstacles;
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("ObstacleManager: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		cmlReader cmlReader2 = new cmlReader("Data/Obstacles");
		if (cmlReader2 == null)
		{
			return;
		}
		foreach (cmlData item in cmlReader2.Children())
		{
			ObstacleSpawnGroup obstacleSpawnGroup = new ObstacleSpawnGroup();
			obstacleSpawnGroup.distance = float.Parse(item["distance"]);
			int iD = item.ID;
			obstacleSpawnGroup.weightTotal = 0;
			foreach (cmlData item2 in cmlReader2.Children(iD))
			{
				ObstacleSpawnData obstacleSpawnData = new ObstacleSpawnData();
				obstacleSpawnData.obstacleName = item2["name"];
				obstacleSpawnData.count = int.Parse(item2["count"]);
				obstacleSpawnData.weight = int.Parse(item2["weight"]);
				obstacleSpawnGroup.weightTotal += obstacleSpawnData.weight;
				obstacleSpawnGroup.spawnDatas.Add(obstacleSpawnData);
			}
			obstacleSpawnGroups.Add(obstacleSpawnGroup);
		}
		obstacleSpawnGroups.Sort((ObstacleSpawnGroup a, ObstacleSpawnGroup b) => b.distance.CompareTo(a.distance));
	}

	private void Start()
	{
		segmentOccupied = new bool[3, maxTileSpawnSegments];
	}

	public void SpawnObstacle(ObstacleSpawnSockets ObstacleSocket)
	{
		GameObject gameObject = null;
		int num = 0;
		if (ObstacleSocket == null || ObstacleSocket.possibleObjectsToSpawn.Length == 0)
		{
			return;
		}
		string text = ChooseObstacle(ObstacleSocket);
		if ((float)Random.Range(1, 100) >= ObstacleSocket.obstacleChance)
		{
			return;
		}
		gameObject = ((text != null) ? GameObjectPool.instance.GetNextFree(text, true) : null);
		if (gameObject == null)
		{
			Debug.LogWarning("ObstacleManager: No free objects in ObjectPool of Obstacle Type - " + text);
			return;
		}
		Obstacle component = gameObject.GetComponent<Obstacle>();
		Vector3 position;
		switch (ObstacleSocket.railNumber)
		{
		case 0:
			position = ObstacleSocket.transform.position;
			break;
		case 1:
			num = 2;
			position = TileSpawnManager.instance.getSpawnPosition(num, ObstacleSocket.transform.position.x);
			break;
		case 2:
			num = 0;
			position = TileSpawnManager.instance.getSpawnPosition(num, ObstacleSocket.transform.position.x);
			break;
		default:
			num = 1;
			position = TileSpawnManager.instance.getSpawnPosition(num, ObstacleSocket.transform.position.x);
			break;
		}
		if (component != null)
		{
			component.rail = num;
			if (component.rigidbody == null)
			{
				component.transform.position = position;
			}
			else
			{
				component.rigidbody.MovePosition(position);
			}
		}
		Obstacles.Add(gameObject);
	}

	private string ChooseObstacle(ObstacleSpawnSockets ObstacleSocket)
	{
		if (ObstacleSocket == null || ObstacleSocket.possibleObjectsToSpawn == null || ObstacleSocket.possibleObjectsToSpawn.Length == 0)
		{
			return null;
		}
		int num = Random.Range(0, ObstacleSocket.possibleObjectsToSpawn.Length - 1);
		if (ObstacleSocket.possibleObjectsToSpawn[num] != null)
		{
			return ObstacleSocket.possibleObjectsToSpawn[num].name;
		}
		return null;
	}

	private string ChooseObstacle(ObstacleSpawnGroup spawnGroup)
	{
		string result = string.Empty;
		if (spawnGroup != null)
		{
			int num = 0;
			foreach (ObstacleSpawnData spawnData in spawnGroup.spawnDatas)
			{
				if (AllowedObstacleForThisEnvironment(spawnData.obstacleName))
				{
					num += spawnData.weight;
				}
			}
			int num2 = Random.Range(1, num);
			foreach (ObstacleSpawnData spawnData2 in spawnGroup.spawnDatas)
			{
				if (AllowedObstacleForThisEnvironment(spawnData2.obstacleName))
				{
					num2 -= spawnData2.weight;
					if (num2 <= 0)
					{
						result = spawnData2.obstacleName;
						break;
					}
				}
			}
		}
		return result;
	}

	private bool AllowedObstacleForThisEnvironment(string obstacleName)
	{
		for (int i = 0; i < allowedAnywhere.Length; i++)
		{
			if (allowedAnywhere[i].name.Equals(obstacleName))
			{
				return true;
			}
		}
		if (TileSpawnManager.instance.currentEnvironmentType == TileSpawnManager.ENVType.ENV_Axm)
		{
			for (int j = 0; j < allowedAXMObstacles.Length; j++)
			{
				if (allowedAXMObstacles[j].name.Equals(obstacleName))
				{
					return true;
				}
			}
		}
		else if (TileSpawnManager.instance.currentEnvironmentType == TileSpawnManager.ENVType.ENV_Pol)
		{
			for (int k = 0; k < allowedPOLObstacles.Length; k++)
			{
				if (allowedPOLObstacles[k].name.Equals(obstacleName))
				{
					return true;
				}
			}
		}
		else if (TileSpawnManager.instance.currentEnvironmentType == TileSpawnManager.ENVType.ENV_Ter)
		{
			for (int l = 0; l < allowedTERObstacles.Length; l++)
			{
				if (allowedTERObstacles[l].name.Equals(obstacleName))
				{
					return true;
				}
			}
		}
		return false;
	}

	private float getPercentageChanceToSpawnObstacle()
	{
		float travelDist = GameController.instance.playerController.GetTravelDist();
		int num = obstacleSpawnPct.Length;
		for (int i = 0; i < num; i++)
		{
			float distance = obstacleSpawnPct[i].distance;
			float num2 = float.PositiveInfinity;
			if (i + 1 < obstacleSpawnPct.Length)
			{
				num2 = obstacleSpawnPct[i + 1].distance;
			}
			if (travelDist >= distance && travelDist < num2)
			{
				return obstacleSpawnPct[i].percentageChance;
			}
		}
		return 0f;
	}

	public void InitObstacleSegments()
	{
		for (int i = 0; i < maxTileSpawnSegments; i++)
		{
			segmentOccupied[0, i] = false;
			segmentOccupied[1, i] = false;
			segmentOccupied[2, i] = false;
		}
	}

	public int getSpawnSegmentsByDist()
	{
		int num = 0;
		float travelDist = GameController.instance.playerController.GetTravelDist();
		int num2 = obstacleSpawnPct.Length;
		for (int i = 0; i < num2; i++)
		{
			float distance = obstacleSpawnPct[i].distance;
			float num3 = float.PositiveInfinity;
			if (i + 1 < obstacleSpawnPct.Length)
			{
				num3 = obstacleSpawnPct[i + 1].distance;
			}
			if (travelDist >= distance && travelDist < num3)
			{
				num = obstacleSpawnPct[i].segmentsPerTile;
				break;
			}
		}
		if (num == 0)
		{
			num = tileSpawnSegments;
		}
		else if (num > maxTileSpawnSegments)
		{
			num = maxTileSpawnSegments;
		}
		return num;
	}

	public GameObject SpawnObstacle()
	{
		GameObject gameObject = null;
		int spawnSegmentsByDist = getSpawnSegmentsByDist();
		float travelDist = GameController.instance.playerController.GetTravelDist();
		float num = TileSpawnManager.instance.worldPieceSize / (float)spawnSegmentsByDist;
		float lastRileTileOffset = TileSpawnManager.instance.getLastRileTileOffset();
		lastRileTileOffset += (0f - TileSpawnManager.instance.worldPieceSize) / 2f;
		int num2 = Random.Range(0, 10) % 3;
		int num3 = 0;
		for (int i = 0; i < spawnSegmentsByDist; i++)
		{
			float num4 = Random.Range(0f, 100f);
			if (!(num4 < getPercentageChanceToSpawnObstacle()))
			{
				continue;
			}
			foreach (ObstacleSpawnGroup obstacleSpawnGroup in obstacleSpawnGroups)
			{
				if (!(travelDist >= obstacleSpawnGroup.distance))
				{
					continue;
				}
				string text = ChooseObstacle(obstacleSpawnGroup);
				if (string.IsNullOrEmpty(text) || !AllowedObstacleForThisEnvironment(text))
				{
					continue;
				}
				Vector3 newPosition = Vector3.zero;
				Vector3 newDirection = Vector3.zero;
				int num5 = 0;
				switch (num2)
				{
				case 0:
					num5 = (((num3 & 1) == 0) ? 2 : 0);
					break;
				case 1:
					num5 = ((((uint)num3 & (true ? 1u : 0u)) != 0) ? 1 : 0);
					break;
				default:
					num5 = ((((uint)num3 & (true ? 1u : 0u)) != 0) ? 1 : 2);
					break;
				}
				float xOffset = lastRileTileOffset + (float)i * num;
				if (!segmentOccupied[num5, i])
				{
					num5 = (num5 + 1) % 3;
				}
				if (!segmentOccupied[num5, i])
				{
					TileSpawnManager.instance.getSpawnPositionAndDirection(num5, xOffset, ref newPosition, ref newDirection);
					if (newPosition != Vector3.zero)
					{
						gameObject = GameObjectPool.instance.GetNextFree(text, true);
						gameObject.transform.position = newPosition;
						if (gameObject.name.Contains("OBS_Mine"))
						{
							gameObject.transform.position += Vector3.up;
						}
						else if (gameObject.name.Contains("OBS_Electro"))
						{
							gameObject.transform.position -= Vector3.up * 0.36f;
						}
						gameObject.transform.forward = newDirection;
						Obstacles.Add(gameObject);
						segmentOccupied[num5, i] = true;
					}
				}
				num3++;
			}
		}
		return null;
	}
}
