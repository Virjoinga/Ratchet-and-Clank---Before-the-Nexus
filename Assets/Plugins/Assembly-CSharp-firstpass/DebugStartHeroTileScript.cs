using UnityEngine;

public class DebugStartHeroTileScript : MonoBehaviour
{
	private void OnClick()
	{
		GameController.instance.gameState = GameController.eGameState.GS_OnGround;
		TileSpawnManager.instance.overwriteStartTile = true;
		TileSpawnManager.instance.tileSpawnState = TileSpawnManager.TileSpawnState.Hero;
		TileSpawnManager.instance.startTile = GameObjectPool.instance.GetNextFree("AXM_TIL_HERO01");
	}
}
