using UnityEngine;

public class TileSpawnManager : MonoBehaviour
{
	public enum ENVType
	{
		ENV_Axm = 0,
		ENV_Pol = 1,
		ENV_Ter = 2,
		ENV_Max = 3
	}

	public enum TileSpawnState
	{
		Rails = 0,
		StartTunnel = 1,
		Tunnel = 2,
		EndTunnel = 3,
		Ground = 4,
		Hero = 5
	}

	public bool StraightRailsOnly;

	public bool overwriteStartTile;

	public bool grappleTestTiles;

	public bool jumpPadTestTiles;

	public bool noGrappleTiles;

	public int maxPieces = 6;

	public int spawnTileIndex = 4;

	public int minSpawnTileIndex = 2;

	public float spawnTilePosX;

	public int numHeroTiles;

	protected int maxNumHeroTiles = 18;

	public int NumAxmHeroTiles = 17;

	public int NumPolHeroTiles = 15;

	public int NumTerHeroTiles = 12;

	public float worldPieceSize = 48f;

	public int currentRampCount;

	private int axmBuildingIndex = -1;

	private int axmBuildingCount;

	private float axmBuildingSpawnX;

	public int axmBuildingCountMax = 4;

	public int maxNumTilesUntilTransition = 20;

	public int maxNumTilesUntilTransitionR = 10;

	protected int numTilesUntilTransition;

	public float railOffset;

	public float maxSpawnNodeDistance = 4f;

	public bool needBoltsForHeroTiles;

	public int terachnoidsMax = 10;

	public float terachnoidSpawnDistance = 500f;

	public float terachnoidSpawnPercentage = 0.1f;

	public float terachnoidDistanceUntilSpawn;

	public float heroBoltSpawnDistance = 500f;

	public float heroBoltSpawnPercentage = 0.1f;

	public float heroBoltDistanceUntilSpawn;

	public float raritaniumSpawnDistance = 500f;

	public float raritaniumSpawnPercentage = 0.1f;

	public float raritaniumDistanceUntilSpawn;

	public GameObject startTile;

	public GameObject nextTile;

	private TileInfo floorTileInfo;

	private TileInfo railTileInfo;

	private int numSpawnedTiles;

	public int maxTransitTileCount = 20;

	public int transitTilesUntilBiomeChange = 2;

	public int transitTileCount;

	public bool prepareForTransition;

	private int tunnelEndTileNum;

	public int specialRailTileCount;

	public GameObject[] floorTileList;

	public RailComponent[] railTileList;

	private RailComponent lastFloorRailComponent;

	public GameObject lastFloorTile;

	private RailComponent lastRailRailComponent;

	public GameObject lastRailTile;

	private RailComponent targetRailComp;

	private int axmBuildingListSize = 10;

	private GameObject[] axmGBuildingList;

	private GameObject[] axmRBuildingList;

	private int genBuildingListSize = 20;

	private int genBuildingListIndex;

	private GameObject[] genBuildingList;

	private int buildingIndex;

	public ENVType startingEnvironmentType;

	public ENVType nextEnvironmentType;

	public ENVType currentEnvironmentType;

	private string previousLoadTag;

	private string currentLoadTag;

	private string nextLoadTag;

	private bool bFirstLoading;

	public TileSpawnState tileSpawnState;

	public TileSpawnState previousSpawnState;

	public int tileSpawnStateSequence;

	public UILabel DebugLabel;

	public static TileSpawnManager instance;

	private GameObject floorFree;

