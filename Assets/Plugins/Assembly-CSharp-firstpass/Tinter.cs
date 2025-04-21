using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Tinter : MonoBehaviour
{
	protected TintZone currentZone;

	public GameObject objectToTint;

	private void Start()
	{
		if (objectToTint == null)
		{
			objectToTint = base.gameObject;
		}
	}

	private void TintToZone(TintZone newZone)
	{
		currentZone = newZone;
	}

	private void Update()
	{
		if ((bool)currentZone)
		{
			float value = (currentZone.collider.transform.position - base.transform.position).magnitude / (currentZone.collider.bounds.size.x * 0.5f);
			value = Mathf.Clamp01(value);
			objectToTint.renderer.material.color = Color.Lerp(currentZone.outerColor, currentZone.innerColor, currentZone.tintCurve.Evaluate(value));
		}
	}
}
