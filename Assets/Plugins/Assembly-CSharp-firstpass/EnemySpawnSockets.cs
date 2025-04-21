using UnityEngine;

public class EnemySpawnSockets : MonoBehaviour
{
	public enum EnemyTypes
	{
		Thermosplitter_PF = 0,
		SecurityBot_PF = 1,
		Thugs4Less_PF = 2,
		Protoguard_PF = 3,
		BreegusWasp_PF = 4,
		CerulleanSwarmer_PF = 5,
		Leviathan_PF = 6
	}

	public EnemyTypes etEnemy;

	public int numEnemies;

	public int xSpawnLocation;

	public int activateAtDistance;

	public int deactivateAtDistance;
}
