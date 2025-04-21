using System.Collections.Generic;
using UnityEngine;

public class DebugGiveGadgetButton : MonoBehaviour
{
	public enum GadgetType
	{
		GADGET_BoltMultiplier = 0,
		GADGET_Reflector = 1,
		GADGET_ArmorMagnetizer = 2,
		GADGET_Jetpack = 3,
		MEGAWEAP_Groovitron = 4,
		MEGAWEAP_RiftInducer = 5,
		MEGAWEAP_TornadoLauncher = 6
	}

	public UIDebugMenu ParentUIDebugMenu;

	public GadgetType Type;

	public int GadgetLevel = 1;

	private void Start()
	{
	}

	private void OnClick()
	{
		switch (Type)
		{
		case GadgetType.GADGET_BoltMultiplier:
			GadgetManager.instance.ActivateBoltMultiplier();
			break;
		case GadgetType.GADGET_ArmorMagnetizer:
			GadgetManager.instance.ActivateMagnetizer();
			break;
		case GadgetType.GADGET_Reflector:
			GadgetManager.instance.ActivateReflector();
			break;
		case GadgetType.GADGET_Jetpack:
			GadgetManager.instance.ActivateJetpack();
			break;
		case GadgetType.MEGAWEAP_Groovitron:
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.megaWeaponsUsed);
			MegaWeaponManager.instance.ActivateMegaWeapon(MegaWeaponManager.eMegaWeapons.mw_Groovitron);
			List<GameObject> enemies = EnemyManager.instance.getEnemies();
			{
				foreach (GameObject item in enemies)
				{
					EnemyController component3 = item.GetComponent<EnemyController>();
					component3.Groove();
				}
				break;
			}
		}
		case GadgetType.MEGAWEAP_RiftInducer:
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.megaWeaponsUsed);
			MegaWeaponManager.instance.ActivateMegaWeapon(MegaWeaponManager.eMegaWeapons.mw_RiftInducer);
			List<GameObject> enemies = EnemyManager.instance.getEnemies();
			{
				foreach (GameObject item2 in enemies)
				{
					EnemyController component2 = item2.GetComponent<EnemyController>();
					component2.Rift();
				}
				break;
			}
		}
		case GadgetType.MEGAWEAP_TornadoLauncher:
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.megaWeaponsUsed);
			MegaWeaponManager.instance.ActivateMegaWeapon(MegaWeaponManager.eMegaWeapons.mw_Tornado);
			List<GameObject> enemies = EnemyManager.instance.getEnemies();
			{
				foreach (GameObject item3 in enemies)
				{
					EnemyController component = item3.GetComponent<EnemyController>();
					component.Tornado();
				}
				break;
			}
		}
		}
	}
}
