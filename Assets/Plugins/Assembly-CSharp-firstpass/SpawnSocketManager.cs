using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSocketManager : MonoBehaviour
{
	[Serializable]
	public class EnemyMax
	{
		public int Distance;

		public int numEnemies;
	}

	public static SpawnSocketManager instance;

	private List<EnemySpawnSockets> EnemySockets;

	private int MaxEnemySpawnLimit = 1;

	private int curIndex;

	private bool dumpEnemies;

	public EnemyMax[] EnemyMaxAtDistance;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("SpawnSocketManager: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			EnemySockets = new List<EnemySpawnSockets>();
			instance = this;
		}
	}

	private void FixedUpdate()
	{
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			dumpEnemies = true;
			for (int i = 0; i < EnemySockets.Count; i++)
			{
				EnemySpawnSockets enemySpawnSockets = EnemySockets[i];
				if (!((float)enemySpawnSockets.xSpawnLocation > enemySpawnSockets.transform.position.x - GameController.instance.playerController.rigidbody.position.x) || TileSpawnManager.instance.GetGameStateFromTile(2) != GameController.eGameState.GS_OnGround)
				{
					continue;
				}
				if (MaxEnemySpawnLimit <= EnemyManager.instance.getEnemies().Count && enemySpawnSockets.etEnemy != EnemySpawnSockets.EnemyTypes.Leviathan_PF)
				{
					EnemySockets.Remove(enemySpawnSockets);
					break;
				}
				if (enemySpawnSockets.numEnemies + EnemyManager.instance.getEnemies().Count > MaxEnemySpawnLimit)
				{
					EnemyManager.instance.SpawnEnemy(enemySpawnSockets.etEnemy.ToString(), MaxEnemySpawnLimit - EnemyManager.instance.getEnemies().Count, enemySpawnSockets.transform.position);
				}
				else
				{
					EnemyManager.instance.SpawnEnemy(enemySpawnSockets.etEnemy.ToString(), enemySpawnSockets.numEnemies, enemySpawnSockets.transform.position);
				}
				EnemySockets.Remove(enemySpawnSockets);
				break;
			}
		}
		else if ((GameController.instance.gameState == GameController.eGameState.GS_TransitToGnd || GameController.instance.gameState == GameController.eGameState.GS_Grinding) && dumpEnemies)
		{
			dumpEnemies = false;
			if (EnemySockets.Count != 0)
			{
				EnemySockets.Clear();
			}
		}
	}

	public void UpdateEnemyMax()
	{
		if (EnemyMaxAtDistance.Length == 0)
		{
			MaxEnemySpawnLimit = 1;
			return;
		}
		for (int i = 0; i < EnemyMaxAtDistance.Length && (float)EnemyMaxAtDistance[i].Distance < GameController.instance.playerController.GetTravelDist(); i++)
		{
			curIndex = i;
		}
		MaxEnemySpawnLimit = EnemyMaxAtDistance[curIndex].numEnemies;
		if (MaxEnemySpawnLimit <= 0)
		{
			MaxEnemySpawnLimit = 1;
		}
	}

	public void AddSocket(EnemySpawnSockets Socket)
	{
		if (Socket == null)
		{
			Debug.LogError("SpawnSocketManager: AddSocket(): Socket is not an EnemySpawnSockets");
		}
		else if (ActivatedCheck(Socket))
		{
			EnemySockets.Add(Socket);
		}
	}

	private bool ActivatedCheck(EnemySpawnSockets Socket)
	{
		if (Socket.activateAtDistance == 0 && Socket.deactivateAtDistance == 0)
		{
			return true;
		}
		if (GameController.instance.playerController != null && GameController.instance.playerController.GetTravelDist() < (float)Socket.activateAtDistance && GameController.instance.playerController.GetTravelDist() > (float)Socket.deactivateAtDistance)
		{
			return true;
		}
		return true;
	}
}
