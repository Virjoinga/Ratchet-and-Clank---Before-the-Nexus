using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPersistent : MonoBehaviour
{
	public PopupBoxScript Popup;

	public string CurrentPopupPrefab = "ConfirmationPanel";

	public UIAnchor CenterAnchor;

	public UITexture BG;

	public TweenAlpha BGTween;

	public UISprite HexBG;

	public TweenAlpha HexBGTween;

	private bool isChallengeCompleteShowing;

	private bool isAchievementCompleteShowing;

	public GameObject AchievementContainer;

	public GameObject AchievementFX;

	public UITweener AchievementScaleTween;

	public UITweener AchievementBannerTween;

	public IconScript AchievementIcon;

	public GameObject ChallengeContainer;

	public GameObject ChallengeFX;

	public UITweener ChallengeScaleTween;

	public UITweener ChallengeBannerTween;

	public UILabel txt_ChallengeCompleted;

	public UILabel txt_ChallengeCompletedDesc;

	public UILabel txt_AchievementCompleted;

	public UILabel txt_AchievementCompletedDesc;

	public UILabel AchievementReward;

	public UILabel ChallengeReward;

	public GameObject LoadingGear;

	public GameObject MoviePlane;

	public List<ChallengeSystem.ChallengeInfo> QueuedChallengeCompletions = new List<ChallengeSystem.ChallengeInfo>();

	public List<ChallengeSystem.ChallengeInfo> QueuedAchievementCompletions = new List<ChallengeSystem.ChallengeInfo>();

	private bool LastNotificationWasChallenge;

	private bool LastNotificationWasAchievement;

	public UILabel Subtitle;

	public GameObject SubtitleContainer;

	public GameObject Black;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			ShowAchievementCompleted(ChallengeSystem.instance.achievementList[216]);
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			ShowChallengeCompleted(ChallengeSystem.instance.challengeList[1]);
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				UIManager.instance.ShowSubtitle("RCN_MOBILE_FMV_001", 4f);
			}
			else
			{
				UIManager.instance.ShowSubtitle("RCN_MOBILE_FMV_002", 4f);
			}
		}
	}

	private void Start()
	{
		Black.SetActive(false);
		BG.gameObject.SetActive(true);
		HideChallengeCompleted(true);
		HideAchievementCompleted(true);
	}

	private void Awake()
	{
		InstantiatePopup(CurrentPopupPrefab);
		Popup.gameObject.SetActive(false);
	}

	public void InstantiatePopup(string PopupPrefabName)
	{
		if (Popup != null)
		{
			UnityEngine.Object.Destroy(Popup.gameObject);
		}
		CurrentPopupPrefab = PopupPrefabName;
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/" + CurrentPopupPrefab)) as GameObject;
		Popup = gameObject.GetComponent<PopupBoxScript>();
		Popup.transform.parent = CenterAnchor.transform;
		Popup.transform.localPosition = Vector3.zero;
		Popup.transform.localScale = Vector3.one;
	}

	public void ShowPopup(string PopupPrefab, string ConfirmTextLocKey, bool noCancelButton)
	{
		if (PopupPrefab != CurrentPopupPrefab)
		{
			InstantiatePopup(PopupPrefab);
		}
		if (ConfirmTextLocKey != string.Empty)
		{
			Popup.ConfirmTextLoc.key = ConfirmTextLocKey;
			Popup.ConfirmTextLoc.Localize();
			Popup.ConfirmTextLoc.enabled = false;
		}
		Popup.noCancelButton = noCancelButton;
		Popup.Show();
	}

	public void SetPopupOKButtonCallback(UIEventListener.VoidDelegate OKButtonCallback)
	{
		UIEventListener uIEventListener = UIEventListener.Get(Popup.OKButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, OKButtonCallback);
	}

	public void SetPopupCancelButtonCallback(UIEventListener.VoidDelegate CancelButtonCallback)
	{
		UIEventListener uIEventListener = UIEventListener.Get(Popup.CancelButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, CancelButtonCallback);
	}

	public void HidePopup()
	{
		Popup.Hide();
	}

	public void HexTweenComplete()
	{
		if (HexBGTween.tweenFactor == 0f)
		{
			HexBGTween.gameObject.SetActive(false);
		}
	}

	public void BGTweenComplete()
	{
		if (BGTween.tweenFactor == 1f)
		{
			BG.gameObject.SetActive(false);
		}
	}

	public void ShowChallengeCompleted(ChallengeSystem.ChallengeInfo Challenge)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (isChallengeCompleteShowing || isAchievementCompleteShowing)
		{
			QueuedChallengeCompletions.Add(Challenge);
			return;
		}
		LastNotificationWasChallenge = true;
		LastNotificationWasAchievement = false;
		ChallengeContainer.SetActive(true);
		ChallengeScaleTween.Reset();
		ChallengeBannerTween.Reset();
		ChallengeBannerTween.Play(true);
		ChallengeBannerTween.enabled = true;
		ChallengeFX.GetComponent<UIParticle>().Play();
		SFXManager.instance.PlaySound("Achievement_Complete_Notification");
		UILocalize component = txt_ChallengeCompletedDesc.GetComponent<UILocalize>();
		component.key = Challenge.LocKeyDesc;
		component.Localize();
		txt_ChallengeCompletedDesc.text = ChallengeSystem.instance.ChallengeTextVariableReplacement(Challenge, txt_ChallengeCompletedDesc.text);
		component.enabled = false;
		StopCoroutine("ChallengeCompleteLabelTimer");
		StartCoroutine("ChallengeCompleteLabelTimer");
		isChallengeCompleteShowing = true;
		if (isAchievementCompleteShowing)
		{
			HideAchievementCompleted();
			StopCoroutine("AchievementCompleteLabelTimer");
		}
		ChallengeReward.text = "+" + GameController.instance.playerController.ChallengeReward;
		UIManager.instance.SwapFont();
	}

	private IEnumerator ChallengeCompleteLabelTimer()
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + 4f)
		{
			yield return null;
		}
		HideChallengeCompleted();
	}

	public void HideChallengeCompleted(bool instant = false)
	{
		isChallengeCompleteShowing = false;
		if (instant)
		{
			HideChallengeTweenCompleted();
		}
		else if (!CheckQueuedChallenges())
		{
			ChallengeScaleTween.Play(true);
			ChallengeScaleTween.enabled = true;
		}
	}

	public void HideChallengeTweenCompleted()
	{
		if (!isChallengeCompleteShowing)
		{
			ChallengeScaleTween.enabled = false;
			ChallengeContainer.SetActive(false);
		}
	}

	public void ShowAchievementCompleted(ChallengeSystem.ChallengeInfo Challenge)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (isChallengeCompleteShowing || isAchievementCompleteShowing)
		{
			QueuedAchievementCompletions.Add(Challenge);
			return;
		}
		LastNotificationWasChallenge = false;
		LastNotificationWasAchievement = true;
		AchievementContainer.SetActive(true);
		AchievementScaleTween.Reset();
		AchievementBannerTween.Reset();
		AchievementBannerTween.Play(true);
		AchievementBannerTween.enabled = true;
		AchievementFX.GetComponent<UIParticle>().Play();
		SFXManager.instance.PlaySound("Achievement_Complete_Notification");
		UILocalize component = txt_AchievementCompletedDesc.GetComponent<UILocalize>();
		component.key = Challenge.LocKeyName;
		component.Localize();
		component.enabled = false;
		AchievementIcon.SetIconSprite(Challenge.SpriteName, IconScript.HexLevel.HEX_V3);
		StopCoroutine("AchievementCompleteLabelTimer");
		StartCoroutine("AchievementCompleteLabelTimer");
		isAchievementCompleteShowing = true;
		if (isChallengeCompleteShowing)
		{
			HideChallengeCompleted();
			StopCoroutine("ChallengeCompleteLabelTimer");
		}
		AchievementReward.text = "+1";
		UIManager.instance.SwapFont();
	}

	private IEnumerator AchievementCompleteLabelTimer()
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + 4f)
		{
			yield return null;
		}
		HideAchievementCompleted();
	}

	public void HideAchievementCompleted(bool instant = false)
	{
		isAchievementCompleteShowing = false;
		if (instant)
		{
			HideAchievementTweenCompleted();
		}
		else if (!CheckQueuedChallenges())
		{
			AchievementScaleTween.Play(true);
			AchievementScaleTween.enabled = true;
		}
	}

	public void HideAchievementTweenCompleted()
	{
		if (!isAchievementCompleteShowing)
		{
			AchievementScaleTween.enabled = false;
			AchievementContainer.SetActive(false);
		}
	}

	public bool CheckQueuedChallenges()
	{
		if (QueuedAchievementCompletions.Count > 0)
		{
			if (LastNotificationWasChallenge)
			{
				HideChallengeCompleted(true);
			}
			ShowAchievementCompleted(QueuedAchievementCompletions[0]);
			QueuedAchievementCompletions.RemoveAt(0);
			return true;
		}
		if (QueuedChallengeCompletions.Count > 0)
		{
			if (LastNotificationWasAchievement)
			{
				HideAchievementCompleted(true);
			}
			ShowChallengeCompleted(QueuedChallengeCompletions[0]);
			QueuedChallengeCompletions.RemoveAt(0);
			return true;
		}
		return false;
	}
}
