using System;
using UnityEngine;

public class UIFlags : MonoBehaviour
{
	public GameObject USFlag;

	public GameObject EUFlag;

	public GameObject JPFlag;

	public GameObject PreviewFlag;

	public GameObject USSelected;

	public GameObject EUSelected;

	public GameObject JPSelected;

	private string SelectedFlag;

	public GameObject TermsOfUseButton;

	public GameObject PrivacyPolicyButton;

	private void Start()
	{
		USSelected.SetActive(false);
		EUSelected.SetActive(false);
		JPSelected.SetActive(false);
		UIEventListener uIEventListener = UIEventListener.Get(USFlag);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnUSFlagClicked));
		UIEventListener uIEventListener2 = UIEventListener.Get(EUFlag);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnEUFlagClicked));
		UIEventListener uIEventListener3 = UIEventListener.Get(JPFlag);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnJPFlagClicked));
		UIEventListener uIEventListener4 = UIEventListener.Get(TermsOfUseButton);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(TermsOfUseButtonClicked));
		UIEventListener uIEventListener5 = UIEventListener.Get(PrivacyPolicyButton);
		uIEventListener5.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener5.onClick, new UIEventListener.VoidDelegate(PrivacyPolicyButtonClicked));
		UpdateButtons();
	}

	private void OnUSFlagClicked(GameObject Obj)
	{
		SetSelectedFlag("US");
		USSelected.SetActive(true);
		EUSelected.SetActive(false);
		JPSelected.SetActive(false);
	}

	private void OnEUFlagClicked(GameObject Obj)
	{
		SetSelectedFlag("EU");
		USSelected.SetActive(false);
		EUSelected.SetActive(true);
		JPSelected.SetActive(false);
	}

	private void OnJPFlagClicked(GameObject Obj)
	{
		SetSelectedFlag("JP");
		USSelected.SetActive(false);
		EUSelected.SetActive(false);
		JPSelected.SetActive(true);
	}

	private void UpdateButtons()
	{
		Color color = new Color(1f, 1f, 1f, 1f);
		Color color2 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		if (SelectedFlag == null)
		{
			TermsOfUseButton.GetComponentInChildren<UILabel>().color = color2;
			PrivacyPolicyButton.GetComponentInChildren<UILabel>().color = color2;
			TermsOfUseButton.GetComponent<BoxCollider>().collider.enabled = false;
			PrivacyPolicyButton.GetComponent<BoxCollider>().collider.enabled = false;
			TermsOfUseButton.GetComponent<UIButton>().UpdateColor(false, true);
			PrivacyPolicyButton.GetComponent<UIButton>().UpdateColor(false, true);
		}
		else
		{
			TermsOfUseButton.GetComponentInChildren<UILabel>().color = color;
			PrivacyPolicyButton.GetComponentInChildren<UILabel>().color = color;
			TermsOfUseButton.GetComponent<BoxCollider>().collider.enabled = true;
			PrivacyPolicyButton.GetComponent<BoxCollider>().collider.enabled = true;
			TermsOfUseButton.GetComponent<UIButton>().UpdateColor(true, true);
			PrivacyPolicyButton.GetComponent<UIButton>().UpdateColor(true, true);
		}
	}

	private void PrivacyPolicyButtonClicked(GameObject Obj)
	{
		switch (SelectedFlag)
		{
		case "US":
			Application.OpenURL("http://us.playstation.com/support/privacypolicy/");
			break;
		case "EU":
			Application.OpenURL("http://eu.playstation.com/legal");
			break;
		case "JP":
			break;
		}
	}

	private void TermsOfUseButtonClicked(GameObject Obj)
	{
		switch (SelectedFlag)
		{
		case "US":
			Application.OpenURL("http://us.playstation.com/support/termsofservice/");
			break;
		case "EU":
			Application.OpenURL("http://eu.playstation.com/legal");
			break;
		case "JP":
			break;
		}
	}

	private void SetSelectedFlag(string Flag)
	{
		SelectedFlag = Flag;
		UpdateButtons();
	}
}
