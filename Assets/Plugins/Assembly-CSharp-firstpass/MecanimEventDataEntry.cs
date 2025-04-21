using System;
using UnityEngine;

[Serializable]
public class MecanimEventDataEntry
{
	public UnityEngine.Object animatorController;

	public int layer;

	public int stateNameHash;

	public MecanimEvent[] events;

	public MecanimEventDataEntry()
	{
		events = new MecanimEvent[0];
	}

	public MecanimEventDataEntry(MecanimEventDataEntry other)
	{
		animatorController = other.animatorController;
		layer = other.layer;
		stateNameHash = other.stateNameHash;
		if (other.events == null)
		{
			events = new MecanimEvent[0];
			return;
		}
		events = new MecanimEvent[other.events.Length];
		for (int i = 0; i < events.Length; i++)
		{
			events[i] = new MecanimEvent(other.events[i]);
		}
	}
}
