using UnityEngine;

public class CounterScript : MonoBehaviour
{
	public float Delay;

	public float CounterTime = 1f;

	private float StartTime;

	private float CurrentTime;

	private float ValueMax;

	private float ValueStart;

	private float Value;

	private bool isCountingDown;

	public string AppendString;

	public Color FinishedFontColor;

	public bool Started;

	private void Start()
	{
	}

	public void Init(float StartVal = 0f)
	{
		Started = false;
		GetComponent<UILabel>().text = Value + AppendString;
	}

	public void StartCounter(float StartValue, float EndValue)
	{
		if (!Started)
		{
			Started = true;
			base.enabled = true;
			if (StartValue == EndValue)
			{
				Value = StartValue;
			}
			ValueStart = StartValue;
			GetComponent<UILabel>().text = StartValue + AppendString;
		}
		else
		{
			ValueStart = Value;
		}
		if (EndValue - StartValue < 0f)
		{
			isCountingDown = true;
		}
		else
		{
			isCountingDown = false;
		}
		StartTime = Time.realtimeSinceStartup;
		ValueMax = EndValue;
	}

	private void Update()
	{
		if (!Started)
		{
			return;
		}
		CurrentTime = Time.realtimeSinceStartup - StartTime;
		if (CurrentTime < Delay)
		{
			return;
		}
		Value = ValueStart + (ValueMax - ValueStart) * ((CurrentTime - Delay) / CounterTime);
		if (GameController.instance.inMenu)
		{
			SFXManager.instance.PlayUniqueSound("UI_tally");
		}
		if (isCountingDown)
		{
			if (Value < ValueMax)
			{
				FinishCounter();
			}
		}
		else if (Value >= ValueMax)
		{
			FinishCounter();
		}
		int num = (int)Value;
		GetComponent<UILabel>().text = num + AppendString;
	}

	public void FinishCounter()
	{
		Value = ValueMax;
		Started = false;
		SFXManager.instance.StopSound("UI_tally");
		GetComponent<UILabel>().color = FinishedFontColor;
	}
}
