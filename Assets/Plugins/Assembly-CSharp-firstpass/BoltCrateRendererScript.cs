using UnityEngine;

public class BoltCrateRendererScript : MonoBehaviour
{
	public UILabel BoltLabel;

	public UILabel CostLabel;

	public UIButton BuyButton;

	public PurchasableItem Item;

	public IconScript Icon;

	private void Start()
	{
	}

	public void Init(PurchasableItem TheItem, Transform Parent)
	{
		Item = TheItem;
		base.transform.parent = Parent.transform;
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
	}
}