	private GameObject railFree;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		floorTileList = new GameObject[maxPieces];
		railTileList = new RailComponent[maxPieces];
		axmGBuildingList = new GameObject[axmBuildingListSize];
		axmRBuildingList = new GameObject[axmBuildingListSize];
		genBuildingList = new GameObject[genBuildingListSize];
		currentEnvironmentType = ENVType.ENV_Max;
		previousLoadTag = "none";
		currentLoadTag = "none";
		nextLoadTag = "none";
		UIManager.instance.ShowLoadingGear();
		bFirstLoading = true;
		PrepForRestart();
		StreamBiomes();
	}

	public int GetNumSpawnedTiles()
	{
		return numSpawnedTiles;
	}

	public void PrepForRestart()
	{
		nextEnvironmentType = startingEnvironmentType;
		previousSpawnState = TileSpawnState.Hero;
		nextLoadTag = nextEnvironmentType.ToString() + "_" + TileSpawnState.Rails;
	}

	public void ResetSpawnDistances()
	{
		terachnoidDistanceUntilSpawn = terachnoidSpawnDistance;
		heroBoltDistanceUntilSpawn = heroBoltSpawnDistance;
	}

	public void Restart()
	{
		ResetSpawnDistances();
		if (!overwriteStartTile)
		{
			tileSpawnState = TileSpawnState.Rails;
		}
		prepareForTransition = false;
		numSpawnedTiles = 0;
		transitTileCount = 0;
		tunnelEndTileNum = 0;
		axmBuildingCount = 0;
		axmBuildingIndex = -1;
		axmBuildingSpawnX = 0f;
		numTilesUntilTransition = 0;
		tileSpawnStateSequence = 0;
		lastFloorRailComponent = null;
		lastRailRailComponent = null;
		lastFloorTile = null;
		lastRailTile = null;
		for (int i = 0; i < maxPieces; i++)
		{
			floorTileList[i] = null;
			railTileList[i] = null;
		}
		for (int j = 0; j < maxPieces; j++)
		{
			SpawnPiece();
		}
		currentRampCount = 0;
		spawnTilePosX = worldPieceSize - worldPieceSize / 2f;
	}

	public void SetNextEnvironmentType(ENVType envType)
	{
		if (nextEnvironmentType == ENVType.ENV_Pol && GetNextState(previousSpawnState) == TileSpawnState.Rails)
		{
			nextEnvironmentType = ENVType.ENV_Ter;
		}
		currentEnvironmentType = nextEnvironmentType;
		nextEnvironmentType = envType;
		switch (currentEnvironmentType)
		{
		case ENVType.ENV_Axm:
			maxNumHeroTiles = NumAxmHeroTiles;
			break;
		case ENVType.ENV_Pol:
			maxNumHeroTiles = NumPolHeroTiles;
			break;
		default:
			maxNumHeroTiles = NumTerHeroTiles;
			break;
		}
		nextLoadTag = nextEnvironmentType.ToString() + "_" + GetNextState(GetNextState(previousSpawnState));
	}

	public void StreamBiomes()
	{
		if (previousLoadTag != currentLoadTag && previousLoadTag != nextLoadTag)
		{
			GameObjectPool.instance.UnloadTag(previousLoadTag);
		}
		if (nextLoadTag != previousLoadTag && nextLoadTag != currentLoadTag)
		{
			GameObjectPool.instance.LoadTag(nextLoadTag);
		}
		previousLoadTag = currentLoadTag;
		currentLoadTag = nextLoadTag;
	}

	public float getLastRileTileOffset()
	{
		return lastRailTile.transform.position.x;
	}

	public Bounds getLastRileTileBounds()
	{
		return lastRailTile.GetComponent<Renderer>().bounds;
	}

	public int getLeftRail(int currentRail)
	{
		int num = -1;
		switch (currentRail)
		{
		case 0:
			return 2;
		case 1:
			return 0;
		default:
			return -1;
		}
	}

	public int getRightRail(int currentRail)
	{
		int num = -1;
		switch (currentRail)
		{
		case 0:
			return 1;
		case 2:
			return 0;
		default:
			return -1;
		}
	}

	public Vector3 getRailNodePosition(int rail, float xOffset, RailComponent railComp)
	{
		targetRailComp = railComp;
		if (targetRailComp == null)
		{
			return Vector3.zero;
		}
		float num = float.PositiveInfinity;
		Vector3 to = Vector3.zero;
		Vector3 from = Vector3.zero;
		Vector3 zero = Vector3.zero;
		while (targetRailComp != null)
		{
			Vector3[] array = null;
			switch (rail)
			{
			case 0:
				array = targetRailComp.rail2TransformedIndices;
				break;
			case 1:
				array = targetRailComp.rail3TransformedIndices;
				break;
			default:
				array = targetRailComp.rail1TransformedIndices;
				break;
			}
			for (int i = 0; i < array.Length; i++)
			{
				zero = array[i];
				if (zero.x > xOffset && zero.x - xOffset < num)
				{
					to = zero;
					num = zero.x - xOffset;
					from = ((i <= 0) ? array[0] : array[i - 1]);
				}
			}
			if (num < float.PositiveInfinity)
			{
				float num2 = xOffset - from.x;
				float num3 = to.x - from.x;
				float t = 1f;
				if (num3 != 0f)
				{
					t = Mathf.Clamp(num2 / num3, 0f, 1f);
				}
				return Vector3.Lerp(from, to, t);
			}
			targetRailComp = targetRailComp.nextRail;
		}
		return Vector3.zero;
	}

	private int getSpawnTileIndex()
	{
		int num = spawnTileIndex;
		if (num > numSpawnedTiles)
		{
			num = numSpawnedTiles - 1;
		}
		return num;
	}

	public Vector3 getSpawnPosition(int iRailIndex, float xOffset)
	{
		return getRailNodePosition(iRailIndex, xOffset, lastRailRailComponent);
	}

	public void getSpawnPositionAndDirection(int iRailIndex, float xOffset, ref Vector3 newPosition, ref Vector3 newDirection)
	{
		int num = getSpawnTileIndex();
		newPosition = getRailNodePosition(iRailIndex, xOffset, railTileList[num]);
		if (newPosition.x - xOffset > maxSpawnNodeDistance)
		{
			Debug.Log("getSpawnPositionAndDirection() using 1");
			newDirection.x = 0f;
			newDirection.y = 0f;
			newDirection.z = 0f;
			return;
		}
		newDirection = Vector3.forward;
		Vector3 railNodePosition = getRailNodePosition(iRailIndex, newPosition.x + 1f, railTileList[num]);
		if (railNodePosition != Vector3.zero)
		{
			newDirection = railNodePosition - newPosition;
			newDirection.Normalize();
			newDirection = Vector3.Cross(newDirection, base.gameObject.transform.up);
			newDirection.Normalize();
		}
	}

	private void spawnAXMBuilding()
	{
		GameObject gameObject = null;
		if (tileSpawnState == TileSpawnState.Rails && numTilesUntilTransition + 1 >= maxNumTilesUntilTransitionR - axmBuildingCountMax)
		{
			return;
		}
		if (axmBuildingIndex == -1)
		{
			axmBuildingIndex = Random.Range(0, 5);
		}
		if (axmBuildingSpawnX < lastFloorTile.transform.position.x)
		{
			axmBuildingSpawnX = lastFloorTile.transform.position.x;
			axmBuildingCount = 0;
		}
		do
		{
			if (tileSpawnState == TileSpawnState.Ground)
			{
				if (GameController.instance.playerController.isOnTunnelRail() && axmRBuildingList[axmBuildingListSize - 1] != null)
				{
					for (int i = 0; i < axmBuildingListSize; i++)
					{
						if (axmRBuildingList[i] != null)
						{
							GameObjectPool.instance.SetFree(axmRBuildingList[i]);
							axmRBuildingList[i] = null;
						}
					}
				}
				string objectType = string.Format("AXM_BLD_0{0}", axmBuildingIndex + 1);
				gameObject = GameObjectPool.instance.GetNextFree(objectType, true);
				if (gameObject != null)
				{
					int num = 0;
					for (int j = 0; j < axmBuildingListSize; j++)
					{
						if (axmGBuildingList[j] != null)
						{
							num++;
						}
						if (j + 1 < axmBuildingListSize)
						{
							axmGBuildingList[j] = axmGBuildingList[j + 1];
						}
					}
					axmGBuildingList[axmBuildingListSize - 1] = gameObject;
					if (num == 0)
					{
						axmBuildingSpawnX = lastFloorTile.transform.position.x;
						axmBuildingCount = 0;
					}
					float z = lastFloorTile.transform.position.z;
					gameObject.transform.position = new Vector3(axmBuildingSpawnX, 0f, z);
					axmBuildingSpawnX += worldPieceSize * 3f;
				}
				axmBuildingIndex++;
				if (axmBuildingIndex > 6)
				{
					axmBuildingIndex = 0;
				}
			}
			else if (tileSpawnState == TileSpawnState.Rails)
			{
				if (GameController.instance.playerController.isOnTunnelRail() && axmGBuildingList[axmBuildingListSize - 1] != null)
				{
					for (int k = 0; k < axmBuildingListSize; k++)
					{
						if (axmGBuildingList[k] != null)
						{
							GameObjectPool.instance.SetFree(axmGBuildingList[k]);
							axmGBuildingList[k] = null;
						}
					}
				}
				string objectType2 = string.Format("AXM_BLD_TIL_Base0{0}", axmBuildingIndex + 1);
				gameObject = GameObjectPool.instance.GetNextFree(objectType2, true);
				if (gameObject != null)
				{
					int num2 = 0;
					for (int l = 0; l < axmBuildingListSize; l++)
					{
						if (axmRBuildingList[l] != null)
						{
							num2++;
						}
						if (l + 1 < axmBuildingListSize)
						{
							axmRBuildingList[l] = axmRBuildingList[l + 1];
						}
					}
					axmRBuildingList[axmBuildingListSize - 1] = gameObject;
					if (num2 == 0)
					{
						axmBuildingSpawnX = lastRailTile.transform.position.x;
						axmBuildingCount = 0;
					}
					float z2 = lastRailTile.transform.position.z;
					gameObject.transform.position = new Vector3(axmBuildingSpawnX, 0f, z2);
					axmBuildingSpawnX += worldPieceSize * 3f;
				}
				axmBuildingIndex++;
				if (axmBuildingIndex > 7)
				{
					axmBuildingIndex = 0;
				}
			}
			axmBuildingCount++;
		}
		while (axmBuildingCount < axmBuildingCountMax);
	}

	private void hideAxmBuildings()
	{
		if (!(axmGBuildingList[axmBuildingListSize - 1] != null) && !(axmRBuildingList[axmBuildingListSize - 1] != null))
		{
			return;
		}
		for (int i = 0; i < axmBuildingListSize; i++)
		{
			if (axmGBuildingList[i] != null)
			{
				GameObjectPool.instance.SetFree(axmGBuildingList[i]);
				axmGBuildingList[i] = null;
			}
			if (axmRBuildingList[i] != null)
			{
				GameObjectPool.instance.SetFree(axmRBuildingList[i]);
				axmRBuildingList[i] = null;
			}
		}
	}

	private void hideGenBuildings()
	{
		for (int i = 0; i < genBuildingListSize; i++)
		{
			if (genBuildingList[i] != null)
			{
				GameObjectPool.instance.SetFree(genBuildingList[i]);
				genBuildingList[i] = null;
			}
		}
		genBuildingListIndex = 0;
	}

	private void spawnBuilding()
	{
		if (currentEnvironmentType == ENVType.ENV_Axm)
		{
			if (numSpawnedTiles % 3 == 0)
			{
				spawnAXMBuilding();
			}
			if (railTileList[0] != null && railTileList[0].name.Equals("TIL_TUN_PC07"))
			{
				hideGenBuildings();
			}
			return;
		}
		if (railTileList[0] != null && railTileList[0].name.Equals("TIL_TUN_PC07"))
		{
			hideAxmBuildings();
		}
		GameObject gameObject = null;
		if (!(lastFloorTile != null))
		{
			return;
		}
		TileInfo component = lastFloorTile.GetComponent<TileInfo>();
		if (!(component != null))
		{
			return;
		}
		if (component.CompatibleBuilding != null)
		{
			gameObject = GameObjectPool.instance.GetNextFree(component.CompatibleBuilding.name, true);
		}
		else
		{
			TileInfo.CompatibilityData compatibilityData = (TileInfo.CompatibilityData)TileInfo.CompatibilitiesTable[lastFloorTile.name];
			if (compatibilityData != null)
			{
				string[] compatibleBuildings = compatibilityData.CompatibleBuildings;
				if (compatibleBuildings != null)
				{
					int num = compatibleBuildings.Length;
					if (num > 0)
					{
						int num2 = 1;
						buildingIndex += num2;
						if (buildingIndex >= num)
						{
							buildingIndex = 0;
						}
						string text = compatibleBuildings[buildingIndex];
						if (text != null && text.Length > 0)
						{
							gameObject = GameObjectPool.instance.GetNextFree(text, true);
							if (gameObject == null)
							{
								Debug.Log("Building " + text + " returned null");
							}
							else
							{
								Debug.Log("Building " + text + " spawned at X=" + lastFloorTile.transform.position.x + " Y=" + lastFloorTile.transform.position.y + " Z=" + lastFloorTile.transform.position.z);
							}
						}
					}
				}
			}
		}
		if (gameObject != null)
		{
			genBuildingList[genBuildingListIndex] = gameObject;
			genBuildingListIndex++;
			if (genBuildingListIndex >= genBuildingListSize)
			{
				genBuildingListIndex = 0;
			}
			if (genBuildingList[genBuildingListIndex] != null)
			{
				GameObjectPool.instance.SetFree(genBuildingList[genBuildingListIndex]);
				genBuildingList[genBuildingListIndex] = null;
			}
			gameObject.transform.position = new Vector3(lastFloorTile.transform.position.x, 0f, lastFloorTile.transform.position.z);
			axmBuildingSpawnX = gameObject.transform.position.x;
			axmBuildingCount = 0;
		}
		else if (railTileList[0] != null && railTileList[0].name.Equals("TIL_TUN_PC07"))
		{
			hideGenBuildings();
		}
	}

	private string StartHeroTiles()
	{
		string text = null;
		switch (currentEnvironmentType)
		{
		case ENVType.ENV_Axm:
			return "AXM_TIL_HERO01";
		case ENVType.ENV_Pol:
			return "POL_TIL_HERO00";
		default:
			return "TER_TIL_HERO_00";
		}
	}

	private string getStraightTile()
	{
		string text = null;
		switch (currentEnvironmentType)
		{
		case ENVType.ENV_Axm:
			return "AXM_TIL_TS04";
		case ENVType.ENV_Pol:
			return "POL_TIL_TS04";
		default:
			return "TER_TIL_TS04";
		}
	}

	private string GetEnvironmentTunnelTile(bool start)
	{
		string text = null;
		switch (currentEnvironmentType)
		{
		case ENVType.ENV_Axm:
			if (start)
			{
				return "AXM_TIL_PSOTr00";
			}
			if (GetNextState(previousSpawnState) == TileSpawnState.Rails)
			{
				return "TIL_PS04";
			}
			return "AXM_TIL_PTSTr00";
		case ENVType.ENV_Pol:
			if (start)
			{
				return "POL_TIL_PSOTr00";
			}
			if (GetNextState(previousSpawnState) == TileSpawnState.Rails)
			{
				return "TIL_PS04";
			}
			return "POL_TIL_PTSTr00";
		default:
			if (start)
			{
				return "TER_TIL_PSOTr00";
			}
			if (GetNextState(previousSpawnState) == TileSpawnState.Rails)
			{
				return "TIL_PS04";
			}
			return "TER_TIL_PTSTr00";
		}
	}

	private string[] getCompatibleTiles(string s)
	{
		string[] result = null;
		TileInfo.CompatibilityData compatibilityData = null;
		if ((!s.Contains("TER_") || s.Contains("HERO")) && s.Contains("POL_") && !s.Contains("HERO"))
		{
			s = s.Replace("POL_", "AXM_");
		}
		compatibilityData = (TileInfo.CompatibilityData)TileInfo.CompatibilitiesTable[s];
		if (compatibilityData != null)
		{
			result = compatibilityData.CompatibleTiles;
		}
		return result;
	}

	private void spawnFloorTile()
	{
		GameObject gameObject = null;
		string text = getStraightTile();
		if (tileSpawnState == TileSpawnState.Rails)
		{
			text = ((currentEnvironmentType != ENVType.ENV_Ter) ? "TIL_P_Base" : "TER_TIL_P_Base");
		}
		else if (tileSpawnState == TileSpawnState.StartTunnel)
		{
			text = ((previousSpawnState != TileSpawnState.Hero) ? GetEnvironmentTunnelTile(true) : ((currentEnvironmentType != ENVType.ENV_Pol) ? "TIL_TUN_PC01" : GetEnvironmentTunnelTile(true)));
		}
		else if (tileSpawnState == TileSpawnState.Tunnel)
		{
			if (transitTileCount == maxTransitTileCount / 2)
			{
				GameController.instance.ChangeBiome();
			}
			int num = transitTileCount % 4;
			text = ((num != 0 && num != 1) ? "TIL_TUN_PC01" : "TIL_TUN_PC07");
		}
		else if (tileSpawnState == TileSpawnState.EndTunnel)
		{
			tileSpawnStateSequence++;
			switch (GetNextState(previousSpawnState))
			{
			case TileSpawnState.Hero:
				text = "TIL_TUN_PC01";
				break;
			case TileSpawnState.Rails:
				text = ((currentEnvironmentType != ENVType.ENV_Ter) ? "TIL_P_Base" : "TER_TIL_P_Base");
				break;
			default:
				text = GetEnvironmentTunnelTile(false);
				break;
			}
			tunnelEndTileNum = numSpawnedTiles;
		}
		else if (tileSpawnState == TileSpawnState.Hero && numHeroTiles == 0)
		{
			text = StartHeroTiles();
			needBoltsForHeroTiles = false;
		}
		else if (floorTileList[0] == null || lastFloorTile == null)
		{
			if (startTile != null)
			{
				text = startTile.name;
				lastFloorTile = startTile;
				SetSpawnState(lastFloorTile, tileSpawnState);
			}
		}
		else if (overwriteStartTile && nextTile != null && numSpawnedTiles == 1)
		{
			text = nextTile.name;
		}
		else if (lastFloorTile.name.Equals("POL_TIL_TC11"))
		{
			text = "POL_TIL_TC05";
		}
		else if (lastFloorTile.name.Equals("POL_TIL_TCU01"))
		{
			text = "POL_TIL_TSUD00";
		}
		else
		{
			string[] compatibleTiles = getCompatibleTiles(lastFloorTile.name);
			if (compatibleTiles != null && numHeroTiles < maxNumHeroTiles && (tileSpawnState == TileSpawnState.Hero || overwriteStartTile || numSpawnedTiles > 4))
			{
				int num2 = compatibleTiles.Length;
				if (num2 > 0)
				{
					do
					{
						int num3 = Random.Range(0, num2);
						text = compatibleTiles[num3];
					}
					while (text.Contains("TPSTr00") && num2 > 1);
				}
				if (currentEnvironmentType == ENVType.ENV_Pol)
				{
					if (text.Equals("AXM_TIL_TC05"))
					{
						text = "POL_TIL_TC11";
					}
					else if (text.Equals("AXM_TIL_TSUD00"))
					{
						text = "POL_TIL_TCU01";
					}
				}
			}
		}
		if (tileSpawnState == TileSpawnState.Hero)
		{
			numHeroTiles++;
		}
		else
		{
			numTilesUntilTransition++;
		}
		if (tileSpawnState != 0 && tileSpawnState != TileSpawnState.Hero && !text.Equals("POL_TIL_TC05"))
		{
			if (lastFloorTile != null && lastFloorRailComponent != null)
			{
				currentRampCount = (int)(lastFloorTile.transform.TransformPoint(lastFloorRailComponent.frontSocket).y / 6f);
			}
			if (tileSpawnState == TileSpawnState.Ground && prepareForTransition)
			{
				text = ((currentRampCount > 0) ? "AXM_TIL_TSD00" : ((currentRampCount >= 0) ? "AXM_TIL_TS04" : "AXM_TIL_TSU00"));
			}
			else if (lastFloorTile != null && tileSpawnState != TileSpawnState.Tunnel)
			{
				if (text.Contains("U"))
				{
					if (currentRampCount >= 2)
					{
						text = "AXM_TIL_TS04";
					}
				}
				else if (text.Contains("D") && currentRampCount < 0)
				{
					text = "AXM_TIL_TS04";
				}
			}
		}
		if (text.Contains("AXM_") && !text.Contains("HERO"))
		{
			if (currentEnvironmentType == ENVType.ENV_Ter)
			{
				text = text.Replace("AXM_", "TER_");
				if (text.Equals("TER_TIL_TSU00"))
				{
					text = "TER_TIL_TCU000";
				}
			}
			else if (currentEnvironmentType == ENVType.ENV_Pol)
			{
				text = text.Replace("AXM_", "POL_");
			}
		}
		gameObject = GameObjectPool.instance.GetNextFree(text, true);
		if (gameObject == null)
		{
			text = ((currentEnvironmentType == ENVType.ENV_Axm) ? "AXM_TIL_TS04" : ((currentEnvironmentType != ENVType.ENV_Pol) ? "TER_TIL_TS04" : "POL_TIL_TS04"));
			gameObject = GameObjectPool.instance.GetNextFree(text, true);
		}
		RailComponent component = gameObject.GetComponent<RailComponent>();
		if (text.Contains("PSOTr") && transitTileCount == 0 && lastRailRailComponent != null)
		{
			lastRailRailComponent.ConnectRail(component);
			Transform transform = gameObject.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.name.Equals("OBS_WallNode"))
				{
					GameObject nextFree = GameObjectPool.instance.GetNextFree("OBS_Wall");
					nextFree.transform.position = child.transform.position;
				}
			}
		}
		else if (lastFloorRailComponent != null)
		{
			lastFloorRailComponent.ConnectRail(component);
			if (text.Equals("TER_TIL_HERO_14"))
			{
				Transform transform2 = gameObject.transform;
				for (int j = 0; j < transform2.childCount; j++)
				{
					Transform child2 = transform2.GetChild(j);
					if (child2.name.Equals("OBS_WallNode"))
					{
						GameObject nextFree2 = GameObjectPool.instance.GetNextFree("OBS_Wall");
						nextFree2.transform.position = child2.transform.position;
					}
				}
			}
		}
		else
		{
			gameObject.transform.position = Vector3.zero;
		}
		component.TransformAllPoints();
		lastFloorRailComponent = component;
		component.nextRail = null;
		lastFloorTile = gameObject;
		SetSpawnState(lastFloorTile, tileSpawnState);
		floorTileInfo = gameObject.GetComponent<TileInfo>();
	}

	private void SetSpawnState(GameObject go, TileSpawnState state)
	{
		if (go != null)
		{
			TileInfo component = go.GetComponent<TileInfo>();
			if (component != null)
			{
				component.spawnedState = state;
			}
		}
	}

	private bool isMultipartTile(string tilename)
	{
		if (tilename.Equals("TIL_PGO02") || tilename.Equals("TIL_PGO01"))
		{
			return true;
		}
		if (tilename.Equals("TIL_PJO02") || tilename.Equals("TIL_PJO01"))
		{
			return true;
		}
		if (tilename.Equals("TIL_PCA") || tilename.Equals("TIL_PCB"))
		{
			return true;
		}
		if (tilename.Equals("TIL_PC00") || tilename.Equals("TIL_PC01") || tilename.Equals("TIL_PC02") || tilename.Equals("TIL_PC05"))
		{
			return true;
		}
		if (tilename.Equals("TIL_PC07") || tilename.Equals("TIL_PC08") || tilename.Equals("TIL_PC09") || tilename.Equals("TIL_PC10") || tilename.Equals("TIL_PC11"))
		{
			return true;
		}
		if (tilename.Equals("TIL_PD57"))
		{
			return true;
		}
		return false;
	}

	private void spawnRailTile()
	{
		GameObject gameObject = null;
		string text = null;
		railTileInfo = null;
		if (floorTileInfo != null && floorTileInfo.PipeBase)
		{
			text = "TIL_PS04";
			if (overwriteStartTile && numSpawnedTiles == 0 && startTile != null)
			{
				text = startTile.name;
			}
			else if (numSpawnedTiles == 0)
			{
				text = "AXM_TIL_TPSTr00";
			}
			else if (!StraightRailsOnly && (numSpawnedTiles > 1 || overwriteStartTile) && !(railTileList[0] == null) && !(lastRailTile == null) && railOffset != 0f)
			{
				if (overwriteStartTile && nextTile != null && numSpawnedTiles == 1)
				{
					text = nextTile.name;
				}
				else
				{
					railTileInfo = lastRailTile.GetComponent<TileInfo>();
					if (railTileInfo != null)
					{
						TileInfo.CompatibilityData compatibilityData = (TileInfo.CompatibilityData)TileInfo.CompatibilitiesTable[railTileInfo.name];
						if (compatibilityData != null && compatibilityData.CompatibleTiles != null)
						{
							int num = compatibilityData.CompatibleTiles.Length;
							if (num != 0)
							{
								int num2 = Random.Range(0, num);
								text = compatibilityData.CompatibleTiles[num2];
							}
						}
					}
				}
			}
			if (noGrappleTiles)
			{
				if (text.Contains("TIL_PG"))
				{
					text = "TIL_PS04";
				}
			}
			else if (grappleTestTiles)
			{
				switch (numSpawnedTiles % 3)
				{
				case 0:
					text = "TIL_PGO02";
					break;
				case 1:
					text = "TIL_PGO01";
					break;
				default:
					text = "TIL_PGO03";
					break;
				}
			}
			else if (jumpPadTestTiles)
			{
				switch (numSpawnedTiles % 3)
				{
				case 0:
					text = "TIL_PJO02";
					break;
				case 1:
					text = "TIL_PJO01";
					break;
				default:
					text = "TIL_PJO03";
					break;
				}
			}
			else if (numSpawnedTiles < 5 || numSpawnedTiles - tunnelEndTileNum < 3 || numTilesUntilTransition >= maxNumTilesUntilTransitionR - 5)
			{
				if (text.Equals("TIL_PGO02"))
				{
					text = "TIL_PS04";
				}
				else if (text.Equals("TIL_PJO02"))
				{
					text = "TIL_PS04";
				}
				else if (text.Equals("TIL_PS52"))
				{
					text = "TIL_PS04";
				}
				else if (text.Equals("TIL_PS57") || text.Equals("TIL_PS60"))
				{
					text = "TIL_PS04";
				}
				else if (text.Equals("TIL_PSY00") || text.Equals("TIL_PSY01") || text.Equals("TIL_PSY02"))
				{
					text = "TIL_PS04";
				}
			}
			else if (text.Equals("TIL_PGO02") && Random.Range(0, 100) < 75)
			{
				text = "TIL_PJO02";
			}
			if (lastRailTile != null)
			{
				currentRampCount = (int)(lastRailTile.transform.TransformPoint(lastRailRailComponent.frontSocket).y / 6f);
			}
			if (prepareForTransition)
			{
				text = ((currentRampCount > 0) ? "TIL_PSD02" : ((currentRampCount >= 0) ? "TIL_PS04" : "TIL_PSU00"));
			}
			else if (lastRailTile != null)
			{
				if (text.Contains("U"))
				{
					if (currentRampCount >= 2)
					{
						text = "TIL_PS04";
					}
				}
				else if (text.Contains("D") && currentRampCount <= -1)
				{
					text = "TIL_PS04";
				}
			}
			if (text.Equals("TIL_PS61"))
			{
				text = "TIL_PS04";
			}
			else if (text.Equals("TIL_PS62"))
			{
				text = "TIL_PS04";
			}
			else if (text.Equals("TIL_PS63"))
			{
				text = "TIL_PS04";
			}
			else if (text.Equals("TIL_PS64"))
			{
				text = "TIL_PS04";
			}
			gameObject = GameObjectPool.instance.GetNextFree(text, true);
			bool flag = false;
			bool flag2 = false;
			if (gameObject == null)
			{
				gameObject = GameObjectPool.instance.GetNextFree("TIL_PS04", true);
			}
			else if (text.Equals("TIL_PGO01"))
			{
				Swing componentInChildren = gameObject.GetComponentInChildren<Swing>();
				if (componentInChildren != null)
				{
					componentInChildren.SpawnInit();
				}
				flag = true;
			}
			else if (text.Equals("TIL_PJO01"))
			{
				JumpPad componentInChildren2 = gameObject.GetComponentInChildren<JumpPad>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.SpawnInit();
				}
				flag2 = true;
			}
			RailComponent component = gameObject.GetComponent<RailComponent>();
			if (lastRailRailComponent != null)
			{
				lastRailRailComponent.ConnectRail(component);
			}
			else
			{
				gameObject.transform.position = Vector3.zero;
			}
			component.TransformAllPoints();
			lastRailRailComponent = component;
			component.nextRail = null;
			lastRailTile = gameObject;
			if (flag || flag2)
			{
				PickupManager.instance.SpawnBoltsForJumpSwing(component, flag2);
			}
			if (lastFloorTile != null)
			{
				lastFloorTile.transform.position = gameObject.transform.position - Vector3.up * 5f;
			}
			railOffset = 1f;
		}
		else
		{
			lastRailRailComponent = lastFloorRailComponent;
			lastRailTile = lastFloorTile;
			railOffset = 0f;
		}
	}

	private bool isSpecialRailTile(GameObject railTile)
	{
		if (railTile.name.Contains("TIL_PG"))
		{
			return true;
		}
		if (railTile.name.Contains("TIL_PJ"))
		{
			return true;
		}
		if (railTile.name.Equals("TIL_PS52") || railTile.name.Equals("TIL_PS57"))
		{
			return true;
		}
		if (railTile.name.Equals("TIL_PS60"))
		{
			return true;
		}
		if (railTile.name.Equals("TIL_PSY00") || railTile.name.Equals("TIL_PSY01") || railTile.name.Equals("TIL_PSY02"))
		{
			return true;
		}
		return false;
	}

	private bool canSpawnObstacles()
	{
		if (isSpecialRailTile(lastRailTile))
		{
			specialRailTileCount = 0;
			return false;
		}
		if (tileSpawnState == TileSpawnState.StartTunnel || tileSpawnState == TileSpawnState.Tunnel || tileSpawnState == TileSpawnState.EndTunnel)
		{
			return false;
		}
		if (numSpawnedTiles > 5 && numSpawnedTiles - tunnelEndTileNum < 3)
		{
			return false;
		}
		return true;
	}

	private void SpawnPiece()
	{
		spawnFloorTile();
		spawnRailTile();
		spawnBuilding();
		numSpawnedTiles++;
		if (tileSpawnState == TileSpawnState.Tunnel)
		{
			transitTileCount++;
		}
		else if (tileSpawnState == TileSpawnState.Ground || tileSpawnState == TileSpawnState.Hero)
		{
			SpawnSocketManager.instance.UpdateEnemyMax();
			SpawnTileInfoObjects(lastFloorTile);
		}
		relinkRails();
		ObstacleManager.instance.InitObstacleSegments();
		if (!grappleTestTiles)
		{
			if (tileSpawnState == TileSpawnState.Rails)
			{
				if (canSpawnObstacles())
				{
					int num = (GameController.instance.bFirstStart ? 1 : 2);
					if (numSpawnedTiles > num)
					{
						if (numSpawnedTiles == num + 1)
						{
							PickupManager.instance.SpawnPickups(true);
						}
						else
						{
							PickupManager.instance.SpawnPickups(false);
						}
					}
					if (numSpawnedTiles > minSpawnTileIndex && specialRailTileCount > 1)
					{
						ObstacleManager.instance.SpawnObstacle();
					}
				}
				specialRailTileCount++;
			}
			else if (tileSpawnState == TileSpawnState.Tunnel)
			{
				PickupManager.instance.SpawnPickups(true);
			}
			else if (tileSpawnState == TileSpawnState.Hero && needBoltsForHeroTiles)
			{
				PickupManager.instance.SpawnBoltsForTile(2f, lastRailTile, false);
			}
		}
		if (numSpawnedTiles > minSpawnTileIndex && canSpawnObstacles() && tileSpawnState != TileSpawnState.Hero)
		{
			if (!prepareForTransition)
			{
				MegaWeaponManager.instance.HandleMegaWeaponSpawning();
			}
			GadgetManager.instance.HandleGadgetSpawning();
			bool flag = spawnTerachnoid();
			bool flag2 = spawnHeroBolt(flag);
			bool otherSpawn = false;
			if (flag || flag2)
			{
				otherSpawn = true;
			}
			spawnRaritanium(otherSpawn);
		}
	}

	private void relinkRails()
	{
		for (int i = 0; i < maxPieces; i++)
		{
			if (floorTileList[i] == null)
			{
				floorTileList[i] = lastFloorTile;
				railTileList[i] = lastRailRailComponent;
				if (i > 0)
				{
					railTileList[i - 1].nextRail = lastRailRailComponent;
				}
				break;
			}
		}
	}

	private float getUnoccupiedSegmentPosition(out int rail)
	{
		int num = 0;
		float num2 = instance.worldPieceSize / (float)ObstacleManager.instance.getSpawnSegmentsByDist();
		float num3 = 0f;
		int num4 = 0;
		rail = 0;
		do
		{
			rail = Random.Range(0, 3);
			num3 = Random.Range(0f, instance.worldPieceSize);
			num4 = (int)(num3 / num2);
		}
		while (ObstacleManager.instance.segmentOccupied[rail, num4] && num++ < 10);
		ObstacleManager.instance.segmentOccupied[rail, num4] = true;
		return num3;
	}

	private bool spawnTerachnoid()
	{
		float travelDist = GameController.instance.playerController.GetTravelDist();
		if (StatsTracker.instance.GetStat(StatsTracker.Stats.terachnoidsCollected) < (float)terachnoidsMax && travelDist > terachnoidDistanceUntilSpawn)
		{
			terachnoidDistanceUntilSpawn = travelDist + terachnoidSpawnDistance;
			float num = Random.Range(0f, 1f);
			if (num < terachnoidSpawnPercentage)
			{
				int num2 = getSpawnTileIndex();
				GameObject gameObject = floorTileList[num2];
				if (gameObject != null && gameObject.GetComponent<Collider>() != null)
				{
					int rail = 0;
					float unoccupiedSegmentPosition = getUnoccupiedSegmentPosition(out rail);
					float xOffset = gameObject.transform.position.x + unoccupiedSegmentPosition;
					Vector3 spawnPosition = getSpawnPosition(rail, xOffset);
					spawnPosition.y += 1f;
					GameObject nextFree = GameObjectPool.instance.GetNextFree("Terachnoid_PF");
					if (nextFree != null)
					{
						nextFree.transform.position = spawnPosition;
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool spawnHeroBolt(bool otherSpawn)
	{
		float travelDist = GameController.instance.playerController.GetTravelDist();
		if (travelDist > heroBoltDistanceUntilSpawn)
		{
			heroBoltDistanceUntilSpawn = travelDist + heroBoltSpawnDistance;
			if (!otherSpawn)
			{
				float num = Random.Range(0f, 1f);
				if (num < heroBoltSpawnPercentage)
				{
					int num2 = getSpawnTileIndex();
					GameObject gameObject = floorTileList[num2];
					if (gameObject != null && gameObject.GetComponent<Collider>() != null)
					{
						int rail = 0;
						float unoccupiedSegmentPosition = getUnoccupiedSegmentPosition(out rail);
						float xOffset = gameObject.transform.position.x + unoccupiedSegmentPosition;
						Vector3 spawnPosition = getSpawnPosition(rail, xOffset);
						spawnPosition.y += 1f;
						GameObject nextFree = GameObjectPool.instance.GetNextFree("PIK_Bolt_Hero", true);
						if (nextFree != null)
						{
							nextFree.transform.position = spawnPosition;
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private void spawnRaritanium(bool otherSpawn)
	{
		float travelDist = GameController.instance.playerController.GetTravelDist();
		if (!(travelDist > raritaniumDistanceUntilSpawn))
		{
			return;
		}
		raritaniumDistanceUntilSpawn = travelDist + raritaniumSpawnDistance;
		if (otherSpawn)
		{
			return;
		}
		float num = Random.Range(0f, 1f);
		if (!(num < raritaniumSpawnPercentage))
		{
			return;
		}
		int num2 = getSpawnTileIndex();
		GameObject gameObject = floorTileList[num2];
		if (gameObject != null && gameObject.GetComponent<Collider>() != null)
		{
			int rail = 0;
			float unoccupiedSegmentPosition = getUnoccupiedSegmentPosition(out rail);
			float xOffset = gameObject.transform.position.x + unoccupiedSegmentPosition;
			Vector3 spawnPosition = getSpawnPosition(rail, xOffset);
			spawnPosition.y += 1f;
			GameObject nextFree = GameObjectPool.instance.GetNextFree("PIK_Raritanium", true);
			if (nextFree != null)
			{
				nextFree.transform.position = spawnPosition;
			}
		}
	}

	public void spawnBoltsForHeroTiles()
	{
		int num = numHeroTiles + 1;
		for (int i = 1; i < maxPieces; i++)
		{
			if (floorTileList[i] != null && num < maxNumHeroTiles)
			{
				PickupManager.instance.SpawnBoltsForTile(2f, floorTileList[i], false);
			}
		}
		needBoltsForHeroTiles = true;
	}

	public GameController.eGameState GetGameStateFromTile(int tile = 0)
	{
		GameController.eGameState result = GameController.eGameState.GS_Grinding;
		if (tile >= 0 && tile < floorTileList.Length && floorTileList[tile] != null)
		{
			TileInfo component = floorTileList[tile].GetComponent<TileInfo>();
			if (component != null)
			{
				switch (component.spawnedState)
				{
				case TileSpawnState.EndTunnel:
					result = GameController.eGameState.GS_TransitToGnd;
					break;
				case TileSpawnState.StartTunnel:
					result = GameController.eGameState.GS_TransitToGnd;
					break;
				case TileSpawnState.Tunnel:
					result = GameController.eGameState.GS_TransitToGnd;
					break;
				case TileSpawnState.Ground:
					result = GameController.eGameState.GS_OnGround;
					break;
				case TileSpawnState.Hero:
					result = GameController.eGameState.GS_OnGround;
					break;
				default:
					result = GameController.eGameState.GS_Grinding;
					break;
				}
			}
		}
		return result;
	}

	private void SpawnTileInfoObjects(GameObject o)
	{
		if (!(o != null) || !(floorTileInfo != null))
		{
			return;
		}
		EnemySpawnSockets[] enemySockets = floorTileInfo.EnemySockets;
		foreach (EnemySpawnSockets enemySpawnSockets in enemySockets)
		{
			if (enemySpawnSockets != null)
			{
				SpawnSocketManager.instance.AddSocket(enemySpawnSockets);
			}
		}
		ObstacleSpawnSockets[] obstacleSockets = floorTileInfo.ObstacleSockets;
		for (int j = 0; j < obstacleSockets.Length; j++)
		{
			ObstacleManager.instance.SpawnObstacle(obstacleSockets[j]);
		}
		LeviathanNodes[] levPathNodes = floorTileInfo.LevPathNodes;
		if (levPathNodes.Length <= 0)
		{
			return;
		}
		foreach (LeviathanNodes leviathanNodes in levPathNodes)
		{
			if (leviathanNodes != null)
			{
				GameController.instance.PathNodes.Add(leviathanNodes);
			}
		}
	}

	private void Update()
	{
		if (bFirstLoading)
		{
			if (!GameObjectPool.instance.IsStreaming())
			{
				bFirstLoading = false;
				UIManager.instance.HideLoadingGear();
				UIManager.instance.InitialLoadComplete();
			}
		}
		else if (GameController.instance.playerController.transform.position.x >= spawnTilePosX)
		{
			UpdateState();
			CycleTiles();
			SpawnPiece();
			UpdateMusic();
			UpdateStreaming();
			spawnTilePosX += worldPieceSize;
		}
		if (!(DebugLabel != null))
		{
			return;
		}
		DebugLabel.text = string.Empty;
		for (int i = 0; i < genBuildingListSize; i++)
		{
			if (genBuildingList[i] != null)
			{
				float x = genBuildingList[i].transform.position.x;
				float y = genBuildingList[i].transform.position.y;
				float z = genBuildingList[i].transform.position.z;
				bool activeSelf = genBuildingList[i].activeSelf;
				UILabel debugLabel = DebugLabel;
				string text = debugLabel.text;
				debugLabel.text = text + genBuildingList[i].name + ": X=" + x + " Y=" + y + " Z=" + z + " ACT=" + activeSelf + "\n";
			}
		}
	}

	private TileSpawnState GetNextState(TileSpawnState previousState)
	{
		TileSpawnState tileSpawnState = TileSpawnState.Rails;
		if (previousState == TileSpawnState.Rails)
		{
			if ((tileSpawnStateSequence & 3) == 3)
			{
				return TileSpawnState.Hero;
			}
			return TileSpawnState.Ground;
		}
		return TileSpawnState.Rails;
	}

	private void UpdateState()
	{
		if (IsTransitioning())
		{
			if (tileSpawnState == TileSpawnState.StartTunnel)
			{
				tileSpawnState = TileSpawnState.Tunnel;
			}
			else if (tileSpawnState == TileSpawnState.EndTunnel)
			{
				tileSpawnState = GetNextState(previousSpawnState);
				numHeroTiles = 0;
				numTilesUntilTransition = 0;
				axmBuildingIndex = -1;
			}
			else if (transitTileCount >= maxTransitTileCount)
			{
				tileSpawnState = TileSpawnState.EndTunnel;
				transitTileCount = 0;
			}
		}
		else
		{
			if (!(lastRailTile != null) || isMultipartTile(lastRailTile.name) || grappleTestTiles)
			{
				return;
			}
			int num = ((tileSpawnState != 0) ? maxNumTilesUntilTransition : maxNumTilesUntilTransitionR);
			if (numTilesUntilTransition >= num - 2 && numTilesUntilTransition <= num)
			{
				prepareForTransition = true;
			}
			else if (numTilesUntilTransition > num || numHeroTiles >= maxNumHeroTiles)
			{
				prepareForTransition = false;
				previousSpawnState = tileSpawnState;
				if (tileSpawnState == TileSpawnState.Hero && currentEnvironmentType == ENVType.ENV_Axm)
				{
					tileSpawnState = TileSpawnState.Tunnel;
				}
				else
				{
					tileSpawnState = TileSpawnState.StartTunnel;
				}
				numHeroTiles = 0;
				numTilesUntilTransition = 0;
			}
		}
	}

	private void UpdateMusic()
	{
		if (floorTileList[0] != null)
		{
			MusicComponent component = floorTileList[0].GetComponent<MusicComponent>();
			if (component != null)
			{
				component.Activate();
			}
		}
	}

	private void UpdateStreaming()
	{
		int num = 0;
		TileSpawnState tileSpawnState = TileSpawnState.Rails;
		for (int i = 0; i < floorTileList.Length; i++)
		{
			if (!(floorTileList[i] != null))
			{
				continue;
			}
			TileInfo component = floorTileList[i].GetComponent<TileInfo>();
			if (component != null)
			{
				if (i == 0)
				{
					tileSpawnState = component.spawnedState;
				}
				if (component.spawnedState == TileSpawnState.Tunnel)
				{
					num++;
				}
			}
		}
		if (tileSpawnState == TileSpawnState.Tunnel && num == 4)
		{
			StreamBiomes();
			GameController.instance.SetSkybox(currentEnvironmentType, GetNextState(previousSpawnState));
		}
	}

	private void CycleTiles()
	{
		if (floorTileList[0] != null && railTileList[0] != null)
		{
			if (floorFree != null && floorFree.activeSelf)
			{
				GameObjectPool.instance.SetFree(floorFree);
			}
			if (railFree != null && railFree.activeSelf && railFree != floorFree)
			{
				GameObjectPool.instance.SetFree(railFree);
			}
			floorFree = floorTileList[0];
			railFree = railTileList[0].gameObject;
			for (int i = 0; i < maxPieces - 1; i++)
			{
				floorTileList[i] = floorTileList[i + 1];
				railTileList[i] = railTileList[i + 1];
			}
			floorTileList[maxPieces - 1] = null;
			railTileList[maxPieces - 1] = null;
		}
	}

	private bool IsTransitioning()
	{
		return tileSpawnState == TileSpawnState.StartTunnel || tileSpawnState == TileSpawnState.Tunnel || tileSpawnState == TileSpawnState.EndTunnel;
	}
}
