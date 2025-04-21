using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TintZone : MonoBehaviour
{
	public Color innerColor;

	public Color outerColor = new Color(0.388f, 0.388f, 0.388f);

	public string affectedTags = "Player";

	protected HashSet<GameObject> visiters = new HashSet<GameObject>();

	public AnimationCurve tintCurve = AnimationCurve.Linear(1f, 0f, 0f, 1f);

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(innerColor.r, innerColor.g, innerColor.b, 0.3f);
		Gizmos.DrawSphere(base.GetComponent<Collider>().transform.position, base.GetComponent<Collider>().bounds.size.x / 2f);
	}

	private void OnTriggerExit(Collider other)
	{
		if (visiters.Contains(other.gameObject))
		{
			visiters.Remove(other.gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (base.gameObject.activeSelf && base.gameObject.activeInHierarchy && affectedTags.ToLower().Contains(other.gameObject.tag.ToLower()) && !visiters.Contains(other.gameObject))
		{
			visiters.Add(other.gameObject);
			other.gameObject.SendMessage("TintToZone", this, SendMessageOptions.DontRequireReceiver);
		}
	}
}
