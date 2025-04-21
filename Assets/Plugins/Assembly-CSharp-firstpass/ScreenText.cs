using System.Collections.Generic;
using UnityEngine;

public class ScreenText : MonoBehaviour
{
	public delegate string ScreenTextDelegate();

	public static ScreenText instance;

	public GUIStyle guiStyle;

	private List<ScreenTextDelegate> textList;

	public int startX = 25;

	public int startY = 25;

	public int width = 300;

	public int height = 20;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("ScreenText: Multiple instances spawned!");
			Object.Destroy(base.gameObject);
		}
		else
		{
			textList = new List<ScreenTextDelegate>();
			instance = this;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void DisplayText(ScreenTextDelegate text)
	{
		textList.Add(text);
	}

	private void OnGUI()
	{
		if (textList != null)
		{
			for (int i = 0; i < textList.Count; i++)
			{
				GUI.Label(new Rect(startX, startY + i * height, width, height), textList[i]());
			}
		}
	}
}
