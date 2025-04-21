using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Grid")]
[ExecuteInEditMode]
public class UIGrid : MonoBehaviour
{
	public enum Arrangement
	{
		Horizontal = 0,
		Vertical = 1
	}

	public Arrangement arrangement;

	public int maxPerLine;

	public float cellWidth = 200f;

	public float cellHeight = 200f;

	public float maxWidth = 600f;

	public bool centerContents;

	public bool repositionNow;

	public bool sorted;

	public bool hideInactive = true;

	private int numInactive;

	private bool mStarted;

	private void Start()
	{
		mStarted = true;
		Reposition();
	}

	private void Update()
	{
		if (repositionNow)
		{
			repositionNow = false;
			Reposition();
		}
	}

	public static int SortByName(Transform a, Transform b)
	{
		return string.Compare(a.name, b.name);
	}

	public void Reposition()
	{
		numInactive = 0;
		if (!mStarted)
		{
			repositionNow = true;
			return;
		}
		Transform transform = base.transform;
		int num = 0;
		int num2 = 0;
		if (sorted)
		{
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if ((bool)child && (!hideInactive || NGUITools.GetActive(child.gameObject)))
				{
					list.Add(child);
				}
			}
			list.Sort(SortByName);
			int j = 0;
			for (int count = list.Count; j < count; j++)
			{
				Transform transform2 = list[j];
				if (!NGUITools.GetActive(transform2.gameObject) && hideInactive)
				{
					numInactive++;
					continue;
				}
				float z = transform2.localPosition.z;
				transform2.localPosition = ((arrangement != 0) ? new Vector3(cellWidth * (float)num2, (0f - cellHeight) * (float)num, z) : new Vector3(cellWidth * (float)num, (0f - cellHeight) * (float)num2, z));
				if (++num >= maxPerLine && maxPerLine > 0)
				{
					num = 0;
					num2++;
				}
			}
		}
		else
		{
			for (int k = 0; k < transform.childCount; k++)
			{
				Transform child2 = transform.GetChild(k);
				if (!NGUITools.GetActive(child2.gameObject) && hideInactive)
				{
					numInactive++;
					continue;
				}
				float z2 = child2.localPosition.z;
				child2.localPosition = ((arrangement != 0) ? new Vector3(cellWidth * (float)num2, (0f - cellHeight) * (float)num, z2) : new Vector3(cellWidth * (float)num, (0f - cellHeight) * (float)num2, z2));
				if (++num >= maxPerLine && maxPerLine > 0)
				{
					num = 0;
					num2++;
				}
			}
		}
		if (centerContents)
		{
			transform.localPosition = new Vector3((maxWidth - (float)(transform.childCount - numInactive) * cellWidth) / 2f, transform.localPosition.y, transform.localPosition.z);
		}
		UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		if (uIDraggablePanel != null)
		{
			uIDraggablePanel.UpdateScrollbars(true);
		}
	}
}
