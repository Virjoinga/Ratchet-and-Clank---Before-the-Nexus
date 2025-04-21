using System;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
	public class PickupSpawnData
	{
		public string pickupName;

		public int weight;

		public int count;
	}

	public class PickupSpawnGroup
	{
		public float distance;

		public int weightTotal;

		public List<PickupSpawnData> spawnDatas = new List<PickupSpawnData>();
	}

	protected enum PickupSpawnMode
	{
		Straight = 0,
		Jumping = 1,
		Left = 2,
		Right = 3,
		Count = 4
	}

	private List<PickupSpawnGroup> pickupSpawnGroups = new List<PickupSpawnGroup>();

	private List<GameObject> Pickups = new List<GameObject>();

	public int percentageChanceToSpawnPickup = 75;

	public float minSpawnSpacing;

	public float percentageChanceForStraight = 50f;

	public float percentageChanceForJump;

	public float percentageChanceForLeft;

	public float percentageChanceForRight;

	public float boltComboPitch = 1f;

	public float boltPrevTime;

	public float boltIntervalTime;

	public float boltPitchIntervalTime;

	protected float[] spawnModeChances = new float[4];

	protected Vector3[] spawnModePositions = new Vector3[4];

	protected float[] spawnModePercentages = new float[4];

	private int railToSpawn = -1;

	public static PickupManager instance;

	private float currentJetPackTimer;

	private int boltsForTileStartRail = -1;

	private int boltsJumpDirection = 1;

	public List<GameObject> getPickups()
	{
		return Pickups;
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("PickupManager: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		boltPrevTime = (boltIntervalTime = Time.time);
		spawnModePercentages[0] = percentageChanceForStraight;
		spawnModePercentages[1] = percentageChanceForJump;
		spawnModePercentages[2] = percentageChanceForLeft;
		spawnModePercentages[3] = percentageChanceForRight;
		cmlReader cmlReader2 = new cmlReader("Data/Pickups");
		if (cmlReader2 == null)
		{
			return;
		}
		foreach (cmlData item in cmlReader2.Children())
		{
			PickupSpawnGroup pickupSpawnGroup = new PickupSpawnGroup();
			pickupSpawnGroup.distance = float.Parse(item["distance"]);
			int iD = item.ID;
			pickupSpawnGroup.weightTotal = 0;
			foreach (cmlData item2 in cmlReader2.Children(iD))
			{
				PickupSpawnData pickupSpawnData = new PickupSpawnData();
				pickupSpawnData.pickupName = item2["name"];
				pickupSpawnData.count = int.Parse(item2["count"]);
				pickupSpawnData.weight = int.Parse(item2["weight"]);
				pickupSpawnGroup.weightTotal += pickupSpawnData.weight;
				pickupSpawnGroup.spawnDatas.Add(pickupSpawnData);
			}
			pickupSpawnGroups.Add(pickupSpawnGroup);
		}
		pickupSpawnGroups.Sort((PickupSpawnGroup a, PickupSpawnGroup b) => b.distance.CompareTo(a.distance));
	}

	private void Start()
	{
	}

	private PickupSpawnData ChoosePickup(PickupSpawnGroup spawnGroup)
	{
		PickupSpawnData result = null;
		if (spawnGroup != null)
		{
			int num = Random.Range(1, spawnGroup.weightTotal);
			foreach (PickupSpawnData spawnData in spawnGroup.spawnDatas)
			{
				num -= spawnData.weight;
				if (num <= 0)
				{
					result = spawnData;
					break;
				}
			}
		}
		return result;
	}

	private void SpawnPickupsFromData(PickupSpawnData spawnData, GameObject lastSpawnedObstacle)
	{
		PlayerController playerController = GameController.instance.playerController;
		float num = TileSpawnManager.instance.worldPieceSize / (float)ObstacleManager.instance.getSpawnSegmentsByDist();
		float num2 = TileSpawnManager.instance.worldPieceSize / (float)spawnData.count;
		if (minSpawnSpacing > 0f && minSpawnSpacing < num2)
		{
			num2 = minSpawnSpacing;
		}
		float x = TileSpawnManager.instance.lastRailTile.transform.position.x;
		x -= TileSpawnManager.instance.worldPieceSize / 2f;
		int num3 = 0;
		int count = spawnData.count;
		if (railToSpawn == -1 || count <= 16)
		{
			railToSpawn = Random.Range(0, 3);
		}
		bool flag = false;
		if (TileSpawnManager.instance.lastRailTile != null && TileSpawnManager.instance.lastRailTile.name.Contains("TIL_TUN"))
		{
			flag = true;
			railToSpawn = 0;
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		PickupSpawnMode pickupSpawnMode = PickupSpawnMode.Straight;
		int num9 = 0;
		float num10 = x;
		while (num9 < ObstacleManager.instance.getSpawnSegmentsByDist() && num3 < count)
		{
			ObstacleManager.instance.segmentOccupied[railToSpawn, num9] = true;
			if (num9 + 1 < ObstacleManager.instance.getSpawnSegmentsByDist())
			{
				ObstacleManager.instance.segmentOccupied[railToSpawn, num9 + 1] = true;
			}
			float num11 = 0f;
			zero = TileSpawnManager.instance.getSpawnPosition(railToSpawn, num10);
			if (zero != Vector3.zero)
			{
				if (pickupSpawnMode != 0)
				{
					float num12 = Mathf.Clamp((num10 - num6) / num5, 0f, 1f);
					if (num12 >= 1f)
					{
						pickupSpawnMode = PickupSpawnMode.Straight;
					}
					else
					{
						switch (pickupSpawnMode)
						{
						case PickupSpawnMode.Left:
						{
							zero.y += Mathf.Abs(Mathf.Sin(num12 * (float)Math.PI)) * 3f;
							float num14 = num7 + num8 * num12;
							if (num14 > zero.z)
							{
								num14 = zero.z;
								pickupSpawnMode = PickupSpawnMode.Straight;
							}
							zero.z = num14;
							break;
						}
						case PickupSpawnMode.Right:
						{
							zero.y += Mathf.Abs(Mathf.Sin(num12 * (float)Math.PI)) * 3f;
							float num13 = num7 - num8 * num12;
							if (num13 < zero.z)
							{
								num13 = zero.z;
								pickupSpawnMode = PickupSpawnMode.Straight;
							}
							zero.z = num13;
							break;
						}
						default:
							zero.y += Mathf.Abs(Mathf.Sin(num12 * (float)Math.PI)) * 5f;
							break;
						}
					}
				}
				GameObject nextFree = GameObjectPool.instance.GetNextFree(spawnData.pickupName, true);
				nextFree.transform.position = zero + Vector3.up;
				Boltz component = nextFree.GetComponent<Boltz>();
				if (component != null)
				{
					component.ExplodeFromObject = false;
					component.numberOfBoltsInSeries = count;
					playerController.Bolts.Add(component);
				}
				num3++;
			}
			zero2 = zero;
			num10 += num2;
			num11 += num2;
			if (num3 >= count || num10 - x >= TileSpawnManager.instance.worldPieceSize)
			{
				break;
			}
			if (pickupSpawnMode == PickupSpawnMode.Straight && num3 % 8 == 0)
			{
				float num15 = 0f;
				for (int i = 0; i < spawnModeChances.Length; i++)
				{
					num15 += spawnModePercentages[i];
				}
				float num16 = Random.value * num15;
				for (int j = 0; j < spawnModeChances.Length; j++)
				{
					num16 -= spawnModePercentages[j];
					if (num16 <= 0f)
					{
						pickupSpawnMode = (PickupSpawnMode)j;
						break;
					}
				}
				if (flag)
				{
					if (pickupSpawnMode != 0)
					{
						pickupSpawnMode = PickupSpawnMode.Jumping;
					}
					else if (zero2.x - num4 < 16f)
					{
						pickupSpawnMode = PickupSpawnMode.Straight;
					}
				}
				if (pickupSpawnMode != 0 && (count - num3 > 16 || flag))
				{
					int num17 = railToSpawn;
					num6 = zero2.x;
					num7 = zero2.z;
					float maxVelocityX = GameController.instance.playerController.maxVelocityX;
					num4 = num6 + num2 * maxVelocityX * playerController.RailJumpTime();
					num5 = num4 - num6;
					switch (pickupSpawnMode)
					{
					case PickupSpawnMode.Left:
						if (railToSpawn == 2)
						{
							pickupSpawnMode = PickupSpawnMode.Right;
							railToSpawn = 0;
						}
						else if (railToSpawn == 0)
						{
							railToSpawn = 2;
						}
						else
						{
							railToSpawn = 0;
						}
						num8 = Mathf.Abs(TileSpawnManager.instance.getSpawnPosition(railToSpawn, num4).z - num7);
						break;
					case PickupSpawnMode.Right:
						if (railToSpawn == 1)
						{
							pickupSpawnMode = PickupSpawnMode.Left;
							railToSpawn = 0;
						}
						else if (railToSpawn == 0)
						{
							railToSpawn = 1;
						}
						else
						{
							railToSpawn = 0;
						}
						num8 = Mathf.Abs(TileSpawnManager.instance.getSpawnPosition(railToSpawn, num4).z - num7);
						break;
					default:
						railToSpawn = num17;
						break;
					}
					if (num8 > 8f)
					{
						pickupSpawnMode = PickupSpawnMode.Straight;
						railToSpawn = num17;
					}
					else if (count - num3 < 8)
					{
						pickupSpawnMode = PickupSpawnMode.Straight;
					}
				}
				else
				{
					num6 = (num7 = (num4 = 0f));
					num5 = 1f;
					num8 = 0f;
				}
			}
			num9 = (int)((num10 - x) / num);
		}
	}

	public void SpawnBoltsForJetpack(float jetpackTimer)
	{
		if (!GameController.instance.playerController.isNearTunnelRail())
		{
			currentJetPackTimer = jetpackTimer;
			SpawnBoltsForTile(4f, null, true);
		}
	}

	public void SpawnBoltsForTile(float spacing, GameObject tileGameObject, bool jetPackBolts)
	{
		PlayerController playerController = GameController.instance.playerController;
		Vector3 zero = Vector3.zero;
		int num = 0;
		if (jetPackBolts)
		{
			num = (int)((playerController.curVelocityX * currentJetPackTimer - 10f) / spacing);
			zero = playerController.transform.position;
		}
		else
		{
			num = (int)(TileSpawnManager.instance.worldPieceSize / spacing);
			zero = tileGameObject.transform.position;
		}
		Vector3 zero2 = Vector3.zero;
		float num2 = 0f;
		float num3 = 0f;
		if (jetPackBolts)
		{
			num2 = zero.x + 20f;
			num3 = num2 + (float)num * spacing;
			boltsForTileStartRail = playerController.currentRail;
		}
		else
		{
			num2 = zero.x - TileSpawnManager.instance.worldPieceSize / 2f;
			num3 = num2 + TileSpawnManager.instance.worldPieceSize;
		}
		float num4 = num2;
		if (boltsForTileStartRail == -1)
		{
			boltsForTileStartRail = playerController.currentRail;
		}
		int num5 = boltsForTileStartRail;
		int num6 = 0;
		float num7 = 1f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		PickupSpawnMode pickupSpawnMode = PickupSpawnMode.Straight;
		while (num4 < num3)
		{
			int resultingRailNum = 0;
			zero.x = num4;
			zero = playerController.getRailNodePosition(zero, Vector3.right, num5, out resultingRailNum, true);
			if (playerController.railNodePositionComponent == null)
			{
				num4 += spacing;
				continue;
			}
			zero.x = num4;
			if (pickupSpawnMode != 0)
			{
				float num11 = Mathf.Clamp((num4 - num10) / num7, 0f, 1f);
				if (num11 >= 1f)
				{
					pickupSpawnMode = PickupSpawnMode.Straight;
				}
				else
				{
					switch (pickupSpawnMode)
					{
					case PickupSpawnMode.Left:
					{
						float num13 = num9 + num8 * num11;
						if (num13 > zero.z)
						{
							num13 = zero.z;
							pickupSpawnMode = PickupSpawnMode.Straight;
						}
						zero.z = num13;
						break;
					}
					case PickupSpawnMode.Right:
					{
						float num12 = num9 - num8 * num11;
						if (num12 < zero.z)
						{
							num12 = zero.z;
							pickupSpawnMode = PickupSpawnMode.Straight;
						}
						zero.z = num12;
						break;
					}
					}
				}
				num6 = 0;
			}
			else
			{
				num6++;
			}
			if (playerController.railNodePositionComponent.name.Contains("TIL_TUN"))
			{
				num4 = num3;
			}
			else
			{
				GameObject nextFree = GameObjectPool.instance.GetNextFree("PIK_Bolt_Gold", true);
				if (jetPackBolts)
				{
					nextFree.transform.position = zero + Vector3.up * GadgetManager.instance.GetMaxJetpackAltitude();
				}
				else
				{
					nextFree.transform.position = zero + Vector3.up;
				}
				Boltz component = nextFree.GetComponent<Boltz>();
				if (component != null)
				{
					component.ExplodeFromObject = false;
					component.numberOfBoltsInSeries = num;
					playerController.Bolts.Add(component);
				}
			}
			zero2 = zero;
			if (pickupSpawnMode == PickupSpawnMode.Straight && num6 >= 12)
			{
				if (Random.Range(0, 100) < 75)
				{
					if (boltsJumpDirection == 1)
					{
						if (num5 == 1)
						{
							boltsJumpDirection = -1;
							pickupSpawnMode = PickupSpawnMode.Left;
						}
						else
						{
							pickupSpawnMode = PickupSpawnMode.Right;
						}
					}
					else if (boltsJumpDirection == -1)
					{
						if (num5 == 2)
						{
							boltsJumpDirection = 1;
							pickupSpawnMode = PickupSpawnMode.Right;
						}
						else
						{
							pickupSpawnMode = PickupSpawnMode.Left;
						}
					}
				}
				else
				{
					pickupSpawnMode = PickupSpawnMode.Straight;
				}
				if (pickupSpawnMode != 0)
				{
					num10 = zero2.x;
					num9 = zero2.z;
					zero.x = num10 + playerController.curVelocityX * playerController.RailJumpTime();
					num7 = zero.x - num10;
					switch (pickupSpawnMode)
					{
					case PickupSpawnMode.Left:
						num5 = ((num5 == 0) ? 2 : 0);
						zero = playerController.getRailNodePosition(zero, Vector3.right, num5, out resultingRailNum, true);
						num8 = Mathf.Abs(zero.z - num9);
						break;
					case PickupSpawnMode.Right:
						num5 = ((num5 == 0) ? 1 : 0);
						zero = playerController.getRailNodePosition(zero, Vector3.right, num5, out resultingRailNum, true);
						num8 = Mathf.Abs(zero.z - num9);
						break;
					default:
						pickupSpawnMode = PickupSpawnMode.Straight;
						break;
					}
				}
				else
				{
					num10 = (num9 = 0f);
					num8 = 0f;
				}
			}
			num4 += spacing;
		}
		boltsForTileStartRail = num5;
	}

	public void SpawnBoltsForJumpSwing(RailComponent railComp, bool isJumpPad)
	{
		GameObject gameObject = railComp.gameObject;
		Vector3 position = gameObject.transform.position;
		float num = position.x - TileSpawnManager.instance.worldPieceSize / 4f;
		float y = position.y;
		float z = position.z;
		PlayerController playerController = GameController.instance.playerController;
		if (isJumpPad)
		{
			for (int i = 0; i < 3; i++)
			{
				switch (i)
				{
				case 0:
					position.z = z + 6.5f;
					break;
				case 1:
					position.z = z;
					break;
				default:
					position.z = z - 6.5f;
					break;
				}
				for (int j = 0; j < 10; j++)
				{
					position.x = num + (float)(j * 3);
					for (int k = 0; k < 3; k++)
					{
						position.y = y + 12f + (float)k * 0.7f;
						GameObject nextFree = GameObjectPool.instance.GetNextFree("PIK_Bolt_Gold", true);
						nextFree.transform.position = position;
						Boltz component = nextFree.GetComponent<Boltz>();
						if (component != null)
						{
							component.ExplodeFromObject = false;
							component.numberOfBoltsInSeries = 10;
							playerController.Bolts.Add(component);
						}
					}
				}
			}
			return;
		}
		for (int l = 0; l < 10; l++)
		{
			position.x = num + (float)(l * 3);
			for (int m = 0; m < 5; m++)
			{
				position.y = y + (float)(m * 2);
				GameObject nextFree2 = GameObjectPool.instance.GetNextFree("PIK_Bolt_Gold", true);
				nextFree2.transform.position = position;
				Boltz component2 = nextFree2.GetComponent<Boltz>();
				if (component2 != null)
				{
					component2.ExplodeFromObject = false;
					component2.numberOfBoltsInSeries = 10;
					playerController.Bolts.Add(component2);
				}
			}
		}
	}

	public void SpawnPickups(bool forceSpawn)
	{
		int numSpawnedTiles = TileSpawnManager.instance.GetNumSpawnedTiles();
		float num = (float)(numSpawnedTiles - 1) * TileSpawnManager.instance.worldPieceSize;
		int num2 = Random.Range(0, 100);
		if (num2 >= percentageChanceToSpawnPickup && !forceSpawn)
		{
			return;
		}
		foreach (PickupSpawnGroup pickupSpawnGroup in pickupSpawnGroups)
		{
			if (num > pickupSpawnGroup.distance)
			{
				PickupSpawnData pickupSpawnData = ChoosePickup(pickupSpawnGroup);
				if (pickupSpawnData != null)
				{
					SpawnPickupsFromData(pickupSpawnData, null);
				}
				break;
			}
		}
	}

	private PickupSpawnData ChooseBoltsOnly(PickupSpawnGroup spawnGroup)
	{
		PickupSpawnData result = null;
		if (spawnGroup != null)
		{
			foreach (PickupSpawnData spawnData in spawnGroup.spawnDatas)
			{
				if (spawnData.pickupName == "PIK_Raritanium")
				{
					spawnGroup.weightTotal -= spawnData.weight;
					spawnGroup.spawnDatas.Remove(spawnData);
					break;
				}
			}
		}
		if (spawnGroup != null)
		{
			int num = Random.Range(1, spawnGroup.weightTotal);
			foreach (PickupSpawnData spawnData2 in spawnGroup.spawnDatas)
			{
				num -= spawnData2.weight;
				if (num <= 0)
				{
					result = spawnData2;
					break;
				}
			}
		}
		return result;
	}

	public void SpawnExplosionOfBolts(Vector3 Position, bool theBoss = false)
	{
		float num = Random.Range(0f, 1f);
		if (!(num < (float)percentageChanceToSpawnPickup))
		{
			return;
		}
		foreach (PickupSpawnGroup pickupSpawnGroup in pickupSpawnGroups)
		{
			if (!(Position.x > pickupSpawnGroup.distance))
			{
				continue;
			}
			PickupSpawnData pickupSpawnData = ChooseBoltsOnly(pickupSpawnGroup);
			if (pickupSpawnData != null)
			{
				if (theBoss)
				{
					SpawnRaritanium(Position);
				}
				SpawnExplosionFromData(pickupSpawnData, Position);
			}
			break;
		}
	}

	public void SpawnRaritanium(Vector3 Position)
	{
		PickupSpawnData pickupSpawnData = new PickupSpawnData();
		Vector3 spawnPosition = TileSpawnManager.instance.getSpawnPosition(0, Position.x);
		spawnPosition.y += 1f;
		pickupSpawnData.pickupName = "PIK_Raritanium";
		pickupSpawnData.count = 1;
		SpawnExplosionFromData(pickupSpawnData, spawnPosition);
	}

	private void SpawnExplosionFromData(PickupSpawnData spawnData, Vector3 Position)
	{
		PlayerController playerController = GameController.instance.playerController;
		int num = 5;
		float num2 = 0f;
		float z = TileSpawnManager.instance.floorTileList[0].transform.position.z;
		Vector3 targetPosForArc = default(Vector3);
		for (int i = 1; i <= spawnData.count; i++)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(spawnData.pickupName, true, true);
			Boltz component = nextFree.GetComponent<Boltz>();
			targetPosForArc.x = Random.Range(0, 30);
			targetPosForArc.y = Random.Range(5, 20);
			if (Position.z < z - 7f)
			{
				num2 = z - Position.z;
			}
			else if (Position.z > z + 7f)
			{
				num2 = z - Position.z;
			}
			targetPosForArc.z = Random.Range(num2 - (float)num, num2 + (float)num);
			Position.y += 1.5f;
			nextFree.transform.position = Position;
			targetPosForArc += Position;
			if (component != null)
			{
				component.TargetPosForArc = targetPosForArc;
				component.ExplodeFromObject = true;
				component.numberOfBoltsInSeries = spawnData.count;
				playerController.Bolts.Add(component);
			}
		}
	}
}
