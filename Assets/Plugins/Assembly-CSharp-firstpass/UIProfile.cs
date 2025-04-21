using System;
using System.Collections.Generic;
using UnityEngine;

public class UIProfile : UIScreen
{
	public class ProfileInfo
	{
		public string LocKey;

		public string SpriteName;

		public string AppendString;

		public StatsTracker.Stats StatEnum;
	}

	public class ProfileGroupInfo
	{
		public string LocKey;

		public List<ProfileInfo> Data;

		public UIGrid Grid;
	}

	public Transform ScrollingList;

	public Transform RoundScrollingList;

	public ProfileItemRendererScript ProfileItemRenderer;

	public ProfileHeaderRendererScript ProfileHeaderPrefab;

	public float GridCellHeight = 70f;

	private ProfileHeaderRendererScript LastHeader;

	private UIGrid LastGrid;

	public List<ProfileGroupInfo> ProfileStats = new List<ProfileGroupInfo>();

	private UIGrid Grid;

	public UIGrid GridPrefab;

	public UIGrid LifetimeGrid;

	public PanelManager HorizPanel;

	private bool ShowingLifetimeStats;

	private void Start()
	{
		EasyAnalytics.Instance.sendView("/Profile");
		ScrollingList.GetComponent<UIDraggablePanel>().ResetPosition();
		LoadProfileInfo();
		PopulateProfileInfo();
		HorizPanel.CycleCallback = Cycle;
		ScrollingList.GetComponent<UIDraggablePanel>().ResetPosition();
		Grid.Reposition();
		Grid.repositionNow = true;
		Vector4 clipRange = ScrollingList.GetComponent<UIPanel>().clipRange;
		clipRange.y += 1f;
		ScrollingList.GetComponent<UIPanel>().clipRange = clipRange;
	}

	private void Cycle()
	{
		if (HorizPanel.GetCurrentPanelIndex() == 0)
		{
			ShowingLifetimeStats = false;
		}
		else
		{
			ShowingLifetimeStats = true;
		}
		UpdateProfileInfo();
	}

	private void LoadProfileInfo()
	{
		Dictionary<string, StatsTracker.Stats> dictionary = new Dictionary<string, StatsTracker.Stats>();
		int num = Enum.GetNames(typeof(StatsTracker.Stats)).Length;
		for (int i = 0; i < num; i++)
		{
			dictionary.Add(((StatsTracker.Stats)i).ToString(), (StatsTracker.Stats)i);
		}
		cmlReader cmlReader2 = new cmlReader("Data/Profile");
		if (cmlReader2 == null)
		{
			return;
		}
		List<cmlData> list = cmlReader2.Children();
		foreach (cmlData item in list)
		{
			ProfileGroupInfo profileGroupInfo = new ProfileGroupInfo();
			profileGroupInfo.Data = new List<ProfileInfo>();
			profileGroupInfo.LocKey = item["LocKey"];
			List<cmlData> list2 = cmlReader2.Children(item.ID);
			foreach (cmlData item2 in list2)
			{
				ProfileInfo profileInfo = new ProfileInfo();
				profileInfo.SpriteName = item2["SpriteName"];
				profileInfo.LocKey = item2["LocKey"];
				profileInfo.AppendString = item2["AppendString"];
				profileInfo.StatEnum = dictionary[item2["Stat"]];
				profileGroupInfo.Data.Add(profileInfo);
			}
			ProfileStats.Add(profileGroupInfo);
		}
	}

	public void PopulateProfileInfo()
	{
		float num = 50f;
		int num2 = 0;
		foreach (ProfileGroupInfo profileStat in ProfileStats)
		{
			ProfileHeaderRendererScript profileHeaderRendererScript = (ProfileHeaderRendererScript)UnityEngine.Object.Instantiate(ProfileHeaderPrefab);
			profileHeaderRendererScript.transform.parent = ScrollingList;
			if (num2 != 0)
			{
				profileHeaderRendererScript.transform.localPosition = new Vector3(0f, LastGrid.transform.localPosition.y - (float)LastGrid.transform.GetChildCount() * GridCellHeight, 0f);
			}
			else
			{
				profileHeaderRendererScript.transform.localPosition = Vector3.zero;
			}
			profileHeaderRendererScript.transform.localScale = Vector3.one;
			profileHeaderRendererScript.Label.GetComponent<UILocalize>().key = profileStat.LocKey;
			profileHeaderRendererScript.Label.GetComponent<UILocalize>().Localize();
			LastHeader = profileHeaderRendererScript;
			Grid = (UIGrid)UnityEngine.Object.Instantiate(GridPrefab);
			Grid.cellHeight = GridCellHeight;
			Grid.transform.parent = ScrollingList.transform;
			Grid.transform.localPosition = new Vector3(0f, 0f, -10f);
			Grid.transform.localScale = Vector3.one;
			List<ProfileInfo> data = profileStat.Data;
			for (int i = 0; i < data.Count; i++)
			{
				ProfileItemRendererScript profileItemRendererScript = (ProfileItemRendererScript)UnityEngine.Object.Instantiate(ProfileItemRenderer);
				profileItemRendererScript.Init(data[i], true, i, Grid.transform);
				if (i % 2 != 0)
				{
					profileItemRendererScript.transform.Find("Background").gameObject.SetActive(false);
				}
				if (i == data.Count - 1)
				{
					profileItemRendererScript.transform.Find("Line").gameObject.SetActive(false);
				}
			}
			Grid.repositionNow = true;
			Grid.Reposition();
			if (num2 != 0)
			{
				Grid.transform.localPosition = new Vector3(0f, LastHeader.transform.localPosition.y - num, -10f);
			}
			else
			{
				Grid.transform.localPosition = new Vector3(0f, num * -1f, -10f);
			}
			LastGrid = Grid;
			profileStat.Grid = Grid;
			num2++;
		}
		UpdateProfileInfo();
		UIManager.instance.SwapFont();
	}

	public void UpdateProfileInfo()
	{
		foreach (ProfileGroupInfo profileStat in ProfileStats)
		{
			for (int i = 0; i < profileStat.Grid.transform.GetChildCount(); i++)
			{
				ProfileItemRendererScript component = profileStat.Grid.transform.GetChild(i).GetComponent<ProfileItemRendererScript>();
				if (ShowingLifetimeStats)
				{
					component.ValueLabel.text = (int)StatsTracker.instance.GetLifetimeStat(profileStat.Data[component.ProfileStatIndex].StatEnum) + profileStat.Data[component.ProfileStatIndex].AppendString;
				}
				else
				{
					component.ValueLabel.text = (int)StatsTracker.instance.GetSingleRunBestStat(profileStat.Data[component.ProfileStatIndex].StatEnum) + profileStat.Data[component.ProfileStatIndex].AppendString;
				}
			}
		}
	}
}
