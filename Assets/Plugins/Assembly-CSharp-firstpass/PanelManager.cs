using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
	public delegate void ButtonCycleCallback();

	public UIPanel[] PanelArray;

	public UIPanel StartPanel;

	public List<PanelButtonScript> LeftButtons;

	public List<PanelButtonScript> RightButtons;

	private int CurrentPanelIndex;

	public ButtonCycleCallback CycleCallback;

	private float swipeSensitivity = 9f;

	private bool alreadySwiping;

	private void OnEnable()
	{
		bool flag = false;
		for (int i = 0; i < PanelArray.Length; i++)
		{
			if (StartPanel == PanelArray[i])
			{
				flag = true;
				CurrentPanelIndex = i;
				PanelArray[i].gameObject.SetActive(true);
			}
			else
			{
				PanelArray[i].gameObject.SetActive(false);
			}
		}
		if (flag)
		{
			CheckButtons();
		}
		else
		{
			Debug.LogError("PanelManager " + base.name + ": could not find starting panel");
		}
	}

	private void touchMoved(Touch t)
	{
		if (!alreadySwiping)
		{
			alreadySwiping = true;
			if (t.deltaPosition.x < 0f - swipeSensitivity)
			{
				SwitchMenu(PanelButtonScript.ScrollDirection.DIR_Right);
				SFXManager.instance.PlayUniqueSound("UI_Swipe");
			}
			else if (t.deltaPosition.x > swipeSensitivity)
			{
				SwitchMenu(PanelButtonScript.ScrollDirection.DIR_Left);
				SFXManager.instance.PlayUniqueSound("UI_Swipe");
			}
		}
	}

	private void touchEnded()
	{
		alreadySwiping = false;
	}

	public void Update()
	{
		if (UIManager.instance.SwipingOn && Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			if (touch.phase == TouchPhase.Ended)
			{
				touchEnded();
			}
			else if (touch.phase == TouchPhase.Moved)
			{
				touchMoved(touch);
			}
		}
	}

	public void SwitchMenu(PanelButtonScript.ScrollDirection ScrollDir)
	{
		bool flag = false;
		switch (ScrollDir)
		{
		case PanelButtonScript.ScrollDirection.DIR_Left:
			if (CycleLeft())
			{
				flag = true;
			}
			break;
		case PanelButtonScript.ScrollDirection.DIR_Right:
			if (CycleRight())
			{
				flag = true;
			}
			break;
		default:
			Debug.LogError("PanelManager " + base.name + ": Invalid ScrollDirection");
			break;
		}
		if (flag && CycleCallback != null)
		{
			CycleCallback();
		}
		UIManager.instance.SwapFont();
	}

	public void SwitchMenu(int index)
	{
		PanelArray[CurrentPanelIndex].gameObject.SetActive(false);
		PanelArray[index].gameObject.SetActive(true);
		CurrentPanelIndex = index;
		CheckButtons();
		if (CycleCallback != null)
		{
			CycleCallback();
		}
		UIManager.instance.SwapFont();
	}

	private bool CycleLeft()
	{
		if (CurrentPanelIndex - 1 < 0)
		{
			return false;
		}
		PanelArray[CurrentPanelIndex].gameObject.SetActive(false);
		CurrentPanelIndex--;
		PanelArray[CurrentPanelIndex].gameObject.SetActive(true);
		CheckButtons();
		return true;
	}

	private bool CycleRight()
	{
		if (CurrentPanelIndex + 1 >= PanelArray.Length)
		{
			return false;
		}
		PanelArray[CurrentPanelIndex].gameObject.SetActive(false);
		CurrentPanelIndex++;
		PanelArray[CurrentPanelIndex].gameObject.SetActive(true);
		CheckButtons();
		return true;
	}

	private void CheckButtons()
	{
		if (CurrentPanelIndex == 0)
		{
			foreach (PanelButtonScript leftButton in LeftButtons)
			{
				leftButton.GetComponent<BoxCollider>().collider.enabled = false;
				leftButton.GetComponentInChildren<UISprite>().alpha = 0f;
			}
			{
				foreach (PanelButtonScript rightButton in RightButtons)
				{
					rightButton.GetComponent<BoxCollider>().collider.enabled = true;
					rightButton.GetComponentInChildren<UISprite>().alpha = 1f;
				}
				return;
			}
		}
		if (CurrentPanelIndex == PanelArray.Length - 1)
		{
			foreach (PanelButtonScript leftButton2 in LeftButtons)
			{
				leftButton2.GetComponent<BoxCollider>().collider.enabled = true;
				leftButton2.GetComponentInChildren<UISprite>().alpha = 1f;
			}
			{
				foreach (PanelButtonScript rightButton2 in RightButtons)
				{
					rightButton2.GetComponent<BoxCollider>().collider.enabled = false;
					rightButton2.GetComponentInChildren<UISprite>().alpha = 0f;
				}
				return;
			}
		}
		foreach (PanelButtonScript leftButton3 in LeftButtons)
		{
			leftButton3.GetComponent<BoxCollider>().collider.enabled = true;
			leftButton3.GetComponentInChildren<UISprite>().alpha = 1f;
		}
		foreach (PanelButtonScript rightButton3 in RightButtons)
		{
			rightButton3.GetComponent<BoxCollider>().collider.enabled = true;
			rightButton3.GetComponentInChildren<UISprite>().alpha = 1f;
		}
	}

	public int GetCurrentPanelIndex()
	{
		return CurrentPanelIndex;
	}
}
