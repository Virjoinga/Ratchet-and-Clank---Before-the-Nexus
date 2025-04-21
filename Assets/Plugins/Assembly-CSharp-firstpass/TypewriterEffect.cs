using UnityEngine;

[AddComponentMenu("NGUI/Examples/Typewriter Effect")]
[RequireComponent(typeof(UILabel))]
public class TypewriterEffect : MonoBehaviour
{
	public int charsPerSecond = 50;

	private UILabel mLabel;

	private string mText;

	private string OriginalText = string.Empty;

	private int mOffset;

	private float mNextChar;

	public void UpdateOriginalText(string text)
	{
		OriginalText = text;
	}

	public void Reset()
	{
		mLabel = null;
		mOffset = 0;
		mNextChar = 0f;
		base.enabled = true;
		mLabel = GetComponent<UILabel>();
		mLabel.enabled = true;
		if (OriginalText == string.Empty)
		{
			OriginalText = mLabel.text;
		}
		mLabel.symbolStyle = UIFont.SymbolStyle.None;
		mText = mLabel.font.WrapText(OriginalText, (float)mLabel.lineWidth / mLabel.cachedTransform.localScale.x, mLabel.maxLineCount, true, UIFont.SymbolStyle.None);
	}

	private void Update()
	{
		if (mLabel == null)
		{
			mLabel = GetComponent<UILabel>();
			mLabel.enabled = true;
			mLabel.symbolStyle = UIFont.SymbolStyle.None;
			if (OriginalText == string.Empty)
			{
				OriginalText = mLabel.text;
			}
			mText = mLabel.font.WrapText(OriginalText, (float)mLabel.lineWidth / mLabel.cachedTransform.localScale.x, mLabel.maxLineCount, true, UIFont.SymbolStyle.None);
		}
		if (mOffset < mText.Length)
		{
			if (!(mNextChar <= Time.time))
			{
				return;
			}
			charsPerSecond = Mathf.Max(1, charsPerSecond);
			float num = 1f / (float)charsPerSecond;
			char c = mText[mOffset];
			if (c == '.' || c == ',' || c == '!' || c == '?')
			{
				num *= 4f;
			}
			string text = mText.Substring(0, ++mOffset);
			switch (c)
			{
			case '[':
			{
				for (int i = 0; i < 20; i++)
				{
					text = mText.Substring(0, ++mOffset);
					int num2 = text.IndexOf(']', mOffset - 1);
					if (num2 != -1)
					{
						break;
					}
				}
				break;
			}
			case '\\':
				text = mText.Substring(0, ++mOffset);
				break;
			}
			mLabel.text = text;
			mNextChar = Time.time + num;
		}
		else
		{
			base.enabled = false;
		}
	}
}
