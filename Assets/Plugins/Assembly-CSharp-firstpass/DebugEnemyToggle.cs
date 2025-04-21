using System;
using UnityEngine;

public class DebugEnemyToggle : MonoBehaviour
{
	private enum DebugEnemyTypes
	{
		Standard = 0,
		Thermosplitter_PF = 1,
		BreegusWasp_PF = 2,
		Thugs4Less_PF = 3,
		SecurityBot_PF = 4,
		Protoguard_PF = 5,
		CerulleanSwarmer_PF = 6,
		Max = 7
	}

	private const string labelString = "Enemy:\n";

	private UILabel label;

	private DebugEnemyTypes currentType;

	private void Start()
	{
		if (label == null)
		{
			Transform transform = base.transform.FindChild("Label");
			if (transform != null)
			{
				label = transform.gameObject.GetComponent<UILabel>();
			}
		}
		currentType = (DebugEnemyTypes)(int)Enum.Parse(typeof(DebugEnemyTypes), EnemyManager.instance.enemyOverride);
		if (label != null)
		{
			label.text = GetCurrentLabel();
		}
	}

	private void OnClick()
	{
		if (++currentType >= DebugEnemyTypes.Max)
		{
			currentType = DebugEnemyTypes.Standard;
		}
		if (label != null)
		{
			label.text = GetCurrentLabel();
		}
		EnemyManager.instance.enemyOverride = currentType.ToString();
	}

	private string GetCurrentLabel()
	{
		return "Enemy:\n" + currentType;
	}
}
