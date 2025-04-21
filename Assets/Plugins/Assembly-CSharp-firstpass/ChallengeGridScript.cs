using System.Collections.Generic;
using UnityEngine;

public class ChallengeGridScript : MonoBehaviour
{
	public UIGrid Grid;

	public ChallengeItemRendererScript ListItemRenderer;

	public UILabel NumCompletedLabel;

	public bool isAchievementGrid;

	public bool showCompletionStatus;

	public bool showIcon;

	public int numCompleted;

	public bool ManuallyPopulated;

	private void OnEnable()
	{
		if (Grid == null)
		{
			Grid = GetComponent<UIGrid>();
		}
	}

	private void Start()
	{
		Grid = GetComponent<UIGrid>();
		PopulateList();
		UpdateActiveChallenges();
	}

	private void EmptyList()
	{
		if (!(Grid != null))
		{
			return;
		}
		foreach (Transform item in Grid.transform)
		{
			Object.Destroy(item.gameObject);
		}
	}

	public void PopulateList()
	{
		int num = 0;
		if (!ManuallyPopulated)
		{
			EmptyList();
		}
		if (isAchievementGrid)
		{
			Dictionary<int, ChallengeSystem.ChallengeInfo> achievementList = ChallengeSystem.instance.achievementList;
			foreach (KeyValuePair<int, ChallengeSystem.ChallengeInfo> item in achievementList)
			{
				ChallengeItemRendererScript challengeItemRendererScript = (ManuallyPopulated ? Grid.transform.GetChild(num).GetComponent<ChallengeItemRendererScript>() : ((ChallengeItemRendererScript)Object.Instantiate(ListItemRenderer)));
				challengeItemRendererScript.Init(achievementList[item.Key], achievementList[item.Key].UID, Grid.transform);
				num++;
			}
		}
		else
		{
			List<ChallengeSystem.ChallengeInfo> activeChallengeList = ChallengeSystem.instance.GetActiveChallengeList();
			foreach (ChallengeSystem.ChallengeInfo item2 in activeChallengeList)
			{
				if (!ManuallyPopulated)
				{
					ChallengeItemRendererScript challengeItemRendererScript2 = (ChallengeItemRendererScript)Object.Instantiate(ListItemRenderer);
					challengeItemRendererScript2.Init(item2, item2.UID, Grid.transform);
				}
				else
				{
					ChallengeItemRendererScript challengeItemRendererScript2 = Grid.transform.GetChild(num).GetComponent<ChallengeItemRendererScript>();
					challengeItemRendererScript2.Init(item2, item2.UID, Grid.transform, false);
				}
				num++;
			}
		}
		Grid.Reposition();
		Grid.repositionNow = true;
	}

	private void Update()
	{
		if (Grid.transform.GetChild(0).transform.position.y < 0f)
		{
			RepositionGrid();
		}
	}

	public void RepositionGrid()
	{
		Grid.Reposition();
		Grid.repositionNow = true;
	}

	public void UpdateActiveChallenges()
	{
		numCompleted = 0;
		for (int i = 0; i < Grid.transform.GetChildCount(); i++)
		{
			ChallengeItemRendererScript component = Grid.transform.GetChild(i).GetComponent<ChallengeItemRendererScript>();
			ChallengeSystem.ChallengeInfo challenge = component.Challenge;
			if (!ChallengeSystem.instance.achievementList.ContainsKey(challenge.UID))
			{
				PopulateList();
				break;
			}
		}
		for (int j = 0; j < Grid.transform.GetChildCount(); j++)
		{
			ChallengeItemRendererScript component2 = Grid.transform.GetChild(j).GetComponent<ChallengeItemRendererScript>();
			ChallengeSystem.ChallengeInfo challenge2 = component2.Challenge;
			int value = ((!isAchievementGrid) ? ((int)StatsTracker.instance.GetStat(challenge2.PrimaryStat)) : ((int)StatsTracker.instance.GetLifetimeStat(challenge2.PrimaryStat)));
			int num = (int)(challenge2.PrimaryInit + (float)challenge2.CurrentLevel * challenge2.PrimaryScale);
			if (challenge2.CurrentComplete)
			{
				num = (int)(challenge2.PrimaryInit + (float)challenge2.CompleteLevel * challenge2.PrimaryScale);
				value = num;
			}
			value = Mathf.Clamp(value, 0, num);
			UILocalize component3 = component2.DescLabel.GetComponent<UILocalize>();
			component3.key = challenge2.LocKeyDesc;
			component3.Localize();
			component2.DescLabel.text = ChallengeSystem.instance.ChallengeTextVariableReplacement(challenge2, component3.GetComponent<UILabel>().text);
			component3.enabled = false;
			if (showCompletionStatus)
			{
				component2.CompletionStatusLabel.text = value + "/" + num;
				component2.ProgressBar.sliderValue = (float)value / (float)num;
			}
			if (!isAchievementGrid)
			{
				if (ChallengeSystem.instance.CheckChallengeCompletion(challenge2.UID))
				{
					numCompleted++;
					if (component2.CompletedIcon != null)
					{
						component2.CompletedIcon.isChecked = true;
					}
					if (component2.Reward != null)
					{
						component2.RewardLabel.text = "+500";
					}
					if (!ManuallyPopulated)
					{
						component2.DescLabel.color = new Color(1f, 1f, 1f, 1f);
					}
				}
				else
				{
					if (component2.CompletedIcon != null)
					{
						component2.CompletedIcon.isChecked = false;
					}
					if (component2.Reward != null)
					{
						component2.RewardLabel.text = "+0";
					}
					if (!ManuallyPopulated)
					{
						component2.DescLabel.color = new Color(1f, 1f, 1f, 0.75f);
					}
				}
				if (NumCompletedLabel != null)
				{
					NumCompletedLabel.text = numCompleted + "/3";
				}
			}
			else
			{
				string text = component2.DescLabel.text;
				component3.key = challenge2.LocKeyName;
				component3.Localize();
				component2.DescLabel.text = "[FFCC66]" + component2.DescLabel.text + "[-]\n" + text;
				if (ChallengeSystem.instance.CheckAchievementCompletion(challenge2.UID))
				{
					numCompleted++;
					component2.Icon.SetIconSprite(challenge2.SpriteName, IconScript.HexLevel.HEX_V3);
					component2.Icon.IconSprite.alpha = 1f;
					component2.DescLabel.color = new Color(1f, 1f, 1f, 1f);
					component2.Background.alpha = 1f;
				}
				else
				{
					component2.DescLabel.color = new Color(1f, 1f, 1f, 0.75f);
					component2.Icon.SetIconSprite(challenge2.SpriteName, IconScript.HexLevel.HEX_V1);
					component2.Icon.IconSprite.alpha = 0.5f;
					component2.Background.alpha = 0f;
				}
				if (NumCompletedLabel != null)
				{
					NumCompletedLabel.text = numCompleted + "/" + Grid.transform.GetChildCount();
				}
			}
		}
		UIManager.instance.SwapFont();
	}
}
