using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
	public class CompatibilityData
	{
		public string[] CompatibleTiles;

		public string[] CompatibleBuildings;
	}

	public static Hashtable CompatibilitiesTable = new Hashtable();

	public EnemySpawnSockets[] EnemySockets;

	public ObstacleSpawnSockets[] ObstacleSockets;

	public LeviathanNodes[] LevPathNodes;

	public GameObject CompatibleBuilding;

	public bool PipeBase;

	public string tileName;

	public TileSpawnManager.TileSpawnState spawnedState;

	private void Awake()
	{
		string text = base.gameObject.name;
		if (text.Contains("(Clone"))
		{
			text = text.Substring(0, text.IndexOf("(Clone"));
		}
		tileName = text;
		if (CompatibilitiesTable.Contains(tileName))
		{
			return;
		}
		CompatibilityData compatibilityData = new CompatibilityData();
		cmlReader cmlReader2 = new cmlReader("CompatibilityLists/" + tileName);
		if (cmlReader2 != null)
		{
			List<cmlData> list = cmlReader2.Children(cmlReader2.GetFirstNodeOfType("CompatibleTiles").ID);
			List<string> list2 = new List<string>();
			foreach (cmlData item in list)
			{
				string text2 = item["name"];
				if (!string.IsNullOrEmpty(text2))
				{
					list2.Add(text2);
				}
			}
			compatibilityData.CompatibleTiles = new string[list2.Count];
			int num = 0;
			foreach (string item2 in list2)
			{
				compatibilityData.CompatibleTiles[num++] = item2;
			}
			List<cmlData> list3 = cmlReader2.Children(cmlReader2.GetFirstNodeOfType("CompatibleBuildings").ID);
			list2.Clear();
			foreach (cmlData item3 in list3)
			{
				string text3 = item3["name"];
				if (!string.IsNullOrEmpty(text3))
				{
					list2.Add(text3);
				}
			}
			compatibilityData.CompatibleBuildings = new string[list2.Count];
			num = 0;
			foreach (string item4 in list2)
			{
				compatibilityData.CompatibleBuildings[num++] = item4;
			}
		}
		CompatibilitiesTable[tileName] = compatibilityData;
	}
}
