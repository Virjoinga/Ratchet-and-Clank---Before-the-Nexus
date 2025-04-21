using UnityEngine;

public class PopupBoxScript : MonoBehaviour
{
	public UIGrid ButtonGrid;

	public UIButton OKButton;

	public UIButton CancelButton;

	public UILabel ConfirmText;

	public UILocalize ConfirmTextLoc;

	public UITweener AlphaTween;

	public UITweener ScaleTween;

	public GameObject Overlay;

	public bool typewriterEffect;

	public bool noCancelButton;

	private void Start()
	{
		if (ConfirmText != null)
		{
			ConfirmTextLoc = ConfirmText.GetComponent<UILocalize>();
		}
	}

	public void Show()
	{
		SFXManager.instance.PlaySound("UI_confirm");
		base.gameObject.SetActive(true);
		AlphaTween.Reset();
		ScaleTween.Reset();
		AlphaTween.Play(true);
		ScaleTween.Play(true);
		if (typewriterEffect)
		{
			ConfirmText.GetComponent<TypewriterEffect>().Reset();
		}
		UIEventListener.Get(Overlay).onClick = OverlayClicked;
		if (CancelButton != null)
		{
			if (noCancelButton)
			{
				CancelButton.gameObject.SetActive(false);
			}
			else
			{
				CancelButton.gameObject.SetActive(true);
			}
			UIEventListener.Get(CancelButton.gameObject).onClick = OverlayClicked;
		}
		if (OKButton != null)
		{
			UIEventListener.Get(OKButton.gameObject).onClick = OKClicked;
		}
		if (ButtonGrid != null)
		{
			ButtonGrid.Reposition();
		}
		UIManager.instance.SwapFont();
		UIManager.instance.SwipingOn = false;
	}

	public void OverlayClicked(GameObject obj)
	{
		SFXManager.instance.PlaySound("UI_ClosePopup");
		Hide();
	}

	public void OKClicked(GameObject obj)
	{
		Hide();
		if (noCancelButton)
		{
			OverlayClicked(obj);
		}
	}

	public void Hide()
	{
		UIManager.instance.SwipingOn = true;
		base.gameObject.SetActive(false);
	}
}
