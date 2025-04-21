using System.Collections.Generic;
using UnityEngine;

public class UICredits : UIScreen
{
	public class CreditInfo
	{
		public string Name;
	}

	public class CreditGroupInfo
	{
		public string Name;

		public List<CreditInfo> Credits;
	}

	private TweenPosition CreditTweener;

	public GameObject CreditsGridOffset;

	public UIPanel CreditsPanel;

	public UIGrid CreditsGrid;

	public float StartingYOffset;

	public float CreditsSpeed = 5f;

	public float DestinationY;

	public Vector4 StartClip;

	public Vector3 StartPosition;

	private List<CreditGroupInfo> CreditList = new List<CreditGroupInfo>();

	public GameObject CreditRenderer;

	public CreditHeaderRendererScript CreditHeaderRenderer;

	private int total;

	private bool isPopulated;

	private float RealTime;

	private void LoadCreditInfo()
	{
		if (isPopulated)
		{
			return;
		}
		int num = 0;
		cmlReader cmlReader2 = new cmlReader("Data/Credits");
		if (cmlReader2 != null)
		{
			List<cmlData> list = cmlReader2.Children();
			foreach (cmlData item in list)
			{
				CreditGroupInfo creditGroupInfo = new CreditGroupInfo();
				creditGroupInfo.Credits = new List<CreditInfo>();
				creditGroupInfo.Name = item["Name"];
				List<cmlData> list2 = cmlReader2.Children(item.ID);
				CreditHeaderRendererScript creditHeaderRendererScript = (CreditHeaderRendererScript)Object.Instantiate(CreditHeaderRenderer);
				creditHeaderRendererScript.Image.spriteName = item["SpriteName"];
				creditHeaderRendererScript.Image.transform.localPosition = new Vector3(creditHeaderRendererScript.Image.transform.localPosition.x + (float)int.Parse(item["SpriteOffset"]), creditHeaderRendererScript.Image.transform.localPosition.y, creditHeaderRendererScript.Image.transform.localPosition.z);
				creditHeaderRendererScript.Image.MakePixelPerfect();
				creditHeaderRendererScript.Image.transform.localScale = new Vector3(creditHeaderRendererScript.Image.transform.localScale.x * 2f, creditHeaderRendererScript.Image.transform.localScale.y * 2f, creditHeaderRendererScript.Image.transform.localScale.z * 2f);
				creditHeaderRendererScript.name = "0";
				if (num < 100)
				{
					creditHeaderRendererScript.name += "0";
				}
				if (num < 10)
				{
					creditHeaderRendererScript.name += "0";
				}
				creditHeaderRendererScript.name = creditHeaderRendererScript.name + num + "CreditHeaderRenderer";
				creditHeaderRendererScript.transform.parent = CreditsGrid.transform;
				creditHeaderRendererScript.transform.localPosition = Vector3.zero;
				creditHeaderRendererScript.transform.localScale = Vector3.one;
				creditHeaderRendererScript.transform.Find("Label").GetComponent<UILabel>().text = creditGroupInfo.Name;
				foreach (cmlData item2 in list2)
				{
					num++;
					CreditInfo creditInfo = new CreditInfo();
					creditInfo.Name = item2["Name"];
					creditGroupInfo.Credits.Add(creditInfo);
					GameObject gameObject = (GameObject)Object.Instantiate(CreditRenderer);
					gameObject.name = "0";
					if (num < 100)
					{
						gameObject.name += "0";
					}
					if (num < 10)
					{
						gameObject.name += "0";
					}
					gameObject.name = gameObject.name + num + "CreditRenderer";
					gameObject.transform.parent = CreditsGrid.transform;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localScale = Vector3.one;
					gameObject.transform.Find("Label").GetComponent<UILabel>().text = creditInfo.Name;
				}
				if (creditHeaderRendererScript.GetComponent<UIPanel>() != null)
				{
					Object.Destroy(creditHeaderRendererScript.GetComponent<UIPanel>());
				}
				CreditList.Add(creditGroupInfo);
				num++;
			}
			CreditsGrid.repositionNow = true;
			CreditsGrid.Reposition();
			total = num;
		}
		isPopulated = true;
		StartClip = new Vector4(CreditsPanel.clipRange.x, CreditsPanel.clipRange.y - StartingYOffset, CreditsPanel.clipRange.z, CreditsPanel.clipRange.w);
		StartPosition = new Vector3(CreditsPanel.transform.localPosition.x, CreditsPanel.transform.localPosition.y + StartingYOffset, CreditsPanel.transform.localPosition.z);
		CreditsPanel.clipRange = StartClip;
		CreditsPanel.transform.localPosition = StartPosition;
		DestinationY += CreditsPanel.transform.localPosition.y + (float)total * CreditsGrid.cellHeight - StartingYOffset;
		Debug.Log("DestinationY: " + DestinationY);
	}

	private void Start()
	{
		LoadCreditInfo();
	}

	public override void Show()
	{
		LoadCreditInfo();
		base.Show();
		EasyAnalytics.Instance.sendView("/Credits");
		MusicManager.instance.Play(MusicManager.eMusicTrackType.Groove, true, 0f);
	}

	private void Update()
	{
		float num = UpdateRealTimeDelta();
		float num2 = num * CreditsSpeed;
		CreditsPanel.transform.localPosition = new Vector3(CreditsPanel.transform.localPosition.x, CreditsPanel.transform.localPosition.y + num2, CreditsPanel.transform.localPosition.z);
		CreditsPanel.clipRange = new Vector4(CreditsPanel.clipRange.x, CreditsPanel.clipRange.y - num2, CreditsPanel.clipRange.z, CreditsPanel.clipRange.w);
		if (CreditsPanel.transform.localPosition.y >= DestinationY)
		{
			CreditsPanel.transform.localPosition = StartPosition;
			CreditsPanel.clipRange = StartClip;
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.creditsWatched);
			StatsTracker.instance.SaveStatsAndReset();
		}
		else if (CreditsPanel.transform.localPosition.y < StartPosition.y)
		{
			CreditsPanel.transform.localPosition = StartPosition;
			CreditsPanel.clipRange = StartClip;
		}
	}

	private void CreditsComplete()
	{
		Show();
	}
}
