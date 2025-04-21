using UnityEngine;

public class DebugStartGroundScript : MonoBehaviour
{
	private void OnClick()
	{
		GameController.instance.gameState = GameController.eGameState.GS_TransitToGnd;
		TileSpawnManager.instance.overwriteStartTile = true;
		TileSpawnManager.instance.tileSpawnState = TileSpawnManager.TileSpawnState.Ground;
		TileSpawnManager.instance.startTile = GameObjectPool.instance.GetNextFree("AXM_TIL_TS04");
	}
}
