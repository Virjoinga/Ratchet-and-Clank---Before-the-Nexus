using UnityEngine;

[RequireComponent(typeof(UILabel))]
[ExecuteInEditMode]
public class LabelFit : MonoBehaviour
{
	public enum FitType
	{
		First = 0,
		Horizontal = 1,
		Vertical = 2,
		Both = 3
	}

	public FitType type;

	public float width = 100f;

	public float height = 100f;

	public float centerX;

	public float centerY;

	public Vector3 MinScale = new Vector3(1f, 1f, 1f);

	public Vector3 MaxScale = new Vector3(32f, 32f, 1f);

	private UILabel m_label;

	private void OnEnable()
	{
		m_label = GetComponent<UILabel>();
	}

	public void Update()
	{
		DoUpdate();
	}

	public void DoUpdate()
	{
		float num = width / m_label.relativeSize.x;
		float num2 = height / m_label.relativeSize.y;
		Vector3 localScale = m_label.transform.localScale;
		if (type == FitType.First)
		{
			if (num < num2)
			{
				localScale.x = (localScale.y = num);
			}
			else
			{
				localScale.x = (localScale.y = num2);
			}
		}
		else
		{
			if (type == FitType.Both || type == FitType.Horizontal)
			{
				localScale.x = num;
			}
			if (type == FitType.Both || type == FitType.Vertical)
			{
				localScale.y = num2;
			}
		}
		localScale.x = Mathf.Clamp(localScale.x, MinScale.x, MaxScale.x);
		localScale.y = Mathf.Clamp(localScale.y, MinScale.y, MaxScale.y);
		m_label.transform.localScale = localScale;
	}

	public void OnDrawGizmos()
	{
		Vector3 position = m_label.transform.position;
		position.x += centerX * m_label.transform.parent.lossyScale.x;
		position.y += centerY * m_label.transform.parent.lossyScale.y;
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(position, new Vector3(width * base.transform.parent.lossyScale.x, height * base.transform.parent.lossyScale.y, 0.01f));
	}
}
